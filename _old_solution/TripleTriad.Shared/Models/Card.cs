namespace TripleTriad.Models;

public sealed partial class Card
{
    public string ImageUri { get => $"Assets/E{Edition:D2}/{Id}.png"; }
}
