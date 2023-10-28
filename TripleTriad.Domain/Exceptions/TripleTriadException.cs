namespace TripleTriad.Exceptions;

public abstract class TripleTriadException : Exception
{
    protected TripleTriadException(string? message) : base(message)
    {

    }
}