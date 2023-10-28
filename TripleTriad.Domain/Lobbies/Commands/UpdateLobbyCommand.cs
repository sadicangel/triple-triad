using AutoMapper;
using FluentValidation;
using MediatR;
using TripleTriad.Exceptions;
using TripleTriad.Games;
using TripleTriad.Interfaces;
using TripleTriad.Lobbies.Dtos;
using TripleTriad.Lobbies.Events;

namespace TripleTriad.Lobbies.Commands;
public sealed class UpdateLobbyCommand : IRequest
{
    public required string LobbyId { get; init; }

    public bool IsReady { get; init; }

    public string? DiplayName { get; init; }

    public Ruleset? Rules { get; init; }

    public string UserId { get; set; } = null!;

    public sealed class Validator : AbstractValidator<UpdateLobbyCommand>
    {
        public Validator()
        {
            RuleFor(e => e.LobbyId).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<UpdateLobbyCommand>
    {
        private readonly IPublisher _mediator;
        private readonly IMapper _mapper;
        private readonly ILobbyRepository _lobbyRepository;

        public Handler(IPublisher mediator, IMapper mapper, ILobbyRepository lobbyRepository)
        {
            _mediator = mediator;
            _mapper = mapper;
            _lobbyRepository = lobbyRepository;
        }

        public async Task<Unit> Handle(UpdateLobbyCommand request, CancellationToken cancellationToken)
        {
            var lobby = await _lobbyRepository.GetByIdAsync(request.LobbyId, cancellationToken);
            if (lobby is null)
                throw new NotFoundException("Invalid lobby");

            var lobbyUser = lobby.Users.SingleOrDefault(u => u.UserId == request.UserId);
            if (lobbyUser is null)
                throw new UnauthorizedException("Invalid user");

            // Only the owner can change these settings
            if (lobby.OwnerId == request.UserId)
            {
                if (!string.IsNullOrWhiteSpace(request.DiplayName))
                    lobby.DisplayName = request.DiplayName;
                if (request.Rules is not null)
                    lobby.Rules = request.Rules;
            }

            lobbyUser.IsReady = request.IsReady;

            await _lobbyRepository.UpdateAsync(lobby, cancellationToken);

            await _mediator.Publish(new LobbyUpdatedEvent { Data = _mapper.Map<LobbyDto>(lobby) }, cancellationToken);

            return Unit.Value;
        }
    }
}
