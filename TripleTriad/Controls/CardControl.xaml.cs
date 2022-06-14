using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;
using TripleTriad.Models;
using TripleTriad.ViewModels.Explicit;
using Windows.UI;

namespace TripleTriad.Controls;

public sealed partial class CardControl : UserControl
{
    private TaskCompletionSource? _flipTaskSource;
    private TaskCompletionSource? _colorTaskSource;
    private Color _toColor;
    private readonly ColorAnimation _colorAnimation;
    private readonly Storyboard _colorStoryboard;

    public CardViewModel Card { get => (CardViewModel)GetValue(CardProperty); set => SetValue(CardProperty, value); }
    public static readonly DependencyProperty CardProperty =
        DependencyProperty.Register(nameof(Card), typeof(CardViewModel), typeof(CardControl), new PropertyMetadata(null, OnCardChanged));

    public bool IsSelected { get => (bool)GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(CardControl), new PropertyMetadata(null, OnIsSelectedChanged));

    public CardControl()
    {
        InitializeComponent();
        var duration = TimeSpan.FromSeconds((double)RootGrid.Resources["FlipDuration"]);
        _colorAnimation = new ColorAnimation { Duration = duration };
        Storyboard.SetTarget(_colorAnimation, BackgroundBrush);
        Storyboard.SetTargetProperty(_colorAnimation, nameof(SolidColorBrush.Color));
        _colorStoryboard = new Storyboard { Children = { _colorAnimation } };
        _colorStoryboard.Completed += Color_Completed;
    }

    private static void OnCardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CardControl cardControl && e.NewValue is CardViewModel card)
        {
            cardControl.RootGrid.DataContext = card;
            card.View = cardControl;
        }
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CardControl cardControl && e.NewValue is bool isHighlighted)
        {
            if (isHighlighted)
                VisualStateManager.GoToState(cardControl, "Selected", useTransitions: false);
            else
                VisualStateManager.GoToState(cardControl, "Normal", useTransitions: false);
        }
    }

    private void CardStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
    {
        Debug.WriteLine($"{e.OldState?.Name ?? "null"} => {e.NewState?.Name ?? "null"}");
    }

    public void RemoveVisualStates() => VisualStateManager.GoToState(this, "Normal", useTransitions: false);

    public Task FlipAsync(Direction direction)
    {
        _flipTaskSource = new TaskCompletionSource();
        VisualStateManager.GoToState(this, $"Flip{direction}", useTransitions: false);
        return _flipTaskSource.Task;
    }

    private void Flip_Completed(object? sender, object e)
    {
        VisualStateManager.GoToState(this, "Normal", useTransitions: false);
        _flipTaskSource?.SetResult();
        _flipTaskSource = null;
    }

    public Task ColorAsync(Color color)
    {
        _colorTaskSource = new TaskCompletionSource();
        _colorAnimation.From = Card.Color;
        _colorAnimation.To = _toColor = color;
        _colorStoryboard.Begin();
        return _colorTaskSource.Task;
    }

    private void Color_Completed(object? sender, object e)
    {
        Card.Color = _toColor;
        _colorTaskSource?.SetResult();
        _colorTaskSource = null;
    }

    public void Highlight(BoardRules rule)
    {
        VisualStateManager.GoToState(this, $"Highlight{rule}", useTransitions: false);
    }
}