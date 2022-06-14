using Microsoft.UI;
using System.Diagnostics.CodeAnalysis;
using TripleTriad.Controls;
using TripleTriad.Models;

namespace TripleTriad.ViewModels.Explicit;

public sealed class CellViewModel : BaseViewModel<Cell, CellControl>
{
    public int Index { get; init; }
    public int Row { get => Index / 3; }
    public int Column { get => Index % 3; }
    public PlayerViewModel? Player { get => _player; set => SetProperty(ref _player, value, OnPlayerChanged); }
    private PlayerViewModel? _player;
    public CardViewModel? Card { get => _card; set => SetProperty(ref _card, value, OnCardChanged); }
    private CardViewModel? _card;

    public CellViewModel() => Model = new Cell();

    public Element Element { get => _element; set => SetProperty(ref _element, value); }
    private Element _element;

    public bool HasCard { get => _card is not null; }

    private void OnPlayerChanged(PlayerViewModel? player)
    {
        Model.Player = player?.Model;
    }

    private void OnCardChanged(CardViewModel? card)
    {
        Model.Card = card?.Model;
        OnPropertyChanged(nameof(HasCard));
    }

    public void Empty()
    {
        Player = null;
        Card = null;
    }
}
