using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.AutoClient;
using Microsoft.Extensions.Options;
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

var app = builder.Build();

await app.StartAsync();

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

var gameClient = new GameClient(connection);

await connection.StartAsync();

var serverState = await gameClient.GetServerStateAsync();

Console.ReadKey();
await connection.StopAsync();
await app.StopAsync();

file sealed class GameClient(HubConnection connection) : GameClientBase(connection)
{
    public override Task OnUserConnected(OnlineUser user)
    {
        Console.WriteLine($"{user.UserName} has connected");
        return Task.CompletedTask;
    }

    public override Task OnUserDisconnected(OnlineUser user)
    {
        Console.WriteLine($"{user.UserName} has disconnected");
        return Task.CompletedTask;
    }
}