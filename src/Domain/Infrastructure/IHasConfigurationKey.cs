namespace TripleTriad.Infrastructure;
public interface IHasConfigurationKey
{
    static abstract string ConfigurationSectionKey { get; }
}
