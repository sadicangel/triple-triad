namespace TripleTriad.Contracts;

public interface ISeatController
{
    Seat Seat { get; }

    Task RunAsync(
        IGameSession session,
        CancellationToken cancellationToken = default);
}

public sealed class HumanSeatController(Seat seat) : ISeatController
{
    public Seat Seat { get; } = seat;

    public Task RunAsync(
        IGameSession session,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

public sealed class AiSeatController(Seat seat) : ISeatController
{
    public Seat Seat { get; } = seat;

    public async Task RunAsync(
        IGameSession session,
        CancellationToken cancellationToken = default)
    {
        GameEvent? pendingTurnEvent = null;

        await foreach (var update in session.SubscribeUpdatesAsync(cancellationToken))
        {
            switch (update)
            {
                case GameSessionEventUpdate { GameEvent: MatchStartedEvent started }:
                    await TrySendCommandAsync(session, started, started.Snapshot, cancellationToken);
                    break;
                case GameSessionEventUpdate { GameEvent: TurnChangedEvent turnChanged }:
                    pendingTurnEvent = turnChanged.ActiveSeat == Seat
                        ? turnChanged
                        : null;
                    break;
                case GameSessionSnapshotUpdate snapshotUpdate when pendingTurnEvent is not null:
                    var turnEvent = pendingTurnEvent;
                    pendingTurnEvent = null;
                    await TrySendCommandAsync(session, turnEvent, snapshotUpdate.Snapshot, cancellationToken);
                    break;
            }
        }
    }

    private async ValueTask TrySendCommandAsync(
        IGameSession session,
        GameEvent gameEvent,
        MatchSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        var command = CreateCommand(gameEvent, snapshot);
        if (command is not null)
            await session.SendCommandAsync(command, cancellationToken);
    }

    private GameCommand? CreateCommand(GameEvent gameEvent, MatchSnapshot snapshot)
    {
        if (snapshot.IsComplete || snapshot.ActiveSeat != Seat || !ShouldAct(gameEvent))
            return null;

        var card = snapshot.Hands
            .FirstOrDefault(hand => hand.Seat == Seat)
            ?.Cards
            .FirstOrDefault();
        var boardCell = snapshot.Board.FirstOrDefault(cell => cell.Card is null);

        if (card is null || boardCell is null)
            return null;

        return new PlayCardCommand(
            card.CardInstanceId,
            boardCell.Index,
            $"ai-{Guid.NewGuid():N}");
    }

    private bool ShouldAct(GameEvent gameEvent) =>
        gameEvent switch
        {
            MatchStartedEvent started => started.StartingSeat == Seat,
            TurnChangedEvent turnChanged => turnChanged.ActiveSeat == Seat,
            _ => false,
        };
}

public sealed class RemoteSeatController(Seat seat) : ISeatController
{
    public Seat Seat { get; } = seat;

    public Task RunAsync(
        IGameSession session,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
