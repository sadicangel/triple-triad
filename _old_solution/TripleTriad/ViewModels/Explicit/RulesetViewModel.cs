using TripleTriad.Models;

namespace TripleTriad.ViewModels.Explicit;

public sealed class RulesetViewModel : BaseViewModel<Ruleset>
{
    public MatchRules MatchRules { get => Model.MatchRules; set => SetProperty(m => m.MatchRules, (m, v) => m.MatchRules = v, value); }

    public BoardRules BoardRules { get => Model.BoardRules; set => SetProperty(m => m.BoardRules, (m, v) => m.BoardRules = v, value); }

    public TradeRules TradeRules { get => Model.TradeRules; set => SetProperty(m => m.TradeRules, (m, v) => m.TradeRules = v, value); }

    public RulesetViewModel() => Model = new Ruleset();

    public bool HasRule(MatchRules rule) => Model.HasRule(rule);
    public bool HasRule(BoardRules rule) => Model.HasRule(rule);
    public bool HasRule(TradeRules rule) => Model.HasRule(rule);
}