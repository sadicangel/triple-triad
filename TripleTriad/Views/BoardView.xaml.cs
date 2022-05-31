using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TripleTriad.Models;
using TripleTriad.ViewModels.Explicit;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;

namespace TripleTriad.Views;

public sealed partial class BoardView : UserControl
{
    public BoardViewModel Board { get => (BoardViewModel)GetValue(BoardProperty); set => SetValue(BoardProperty, value); }
    public static readonly DependencyProperty BoardProperty =
        DependencyProperty.Register(nameof(Board), typeof(BoardViewModel), typeof(BoardView), new PropertyMetadata(null, OnBoardChanged));

    private MoveViewModel? activeMove;

    public BoardView()
    {
        InitializeComponent();
        RootGrid.DataContext = null;
    }

    private static void OnBoardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if(d is BoardView boardView)
            boardView.RootGrid.DataContext = (BoardViewModel)e.NewValue;
    }

    private void OnPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState((CardView)sender, "Selected", useTransitions: false);
    }

    private void OnPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState((CardView)sender, "Normal", useTransitions: false);
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
        activeMove = new MoveViewModel(Board.ActivePlayer, ((CardView)sender).Card);
    }

    private void OnDropCompleted(UIElement sender, DropCompletedEventArgs args)
    {
        var cardView = (CardView)sender;
        if (args.DropResult == DataPackageOperation.Move)
        {
            // Update indices of cards comming after.
            for (int i = cardView.Card.HandIndex + 1; i < Board.ActiveHand.Count; ++i)
                Board.ActiveHand[i].HandIndex--;
            Board.ActiveHand.RemoveAt(cardView.Card.HandIndex);
            Board.IsLeftActive = !Board.IsLeftActive;
        }
        else
        {
            cardView.Opacity = 1;
        }
        activeMove = null;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        var hasCard = activeMove is not null;
        e.AcceptedOperation = hasCard ? DataPackageOperation.Move : DataPackageOperation.None;
        if (hasCard)
        {
            var move = activeMove!;
            var cell = ((CellView)sender).Cell;
            cell.Card = move.Card;
            cell.Player = move.Player;
            var neighbours = Board.GetCellNeighbours(cell);
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
        var cellView = (CellView)sender;
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
        VisualStateManager.GoToState((CellView)sender, "Normal", useTransitions: false);
    }
}
