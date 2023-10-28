using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TripleTriad.Exceptions;
using TripleTriad.Interfaces;
using TripleTriad.Lobbies.Dtos;
using TripleTriad.Lobbies.Events;
using TripleTriad.Users;
using TripleTriad.Users.Dtos;

namespace TripleTriad.Lobbies.Commands;

public sealed class HostLobbyCommand : IRequest<LobbyDto>
{
    public required string LobbyDisplayName { get; init; }

    public required string UserId { get; init; }

    public sealed class Validator : AbstractValidator<HostLobbyCommand>
    {
        public Validator()
        {
            RuleFor(e => e.LobbyDisplayName).NotEmpty();
            RuleFor(e => e.UserId).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<HostLobbyCommand, LobbyDto>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILobbyRepository _lobbyRepository;
        private readonly IUserLobbyCache _userLobbyCache;
        private readonly UserManager<User> _userManager;

        public Handler(IMapper mapper, IMediator mediator, ILobbyRepository lobbyRepository, IUserLobbyCache userLobbyCache, UserManager<User> userManager)
        {
            _mapper = mapper;
            _mediator = mediator;
            _lobbyRepository = lobbyRepository;
            _userLobbyCache = userLobbyCache;
            _userManager = userManager;
        }

        public async Task<LobbyDto> Handle(HostLobbyCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
                throw new NotFoundException("Invalid user");

            if (await _userLobbyCache.GetAsync(user.Id, cancellationToken) is string lobbyId)
            {
                await _mediator.Send(new LeaveLobbyCommand
                {
                    LobbyId = lobbyId,
                    UserId = user.Id,
                    IgnoreMissing = true
                }, cancellationToken);
            }

            var lobby = await _lobbyRepository.AddAsync(new Lobby
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = request.LobbyDisplayName,
                OwnerId = user.Id,
                Users = { new LobbyUser { UserId = user.Id } }
            }, cancellationToken);

            await _userLobbyCache.SetAsync(user.Id, lobby.Id, cancellationToken);

            await _mediator.Publish(new LobbyCreatedEvent { Data = _mapper.Map<LobbyDto>(lobby) }, cancellationToken);

            var lobbyDto = _mapper.Map<LobbyDto>(lobby);
            var userDto = _mapper.Map<UserDto>(user);
            await _mediator.Publish(new UserJoinedLobbyEvent
            {
                Data = new UserInLobbyDto
                {
                    Lobby = lobbyDto,
                    User = userDto,
                }
            }, cancellationToken);

            return lobbyDto;
        }
    }
}
