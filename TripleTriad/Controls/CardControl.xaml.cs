using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using TripleTriad.ViewModels.Explicit;

namespace TripleTriad.Controls;

public sealed partial class CardControl : UserControl
{
    public CardViewModel Card { get => (CardViewModel)GetValue(CardProperty); set => SetValue(CardProperty, value); }
    public static readonly DependencyProperty CardProperty =
        DependencyProperty.Register(nameof(Card), typeof(CardViewModel), typeof(CardControl), new PropertyMetadata(null, OnCardChanged));

    public CardControl()
    {
        InitializeComponent();
    }

    private static void OnCardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CardControl cardControl && e.NewValue is CardViewModel card)
            cardControl.RootGrid.DataContext = card;
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