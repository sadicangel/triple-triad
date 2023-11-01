using TripleTriad.ServerState;

namespace TripleTriad;

public interface IGameHubHandler
{
    Task<ServerStateResponse> GetServerStateAsync();
}