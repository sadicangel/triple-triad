using AutoMapper;
using FluentValidation;
using MediatR;
using System.Drawing;
using TripleTriad.Exceptions;
using TripleTriad.Games;
using TripleTriad.Games.Dtos;
using TripleTriad.Games.Events;
using TripleTriad.Interfaces;

namespace TripleTriad.Lobbies.Commands;
public sealed class StartGameCommand : IRequest
{
    public required string LobbyId { get; init; }
    public required string UserId { get; init; }

    public sealed class Validator : AbstractValidator<StartGameCommand>
    {
        public Validator()
        {
            RuleFor(e => e.LobbyId).NotEmpty();
            RuleFor(e => e.UserId).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<StartGameCommand>
    {
        private readonly IPublisher _mediator;
        private readonly IMapper _mapper;
        private readonly ILobbyRepository _lobbyRepository;
        private readonly IGameRepository _gameRepository;

        public Handler(IPublisher mediator, IMapper mapper, ILobbyRepository lobbyRepository, IGameRepository gameRepository)
        {
            _mediator = mediator;
            _mapper = mapper;
            _lobbyRepository = lobbyRepository;
            _gameRepository = gameRepository;
        }

        public async Task<Unit> Handle(StartGameCommand request, CancellationToken cancellationToken)
        {
            var lobby = await _lobbyRepository.GetByIdAsync(request.LobbyId, cancellationToken);
            if (lobby is null)
                throw new NotFoundException("Invalid lobby");
            if (!lobby.Users.All(e => e.IsReady))
                throw new BadRequestException("One or more users not ready");

            var game = new Game
            {
                Id = Guid.NewGuid().ToString(),
                Rules = lobby.Rules,
                LeftPlayer = new Player
                {
                    UserId = lobby.Users[0].UserId,
                    UserName = lobby.Users[0].UserName,
                    Color = (uint)Color.DarkGreen.ToArgb(),
                    Side = Side.Left,
                    Hand = new List<Card>()
                },
                RightPlayer = new Player
                {
                    UserId = lobby.Users[1].UserId,
                    UserName = lobby.Users[1].UserName,
                    Color = (uint)Color.DarkRed.ToArgb(),
                    Side = Side.Right,
                    Hand = new List<Card>()
                },
                ActiveSide = (Side)Random.Shared.Next(2),
            };

            await _gameRepository.AddAsync(game, cancellationToken);

            await _mediator.Publish(new GameStartedEvent
            {
                Data = _mapper.Map<GameDto>(game)
            }, cancellationToken);

            return Unit.Value;
        }
    }
}
