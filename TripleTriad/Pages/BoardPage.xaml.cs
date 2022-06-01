using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TripleTriad.Controls;
using TripleTriad.Models;
using TripleTriad.ViewModels;
using TripleTriad.ViewModels.Explicit;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;

namespace TripleTriad.Pages;

public sealed partial class BoardPage : Page
{
    private BoardViewModel ViewModel { get; }
    private MoveViewModel? _activeMove;

    public BoardPage()
    {
        InitializeComponent();
        DataContext = ViewModel = App.GetService<BoardViewModel>();
    }

    private void OnPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState((CardControl)sender, "Selected", useTransitions: false);
    }

    private void OnPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState((CardControl)sender, "Normal", useTransitions: false);
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
        var cardView = (CardControl)sender;
        if (args.DropResult == DataPackageOperation.Move)
        {
            // Update indices of cards comming after.
            for (int i = cardView.Card.HandIndex + 1; i < ViewModel.ActiveHand.Count; ++i)
                ViewModel.ActiveHand[i].HandIndex--;
            ViewModel.ActiveHand.RemoveAt(cardView.Card.HandIndex);
            ViewModel.IsLeftActive = !ViewModel.IsLeftActive;
        }
        else
        {
            cardView.Opacity = 1;
        }
        _activeMove = null;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        var hasCard = _activeMove is not null;
        e.AcceptedOperation = hasCard ? DataPackageOperation.Move : DataPackageOperation.None;
        if (hasCard)
        {
            var move = _activeMove!;
            var cell = ((CellControl)sender).Cell;
            cell.Card = move.Card;
            cell.Player = move.Player;
            var neighbours = ViewModel.GetCellNeighbours(cell);
            if (cell.BeatsOther(neighbours.Left, Direction.Left))
            {
                neighbours.Left.FlipCard(Direction.Left);
                neighbours.Left.Player = cell.Player;
            }
            if (cell.BeatsOther(neighbours.Up, Direction.Up))
            {
                neighbours.Up.FlipCard(Direction.Up);
                neighbours.Up.Player = cell.Player;
            }
            if (cell.BeatsOther(neighbours.Right, Direction.Right))
            {
                neighbours.Right.FlipCard(Direction.Right);
                neighbours.Right.Player = cell.Player;
            }
            if (cell.BeatsOther(neighbours.Down, Direction.Down))
            {
                neighbours.Down.FlipCard(Direction.Down);
                neighbours.Down.Player = cell.Player;
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
}
