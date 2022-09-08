namespace TripleTriad.Models;

public static class DirectionExtensions
{
    public static Direction Opposite(this Direction direction)
    {
        return direction switch
        {
            Direction.Left => Direction.Right,
            Direction.Up => Direction.Down,
            Direction.Right => Direction.Left,
            Direction.Down => Direction.Up,
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
    }
}