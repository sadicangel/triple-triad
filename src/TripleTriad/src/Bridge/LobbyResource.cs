using Godot;

namespace TripleTriad.Bridge;

[GlobalClass]
public sealed partial class LobbyRuleOptionResource : Resource
{
    [Export] public string Name { get; set; } = string.Empty;

    [Export] public bool Enabled { get; set; }
}

[GlobalClass]
public sealed partial class LobbySeatResource : Resource
{
    [Export] public string Seat { get; set; } = string.Empty;

    [Export] public bool IsLocal { get; set; }

    [Export] public bool Occupied { get; set; }

    [Export] public bool CanTake { get; set; }

    [Export] public string Name { get; set; } = string.Empty;

    [Export] public string Kind { get; set; } = string.Empty;

    [Export] public bool Ready { get; set; }

    [Export] public bool Connected { get; set; }
}

[GlobalClass]
public sealed partial class LobbySnapshotResource : Resource
{
    [Export] public bool HasLobby { get; set; }

    [Export] public string Mode { get; set; } = string.Empty;

    [Export] public string LocalSeat { get; set; } = string.Empty;

    [Export] public Godot.Collections.Array Rules { get; set; } = [];

    [Export] public bool CanStart { get; set; }

    [Export] public bool CanSelectCards { get; set; }

    [Export] public Godot.Collections.Array SelectedCards { get; set; } = [];

    [Export] public bool IsMatchStarting { get; set; }

    [Export] public string Status { get; set; } = string.Empty;

    [Export] public Godot.Collections.Array Seats { get; set; } = [];
}

[GlobalClass]
public sealed partial class ConnectionStateResource : Resource
{
    [Export] public long Sequence { get; set; }

    [Export] public string State { get; set; } = string.Empty;

    [Export] public string Reason { get; set; } = string.Empty;
}
