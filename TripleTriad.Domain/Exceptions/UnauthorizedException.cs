namespace TripleTriad.Exceptions;

public sealed class UnauthorizedException : TripleTriadException
{
    public UnauthorizedException(string? message) : base(message)
    {
    }
}
