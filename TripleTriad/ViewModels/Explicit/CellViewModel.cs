using Microsoft.UI;
using System.Diagnostics.CodeAnalysis;
using TripleTriad.Models;

namespace TripleTriad.ViewModels.Explicit;

public sealed class CellViewModel : BaseViewModel
{
    public int Row { get; init; }
    public int Column { get; init; }
    public PlayerViewModel? Player { get => _player; set => SetProperty(ref _player, value).Then(OnPlayerChanged); }
    private PlayerViewModel? _player;
    public CardViewModel? Card { get => _card; set => SetProperty(ref _card, value); }
    private CardViewModel? _card;

    public event EventHandler<Direction>? FlipRequested;

    private void OnPlayerChanged(PlayerViewModel? player)
    {
        if (Card is not null)
            Card.Color = player?.Color ?? Colors.GhostWhite;
    }

    public bool BeatsOther([NotNullWhen(true)] CellViewModel? other, Direction direction)
    {
        if (other is null || other.Card is null || Card is null || other.Player?.Color == Player?.Color)
            return false;

        var thisCard = Card;
        var otherCard = other.Card;
        switch (direction)
        {
            case Direction.Left:
                return thisCard.Left > otherCard.Right;
            case Direction.Up:
                return thisCard.Up > otherCard.Down;
            case Direction.Right:
                return thisCard.Right > otherCard.Left;
            case Direction.Down:
                return thisCard.Down > otherCard.Up;
            default:
                throw new InvalidOperationException();
        }
    }

    public void FlipCard(Direction direction)
    {
        FlipRequested?.Invoke(this, direction);
    }
}
