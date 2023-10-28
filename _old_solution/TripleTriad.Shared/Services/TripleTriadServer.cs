using CommunityToolkit.Mvvm.Messaging;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using TripleTriad.Models;

namespace TripleTriad.Services;

public sealed class TripleTriadServer : TripleTriadService.TripleTriadServiceBase, ITripleTriadServer
{
    private readonly IMessenger _messenger;
    private readonly Server _server;
    private Board? _board;
    private int _subscriptionCounter = 0;
    private readonly ConcurrentDictionary<string, IServerStreamWriter<Message>> _subscriptions = new();
    private readonly BufferBlock<Message> _messageBuffer = new(new DataflowBlockOptions { EnsureOrdered = true });

    public Player Player { get; }

    public bool IsHosting { get => true; }

    public TripleTriadServer(IMessenger messenger, Player player, int port)
    {
        _messenger = messenger;
        _server = new Server
        {
            Services = { TripleTriadService.BindService(this) },
            Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
        };
        Player = player;
    }

    public async ValueTask DisposeAsync() => await _server.ShutdownAsync();

    public Task<Board> GetBoardAsync()
    {
        if(_board is null)
        {
            _board = new Board
            {
                LeftPlayer = Player,
                Ruleset = new Ruleset
                {
                    MatchRules = MatchRules.Open,
                    BoardRules = BoardRules.None,
                    TradeRules = TradeRules.One
                },
                IsLeftActive = true
            };
            _board.Cells.AddRange(Enumerable.Range(0, 9).Select(i => new Cell { Index = i }));
        }
        return Task.FromResult(_board);
    }

    // Service Implementation

    public Task HostAsync(CancellationToken cancellationToken)
    {
        _server.Start();
        return Task.CompletedTask;
    }

    //public override async Task<Message> Receive(MessageSelector request, ServerCallContext context)
    //{
    //    switch ((Message.ContentOneofCase)request.Type)
    //    {
    //        case Message.ContentOneofCase.Ruleset:
    //            return new Message { Ruleset = (await GetBoardAsync()).Ruleset };
    //        case Message.ContentOneofCase.Board:
    //            return new Message { Board = await GetBoardAsync() };
    //        default:
    //            return ProtobufHelper.EmptyMessage;
    //    }
    //}

    public override Task<Empty> Send(Message request, ServerCallContext context)
    {
        _messenger.Send(request, nameof(ITripleTriadServer));
        return ProtobufHelper.EmptyResponseTask;
    }

    public Task SendMessageAsync(Message message)
    {
        _messageBuffer.Post(message);
        return Task.CompletedTask;
    }
}
