namespace TripleTriad.Exceptions;

public class NotFoundException : TripleTriadException
{
    public NotFoundException(string? message) : base(message)
    {
    }
}