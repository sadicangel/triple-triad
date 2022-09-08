using Microsoft.UI;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using TripleTriad.Models;
using TripleTriad.Pages;
using TripleTriad.Repositories;
using TripleTriad.ViewModels.Explicit;

namespace TripleTriad.ViewModels;

public sealed class BoardViewModel : BaseViewModel<Board, BoardPage>
{
    private bool _isStarted;
    private readonly ICardRepository _repository;
    private RulesetViewModel _ruleset = new()
    {
        MatchRules = MatchRules.Open | MatchRules.Random,
        BoardRules = BoardRules.Plus | BoardRules.Same | BoardRules.Combo,
        TradeRules = TradeRules.One
    };
    private PlayerViewModel _leftPlayer = new() { Name = "Player 1", Color = Colors.DarkGreen };
    private PlayerViewModel _rightPlayer = new() { Name = "Player 2", Color = Colors.DarkRed };
    private ObservableCollection<CardViewModel> _leftHand = new();
    private ObservableCollection<CardViewModel> _rightHand = new();
    private ObservableCollection<CellViewModel> _cells = new();
    private PlayerViewModel? _winner = new();

    public bool IsStarted { get => _isStarted; set => SetProperty(ref _isStarted, value); }

    public bool IsLeftActive { get => Model.IsLeftActive; set => SetProperty(m => m.IsLeftActive, (m, v) => m.IsLeftActive = v, value, OnIsLeftActiveChanged); }

    public bool IsRightActive { get => !Model.IsLeftActive; }

    public RulesetViewModel Ruleset { get => _ruleset; set => SetPropertyNotNull(ref _ruleset, value, OnRulesetChanged); }

    public PlayerViewModel LeftPlayer { get => _leftPlayer; set => SetPropertyNotNull(ref _leftPlayer, value, OnLeftPlayerChanged); }

    public PlayerViewModel RightPlayer { get => _rightPlayer; set => SetPropertyNotNull(ref _rightPlayer, value, OnRightPlayerChanged); }

    public PlayerViewModel ActivePlayer { get => Model.IsLeftActive ? LeftPlayer : RightPlayer; }

    public ObservableCollection<CardViewModel> LeftHand { get => _leftHand; set => SetPropertyNotNull(ref _leftHand, value, OnLeftHandChanged); }

    public ObservableCollection<CardViewModel> RightHand { get => _rightHand; set => SetPropertyNotNull(ref _rightHand, value, OnRightHandChanged); }

    public ObservableCollection<CardViewModel> ActiveHand { get => Model.IsLeftActive ? LeftHand : RightHand; }

    public ObservableCollection<CellViewModel> Cells { get => _cells; set => SetPropertyNotNull(ref _cells, value, OnCellsChanged); }

    [MemberNotNullWhen(true, nameof(Winner))]
    public bool IsGameOver { get => Model.IsGameOver; set => SetProperty(m => m.IsGameOver, (m, v) => m.IsGameOver = v, value, OnIsGameOverChanged); }

    public PlayerViewModel? Winner { get => _winner; set => SetPropertyNotNull(ref _winner, value, OnWinnerChanged); }

    public BoardViewModel(ICardRepository repository)
    {
        _repository = repository;
        LeftHand.CollectionChanged += LeftHand_CollectionChanged;
        RightHand.CollectionChanged += RightHand_CollectionChanged;
        ResetBoard();
    }

    private void LeftHand_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CardViewModel card in e.OldItems)
                Model.LeftHand.Remove(card.Model);

        if (e.NewItems is not null)
            foreach (CardViewModel card in e.NewItems)
                Model.LeftHand.Add(card.Model);
    }

    private void RightHand_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CardViewModel card in e.OldItems)
                Model.RightHand.Remove(card.Model);

        if (e.NewItems is not null)
            foreach (CardViewModel card in e.NewItems)
                Model.RightHand.Add(card.Model);
    }

    private void OnIsLeftActiveChanged(bool isLeftActive)
    {
        OnPropertyChanged(nameof(IsRightActive));
        OnPropertyChanged(nameof(ActivePlayer));
        OnPropertyChanged(nameof(ActiveHand));
    }

    private void OnRulesetChanged(RulesetViewModel ruleset)
    {
        Model.Ruleset = ruleset.Model;
    }

    private void OnLeftPlayerChanged(PlayerViewModel leftPlayer)
    {
        Model.LeftPlayer = leftPlayer.Model;
    }

    private void OnRightPlayerChanged(PlayerViewModel rightPlayer)
    {
        Model.RightPlayer = rightPlayer.Model;
    }

    private void OnLeftHandChanged(ObservableCollection<CardViewModel> leftHand)
    {
        Model.LeftHand.Clear();
        Model.LeftHand.AddRange(leftHand.Select(card => card.Model));
    }

    private void OnRightHandChanged(ObservableCollection<CardViewModel> rightHand)
    {
        Model.RightHand.Clear();
        Model.RightHand.AddRange(rightHand.Select(card => card.Model));
    }

    private void OnCellsChanged(ObservableCollection<CellViewModel> cells)
    {
        Model.Cells.Clear();
        Model.Cells.AddRange(cells.Select(cell => cell.Model));
    }

    private void OnIsGameOverChanged(bool isGameOver)
    {
        if(isGameOver)
            Winner = Cells.GroupBy(cell => cell.Player).MaxBy(g => g.Count())!.First().Player!;
    }

    private void OnWinnerChanged(PlayerViewModel? player)
    {
        Model.Winner = player?.Model;
    }

    public void ResetBoard()
    {
        Cells = new(Enumerable.Range(0, 9).Select(i => new CellViewModel { Index = i }));
        var list = new List<CardViewModel>(capacity: 10);
        for (int i = 0; i < 10; ++i)
        {
            var random = Random.Shared.Next(110);
            var id = $"CardE01T{random / 11 + 1:D2}N{random + 1:D3}V1";
            var card = _repository.FindById(id);
            list.Add(new CardViewModel
            {
                Model = card!,
                Color = i < 5 ? LeftPlayer.Color : RightPlayer.Color,
                HandIndex = i % 5
            });
        }
        LeftHand = new ObservableCollection<CardViewModel>(list.GetRange(0, 5));
        RightHand = new ObservableCollection<CardViewModel>(list.GetRange(5, 5));
        IsLeftActive = Random.Shared.Next(2) == 0;
    }

    private static async Task FlipAffectedCards(AffectedCells affectedCells, PlayerViewModel player)
    {
        await Task.WhenAll(affectedCells.CellsUnowned().Select(async item =>
        {
            var (direction, affectedCell) = item;
            affectedCell.Player = player;
            await Task.WhenAll(
                affectedCell.Card!.ColorAsync(player.Color),
                affectedCell.Card!.FlipAsync(direction));
        }));
    }

    public Task ShowRuleAsync(BoardRules rule, IEnumerable<DirectedCell> cells) => View.ShowRuleAsync(rule, cells) ?? Task.CompletedTask;

    public async Task ExecuteMove(CellViewModel cell, MoveViewModel move)
    {
        cell.Card = move.Card;
        cell.Player = move.Player;
        // Update indices of cards comming after.
        for (int i = cell.Card.HandIndex + 1; i < ActiveHand.Count; ++i)
            ActiveHand[i].HandIndex--;
        ActiveHand.RemoveAt(cell.Card.HandIndex);

        var adjacentCells = GetAdjacentCells(cell);

        if (Ruleset.HasRule(BoardRules.Same) && TestSame(cell, adjacentCells, Ruleset.HasRule(BoardRules.SameWall), out AffectedCells? affectedCells))
        {
            await ShowRuleAsync(BoardRules.Same, affectedCells.Cells());
            await FlipAffectedCards(affectedCells, move.Player);
            if (Ruleset.HasRule(BoardRules.Combo))
                await ExecuteCombo(affectedCells, move.Player);
        }
        else if (Ruleset.HasRule(BoardRules.Plus) && TestPlus(cell, adjacentCells, out affectedCells))
        {
            await ShowRuleAsync(BoardRules.Plus, affectedCells.Cells());
            await FlipAffectedCards(affectedCells, move.Player);
            if (Ruleset.HasRule(BoardRules.Combo))
                await ExecuteCombo(affectedCells, move.Player);
        }
        else if (TestBeat(cell, adjacentCells, out affectedCells))
        {
            await FlipAffectedCards(affectedCells, move.Player);
        }
        IsGameOver = Cells.All(cell => cell.HasCard);

    }

    private async Task ExecuteCombo(AffectedCells affectedCells, PlayerViewModel player)
    {
        if (affectedCells.CellsUnowned().Any())
        {
            await ShowRuleAsync(BoardRules.Combo, affectedCells.CellsUnowned());
            foreach (var (_, cell) in affectedCells.CellsUnowned())
            {
                var adjacentCells = GetAdjacentCells(cell);
                if (TestBeat(cell, adjacentCells, out AffectedCells? newAffectedCells))
                    await FlipAffectedCards(newAffectedCells, player);
            }
        }
    }

    private AdjacentCells GetAdjacentCells(CellViewModel cell)
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
        return new AdjacentCells
        {
            Left = left,
            Up = up,
            Right = right,
            Down = down
        };
    }

    private static bool TestSame(CellViewModel x, AdjacentCells adjacentCells, bool useWall, [NotNullWhen(true)] out AffectedCells? affectedCells)
    {
        if (x is null || x.Card is null)
            throw new InvalidOperationException("Invalid cell");

        affectedCells = new AffectedCells(x);
        var sames = 0;

        // LEFT
        var value = useWall ? 10 : 0;
        if (x.Column > 0)
            value = adjacentCells.Left.ValueOrZero(Direction.Right);
        if (x.Card.Left == value)
        {
            ++sames;
            affectedCells.Left = adjacentCells.Left;
        }

        // UP
        value = useWall ? 10 : 0;
        if (x.Row > 0)
            value = adjacentCells.Up.ValueOrZero(Direction.Down);
        if (x.Card.Up == value)
        {
            ++sames;
            affectedCells.Up = adjacentCells.Up;
            if (sames >= 2 && affectedCells.CountUnowned >= 1)
                return true;
        }

        // RIGHT
        value = useWall ? 10 : 0;
        if (x.Column < 2)
            value = adjacentCells.Right.ValueOrZero(Direction.Left);
        if (x.Card.Right == value)
        {
            ++sames;
            affectedCells.Right = adjacentCells.Right;
            if (sames == 2 && affectedCells.CountUnowned >= 1)
                return true;
        }

        // DOWN
        value = useWall ? 10 : 0;
        if (x.Row < 2)
            value = adjacentCells.Down.ValueOrZero(Direction.Up);
        if (x.Card.Down == value)
        {
            ++sames;
            affectedCells.Down = adjacentCells.Down;
            if (sames == 2 && affectedCells.CountUnowned >= 1)
                return true;
        }

        // No matches.
        affectedCells = null;
        return false;
    }

    private static bool TestPlus(CellViewModel x, AdjacentCells adjacentCells, [NotNullWhen(true)] out AffectedCells? affectedCells)
    {
        if (x is null || x.Card is null)
            throw new InvalidOperationException("Invalid cell");

        var leftValue = adjacentCells.Left.ValueOrZero(Direction.Right);
        if (leftValue > 0)
            leftValue += x.Card.Left;
        var upValue = adjacentCells.Up.ValueOrZero(Direction.Down);
        if (upValue > 0)
            upValue += x.Card.Up;
        // LEFT & UP
        if (leftValue != 0 && leftValue == upValue)
        {
            affectedCells = new AffectedCells(x)
            {
                Left = adjacentCells.Left,
                Up = adjacentCells.Up
            };
            if (affectedCells.CountUnowned > 0)
                return true;
        }

        var rightValue = adjacentCells.Right.ValueOrZero(Direction.Left);
        if (rightValue > 0)
            rightValue += x.Card.Right;
        // LEFT & RIGHT
        if (leftValue != 0 && leftValue == rightValue)
        {
            affectedCells = new AffectedCells(x)
            {
                Left = adjacentCells.Left,
                Right = adjacentCells.Right
            };
            if (affectedCells.CountUnowned > 0)
                return true;
        }
        // UP & RIGHT
        if (upValue != 0 && upValue == rightValue)
        {
            affectedCells = new AffectedCells(x)
            {
                Up = adjacentCells.Up,
                Right = adjacentCells.Right
            };
            if (affectedCells.CountUnowned > 0)
                return true;
            return true;
        }

        var downValue = adjacentCells.Down.ValueOrZero(Direction.Up);
        if (downValue > 0)
            downValue += x.Card.Down;
        // LEFT & DOWN
        if (leftValue != 0 && leftValue == downValue)
        {
            affectedCells = new AffectedCells(x)
            {
                Left = adjacentCells.Left,
                Down = adjacentCells.Down
            };
            if (affectedCells.CountUnowned > 0)
                return true;
        }
        // UP & DOWN
        if (upValue != 0 && upValue == downValue)
        {
            affectedCells = new AffectedCells(x)
            {
                Up = adjacentCells.Up,
                Down = adjacentCells.Down
            };
            if (affectedCells.CountUnowned > 0)
                return true;
        }
        // RIGHT & DOWN
        if (rightValue != 0 && rightValue == downValue)
        {
            affectedCells = new AffectedCells(x)
            {
                Right = adjacentCells.Right,
                Down = adjacentCells.Down
            };
            if (affectedCells.CountUnowned > 0)
                return true;
        }

        // No matches
        affectedCells = null;
        return false;
    }

    private static bool TestBeat(CellViewModel x, AdjacentCells adjacentCells, [NotNullWhen(true)] out AffectedCells? affectedCells)
    {
        if (x is null || x.Card is null)
            throw new InvalidOperationException("Invalid cell");

        var count = 0;
        affectedCells = new AffectedCells(x);
        // LEFT
        var value = adjacentCells.Left.ValueOrZeroWithElement(Direction.Right);
        if (value != 0 && x.ValueOrZeroWithElement(Direction.Left) > value)
        {
            affectedCells.Left = adjacentCells.Left;
            ++count;
        }
        // UP
        value = adjacentCells.Up.ValueOrZeroWithElement(Direction.Down);
        if (value != 0 && x.ValueOrZeroWithElement(Direction.Up) > value)
        {
            affectedCells.Up = adjacentCells.Up;
            ++count;
        }
        // RIGHT
        value = adjacentCells.Right.ValueOrZeroWithElement(Direction.Left);
        if (value != 0 && x.ValueOrZeroWithElement(Direction.Right) > value)
        {
            affectedCells.Right = adjacentCells.Right;
            ++count;
        }
        // DOWN
        value = adjacentCells.Down.ValueOrZeroWithElement(Direction.Up);
        if (value != 0 && x.ValueOrZeroWithElement(Direction.Down) > value)
        {
            affectedCells.Down = adjacentCells.Down;
            ++count;
        }

        // No matches?
        if (count == 0)
        {
            affectedCells = null;
            return false;
        }
        return true;
    }
}
