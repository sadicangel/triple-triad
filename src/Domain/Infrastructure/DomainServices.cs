using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using TripleTriad.Services;

namespace TripleTriad.Infrastructure;

public static class DomainServices
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddValidatedOptions<ServerOptions>();
        services.AddValidatedOptions<PgSqlOptions>();
        services.AddSingleton<UuidProvider>();
        services.AddSingleton(provider => new NpgsqlDataSourceBuilder(provider.GetRequiredService<IOptions<PgSqlOptions>>().Value.ConnectionString).Build());
        services.AddDbContextFactory<DataContext>((provider, opts) => opts.UseNpgsql(provider.GetRequiredService<IOptions<PgSqlOptions>>().Value.ConnectionString));
        //services.AddMarten(provider =>
        //{
        //    var options = new StoreOptions
        //    {
        //        DatabaseSchemaName = "public",
        //        AutoCreateSchemaObjects = AutoCreate.All,
        //    };

        //    options.Connection(provider.GetRequiredService<IOptions<PostgresOptions>>().Value.ConnectionString);

        //    options.UseDefaultSerialization(
        //        serializerType: SerializerType.SystemTextJson,
        //        enumStorage: EnumStorage.AsString);

        //    options.Projections.Add<ArtistProjection>(ProjectionLifecycle.Inline);
        //    options.Schema.For<Artist>().Identity(a => a.ArtistId).UseOptimisticConcurrency(true);

        //    options.Projections.Add<AlbumProjection>(ProjectionLifecycle.Inline);
        //    options.Schema.For<Album>().Identity(a => a.AlbumId).UseOptimisticConcurrency(true);

        //    options.Projections.Add<TrackProjection>(ProjectionLifecycle.Inline);
        //    options.Schema.For<Track>().Identity(a => a.TrackId).UseOptimisticConcurrency(true);

        //    return options;
        //})
        //    .UseLightweightSessions();
        //services.AddValidatorsFromAssembly(typeof(DomainServices).Assembly, ServiceLifetime.Singleton);

        return services;
    }

    public static IServiceCollection AddValidatedOptions<T>(this IServiceCollection services) where T : class, IHasConfigurationKey
    {
        services.AddOptions<T>()
            .BindConfiguration(T.ConfigurationSectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}