namespace TripleTriad.Models;

public interface ITripleTriadUser : IAsyncDisposable
{
    public Player Player { get; }

    public bool IsHosting { get; }

    public event EventHandler<Player>? PlayerConnected;

    public event EventHandler<Message>? MessageReceived;

    public Task<Board> GetBoardAsync();

    public Task SendMessageAsync(Message message);

    public Task SendMessageAsync(bool ready) => SendMessageAsync(new Message { Player = Player, Ready = ready });

    public Task SendMessageAsync(string text) => SendMessageAsync(new Message { Player = Player, Text = text });

    public Task SendMessageAsync(Board board) => SendMessageAsync(new Message { Player = Player, Board = board });

    public Task SendMessageAsync(Ruleset ruleset) => SendMessageAsync(new Message { Player = Player, Ruleset = ruleset });

    public Task SendMessageAsync(Move move) => SendMessageAsync(new Message { Player = Player, Move = move });
}
 