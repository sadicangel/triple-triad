using Grpc.Net.Client;
using TripleTriad.Models;

namespace TripleTriad.Services;


public sealed class TripleTriadClient : TripleTriadService.TripleTriadServiceClient, ITripleTriadClient
{
    private readonly GrpcChannel _channel;
    private Subscription? _subscription;

    public Player Player { get; }

    public bool IsHosting { get => false; }

    public event EventHandler<Player>? PlayerConnected;

    public event EventHandler<Message>? MessageReceived;

    public TripleTriadClient(Player player, GrpcChannel channel) : base(channel)
    {
        _channel = channel;
        Player = player;
    }

    public async ValueTask DisposeAsync()
    {
        if (_subscription is not null)
            await UnsubscribeAsync(_subscription);
        await _channel.ShutdownAsync();
    }

    public async Task<Player> JoinAsync(CancellationToken cancellationToken)
    {
        var request = new HandshakeRequest { ClientPlayer = Player };
        var response = await HandshakeAsync(request, cancellationToken: cancellationToken);
        _subscription = response.Subscription;

        await SubscribeMessages(_subscription, cancellationToken);

        PlayerConnected?.Invoke(this, response.ServerPlayer);

        return response.ServerPlayer;
        
        Task SubscribeMessages(Subscription subscription, CancellationToken cancellationToken)
        {
            // Run this "forever".
            Task.Run(async () =>
            {
                var call = Subscribe(subscription, cancellationToken: cancellationToken);
                while (await call.ResponseStream.MoveNext(cancellationToken))
                    MessageReceived?.Invoke(this, call.ResponseStream.Current);
                Console.WriteLine();
            }, cancellationToken)
                .ConfigureAwait(false)
                .GetAwaiter();

            return Task.CompletedTask;
        }
    }

    public async Task<Board> GetBoardAsync()
    {
        var boardResponse = await ReceiveAsync(new MessageSelector { Type = (int)Message.ContentOneofCase.Board });
        return boardResponse.Board;
    }

    public async Task SendMessageAsync(Message message)
    {
        await SendAsync(message);
    }
}