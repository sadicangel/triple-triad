namespace TripleTriad.ViewModels.Explicit;

public static class SetPropertyResultExtensions
{
    public static SetPropertyResult<T> Then<T>(this SetPropertyResult<T> result, Action<T> then)
    {
        if (result.Changed)
            then.Invoke(result.Value);
        return result;
    }

    public static SetPropertyResult<T> Else<T>(this SetPropertyResult<T> result, Action<T> @else)
    {
        if (!result.Changed)
            @else.Invoke(result.Value);
        return result;
    }
}
