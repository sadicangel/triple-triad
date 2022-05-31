using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using TripleTriad.ViewModels.Explicit;

namespace TripleTriad.Views;

public sealed partial class CardView : UserControl
{
    public CardViewModel Card { get => (CardViewModel)GetValue(CardProperty); set => SetValue(CardProperty, value); }
    public static readonly DependencyProperty CardProperty =
        DependencyProperty.Register(nameof(Card), typeof(CardViewModel), typeof(CardView), new PropertyMetadata(null, OnCardChanged));

    public CardView()
    {
        InitializeComponent();
    }

    private static void OnCardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CardView cardView && e.NewValue is CardViewModel card)
            cardView.RootGrid.DataContext = card;
    }

    private void CardStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
    {
        Debug.WriteLine($"{e.OldState?.Name ?? "null"} => {e.NewState?.Name ?? "null"}");
    }

    private void Flip_Completed(object sender, object e)
    {
        VisualStateManager.GoToState(this, "Normal", useTransitions: false);
    }
}