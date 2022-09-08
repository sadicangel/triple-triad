using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using TripleTriad.Models;
using TripleTriad.ViewModels.Explicit;

namespace TripleTriad.Controls;

public sealed partial class CellControl : UserControl
{
    public CellViewModel Cell { get => (CellViewModel)GetValue(CellProperty); set => SetValue(CellProperty, value); }
    public static readonly DependencyProperty CellProperty =
        DependencyProperty.Register(nameof(Cell), typeof(CellViewModel), typeof(CellControl), new PropertyMetadata(null, OnCellChanged));

    public CellControl()
    {
        InitializeComponent();
    }

    private static void OnCellChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CellControl cellControl)
        {
            var viewModel = (CellViewModel)e.NewValue;
            cellControl.GridRoot.DataContext = viewModel;
            viewModel.View = cellControl;
        }
    }
}
