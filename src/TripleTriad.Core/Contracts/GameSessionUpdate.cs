namespace TripleTriad.Contracts;

public enum SessionConnectionState
{
    NotStarted,
    Connecting,
    Connected,
    Reconnecting,
    Disconnected,
    Failed,
    Closed,
}
