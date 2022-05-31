using Microsoft.UI;
using System.Collections.ObjectModel;
using TripleTriad.Models;

namespace TripleTriad.ViewModels.Explicit;

public sealed class BoardViewModel : BaseViewModel
{
    public bool IsLeftActive { get => _isLeftActive; set => SetProperty(ref _isLeftActive, value).Then(OnIsLeftActiveChanged); }
    private bool _isLeftActive = true;

    public bool IsRightActive { get => !_isLeftActive; }

    public PlayerViewModel LeftPlayer { get; } = new PlayerViewModel
    {
        Model = new Player { Name = "Player 1" },
        Color = Colors.DarkGreen
    };

    public PlayerViewModel RightPlayer { get; } = new PlayerViewModel
    {
        Model = new Player { Name = "Player 2" },
        Color = Colors.DarkRed
    };

    public PlayerViewModel ActivePlayer { get => _isLeftActive ? LeftPlayer : RightPlayer; }

    public ObservableCollection<CardViewModel> LeftHand { get; } = new();

    public ObservableCollection<CardViewModel> RightHand { get; } = new();

    public ObservableCollection<CardViewModel> ActiveHand { get => _isLeftActive ? LeftHand : RightHand; }

    public List<CellViewModel> Cells { get; } = Enumerable.Range(0, 9).Select(i => new CellViewModel
    {
        Row = i / 3,
        Column = i % 3,
    }).ToList();

    private void OnIsLeftActiveChanged(bool obj)
    {
        NotifyPropertyChanged(nameof(IsRightActive));
        NotifyPropertyChanged(nameof(ActivePlayer));
        NotifyPropertyChanged(nameof(ActiveHand));
    }

    public NeighbourCells GetCellNeighbours(CellViewModel cell)
    {
        CellViewModel? left = null, up = null, right = null, down = null;
        if (cell.Column > 0)
            left = Cells[cell.Row * 3 + cell.Column - 1];
        if (cell.Row > 0)
            up = Cells[(cell.Row - 1) * 3 + cell.Column];
        if (cell.Column < 2)
            right = Cells[cell.Row * 3 + cell.Column + 1];
        if (cell.Row < 2)
            down = Cells[(cell.Row + 1) * 3 + cell.Column];
        return new NeighbourCells(left, up, right, down);
    }
}
