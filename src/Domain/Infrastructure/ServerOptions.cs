using System.ComponentModel.DataAnnotations;

namespace TripleTriad.Infrastructure;

public sealed record class ServerOptions([property: Required] string Url) : IHasConfigurationKey
{
    public static string ConfigurationSectionKey { get => "Server"; }

    public ServerOptions() : this("https://localhost:7227") { }
}
