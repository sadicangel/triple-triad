using TripleTriad.Contracts;
using TripleTriad.Data;
using TripleTriad.Lobby;
using TripleTriad.Networking;
using TripleTriad.Sessions;

var repoRoot = FindRepoRoot();
var catalogPath = Path.Combine(repoRoot, "src", "TripleTriad", "assets", "triple_triad", "cards.json");
var catalog = CardCatalog.Load(catalogPath);

Assert(catalog.Cards.Count == 110, "loads all 110 FFVIII card definitions");
Assert(catalog.Get(110).Name == "Squall", "loads card definitions by number");

var unstarted = new LocalGameSession(catalog);
Assert(unstarted.CurrentSnapshot is null, "current snapshot starts empty before StartAsync");
await AssertThrowsAsync<InvalidOperationException>(
    async () => await unstarted.SendCommandAsync(new PlayCardCommand("unknown", 0, "before-start")),
    "SendCommandAsync before StartAsync throws");

var session = new LocalGameSession(catalog);
var events = new List<GameEvent>();
var eventOrder = new List<string>();
var secondSubscriberEvents = new List<GameEvent>();
var secondSubscriberEventOrder = new List<string>();
await using var eventReader = session.SubscribeEventsAsync().GetAsyncEnumerator();
await using var secondSubscriberReader = session.SubscribeEventsAsync().GetAsyncEnumerator();

var initial = await session.StartAsync();
await AppendNextEventsAsync(eventReader, events, eventOrder, 1, "StartAsync events");
await AppendNextEventsAsync(secondSubscriberReader, secondSubscriberEvents, secondSubscriberEventOrder, 1, "second StartAsync subscriber events");
Assert(session.ConnectionState == SessionConnectionState.Connected, "StartAsync leaves the session connected");
Assert(ReferenceEquals(session.CurrentSnapshot, initial), "StartAsync stores the initial snapshot cache");
AssertSequence(
    eventOrder,
    ["MatchStartedEvent"],
    "StartAsync emits the match-start event");
var matchStarted = events.OfType<MatchStartedEvent>().Single();
Assert(matchStarted.StartingSeat == Seat.Blue, "MatchStarted includes the starting seat");
Assert(ReferenceEquals(matchStarted.Snapshot, initial), "MatchStarted includes the initial snapshot");
AssertSequence(secondSubscriberEventOrder, eventOrder, "multiple session subscribers receive the same start updates");
Assert(
    secondSubscriberEvents.OfType<MatchStartedEvent>().Any(update => update.StartingSeat == Seat.Blue),
    "second subscriber receives MatchStarted");
eventOrder.Clear();

var blueHand = initial.Hands.Single(hand => hand.Seat == Seat.Blue);
var redHand = initial.Hands.Single(hand => hand.Seat == Seat.Red);

Assert(initial.BlueScore == 5 && initial.RedScore == 5, "initial score counts both full hands");
Assert(session.Rules == GameRules.Default, "mock game session exposes default rules");
Assert(initial.Rules == GameRules.Default, "initial mock session snapshot uses default rules");
Assert(blueHand.Cards.All(card => card.IsPlayable), "local active hand is playable");
Assert(redHand.Cards.All(card => !card.IsFaceUp && !card.IsPlayable), "opponent hand visibility is state-driven and hidden by default");
Assert(initial.Board.Count(cell => cell.CanDrop) == 9, "all empty board cells accept the local first move");

await session.SendCommandAsync(new PlayCardCommand(blueHand.Cards[1].CardInstanceId, 99, "invalid-slot"));
await AppendNextEventsAsync(eventReader, events, eventOrder, 1, "invalid move event");
Assert(events.Last() is MoveRejectedEvent, "invalid slot emits MoveRejected on the event stream");
AssertSequence(eventOrder, ["MoveRejectedEvent"], "rejected move emits only rejection event");
eventOrder.Clear();

var playBlue = GameCommandFactory.CreatePlayCardCommand(blueHand.Cards[1], initial.Board[0], "blue-play");
await session.SendCommandAsync(playBlue);
await AppendNextEventsAsync(eventReader, events, eventOrder, 2, "blue move events");

var afterBlue = session.CurrentSnapshot ?? throw new InvalidOperationException("Accepted move did not cache a snapshot.");
Assert(ReferenceEquals(session.CurrentSnapshot, events.OfType<TurnChangedEvent>().Last().Snapshot), "accepted move caches the emitted event snapshot");
Assert(afterBlue.Board[0].Card?.CardInstanceId == playBlue.CardInstanceId, "accepted move places the selected card");
Assert(afterBlue.ActiveSeat == Seat.Red, "accepted move advances the turn");
Assert(afterBlue.Hands.Single(hand => hand.Seat == Seat.Blue).Cards.All(card => !card.IsPlayable), "local cards are not playable during opponent turn");
var bluePlayed = events.OfType<CardPlayedEvent>().Single();
Assert(bluePlayed.BoardSlotIndex == 0, "accepted move raises CardPlayed");
Assert(bluePlayed.Card.CardInstanceId == playBlue.CardInstanceId, "CardPlayed includes played card snapshot");
Assert(bluePlayed.Card.Owner == Seat.Blue, "CardPlayed card snapshot keeps played owner");
Assert(bluePlayed.SourceSeat == Seat.Blue, "CardPlayed includes source seat");
Assert(bluePlayed.SourceHandIndex == 1, "CardPlayed includes source hand index");
Assert(ReferenceEquals(bluePlayed.Snapshot, afterBlue), "CardPlayed carries the final move snapshot");
AssertSequence(eventOrder, ["CardPlayedEvent", "TurnChangedEvent"], "accepted move emits play then turn");
eventOrder.Clear();

AssertThrows<InvalidOperationException>(
    () => GameCommandFactory.CreatePlayCardCommand(blueHand.Cards[2], afterBlue.Board[0], "occupied"),
    "command factory rejects occupied slots");

await session.SendCommandAsync(new PlayCardCommand(redHand.Cards[0].CardInstanceId, 1, "red-play"));
await AppendNextEventsAsync(eventReader, events, eventOrder, 3, "red move events");
var afterRed = session.CurrentSnapshot ?? throw new InvalidOperationException("Opponent move did not cache a snapshot.");

Assert(afterRed.Board[1].Card?.Owner == Seat.Red, "opponent card is placed");
Assert(afterRed.Board[0].Card?.Owner == Seat.Red, "simple adjacent rank capture flips ownership");
Assert(afterRed.RedScore == 6 && afterRed.BlueScore == 4, "capture updates score totals");
var redPlayed = events.OfType<CardPlayedEvent>().Last();
Assert(redPlayed.SourceSeat == Seat.Red, "opponent CardPlayed includes source seat");
Assert(redPlayed.SourceHandIndex == 0, "opponent CardPlayed includes source hand index");
var captured = events.OfType<CardCapturedEvent>().Single();
Assert(captured.BoardSlotIndex == 0, "capture raises CardCaptured for the flipped board card");
Assert(captured.PreviousOwner == Seat.Blue, "capture event includes previous owner");
Assert(captured.NewOwner == Seat.Red, "capture event includes new owner");
Assert(captured.Card.Owner == Seat.Red, "capture event includes post-capture card snapshot");
Assert(ReferenceEquals(captured.Snapshot, afterRed), "capture event carries the final move snapshot");
AssertSequence(eventOrder, ["CardPlayedEvent", "CardCapturedEvent", "TurnChangedEvent"], "capture move emits play/capture/turn");

AssertStrictlyIncreasing(events.Select(update => update.Sequence), "all session events are strictly ordered");

var redSeatSession = new LocalGameSession(catalog, Seat.Red);
using var redSeatAiLifetime = new CancellationTokenSource();
var redSeatAiTask = new AiSeatController(Seat.Blue).RunAsync(redSeatSession, redSeatAiLifetime.Token);
var redSeatEvents = new List<GameEvent>();
var redSeatEventOrder = new List<string>();
await using var redSeatEventReader = redSeatSession.SubscribeEventsAsync().GetAsyncEnumerator();

var redSeatInitial = await redSeatSession.StartAsync();
await AppendNextEventsAsync(redSeatEventReader, redSeatEvents, redSeatEventOrder, 3, "red-seat opening move events");
var redSeatOpening = redSeatSession.CurrentSnapshot ?? throw new InvalidOperationException("AI opener did not cache a snapshot.");
Assert(redSeatSession.ConnectionState == SessionConnectionState.Connected, "red-seat session connects before auto-playing the AI opener");
Assert(redSeatInitial.ActiveSeat == Seat.Blue, "red-seat initial snapshot starts with Blue");
Assert(redSeatOpening.LocalSeat == Seat.Red, "red-seat opening snapshot keeps Red as local seat");
Assert(redSeatOpening.ActiveSeat == Seat.Red, "AI opener advances the first playable turn to local Red");
Assert(redSeatOpening.Board.Count(cell => cell.Card?.Owner == Seat.Blue) == 1, "AI opener places one Blue card");
Assert(redSeatOpening.Board.Count(cell => cell.CanDrop) == 8, "remaining empty cells accept the local Red move");
Assert(redSeatOpening.Hands.Single(hand => hand.Seat == Seat.Red).Cards.All(card => card.IsPlayable), "local Red hand is playable after the AI opener");
AssertSequence(
    redSeatEventOrder,
    ["MatchStartedEvent", "CardPlayedEvent", "TurnChangedEvent"],
    "red-seat startup emits match start before the AI opening move");
await StopControllerAsync(redSeatAiLifetime, redSeatAiTask);

var aiOpponentSession = new LocalGameSession(catalog);
using var aiOpponentLifetime = new CancellationTokenSource();
var aiOpponentTask = new AiSeatController(Seat.Red).RunAsync(aiOpponentSession, aiOpponentLifetime.Token);
var aiOpponentEvents = new List<GameEvent>();
var aiOpponentEventOrder = new List<string>();
await using var aiOpponentEventReader = aiOpponentSession.SubscribeEventsAsync().GetAsyncEnumerator();

var aiOpponentInitial = await aiOpponentSession.StartAsync();
await AppendNextEventsAsync(aiOpponentEventReader, aiOpponentEvents, aiOpponentEventOrder, 1, "AI-opponent start events");
aiOpponentEventOrder.Clear();

var aiOpponentBlueHand = aiOpponentInitial.Hands.Single(hand => hand.Seat == Seat.Blue);
var playBeforeAi = GameCommandFactory.CreatePlayCardCommand(aiOpponentBlueHand.Cards[0], aiOpponentInitial.Board[8], "blue-before-ai");
await aiOpponentSession.SendCommandAsync(playBeforeAi);
await AppendNextEventsAsync(aiOpponentEventReader, aiOpponentEvents, aiOpponentEventOrder, 4, "AI-opponent response events");
var afterAiOpponentResponse = aiOpponentSession.CurrentSnapshot ?? throw new InvalidOperationException("AI response did not cache a snapshot.");

Assert(afterAiOpponentResponse.ActiveSeat == Seat.Blue, "AI response advances the turn back to local Blue");
Assert(afterAiOpponentResponse.Board.Count(cell => cell.Card?.Owner == Seat.Red) == 1, "AI response places one Red card");
Assert(afterAiOpponentResponse.Board[8].Card?.Owner == Seat.Blue, "human card remains on the selected slot");
AssertSequence(
    aiOpponentEventOrder,
    ["CardPlayedEvent", "TurnChangedEvent", "CardPlayedEvent", "TurnChangedEvent"],
    "AI responds after receiving the turn-change event snapshot");
await StopControllerAsync(aiOpponentLifetime, aiOpponentTask);

var selectedHands = new Dictionary<Seat, IReadOnlyList<int>>
{
    [Seat.Blue] = new[] { 1, 2, 3, 4, 5 },
    [Seat.Red] = new[] { 6, 7, 8, 9, 10 },
};
var selectedSession = new LocalGameSession(catalog, selectedCardNumbers: selectedHands);
var selectedInitial = await selectedSession.StartAsync();
AssertSequence(
    selectedInitial.Hands.Single(hand => hand.Seat == Seat.Blue).Cards.Select(card => card.CardNumber).ToArray(),
    [1, 2, 3, 4, 5],
    "selected Blue hand appears in the initial snapshot in order");
AssertSequence(
    selectedInitial.Hands.Single(hand => hand.Seat == Seat.Red).Cards.Select(card => card.CardNumber).ToArray(),
    [6, 7, 8, 9, 10],
    "selected Red hand appears in the initial snapshot in order");

var selectionValidationLobby = new LocalLobbySession(LocalLobbyMode.Solo, "Validator");
await selectionValidationLobby.StartAsync();
await AssertThrowsAsync<ArgumentException>(
    async () => await selectionValidationLobby.SetSelectedCardsAsync([1, 2, 3, 4]),
    "selected card validation rejects fewer than five cards");
await AssertThrowsAsync<ArgumentException>(
    async () => await selectionValidationLobby.SetSelectedCardsAsync([1, 1, 2, 3, 4]),
    "selected card validation rejects duplicate cards");
await AssertThrowsAsync<ArgumentOutOfRangeException>(
    async () => await selectionValidationLobby.SetSelectedCardsAsync([1, 2, 3, 4, 111]),
    "selected card validation rejects unknown cards");

await AssertInMemoryTransportAsync();
await AssertLobbyFlowAsync();
await AssertLocalLobbyFlowAsync();

Console.WriteLine("TripleTriad.Tests passed.");

static string FindRepoRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "triple-triad.slnx")))
        directory = directory.Parent;

    return directory?.FullName
        ?? throw new InvalidOperationException("Could not locate repository root.");
}

static string DescribeEvent(GameEvent gameEvent) => gameEvent.GetType().Name;

static async ValueTask AppendNextEventsAsync(
    IAsyncEnumerator<GameEvent> reader,
    List<GameEvent> events,
    List<string> eventOrder,
    int count,
    string message)
{
    for (var index = 0; index < count; index++)
    {
        var moveNextTask = reader.MoveNextAsync().AsTask();
        var completed = await Task.WhenAny(moveNextTask, Task.Delay(TimeSpan.FromSeconds(5)));
        if (completed != moveNextTask)
            throw new InvalidOperationException($"Assertion failed: timed out waiting for {message}.");

        if (!await moveNextTask)
            throw new InvalidOperationException($"Assertion failed: event stream ended while waiting for {message}.");

        AssertValidGameEvent(reader.Current);
        events.Add(reader.Current);
        eventOrder.Add(DescribeEvent(reader.Current));
    }
}

static void AssertValidGameEvent(GameEvent gameEvent)
{
    Assert(gameEvent.Sequence > 0, $"{gameEvent.GetType().Name} has a positive sequence");
    Assert(gameEvent.Snapshot is not null, $"{gameEvent.GetType().Name} carries a snapshot");
    Assert(!string.IsNullOrWhiteSpace(gameEvent.Type), $"{gameEvent.GetType().Name} carries a type tag");
}

static async ValueTask StopControllerAsync(
    CancellationTokenSource lifetime,
    Task controllerTask)
{
    lifetime.Cancel();

    try
    {
        await controllerTask;
    }
    catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
}

static async ValueTask AssertInMemoryTransportAsync()
{
    var (first, second) = InMemoryMatchTransport.CreatePair();
    var firstReady = NetworkMessage.Create(new LobbyReadyChangeRequested(true));
    var secondReady = NetworkMessage.Create(new LobbyReadyChangeRequested(false));
    Assert(firstReady.Type == NetworkMessageTypes.LobbyReadyChangeRequested, "ready request uses the ready-request network tag");
    Assert(secondReady.Type == NetworkMessageTypes.LobbyReadyChangeRequested, "second ready request uses the ready-request network tag");

    await first.SendAsync(firstReady);
    await first.SendAsync(secondReady);

    await using var secondReader = second.ReadMessagesAsync().GetAsyncEnumerator();
    Assert(
        ReferenceEquals(firstReady, await ReadNextNetworkMessageAsync(secondReader, "first transport message")),
        "paired transports deliver the first typed message in order");
    Assert(
        ReferenceEquals(secondReady, await ReadNextNetworkMessageAsync(secondReader, "second transport message")),
        "paired transports deliver the second typed message in order");

    await second.SendAsync(NetworkMessage.Create(new LobbyJoinRequested("Guest")));

    await using var firstReader = first.ReadMessagesAsync().GetAsyncEnumerator();
    Assert(
        await ReadNextNetworkMessageAsync(firstReader, "return transport message") is
        {
            Type: NetworkMessageTypes.LobbyJoinRequested,
            Payload: LobbyJoinRequested { PlayerName: "Guest" },
        },
        "paired transports deliver tagged payloads in both directions");

    var (cancelledTransport, _) = InMemoryMatchTransport.CreatePair();
    using var cancellation = new CancellationTokenSource();
    await using var cancelledReader = cancelledTransport.ReadMessagesAsync(cancellation.Token).GetAsyncEnumerator();
    cancellation.Cancel();

    await AssertThrowsAsync<OperationCanceledException>(
        async () => await cancelledReader.MoveNextAsync(),
        "transport reads observe cancellation");
}

static async ValueTask AssertLobbyFlowAsync()
{
    var (hostTransport, clientTransport) = InMemoryMatchTransport.CreatePair();
    var hostLobby = new HostLobbySession(hostTransport, "Host");
    var clientLobby = new ClientLobbySession(clientTransport, "Guest");

    await using var hostReader = hostLobby.ReadUpdatesAsync().GetAsyncEnumerator();
    await using var clientReader = clientLobby.ReadUpdatesAsync().GetAsyncEnumerator();

    var hostInitial = await hostLobby.StartAsync();
    Assert(hostInitial.LocalSeat == Seat.Blue, "host lobby starts as Blue");

    await clientLobby.StartAsync();

    var hostJoined = await ReadLobbySnapshotAsync(
        hostReader,
        snapshot => HasPlayer(snapshot, Seat.Blue) && HasPlayer(snapshot, Seat.Red),
        "host joined lobby snapshot");
    var clientJoined = await ReadLobbySnapshotAsync(
        clientReader,
        snapshot => HasPlayer(snapshot, Seat.Blue) && HasPlayer(snapshot, Seat.Red),
        "client joined lobby snapshot");

    Assert(hostJoined.LocalSeat == Seat.Blue, "host snapshots keep Blue as local seat");
    Assert(clientJoined.LocalSeat == Seat.Red, "client snapshots keep Red as local seat");
    Assert(GetPlayer(hostJoined, Seat.Blue).PlayerName == "Host", "joined lobby includes the host player");
    Assert(GetPlayer(hostJoined, Seat.Red).PlayerName == "Guest", "joined lobby includes the guest player");

    var openRules = GameRules.Open;
    await hostLobby.SetRulesAsync(openRules);

    var hostRules = await ReadLobbySnapshotAsync(
        hostReader,
        snapshot => RulesEqual(snapshot.Rules, GameRules.Open),
        "host rules snapshot");
    var clientRules = await ReadLobbySnapshotAsync(
        clientReader,
        snapshot => RulesEqual(snapshot.Rules, GameRules.Open),
        "client rules snapshot");

    AssertRules(hostRules.Rules, GameRules.Open, "host rule changes are visible locally");
    AssertRules(clientRules.Rules, GameRules.Open, "host rule changes are visible to the client");

    var hostSelection = new[] { 1, 2, 3, 4, 5 };
    var clientSelection = new[] { 6, 7, 8, 9, 10 };
    await hostLobby.SetSelectedCardsAsync(hostSelection);

    var hostSelected = await ReadLobbySnapshotAsync(
        hostReader,
        snapshot => SelectionEquals(snapshot, Seat.Blue, hostSelection),
        "host selected-card snapshot");
    var clientHiddenHostSelection = await ReadLobbySnapshotAsync(
        clientReader,
        snapshot => !HasSelection(snapshot, Seat.Blue) && !HasSelection(snapshot, Seat.Red),
        "client snapshot without host selected cards");

    AssertSelection(hostSelected, Seat.Blue, hostSelection, "host sees only the host selected cards");
    AssertNoSelection(clientHiddenHostSelection, Seat.Blue, "client does not see host selected cards");

    await clientLobby.SetSelectedCardsAsync(clientSelection);

    var hostHiddenClientSelection = await ReadLobbySnapshotAsync(
        hostReader,
        snapshot => SelectionEquals(snapshot, Seat.Blue, hostSelection) && !HasSelection(snapshot, Seat.Red),
        "host snapshot without client selected cards");
    var clientSelected = await ReadLobbySnapshotAsync(
        clientReader,
        snapshot => SelectionEquals(snapshot, Seat.Red, clientSelection) && !HasSelection(snapshot, Seat.Blue),
        "client selected-card snapshot");

    AssertNoSelection(hostHiddenClientSelection, Seat.Red, "host does not see client selected cards");
    AssertSelection(clientSelected, Seat.Red, clientSelection, "client sees only the client selected cards");

    await hostLobby.SetReadyAsync(true);

    await ReadLobbySnapshotAsync(
        hostReader,
        snapshot => IsReady(snapshot, Seat.Blue) && !IsReady(snapshot, Seat.Red),
        "host ready snapshot");
    await ReadLobbySnapshotAsync(
        clientReader,
        snapshot => IsReady(snapshot, Seat.Blue) && !IsReady(snapshot, Seat.Red),
        "client sees host ready snapshot");

    await clientLobby.SetReadyAsync(true);

    var hostBothReady = await ReadLobbySnapshotAsync(
        hostReader,
        BothPlayersReady,
        "host both-ready snapshot");
    var clientBothReady = await ReadLobbySnapshotAsync(
        clientReader,
        BothPlayersReady,
        "client both-ready snapshot");

    Assert(hostBothReady.CanStart, "host can start once both players are ready");
    Assert(clientBothReady.CanStart, "client can start once both players are ready");

    var hostStarted = await ReadLobbyMatchStartedAsync(hostReader, "host match start update");
    var clientStarted = await ReadLobbyMatchStartedAsync(clientReader, "client match start update");
    var hostSetup = await hostLobby.WaitForMatchStartAsync();
    var clientSetup = await clientLobby.WaitForMatchStartAsync();

    AssertMatchSetupsEqual(hostStarted.Setup, hostSetup, "host match-start update matches WaitForMatchStartAsync");
    AssertMatchSetupsEqual(clientStarted.Setup, clientSetup, "client match-start update matches WaitForMatchStartAsync");
    AssertMatchSetupsEqual(hostSetup, clientSetup, "host and client receive the same match setup");
    AssertRules(hostSetup.Rules, GameRules.Open, "match setup keeps the selected rules");
    AssertSetupSelection(hostSetup, Seat.Blue, hostSelection, "match setup includes host selected cards");
    AssertSetupSelection(hostSetup, Seat.Red, clientSelection, "match setup includes client selected cards");

    await clientTransport.SendAsync(NetworkMessage.Create(new PlayCardCommand("future-card", 0, "future-handoff")));

    await using var handoffReader = hostTransport.ReadMessagesAsync().GetAsyncEnumerator();
    Assert(
        await ReadNextNetworkMessageAsync(handoffReader, "post-lobby handoff message") is
        {
            Type: NetworkMessageTypes.GameCommand,
            Payload: PlayCardCommand { ClientRequestId: "future-handoff" },
        },
        "lobby stops reading after match start and leaves the transport usable");
}

static async ValueTask AssertLocalLobbyFlowAsync()
{
    var soloLobby = new LocalLobbySession(LocalLobbyMode.Solo, "Player");
    var soloInitial = await soloLobby.StartAsync();

    Assert(soloInitial.LocalSeat == Seat.Blue, "solo lobby starts the player as Blue");
    Assert(soloInitial.CanStart, "solo lobby can start with an AI opponent");
    Assert(GetPlayer(soloInitial, Seat.Blue).Kind == LobbyPlayerKind.Human, "solo lobby starts with a human player");
    Assert(GetPlayer(soloInitial, Seat.Red).Kind == LobbyPlayerKind.AI, "solo lobby fills the opponent seat with AI");

    var soloSelection = new[] { 11, 12, 13, 14, 15 };
    await soloLobby.SetSelectedCardsAsync(soloSelection);
    AssertSelection(soloLobby.CurrentSnapshot, Seat.Blue, soloSelection, "solo lobby stores the local selected cards");
    AssertNoSelection(soloLobby.CurrentSnapshot, Seat.Red, "solo lobby does not expose an opponent selection");

    await soloLobby.TakeSeatAsync(Seat.Red);
    var soloSwapped = soloLobby.CurrentSnapshot;
    Assert(soloSwapped.LocalSeat == Seat.Red, "solo lobby lets the player take the AI seat");
    Assert(GetPlayer(soloSwapped, Seat.Red).Kind == LobbyPlayerKind.Human, "solo seat switch moves the player");
    Assert(GetPlayer(soloSwapped, Seat.Blue).Kind == LobbyPlayerKind.AI, "solo seat switch moves AI to the opposite seat");
    AssertSelection(soloSwapped, Seat.Red, soloSelection, "solo selected cards follow the player seat switch");
    AssertNoSelection(soloSwapped, Seat.Blue, "solo seat switch does not expose selected cards to the AI seat");

    var selectedRules = GameRules.Open | GameRules.Same;
    await soloLobby.SetRulesAsync(selectedRules);
    AssertRules(soloLobby.CurrentSnapshot.Rules, selectedRules, "solo lobby stores selected rules");

    await soloLobby.SetReadyAsync(true);
    var soloSetup = await soloLobby.WaitForMatchStartAsync();
    AssertRules(soloSetup.Rules, selectedRules, "solo match setup keeps selected rules");
    Assert(GetPlayer(new LobbySnapshot(Seat.Red, soloSetup.Rules, soloSetup.Players, true, true), Seat.Blue).Kind == LobbyPlayerKind.AI, "solo match setup includes AI");
    AssertSetupSelection(soloSetup, Seat.Red, soloSelection, "solo match setup includes selected cards for the local human");

    var randomLobby = new LocalLobbySession(LocalLobbyMode.Solo, "Randomized");
    await randomLobby.StartAsync();
    await randomLobby.SetSelectedCardsAsync([1, 2, 3, 4, 5]);
    await randomLobby.SetRulesAsync(GameRules.Random);
    await randomLobby.SetReadyAsync(true);
    var randomSetup = await randomLobby.WaitForMatchStartAsync();
    Assert(randomSetup.CardSelections.Count == 0, "random rule ignores stored selected cards when creating match setup");

    var hostLobby = new LocalLobbySession(LocalLobbyMode.Host, "Host");
    var hostInitial = await hostLobby.StartAsync();
    Assert(hostInitial.LocalSeat == Seat.Blue, "host lobby starts the player as Blue");
    Assert(HasPlayer(hostInitial, Seat.Blue), "host lobby starts with one human player");
    Assert(!HasPlayer(hostInitial, Seat.Red), "host lobby leaves the joining seat empty");
    Assert(!hostInitial.CanStart, "host lobby cannot start without a joining player");

    await hostLobby.TakeSeatAsync(Seat.Red);
    var hostSwapped = hostLobby.CurrentSnapshot;
    Assert(hostSwapped.LocalSeat == Seat.Red, "host lobby lets the player take a free seat");
    Assert(!HasPlayer(hostSwapped, Seat.Blue), "host lobby leaves the previous seat empty after switching");
    Assert(!hostSwapped.CanStart, "host lobby still cannot start with only one player");

    await hostLobby.SetReadyAsync(true);
    Assert(!hostLobby.CurrentSnapshot.CanStart, "host lobby ready click does not start without a joining player");
}

static async ValueTask<NetworkMessage> ReadNextNetworkMessageAsync(
    IAsyncEnumerator<NetworkMessage> reader,
    string message)
{
    var moveNextTask = reader.MoveNextAsync().AsTask();
    var completed = await Task.WhenAny(moveNextTask, Task.Delay(TimeSpan.FromSeconds(5)));
    if (completed != moveNextTask)
        throw new InvalidOperationException($"Assertion failed: timed out waiting for {message}.");

    if (!await moveNextTask)
        throw new InvalidOperationException($"Assertion failed: message stream ended while waiting for {message}.");

    return reader.Current;
}

static async ValueTask<LobbySnapshot> ReadLobbySnapshotAsync(
    IAsyncEnumerator<LobbyUpdate> reader,
    Func<LobbySnapshot, bool> predicate,
    string message)
{
    for (var index = 0; index < 20; index++)
    {
        var update = await ReadNextLobbyUpdateAsync(reader, message);
        if (update is LobbySnapshotUpdate snapshotUpdate && predicate(snapshotUpdate.Snapshot))
            return snapshotUpdate.Snapshot;
    }

    throw new InvalidOperationException($"Assertion failed: did not find {message}.");
}

static async ValueTask<LobbyMatchStartedUpdate> ReadLobbyMatchStartedAsync(
    IAsyncEnumerator<LobbyUpdate> reader,
    string message)
{
    for (var index = 0; index < 20; index++)
    {
        var update = await ReadNextLobbyUpdateAsync(reader, message);
        if (update is LobbyMatchStartedUpdate matchStarted)
            return matchStarted;
    }

    throw new InvalidOperationException($"Assertion failed: did not find {message}.");
}

static async ValueTask<LobbyUpdate> ReadNextLobbyUpdateAsync(
    IAsyncEnumerator<LobbyUpdate> reader,
    string message)
{
    var moveNextTask = reader.MoveNextAsync().AsTask();
    var completed = await Task.WhenAny(moveNextTask, Task.Delay(TimeSpan.FromSeconds(5)));
    if (completed != moveNextTask)
        throw new InvalidOperationException($"Assertion failed: timed out waiting for {message}.");

    if (!await moveNextTask)
        throw new InvalidOperationException($"Assertion failed: lobby update stream ended while waiting for {message}.");

    return reader.Current;
}

static LobbyPlayerSnapshot GetPlayer(LobbySnapshot snapshot, Seat seat) =>
    snapshot.Players.Single(player => player.Seat == seat);

static bool HasPlayer(LobbySnapshot snapshot, Seat seat) =>
    snapshot.Players.Any(player => player.Seat == seat);

static bool IsReady(LobbySnapshot snapshot, Seat seat) =>
    HasPlayer(snapshot, seat) && GetPlayer(snapshot, seat).IsReady;

static bool BothPlayersReady(LobbySnapshot snapshot) =>
    IsReady(snapshot, Seat.Blue) && IsReady(snapshot, Seat.Red);

static bool HasSelection(LobbySnapshot snapshot, Seat seat) =>
    snapshot.CardSelections.Any(selection => selection.Seat == seat);

static bool SelectionEquals(
    LobbySnapshot snapshot,
    Seat seat,
    IReadOnlyList<int> expectedCardNumbers) =>
    snapshot.CardSelections.Any(selection =>
        selection.Seat == seat
        && selection.CardNumbers.SequenceEqual(expectedCardNumbers));

static void AssertSelection(
    LobbySnapshot snapshot,
    Seat seat,
    IReadOnlyList<int> expectedCardNumbers,
    string message)
{
    var selection = snapshot.CardSelections.SingleOrDefault(selection => selection.Seat == seat);
    if (selection is null)
        throw new InvalidOperationException($"Assertion failed: {message}");

    AssertSequence(selection.CardNumbers, expectedCardNumbers, message);
}

static void AssertSetupSelection(
    MatchSetup setup,
    Seat seat,
    IReadOnlyList<int> expectedCardNumbers,
    string message)
{
    var selection = setup.CardSelections.SingleOrDefault(selection => selection.Seat == seat);
    if (selection is null)
        throw new InvalidOperationException($"Assertion failed: {message}");

    AssertSequence(selection.CardNumbers, expectedCardNumbers, message);
}

static void AssertNoSelection(LobbySnapshot snapshot, Seat seat, string message) =>
    Assert(!HasSelection(snapshot, seat), message);

static bool RulesEqual(GameRules actual, GameRules expected) =>
    actual == expected;

static void AssertRules(GameRules actual, GameRules expected, string message) =>
    Assert(RulesEqual(actual, expected), message);

static void AssertMatchSetupsEqual(MatchSetup actual, MatchSetup expected, string message)
{
    AssertRules(actual.Rules, expected.Rules, message);
    Assert(actual.Players.Count == expected.Players.Count, message);
    Assert(actual.CardSelections.Count == expected.CardSelections.Count, message);

    foreach (var expectedPlayer in expected.Players)
    {
        var actualPlayer = actual.Players.Single(player => player.Seat == expectedPlayer.Seat);
        Assert(actualPlayer == expectedPlayer, message);
    }

    foreach (var expectedSelection in expected.CardSelections)
        AssertSetupSelection(actual, expectedSelection.Seat, expectedSelection.CardNumbers, message);
}

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException($"Assertion failed: {message}");
}

static void AssertSequence<T>(IReadOnlyList<T> actual, IReadOnlyList<T> expected, string message)
{
    if (actual.Count != expected.Count || !actual.SequenceEqual(expected))
        throw new InvalidOperationException($"Assertion failed: {message}. Expected [{string.Join(", ", expected)}], got [{string.Join(", ", actual)}]");
}

static void AssertStrictlyIncreasing(IEnumerable<long> values, string message)
{
    long? previous = null;
    foreach (var value in values)
    {
        if (previous is not null && value <= previous.Value)
            throw new InvalidOperationException($"Assertion failed: {message}.");

        previous = value;
    }
}

static void AssertThrows<TException>(Action action, string message)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException($"Assertion failed: {message}");
}

static async ValueTask AssertThrowsAsync<TException>(Func<ValueTask> action, string message)
    where TException : Exception
{
    try
    {
        await action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException($"Assertion failed: {message}");
}
