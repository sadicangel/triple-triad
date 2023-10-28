using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TripleTriad.Exceptions;
using TripleTriad.Interfaces;
using TripleTriad.Lobbies.Commands;
using TripleTriad.Users.Dtos;
using TripleTriad.Users.Events;

namespace TripleTriad.Users.Commands;

public sealed class DisconnectUserCommand : IRequest
{
    public required string UserId { get; init; }

    public sealed class Validator : AbstractValidator<DisconnectUserCommand>
    {
        public Validator()
        {
            RuleFor(e => e.UserId).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<DisconnectUserCommand>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;
        private readonly IUserLobbyCache _userLobbyCache;

        public Handler(IMapper mapper, IMediator mediator, UserManager<User> userManager, IUserLobbyCache userLobbyCache)
        {
            _mapper = mapper;
            _mediator = mediator;
            _userManager = userManager;
            _userLobbyCache = userLobbyCache;
        }

        public async Task<Unit> Handle(DisconnectUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
                throw new UnauthorizedException("Invalid user");
            if (await _userLobbyCache.GetAsync(user.Id, cancellationToken) is string lobbyId)
                await _mediator.Send(new LeaveLobbyCommand { LobbyId = lobbyId, UserId = request.UserId, IgnoreMissing = true }, cancellationToken);
            await _mediator.Publish(new UserDisconnectedEvent { Data = _mapper.Map<UserDto>(user) }, cancellationToken);
            return Unit.Value;
        }
    }
}
