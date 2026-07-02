using TripleTriad.Bridge;
using TripleTriad.Data;
using TripleTriad.Mock;

var repoRoot = FindRepoRoot();
var catalogPath = Path.Combine(repoRoot, "src", "TripleTriad", "assets", "triple_triad", "cards.json");
var catalog = CardCatalog.Load(catalogPath);

Assert(catalog.Cards.Count == 110, "loads all 110 FFVIII card definitions");
Assert(catalog.Get(110).Name == "Squall", "loads card definitions by number");

var squallRegion = AtlasRegions.CardFace(110);
Assert(squallRegion == new TileRegion(2560, 2304, 256, 256), "maps card 110 to atlas row 9 column 10");
Assert(AtlasRegions.Value(Direction.North, 10) == new TileRegion(2560, 3072, 256, 256), "maps A-rank north value to column A");
Assert(AtlasRegions.Element(Element.Fire) == new TileRegion(256, 3840, 256, 256), "maps fire element to atlas row 15 column 1");

var session = new MockGameSession(catalog, autoPlayOpponent: false);
var events = new List<GameEvent>();
var snapshotChanges = 0;
session.EventRaised += events.Add;
session.SnapshotChanged += _ => snapshotChanges++;

var initial = session.CurrentSnapshot;
var blueHand = initial.Hands.Single(hand => hand.Seat == Seat.Blue);
var redHand = initial.Hands.Single(hand => hand.Seat == Seat.Red);

Assert(initial.BlueScore == 5 && initial.RedScore == 5, "initial score counts both full hands");
Assert(blueHand.Cards.All(card => card.IsPlayable), "local active hand is playable");
Assert(redHand.Cards.All(card => !card.IsFaceUp && !card.IsPlayable), "opponent hand visibility is state-driven and hidden by default");
Assert(initial.Board.Count(cell => cell.CanDrop) == 9, "all empty board cells accept the local first move");

await session.SubmitAsync(new PlayCardCommand(blueHand.Cards[1].CardInstanceId, 99, "invalid-slot"));
Assert(events.OfType<MoveRejectedEvent>().Any(), "invalid slot raises MoveRejected");
events.Clear();

var playBlue = GameCommandFactory.CreatePlayCardCommand(blueHand.Cards[1], initial.Board[0], "blue-play");
await session.SubmitAsync(playBlue);

var afterBlue = session.CurrentSnapshot;
Assert(snapshotChanges == 1, "accepted move publishes a snapshot");
Assert(afterBlue.Board[0].Card?.CardInstanceId == playBlue.CardInstanceId, "accepted move places the selected card");
Assert(afterBlue.ActiveSeat == Seat.Red, "accepted move advances the turn");
Assert(afterBlue.Hands.Single(hand => hand.Seat == Seat.Blue).Cards.All(card => !card.IsPlayable), "local cards are not playable during opponent turn");
Assert(events.OfType<CardPlayedEvent>().Single().BoardSlotIndex == 0, "accepted move raises CardPlayed");
events.Clear();

AssertThrows<InvalidOperationException>(
    () => GameCommandFactory.CreatePlayCardCommand(blueHand.Cards[2], afterBlue.Board[0], "occupied"),
    "command factory rejects occupied slots");

await session.SubmitAsync(new PlayCardCommand(redHand.Cards[0].CardInstanceId, 1, "red-play"));
var afterRed = session.CurrentSnapshot;

Assert(afterRed.Board[1].Card?.Owner == Seat.Red, "opponent card is placed");
Assert(afterRed.Board[0].Card?.Owner == Seat.Red, "simple adjacent rank capture flips ownership");
Assert(afterRed.RedScore == 6 && afterRed.BlueScore == 4, "capture updates score totals");
Assert(events.OfType<CardCapturedEvent>().Single().BoardSlotIndex == 0, "capture raises CardCaptured for the flipped board card");

Console.WriteLine("TripleTriad.Tests passed.");

static string FindRepoRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "triple-triad.slnx")))
        directory = directory.Parent;

    return directory?.FullName
        ?? throw new InvalidOperationException("Could not locate repository root.");
}

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException($"Assertion failed: {message}");
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
