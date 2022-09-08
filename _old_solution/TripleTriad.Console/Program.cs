// See https://aka.ms/new-console-template for more information
using TripleTriad.Models;
using TripleTriad.Services;

const int Port = 50051;
var shutdown = new ManualResetEvent(false);
var started = new ManualResetEvent(false);

static void OnPlayerJoined(object? s, Player e) => Console.WriteLine("{0} joined!", e.Name);
static void OnMessage1(object? s, Message e)
{
    var user = (ITripleTriadUser)s!;
    switch (e.ContentCase)
    {
        case Message.ContentOneofCase.None:
            return;
        case Message.ContentOneofCase.Ready:
            Console.WriteLine("{0} is ready.", e.Player.Name);
            user.SendMessageAsync(ready: true);
            return;
        case Message.ContentOneofCase.Text:
            Console.WriteLine("{0}: {1}", e.Player.Name, e.Text);
            user.SendMessageAsync($"Hi, {e.Player.Name}!");
            return;
        case Message.ContentOneofCase.Move:
            Console.WriteLine("New move from {0}.", e.Player.Name);
            return;
    }
}
static void OnMessage2(object? s, Message e)
{
    var user = (ITripleTriadUser)s!;
    switch (e.ContentCase)
    {
        case Message.ContentOneofCase.None:
            return;
        case Message.ContentOneofCase.Ready:
            Console.WriteLine("{0} is ready.", e.Player.Name);
            return;
        case Message.ContentOneofCase.Text:
            Console.WriteLine("{0}: {1}", e.Player.Name, e.Text);
            return;
        case Message.ContentOneofCase.Move:
            Console.WriteLine("New move from {0}.", e.Player.Name);
            return;
    }
}

Task.Run(async () =>
{
    var server = ITripleTriadServer.Create(new Player { Name = "Player 1" }, Port);
    server.PlayerConnected += OnPlayerJoined;
    server.MessageReceived += OnMessage1;
    await server.HostAsync(CancellationToken.None);
    started.Set();

    shutdown.WaitOne(); // <--- Waits for ever or signal received
    await server.DisposeAsync();

})
    .ConfigureAwait(false)
    .GetAwaiter();

started.WaitOne();

var client = ITripleTriadClient.Create(new Player { Name = "Player 2" }, "http://localhost:50051");
client.MessageReceived += OnMessage2;

var serverPlayer = await client.JoinAsync(CancellationToken.None);
Console.WriteLine("Connected to {0}.", serverPlayer.Name);

await client.SendMessageAsync("Olá!");
await client.SendMessageAsync(ready: true);

Console.WriteLine("Presss any key..");
Console.ReadKey();
shutdown.Set();