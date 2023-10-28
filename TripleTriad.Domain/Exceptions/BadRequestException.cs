namespace TripleTriad.Exceptions;

public class BadRequestException : TripleTriadException
{
    public BadRequestException(string? message) : base(message)
    {
    }
}