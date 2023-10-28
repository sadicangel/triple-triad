namespace TripleTriad.Games;
public enum Side
{
    Left, Right
}
public static class SideExtensions
{
    public static Side Toggle(this Side side)
    {
        return side switch
        {
            Side.Left => Side.Right,
            Side.Right => Side.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(side)),
        };
    }
}