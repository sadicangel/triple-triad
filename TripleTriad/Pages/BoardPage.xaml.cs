using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using TripleTriad.Controls;
using TripleTriad.Models;
using TripleTriad.ViewModels;
using TripleTriad.ViewModels.Explicit;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;

namespace TripleTriad.Pages;

public sealed partial class BoardPage : Page
{
    private TaskCompletionSource? _ruleTaskSource;
    private IEnumerable<DirectedCell>? _ruleCells;
    private MoveViewModel? _activeMove;

    private BoardViewModel ViewModel { get; }

    public BoardPage()
    {
        InitializeComponent();
        DataContext = ViewModel = App.GetService<BoardViewModel>();
        ViewModel.View = this;
    }

    private void OnPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ((CardControl)sender).IsSelected = true;
    }

    private void OnPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ((CardControl)sender).IsSelected = false;
    }

    private async void OnDragStarting(UIElement sender, DragStartingEventArgs args)
    {
        // Generate a bitmap with only the TextBox
        // We need to take the deferral as the rendering won't be completed synchronously
        var deferral = args.GetDeferral();
        var rtb = new RenderTargetBitmap();
        await rtb.RenderAsync(sender);
        sender.Opacity = 0.5;
        var buffer = await rtb.GetPixelsAsync();
        var bitmap = SoftwareBitmap.CreateCopyFromBuffer(buffer,
            BitmapPixelFormat.Bgra8,
            rtb.PixelWidth,
            rtb.PixelHeight,
            BitmapAlphaMode.Premultiplied);
        args.DragUI.SetContentFromSoftwareBitmap(bitmap);
        deferral.Complete();
        _activeMove = new MoveViewModel(ViewModel.ActivePlayer, ((CardControl)sender).Card);
    }

    private void OnDropCompleted(UIElement sender, DropCompletedEventArgs args)
    {
        sender.Opacity = 1;
        _activeMove = null;
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        var hasCard = _activeMove is not null;
        e.AcceptedOperation = hasCard ? DataPackageOperation.Move : DataPackageOperation.None;
        if (hasCard)
        {
            var move = _activeMove!;
            var cell = ((CellControl)sender).Cell;
            move.CellIndex = cell.Index;
            await ViewModel.ExecuteMove(cell, move);
            if (!ViewModel.IsGameOver)
            {
                ViewModel.IsLeftActive = !ViewModel.IsLeftActive;
            }
            else
            {
                Debug.WriteLine("{0} wins", ViewModel.Winner.Name);
                var restartDialog = new ContentDialog
                {
                    XamlRoot = XamlRoot,
                    Title = "Game Over",
                    Content = $"Player {ViewModel.Winner.Name} wins! Restart board?",
                    PrimaryButtonText = "Restart",
                    CloseButtonText = "Exit"
                };

                var result = await restartDialog.ShowAsync();

                // Delete the file if the user clicked the primary button.
                /// Otherwise, do nothing.
                if (result == ContentDialogResult.Primary)
                {
                    ViewModel.ResetBoard();
                }
                else
                {
                    // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                    Application.Current.Exit();
                }
            }
        }
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        var cellView = (CellControl)sender;
        if (cellView.Cell.Card is null)
        {
            VisualStateManager.GoToState(cellView, "Selected", useTransitions: false);
            e.AcceptedOperation = DataPackageOperation.Move;
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.None;
        }
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        VisualStateManager.GoToState((CellControl)sender, "Normal", useTransitions: false);
    }

    public async Task ShowRuleAsync(BoardRules rule, IEnumerable<DirectedCell> cells)
    {
        _ruleTaskSource = new TaskCompletionSource();
        _ruleCells = cells;
        foreach (var (_, cell) in _ruleCells)
            cell.Card!.Hightlight(rule);
        VisualStateManager.GoToState(this, $"Rule{rule}", useTransitions: true);
        await _ruleTaskSource.Task;
    }

    private void ShowRule_Completed(object? sender, object e)
    {
        if (_ruleCells is not null)
        {
            foreach (var (_, cell) in _ruleCells)
                cell.Card!.RemoveVisualStates();
        }
        VisualStateManager.GoToState(this, "Normal", useTransitions: true);
        _ruleTaskSource?.SetResult();
        _ruleTaskSource = null;
        _ruleCells = null;
    }
}
