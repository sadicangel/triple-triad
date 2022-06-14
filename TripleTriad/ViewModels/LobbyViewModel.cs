using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TripleTriad.Models;
using TripleTriad.Pages;
using TripleTriad.Services;
using TripleTriad.ViewModels.Explicit;
using Windows.ApplicationModel.Core;

namespace TripleTriad.ViewModels;

public sealed class LobbyViewModel : BaseViewModel<object, LobbyPage>
{
    private readonly ITripleTriadUser _user;
    private readonly CancellationTokenSource _cancellationSource;
    private readonly ManualResetEventSlim _connected;
    private readonly INavigationService _navigation;
    private RulesetViewModel _ruleset = new()
    {
        Model = new()
        {
            MatchRules = MatchRules.Open | MatchRules.Random,
            BoardRules = BoardRules.Same | BoardRules.Plus | BoardRules.Combo,
            TradeRules = TradeRules.One
        }
    };

    public RulesetViewModel Ruleset { get => _ruleset; set => SetProperty(ref _ruleset, value, OnRulesetChanged); }

    public ObservableCollection<string> Chat { get; } = new();
    
    public RelayCommand<MatchRules> MatchRuleCheckedCommand { get; }
    public RelayCommand<BoardRules> BoardRuleCheckedCommand { get; }
    public RelayCommand<TradeRules> TradeRuleCheckedCommand { get; }
    public RelayCommand<string> SendMessageCommand { get; }
    public bool CanEdit { get => _user?.IsHosting ?? true; }

    public LobbyViewModel(INavigationService navigation, MainViewModel main)
    {
        IsBusy = true;
        _connected = new ManualResetEventSlim();
        _cancellationSource = new CancellationTokenSource();
        _navigation = navigation;
        _user = main.User;
        _user.MessageReceived += User_MessageReceived;
        MatchRuleCheckedCommand = new RelayCommand<MatchRules>(OnMatchRuleChecked, _ => CanEdit);
        BoardRuleCheckedCommand = new RelayCommand<BoardRules>(OnBoardRuleChecked, _ => CanEdit);
        TradeRuleCheckedCommand = new RelayCommand<TradeRules>(OnTradeRuleChecked, _ => CanEdit);
        SendMessageCommand = new RelayCommand<string>(OnSendMessage, msg => !String.IsNullOrWhiteSpace(msg));
    }

    private void User_MessageReceived(object? sender, Message e)
    {
        switch (e.ContentCase)
        {
            case Message.ContentOneofCase.None:
                return;
            case Message.ContentOneofCase.Text:
                RunOnUIThread(() => Chat.Add(e.Text));
                return;
            case Message.ContentOneofCase.Ready:
                Console.WriteLine("{0} is ready.", e.Player.Name);
                return;
            case Message.ContentOneofCase.Ruleset:
                RunOnUIThread(() =>
                {
                    Ruleset.Model = e.Ruleset;
                    OnPropertyChanged(nameof(Ruleset));
                });
                return;
            default:
                return;
        }
    }
    private void OnMatchRuleChecked(MatchRules rule)
    {
        Ruleset.MatchRules ^= rule;
        OnRulesetChanged(Ruleset);
    }

    private void OnBoardRuleChecked(BoardRules rule)
    {
        Ruleset.BoardRules ^= rule;
        OnRulesetChanged(Ruleset);
    }

    private void OnTradeRuleChecked(TradeRules rule)
    {
        Ruleset.TradeRules = rule;
        OnRulesetChanged(Ruleset);
    }

    private void OnRulesetChanged(RulesetViewModel ruleset)
    {
        if (_user.IsHosting)
            _user.SendMessageAsync(Ruleset.Model);
    }

    private void OnSendMessage(string? message)
    {
        if (String.IsNullOrEmpty(message))
            return;

        Chat.Add(message);
        View.MessageTextBox.Text = String.Empty;
        _user.SendMessageAsync(message).ConfigureAwait(false).GetAwaiter();
    }
}