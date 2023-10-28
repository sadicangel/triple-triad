namespace TripleTriad.Exceptions;

public sealed class ForbiddenException : TripleTriadException
{
    public ForbiddenException(string? message) : base(message)
    {
    }
}