using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TripleTriad.Exceptions;
using TripleTriad.Users.Dtos;
using TripleTriad.Users.Events;

namespace TripleTriad.Users.Commands;
public sealed class ConnectUserCommand : IRequest
{
    public required string UserId { get; init; }

    public sealed class Validator : AbstractValidator<ConnectUserCommand>
    {
        public Validator()
        {
            RuleFor(e => e.UserId).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<ConnectUserCommand>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;

        public Handler(IMapper mapper, IMediator mediator, UserManager<User> userManager)
        {
            _mapper = mapper;
            _mediator = mediator;
            _userManager = userManager;
        }

        public async Task<Unit> Handle(ConnectUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
                throw new UnauthorizedException("Invalid user");
            await _mediator.Publish(new UserConnectedEvent { Data = _mapper.Map<UserDto>(user) }, cancellationToken);
            // Rejoin lobby?
            //if (await _userLobbyCache.GetAsync(user.Id, cancellationToken) is string lobbyId)
            //    await _mediator.Send(new JoinLobbyCommand { LobbyId = lobbyId, UserId = user.Id }, cancellationToken);
            return Unit.Value;
        }
    }
}