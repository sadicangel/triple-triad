using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using TripleTriad.Models;

namespace TripleTriad.Services;

public sealed class GameServer : TripleTriadService.TripleTriadServiceBase
{
    private readonly Server _server;
    private readonly ConcurrentDictionary<string, IServerStreamWriter<Message>> _subscriptions = new();
    private readonly BufferBlock<Message> _messageBuffer = new(new DataflowBlockOptions { EnsureOrdered = true });

    private Navigation _navigation = Navigation.GoToMain;
    private Models.Status _leftStatus = Models.Status.None;
    private Models.Status _rightStatus = Models.Status.None;

    public Player? LeftPlayer { get => Board.LeftPlayer; set => Board.LeftPlayer = value; }

    public Player? RightPlayer { get => Board.RightPlayer; set => Board.RightPlayer = value; }

    public Ruleset Ruleset { get => Board.Ruleset; set => Board.Ruleset = value; }

    public Board Board { get; }

    public GameServer(int port)
    {
        _server = new Server
        {
            Services = { TripleTriadService.BindService(this) },
            Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
        };
        Board = new Board
        {
            Ruleset = new Ruleset
            {
                MatchRules = MatchRules.Open | MatchRules.Random,
                BoardRules = BoardRules.None,
                TradeRules = TradeRules.None,
            }
        };
    }

    public Task StartAsync()
    {
        _server.Start();
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        await _server.ShutdownAsync();
    }

    public override Task<JoinResponse> Join(JoinRequest request, ServerCallContext context)
    {
        lock (_server)
        {
            var id = Guid.NewGuid().ToString();
            var isLeft = LeftPlayer is null;
            var color = isLeft ? 0xFF006400 : 0xFF8B0000; // DarkGreen : DarkRed
            var player = new Player
            {
                Id = id,
                Name = request.Name,
                Color = color,
                IsLeft = isLeft
            };

            if (isLeft)
                LeftPlayer = player;
            else if (RightPlayer is null)
                RightPlayer = player;
            else
                return Task.FromResult(new JoinResponse { Success = false });

            return Task.FromResult(new JoinResponse
            {
                Success = true,
                Player = player,
            });
        }
    }

    public override async Task Subscribe(Player request, IServerStreamWriter<Message> responseStream, ServerCallContext context)
    {
        _subscriptions.TryAdd(request.Id, responseStream);
        // If there are two players we're ready to start.
        if (_subscriptions.Count == 2)
        {
            _messageBuffer.Post(new Message { Navigation = Navigation.GoToLobby });
        }
        while (_subscriptions.ContainsKey(request.Id))
        {
            var message = await _messageBuffer.ReceiveAsync();
            await Task.WhenAll(_subscriptions.Values.Select(sub => sub.WriteAsync(message)));
        }
    }

    public override Task<Empty> Unsubscribe(Player request, ServerCallContext context)
    {
        _subscriptions.TryRemove(request.Id, out _);
        return ProtobufHelper.EmptyResponseTask;
    }

    private Navigation MoveToNextPage(bool replay)
    {
        return _navigation = getNextPage(replay);

        Navigation getNextPage(bool replay)
        {
            switch (_navigation)
            {
                case Navigation.None:
                    return Navigation.GoToMain;
                case Navigation.GoToMain:
                    return Navigation.GoToLobby;
                case Navigation.GoToLobby:
                    return Ruleset.HasRule(MatchRules.Random) ? Navigation.GoToBoard : Navigation.GoToBuilder;
                case Navigation.GoToBuilder:
                    return Navigation.GoToBoard;
                case Navigation.GoToBoard:
                    return Navigation.GoToPostScreen;
                case Navigation.GoToPostScreen when replay:
                    goto case Navigation.GoToLobby;
                case Navigation.GoToPostScreen:
                    return Navigation.None;
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    private Player? GetOtherPlayer(Player player)
    {
        return player.IsLeft ? RightPlayer : LeftPlayer;
    }

    public async Task Post(Message message, string? id = null)
    {
        if (id is null)
            _messageBuffer.Post(message);
        else
            await _subscriptions[id].WriteAsync(message);
    }

    public override async Task<Empty> Send(Message request, ServerCallContext context)
    {
        switch (request.ContentCase)
        {
            case Message.ContentOneofCase.Text:
                await Post(request, GetOtherPlayer(request.Player)?.Id);
                break;
            case Message.ContentOneofCase.Status:
                if (request.Player.IsLeft)
                    _leftStatus = request.Status;
                else
                    _rightStatus = request.Status;
                if(_leftStatus == Models.Status.Ready && _rightStatus == Models.Status.Ready)
                    await Post(new Message { Navigation = MoveToNextPage(replay: false) });
                break;
            case Message.ContentOneofCase.Ruleset:
                Ruleset = request.Ruleset;
                await Post(request, GetOtherPlayer(request.Player)?.Id);
                break;
            case Message.ContentOneofCase.Board:
                break;
            case Message.ContentOneofCase.Move:
                break;
            default:
                break;
        }

        return ProtobufHelper.EmptyResponse;
    }
}