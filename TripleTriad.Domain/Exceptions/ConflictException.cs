namespace TripleTriad.Exceptions;

public sealed class ConflictException : TripleTriadException
{
    public ConflictException(string? message) : base(message)
    {
    }
}
