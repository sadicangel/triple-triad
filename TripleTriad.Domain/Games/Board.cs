using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace TripleTriad.Games;

public partial class Board : ObservableRecipient
{
    [ObservableProperty]
    private Ruleset _rules = null!;

    [ObservableProperty]
    private Player _leftPlayer = null!;

    [ObservableProperty]
    private Player _rightPlayer = null!;

    [ObservableProperty]
    private ObservableCollection<Card> _leftHand = null!;

    [ObservableProperty]
    private ObservableCollection<Card> _rightHand = null!;

    [ObservableProperty]
    private List<Cell> _cells = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveHand))]
    [NotifyPropertyChangedFor(nameof(ActivePlayer))]
    private Side _activeSide;

    [ObservableProperty]
    private bool _isGameOver;

    [ObservableProperty]
    private Player? _winner;

    public Player ActivePlayer { get => ActiveSide == Side.Left ? LeftPlayer : RightPlayer; }

    public ObservableCollection<Card> ActiveHand { get => ActiveSide == Side.Left ? LeftHand : RightHand; }

    protected Board(IMessenger messenger) : base(messenger)
    {
    }
}
