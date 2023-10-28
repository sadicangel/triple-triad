using Microsoft.AspNetCore.SignalR;
using TripleTriad.Games;

namespace TripleTriad.Services;
public sealed class TripleTriadGame
{
    private static readonly IReadOnlyList<Direction> Directions = Enum.GetValues<Direction>();
    private readonly IClientProxy _proxy;
    private Board? _board;

    public Board Board { get => _board ?? throw new InvalidOperationException("Board is invalid"); }

    public TripleTriadGame(IClientProxy clientProxy, Board board)
    {
        _proxy = clientProxy;
        _board = board;
    }

    public async Task StartGame()
    {
    }
}
