using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TripleTriad.Game;
using Element = TripleTriad.Game.Element;

namespace TripleTriad.ViewModels;

public sealed class CardFlippedEventArgs : EventArgs
{
    private readonly TaskCompletionSource _taskCompletionSource = new();

    public Axis Axis { get; init; }

    public Task Animation { get => _taskCompletionSource.Task; }

    public void NotifyCompletion() => _taskCompletionSource.SetResult();
}

public sealed partial class CardViewModel : ObservableObject
{
    private static readonly IReadOnlyDictionary<int, string> NumberImageSources =
        Enumerable.Range(1, 10).ToDictionary(k => k, k => $"number_{k:x1}.png");

    private static readonly IReadOnlyDictionary<Element, string> ElementImageSources =
        Enum.GetValues<Element>().ToDictionary(k => k, k => $"element_{k.ToString().ToLowerInvariant()}.png");

    private bool _isFlipping;

    [ObservableProperty]
    private Color _borderColor = Colors.Black;

    [ObservableProperty]
    private Color _backgroundColor = Colors.Wheat;

    public required Card Card { get; init; }

    public string ImageUri { get => Card?.Image ?? "card_missing.png"; }
    public string LeftUri { get => NumberImageSources[Card?.Left ?? 0]; }
    public string UpUri { get => NumberImageSources[Card?.Up ?? 0]; }
    public string RightUri { get => NumberImageSources[Card?.Right ?? 0]; }
    public string DownUri { get => NumberImageSources[Card?.Down ?? 0]; }
    public string ElementUri { get => ElementImageSources[Card?.Element ?? Element.None]; }

    public event EventHandler<CardFlippedEventArgs>? Flipped;

    [RelayCommand]
    private async Task OnTapped()
    {
        if (_isFlipping) return;
        _isFlipping = true;
        var args = new CardFlippedEventArgs
        {
            Axis = (Axis)Random.Shared.Next(2),
        };
        Flipped?.Invoke(this, args);
        await args.Animation;
        _isFlipping = false;
    }
}
