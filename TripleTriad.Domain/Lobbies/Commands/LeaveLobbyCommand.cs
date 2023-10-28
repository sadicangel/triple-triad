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
public sealed class LeaveLobbyCommand : IRequest
{
    public required string LobbyId { get; init; }
    public required string UserId { get; init; }
    public bool IgnoreMissing { get; init; }

    public sealed class Validator : AbstractValidator<HostLobbyCommand>
    {
        public Validator()
        {
            RuleFor(e => e.LobbyDisplayName).NotEmpty();
            RuleFor(e => e.UserId).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<LeaveLobbyCommand>
    {
        private readonly IPublisher _mediator;
        private readonly IMapper _mapper;
        private readonly ILobbyRepository _lobbyRepository;
        private readonly IUserLobbyCache _userLobbyCache;
        private readonly UserManager<User> _userManager;

        public Handler(IPublisher mediator, IMapper mapper, ILobbyRepository lobbyRepository, IUserLobbyCache userLobbyCache, UserManager<User> userManager)
        {
            _mediator = mediator;
            _mapper = mapper;
            _lobbyRepository = lobbyRepository;
            _userLobbyCache = userLobbyCache;
            _userManager = userManager;
        }

        public async Task<Unit> Handle(LeaveLobbyCommand request, CancellationToken cancellationToken)
        {
            var lobby = await _lobbyRepository.GetByIdAsync(request.LobbyId, cancellationToken);
            if (lobby is null)
            {
                if (request.IgnoreMissing)
                    return Unit.Value;
                throw new NotFoundException($"Invalid lobby");
            }

            await _userLobbyCache.RemoveAsync(request.UserId, cancellationToken);

            var removed = lobby.Users.Remove(new LobbyUser { UserId = request.UserId });

            if (lobby.Users.Count > 0)
            {
                lobby.OwnerId = lobby.Users[0].UserId;
                lobby.Users[0].IsReady = false;
                await _lobbyRepository.UpdateAsync(lobby, cancellationToken);
                if (removed && await _userManager.FindByIdAsync(request.UserId) is User user)
                {
                    await _mediator.Publish(new UserLeftLobbyEvent
                    {
                        Data = new UserInLobbyDto
                        {
                            User = _mapper.Map<UserDto>(user),
                            Lobby = _mapper.Map<LobbyDto>(lobby),
                        }
                    }, cancellationToken);
                }
            }
            else
            {
                await _lobbyRepository.DeleteAsync(lobby, cancellationToken);
                await _mediator.Publish(new LobbyDeletedEvent { Data = _mapper.Map<LobbyDto>(lobby) }, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
