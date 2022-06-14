using Google.Protobuf.WellKnownTypes;
using TripleTriad.Models;

namespace TripleTriad.Services;

public static class ProtobufHelper
{
    public static Empty EmptyResponse { get; } = new Empty();
    public static Task<Empty> EmptyResponseTask { get; } = Task.FromResult(EmptyResponse);
    public static Message EmptyMessage { get; } = new Message();
    public static Task<Message> EmptyMessageTask { get; } = Task.FromResult(EmptyMessage);
}
