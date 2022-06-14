namespace TripleTriad.Models;

public sealed partial class Ruleset
{
    public MatchRules MatchRules { get => (MatchRules)MatchRulesInt32; set => MatchRulesInt32 = (int)value; }
    public BoardRules BoardRules { get => (BoardRules)BoardRulesInt32; set => BoardRulesInt32 = (int)value; }
    public TradeRules TradeRules { get => (TradeRules)TradeRulesInt32; set => TradeRulesInt32 = (int)value; }

    public bool HasRule(MatchRules rule) => MatchRules.HasFlag(rule);
    public bool HasRule(BoardRules rule) => BoardRules.HasFlag(rule);
    public bool HasRule(TradeRules rule) => TradeRules == rule;
}