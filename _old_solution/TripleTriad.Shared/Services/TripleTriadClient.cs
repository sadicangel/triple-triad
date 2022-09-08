using CommunityToolkit.Mvvm.Messaging;
using Grpc.Net.Client;
using TripleTriad.Models;

namespace TripleTriad.Services;


public sealed class TripleTriadClient : TripleTriadService.TripleTriadServiceClient, ITripleTriadClient
{
    private readonly IMessenger _messenger;
    private readonly GrpcChannel _channel;
    private Subscription? _subscription;

    public Player Player { get; }

    public bool IsHosting { get => false; }

    public TripleTriadClient(IMessenger messenger, Player player, GrpcChannel channel) : base(channel)
    {
        _messenger = messenger;
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

        _messenger.Send(response.ServerPlayer, nameof(ITripleTriadClient));

        return response.ServerPlayer;
        
        Task SubscribeMessages(Subscription subscription, CancellationToken cancellationToken)
        {
            // Run this "forever".
            Task.Run(async () =>
            {
                var call = Subscribe(subscription, cancellationToken: cancellationToken);
                while (await call.ResponseStream.MoveNext(cancellationToken))
                    _messenger.Send(call.ResponseStream.Current, nameof(ITripleTriadClient));
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