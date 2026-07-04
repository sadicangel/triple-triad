namespace TripleTriad.Contracts;

public enum Seat
{
    Red,
    Blue,
}

public static class SeatExtensions
{
    public static Seat Opponent(this Seat seat) => seat == Seat.Blue ? Seat.Red : Seat.Blue;
}
