namespace TripleTriad.ViewModels.Explicit;

public readonly struct SetPropertyResult<T>
{
    public bool Changed { get; init; }
    public T Value { get; init; }
}
