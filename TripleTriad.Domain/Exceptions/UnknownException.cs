namespace TripleTriad.Exceptions;

public sealed class UnknownException : TripleTriadException
{
    public UnknownException(string? message) : base(message)
    {

    }
}
