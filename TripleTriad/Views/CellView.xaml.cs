using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TripleTriad.Models;
using TripleTriad.ViewModels.Explicit;
namespace TripleTriad.Views;

public sealed partial class CellView : UserControl
{
    public CellViewModel Cell { get => (CellViewModel)GetValue(CellProperty); set => SetValue(CellProperty, value); }
    public static readonly DependencyProperty CellProperty =
        DependencyProperty.Register(nameof(Cell), typeof(CellViewModel), typeof(CellView), new PropertyMetadata(null, OnCellChanged));

    public CellView()
    {
        InitializeComponent();
    }

    private static void OnCellChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CellView cellView)
        {
            if(e.OldValue is CellViewModel old)
                old.FlipRequested -= cellView.FlipRequested;
            var viewModel = (CellViewModel)e.NewValue;
            cellView.GridRoot.DataContext = viewModel;
            viewModel.FlipRequested += cellView.FlipRequested;
        }
    }

    private void FlipRequested(object? sender, Direction direction)
    {
        VisualStateManager.GoToState(Card, $"Flip{direction}", useTransitions: false);
    }
}
