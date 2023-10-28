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

public sealed class JoinLobbyCommand : IRequest<LobbyDto>
{
    public required string LobbyId { get; init; }

    public required string UserId { get; init; }

    public sealed class Validator : AbstractValidator<JoinLobbyCommand>
    {
        public Validator()
        {
            RuleFor(e => e.LobbyId).NotEmpty();
            RuleFor(e => e.UserId).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<JoinLobbyCommand, LobbyDto>
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

        public async Task<LobbyDto> Handle(JoinLobbyCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
                throw new NotFoundException("Invalid user");

            if (await _userLobbyCache.GetAsync(request.UserId, cancellationToken) is string lobbyId)
            {
                await _mediator.Send(new LeaveLobbyCommand
                {
                    LobbyId = lobbyId,
                    UserId = user.Id,
                    IgnoreMissing = true
                }, cancellationToken);
            }

            var lobby = await _lobbyRepository.GetByIdAsync(request.LobbyId, cancellationToken);
            if (lobby is null)
                throw new NotFoundException("Invalid lobby");

            lobby.Users.Add(new LobbyUser { UserId = user.Id });

            await _lobbyRepository.UpdateAsync(lobby, cancellationToken);
            await _userLobbyCache.SetAsync(user.Id, lobby.Id, cancellationToken);

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
