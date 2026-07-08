namespace TripleTriad.Contracts;

public interface ILobbySession
{
    LobbySnapshot CurrentSnapshot { get; }

    IAsyncEnumerable<LobbyUpdate> ReadUpdatesAsync(
        CancellationToken cancellationToken = default);

    ValueTask<LobbySnapshot> StartAsync(
        CancellationToken cancellationToken = default);

    ValueTask SetRulesAsync(
        GameRules rules,
        CancellationToken cancellationToken = default);

    ValueTask TakeSeatAsync(
        Seat seat,
        CancellationToken cancellationToken = default);

    ValueTask SetSelectedCardsAsync(
        IReadOnlyList<int> cardNumbers,
        CancellationToken cancellationToken = default);

    ValueTask SetReadyAsync(
        bool isReady,
        CancellationToken cancellationToken = default);

    ValueTask<MatchSetup> WaitForMatchStartAsync(
        CancellationToken cancellationToken = default);
}
