using CommunityToolkit.Mvvm.Messaging;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TripleTriad.CLI;
using TripleTriad.Games;

var hostname = "localhost";
var containerNames = new Dictionary<string, string>()
{
    ["/tripletriad_server"] = "TripleTriadServer"
};
var dockerClient = new DockerClientConfiguration().CreateClient();
var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true });
var connectionStrings = new Dictionary<string, string?>();
foreach (var container in containers)
{
    if (container.Names.SingleOrDefault(containerNames.ContainsKey) is string containerName)
    {
        var port = container.Ports.Single(p => p.PrivatePort == 443); // HTTPS
        connectionStrings[$"ConnectionStrings:{containerNames[containerName]}"] = $"https://{hostname}:{port.PublicPort}";
    }
}
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((host, configuration) =>
    {
        configuration.AddInMemoryCollection(connectionStrings);
    })
    .ConfigureServices((host, services) =>
    {
        services.AddHttpClient("Server", client => client.BaseAddress = new Uri(host.Configuration.GetConnectionString("TripleTriadServer")!));
    })
    .Build();

await host.StartAsync();

static void WaitForInput()
{
    Console.WriteLine("Presse key to continue...");
    Console.ReadLine();
}

var messenger = WeakReferenceMessenger.Default;
var http = host.Services.GetRequiredService<IHttpClientFactory>().CreateClient("Server");
var hubUrl = $"{host.Services.GetRequiredService<IConfiguration>().GetConnectionString("TripleTriadServer")}/tripletriad";
var bot1 = await TripleTriadBot.CreateAndConnectAsync("tiago", "Pass@123!", ConsoleColor.Green, messenger, http, hubUrl);
var bot2 = await TripleTriadBot.CreateAndConnectAsync("sadic", "Pass@123!", ConsoleColor.Blue, messenger, http, hubUrl);

WaitForInput();
await bot1.HostLobby("lobby1");
WaitForInput();
await bot2.JoinLobby(bot1.Lobby!.Id);
WaitForInput();
await bot1.UpdateLobby(isReady: true, displayName: "Lobby 1", rules: new(bot1.Lobby!.Rules) { BoardRules = BoardRules.None });
WaitForInput();
await bot2.UpdateLobby(isReady: true);
WaitForInput();
await bot1.LeaveLobby();
WaitForInput();
await bot2.UpdateLobby(isReady: true, displayName: "Lobby 2", rules: new(bot2.Lobby!.Rules) { BoardRules = BoardRules.Elemental });
WaitForInput();
await bot2.LeaveLobby();
//var game = new Game(messenger);

//var leftBoard = new ClientBoard(messenger, Side.Left);
//var rightBoard = new ClientBoard(messenger, Side.Right);

//var leftPlayer = new Player
//{
//    UserId = Guid.NewGuid(),
//    DisplayName = "Left Player",
//    Side = Side.Left,
//    Color = 0
//};

//var leftHand = await cardRepository.GetRandomAsync(count: 5);

//var rightPlayer = new Player
//{
//    UserId = Guid.NewGuid(),
//    DisplayName = "Right Player",
//    Side = Side.Right,
//    Color = 0
//};

//var rightHand = await cardRepository.GetRandomAsync(count: 5);
//Trace.Listeners.Add(new ConsoleTraceListener());
//game.StartGame(Ruleset.Default, leftPlayer, leftHand, rightPlayer, rightHand);

//var i = 0;
//var cells = game.Board.Cells;
//while (!leftBoard.IsGameOver)
//{
//    Console.Clear();
//    game.PlayCard(new CardMove { Player = game.Board.ActivePlayer, CellIndex = i++, HandIndex = 0 });
//    WriteRow(cells[0], cells[1], cells[2]);
//    WriteRow(cells[3], cells[4], cells[5]);
//    WriteRow(cells[6], cells[7], cells[8]);
//}

//static void WriteRow(Cell cell1, Cell cell2, Cell cell3)
//{
//    WriteTopOrBot(cell1, cell2, cell3);
//    WriteMiddle(cell1, cell2, cell3);
//    WriteTopOrBot(cell1, cell2, cell3);
//}
//static void WriteTopOrBot(Cell cell1, Cell cell2, Cell cell3) => Console.WriteLine($"  {cell1.GetValueOrSpace(Direction.Up)}  |  {cell2.GetValueOrSpace(Direction.Up)}  |  {cell3.GetValueOrSpace(Direction.Up)} ");
//static void WriteMiddle(Cell cell1, Cell cell2, Cell cell3) => Console.WriteLine($"{cell1.GetValueOrSpace(Direction.Left)}   {cell1.GetValueOrSpace(Direction.Right)}|{cell2.GetValueOrSpace(Direction.Left)}   {cell2.GetValueOrSpace(Direction.Right)}|{cell3.GetValueOrSpace(Direction.Left)}   {cell3.GetValueOrSpace(Direction.Right)}");

await host.StopAsync();