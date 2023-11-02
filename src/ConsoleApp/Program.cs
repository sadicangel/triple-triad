using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.AutoClient;
using Microsoft.Extensions.Options;
using Spectre.Console;
using System.Text.Json;
using TripleTriad;
using TripleTriad.Infrastructure;

const string email = "user@tt.com";
const string password = "Pass@123";

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDomain();
builder.Services.AddApplication();

builder.Services.AddHttpClient(nameof(ITripleTriadClient), (provider, opts) =>
{
    var serverOptions = provider.GetRequiredService<IOptions<ServerOptions>>().Value;
    opts.BaseAddress = new(serverOptions.Url);
});

builder.Services.AddTripleTriadClient(opts => opts.JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web));
builder.Services.AddSingleton(AnsiConsole.Console);

var app = builder.Build();

await app.StartAsync();

var console = app.Services.GetRequiredService<IAnsiConsole>();

var response = default(AccessTokenResponse);
var expireTime = default(DateTimeOffset);

var client = app.Services.GetRequiredService<ITripleTriadClient>();

try
{
    response = await client.Login(new()
    {
        Email = email,
        Password = password
    });
    expireTime = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn);
}
catch (AutoClientException ex)
{
    if (ex.HttpError is { StatusCode: 401 })
    {
        await client.Register(new()
        {
            Email = email,
            Password = password
        });
    }
}


Func<Task<string?>> getAccessToken = async () =>
{
    client = app.Services.GetRequiredService<ITripleTriadClient>();

    var now = DateTimeOffset.UtcNow;

    if (response is null || now > expireTime)
    {
        response = await client.Login(new()
        {
            Email = email,
            Password = password
        });
        expireTime = now.AddSeconds(response.ExpiresIn);
    }

    return response.AccessToken;
};

await using var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7227/triple-triad", opts => opts.AccessTokenProvider = getAccessToken)
    .Build();

await using var gameClient = new GameClient(connection);

await gameClient.StartAsync();

var users = await gameClient.GetUsersAsync();
var lobbies = await gameClient.GetLobbiesAsync();

gameClient.Print(console);

await gameClient.CreateLobbyAsync("test-lobby");

gameClient.Print(console);

console.Input.ReadKey(false);

await app.StopAsync();

file sealed class GameClient(HubConnection connection) : GameClientBase(connection)
{
    public List<OnlineUser> Users { get; } = [];

    public List<Lobby> Lobbies { get; } = [];

    public override async Task<IReadOnlyCollection<OnlineUser>> GetUsersAsync()
    {
        var users = await base.GetUsersAsync();
        Users.AddRange(users);
        return users;
    }

    public override async Task<IReadOnlyCollection<Lobby>> GetLobbiesAsync()
    {
        var lobbies = await base.GetLobbiesAsync();
        Lobbies.AddRange(lobbies);
        return lobbies;
    }

    public override Task OnUserConnected(OnlineUser user)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }

    public override Task OnUserDisconnected(OnlineUser user)
    {
        Users.Remove(user);
        return Task.CompletedTask;
    }

    public override Task OnLobbyCreated(Lobby lobby)
    {
        Lobbies.Add(lobby);
        return Task.CompletedTask;
    }

    public override Task OnLobbyRemoved(Lobby lobby)
    {
        Lobbies.Remove(lobby);
        return Task.CompletedTask;
    }

    public void Print(IAnsiConsole console)
    {
        var usersTable = new Table()
            .Title("Users")
            .AddColumns("UserId", "UserName");
        foreach (var user in Users)
            usersTable.AddRow(user.UserId, user.UserName);

        var lobbiesTable = new Table()
            .Title("Lobbies")
            .AddColumns("LobbyId", "LobbyName");
        foreach (var lobby in Lobbies)
            lobbiesTable.AddRow(lobby.LobbyId, lobby.DisplayName);

        console.Write(usersTable);
        console.Write(lobbiesTable);
    }
}