using TripleTriad.Server.Dtos;

namespace TripleTriad.Server.Events;

public sealed class ServerMessageSentEvent : ServerEvent<ServerMessageDto>
{
    public override string Type { get; init; } = "Server.MessageSent";
}
