using System.Threading.Channels;
using TripleTriad.Contracts;

namespace TripleTriad.Networking;

public sealed class InMemoryMatchTransport : IMatchTransport
{
    private readonly Channel<NetworkMessage> _incoming = Channel.CreateUnbounded<NetworkMessage>();
    private InMemoryMatchTransport? _peer;

    private InMemoryMatchTransport() { }

    public static (InMemoryMatchTransport First, InMemoryMatchTransport Second) CreatePair()
    {
        var first = new InMemoryMatchTransport();
        var second = new InMemoryMatchTransport();

        first._peer = second;
        second._peer = first;

        return (first, second);
    }

    public TransportConnectionState ConnectionState { get; private set; } = TransportConnectionState.Connected;

    public IAsyncEnumerable<NetworkMessage> ReadMessagesAsync(
        CancellationToken cancellationToken = default) =>
        _incoming.Reader.ReadAllAsync(cancellationToken);

    public ValueTask SendAsync(
        NetworkMessage message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (ConnectionState != TransportConnectionState.Connected)
            throw new InvalidOperationException($"Cannot send while the transport is {ConnectionState}.");

        var peer = _peer
            ?? throw new InvalidOperationException("The in-memory transport is not paired.");

        if (peer.ConnectionState != TransportConnectionState.Connected)
            throw new InvalidOperationException($"Cannot send while the peer transport is {peer.ConnectionState}.");

        peer._incoming.Writer.TryWrite(message);
        return ValueTask.CompletedTask;
    }
}
