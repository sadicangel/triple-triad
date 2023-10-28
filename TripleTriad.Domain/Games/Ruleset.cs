using CommunityToolkit.Mvvm.ComponentModel;

namespace TripleTriad.Games;

public sealed partial class Ruleset : ObservableObject, IEquatable<Ruleset>
{
    public static Ruleset Default
    {
        get => new()
        {
            MatchRules = MatchRules.Open,
            BoardRules = BoardRules.Elemental,
            TradeRules = TradeRules.None
        };
    }

    [ObservableProperty]
    private MatchRules _matchRules;

    [ObservableProperty]
    private BoardRules _boardRules;

    [ObservableProperty]
    private TradeRules _tradeRules;

    public Ruleset() { }

    public Ruleset(Ruleset ruleset)
    {
        _matchRules = ruleset._matchRules;
        _boardRules = ruleset._boardRules;
        _tradeRules = ruleset._tradeRules;
    }

    public bool HasRule(MatchRules rule) => MatchRules.HasFlag(rule);
    public bool HasRule(BoardRules rule) => BoardRules.HasFlag(rule);
    public bool HasRule(TradeRules rule) => TradeRules == rule;

    public bool Equals(Ruleset? other) => other is not null && _matchRules == other._matchRules && _boardRules == other._boardRules && _tradeRules == other._tradeRules;
    public override bool Equals(object? obj) => Equals(obj as Ruleset);
    public override int GetHashCode() => HashCode.Combine(_matchRules, _boardRules, _tradeRules);
    public static bool operator ==(Ruleset? lhs, Ruleset? rhs) => lhs is null ? rhs is null : lhs.Equals(rhs);
    public static bool operator !=(Ruleset? lhs, Ruleset? rhs) => !(lhs == rhs);
}