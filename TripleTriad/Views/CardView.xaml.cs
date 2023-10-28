using TripleTriad.Game;
using TripleTriad.ViewModels;

namespace TripleTriad.Views;

public partial class CardView : ContentView
{
	public CardViewModel? Card { get; set; }
	public static readonly BindableProperty CardProperty =
        BindableProperty.Create(nameof(Card), typeof(CardViewModel), typeof(CardView), propertyChanged: OnCardChanged);

    private bool _isFlipping;

	public CardView()
	{
		InitializeComponent();
	}

	private static void OnCardChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var cardView = (CardView)bindable;
		if (oldValue is CardViewModel oldCard)
			oldCard.Flipped -= cardView.FlippedEventHandler;
		if (newValue is CardViewModel newCard)
			newCard.Flipped += cardView.FlippedEventHandler;
		cardView.BindingContext = newValue;
	}

    private async void FlippedEventHandler(object? sender, CardFlippedEventArgs args)
    {
        if (!_isFlipping)
        {
            _isFlipping = true;
            await (args.Axis == Axis.Horizontal ? FlipHorizontal() : FlipVertical());
            _isFlipping = false;
        }
        args.NotifyCompletion();
    }

    private async Task FlipVertical()
    {
        const int animationTime = 500;
        await Task.WhenAll(
            _card.RotateXTo(180, animationTime, Easing.CubicIn),
            _cardFront.FadeTo(0, animationTime, Easing.CubicIn),
            _cardBack.FadeTo(1, animationTime, Easing.CubicIn));
        await Task.WhenAll(
            _card.RotateXTo(360, animationTime, Easing.CubicOut),
            _cardFront.FadeTo(1, animationTime, Easing.CubicOut),
            _cardBack.FadeTo(0, animationTime, Easing.CubicOut));
        _card.RotationX = 0;
    }

    private async Task FlipHorizontal()
    {
        const int animationTime = 500;
        await Task.WhenAll(
            _card.RotateYTo(180, animationTime, Easing.CubicIn),
            _cardFront.FadeTo(0, animationTime, Easing.CubicIn),
            _cardBack.FadeTo(1, animationTime, Easing.CubicIn));
        await Task.WhenAll(
            _card.RotateYTo(360, animationTime, Easing.CubicOut),
            _cardFront.FadeTo(1, animationTime, Easing.CubicOut),
            _cardBack.FadeTo(0, animationTime, Easing.CubicOut));
        _card.RotationY = 0;
    }
}