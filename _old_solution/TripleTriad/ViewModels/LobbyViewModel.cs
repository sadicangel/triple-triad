using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TripleTriad.Extensions;
using TripleTriad.Models;
using TripleTriad.Pages;
using TripleTriad.Services;
using TripleTriad.ViewModels.Explicit;
using Windows.ApplicationModel.Core;

namespace TripleTriad.ViewModels;

public sealed class LobbyViewModel : BaseViewModel<object, LobbyPage>, IRecipient<Message>
{
    private readonly ITripleTriadUser _user;
    private readonly INavigationService _navigation;
    private readonly IMessenger _messenger;
    private RulesetViewModel _ruleset = new()
    {
        Model = new()
        {
            MatchRules = MatchRules.Open | MatchRules.Random,
            BoardRules = BoardRules.Same | BoardRules.Plus | BoardRules.Combo,
            TradeRules = TradeRules.One
        }
    };
    private bool _isReady;
    private bool _otherIsReady;
    private bool _canStart;

    public RulesetViewModel Ruleset { get => _ruleset; set => SetProperty(ref _ruleset, value, OnRulesetChanged); }

    public ObservableCollection<Message> Chat { get; } = new();
    
    public RelayCommand<MatchRules> MatchRuleCheckedCommand { get; }
    public RelayCommand<BoardRules> BoardRuleCheckedCommand { get; }
    public RelayCommand<TradeRules> TradeRuleCheckedCommand { get; }
    public RelayCommand<string> SendMessageCommand { get; }
    public bool CanEdit { get => _user?.IsHosting ?? true; }
    public RelayCommand ReadyCommand { get; }
    public bool CanStart { get => (_user?.IsHosting ?? true) && _canStart; set => SetProperty(ref _canStart, value); }
    public RelayCommand StartCommand { get; }

    public LobbyViewModel(INavigationService navigation, IMessenger messenger, MainViewModel main)
    {
        IsBusy = true;
        _navigation = navigation;
        _messenger = messenger;
        _user = main.User;
        _messenger.Register(this, nameof(ITripleTriadClient));
        _messenger.Register(this, nameof(ITripleTriadServer));
        MatchRuleCheckedCommand = new RelayCommand<MatchRules>(OnMatchRuleChecked);
        BoardRuleCheckedCommand = new RelayCommand<BoardRules>(OnBoardRuleChecked);
        TradeRuleCheckedCommand = new RelayCommand<TradeRules>(OnTradeRuleChecked);
        SendMessageCommand = new RelayCommand<string>(OnSendMessage, msg => !String.IsNullOrWhiteSpace(msg));
        ReadyCommand = new RelayCommand(OnReady);
        StartCommand = new RelayCommand(OnStart);
    }

    public void Receive(Message message)
    {
        switch (message.ContentCase)
        {
            case Message.ContentOneofCase.None:
                return;
            case Message.ContentOneofCase.Text:
                RunOnUIThread(() => Chat.Add(message));
                return;
            case Message.ContentOneofCase.Ruleset:
                RunOnUIThread(() =>
                {
                    Ruleset.Model = message.Ruleset;
                    OnPropertyChanged(nameof(Ruleset));
                });
                return;
            case Message.ContentOneofCase.Status:
                switch (message.Status)
                {
                    case Status.Ready:
                        RunOnUIThread(() =>
                        {
                            _otherIsReady = !_otherIsReady;
                            if (_otherIsReady)
                                Chat.Add(new Message { Text = $"{message.Player.Name} is ready." });
                            CanStart = _isReady && _otherIsReady;
                        });
                        return;
                    case Status.Start:
                        RunOnUIThread(() => _navigation.NavigateTo<BoardViewModel>());
                        return;
                }
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

        var msg = new Message { Player = _user.Player, Text = message };
        Chat.Add(msg);
        View.MessageTextBox.Text = String.Empty;
        _user.SendMessageAsync(msg).ConfigureAwait(false).GetAwaiter();
    }

    private void OnReady()
    {
        _isReady = !_isReady;
        _user.SendMessageAsync(Status.Ready).ConfigureAwait(false).GetAwaiter();
        CanStart = _isReady && _otherIsReady;
    }

    private void OnStart()
    {
        _navigation.NavigateTo<BoardViewModel>();
    }
}