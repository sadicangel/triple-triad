using CommunityToolkit.Mvvm.Input;
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

public sealed class LobbyViewModel : BaseViewModel<object, LobbyPage>
{
    private readonly ITripleTriadUser _user;
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

    public LobbyViewModel(INavigationService navigation, MainViewModel main)
    {
        IsBusy = true;
        _navigation = navigation;
        _user = main.User ?? ITripleTriadServer.Create(new Player { Name = "Test", Color = Colors.DarkGreen.ToUint32(), IsLeft = true }, 50051);
        _user.MessageReceived += User_MessageReceived;
        MatchRuleCheckedCommand = new RelayCommand<MatchRules>(OnMatchRuleChecked);
        BoardRuleCheckedCommand = new RelayCommand<BoardRules>(OnBoardRuleChecked);
        TradeRuleCheckedCommand = new RelayCommand<TradeRules>(OnTradeRuleChecked);
        SendMessageCommand = new RelayCommand<string>(OnSendMessage, msg => !String.IsNullOrWhiteSpace(msg));
        ReadyCommand = new RelayCommand(OnReady);
        StartCommand = new RelayCommand(OnStart, () => CanStart);
    }

    private void User_MessageReceived(object? sender, Message message)
    {
        switch (message.ContentCase)
        {
            case Message.ContentOneofCase.None:
                return;
            case Message.ContentOneofCase.Text:
                RunOnUIThread(() => Chat.Add(message));
                return;
            case Message.ContentOneofCase.Ready:
                RunOnUIThread(() =>
                {
                    if(message.Ready)
                        Chat.Add(new Message { Text = $"{message.Player.Name} is ready." });
                    _otherIsReady = message.Ready;
                    CanStart = _isReady && _otherIsReady;
                });
                return;
            case Message.ContentOneofCase.Ruleset:
                RunOnUIThread(() =>
                {
                    Ruleset.Model = message.Ruleset;
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

        var msg = new Message { Player = _user.Player, Text = message };
        Chat.Add(msg);
        View.MessageTextBox.Text = String.Empty;
        _user.SendMessageAsync(msg).ConfigureAwait(false).GetAwaiter();
    }

    private void OnReady()
    {
        _isReady = !_isReady;
        _user.SendMessageAsync(_isReady).ConfigureAwait(false).GetAwaiter();
        CanStart = _isReady && _otherIsReady;
    }

    private void OnStart()
    {
        _navigation.NavigateTo<BoardViewModel>();
    }
}