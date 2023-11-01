namespace TripleTriad;

public sealed record class Lobby(Guid Id, string Name, Guid Owner, List<Guid> Users, string Rules, bool IsActive);
