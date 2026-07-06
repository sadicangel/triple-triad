namespace TripleTriad.Contracts;

public enum TransportConnectionState
{
    NotStarted,
    Connecting,
    Connected,
    Disconnected,
    Closed,
    Failed,
}

public interface IMatchTransport
{
    TransportConnectionState ConnectionState { get; }

    IAsyncEnumerable<NetworkMessage> ReadMessagesAsync(
        CancellationToken cancellationToken = default);

    ValueTask SendAsync(
        NetworkMessage message,
        CancellationToken cancellationToken = default);
}
