using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TripleTriad.Models;
using TripleTriad.Repositories;

namespace TripleTriad;

public sealed class LiteDbOptions
{
    public ConnectionString ConnectionString { get; set; } = new ConnectionString();
    public BsonMapper BsonMapper { get; set; } = BsonMapper.Global;
}

public static class DependecyInjectionExtensions
{
    public static IServiceCollection AddLiteDB(this IServiceCollection services, Action<LiteDbOptions>? options = null)
    {
        var optionsObj = new LiteDbOptions();
        options?.Invoke(optionsObj);
        services.TryAddSingleton(new LiteDatabase(optionsObj.ConnectionString, optionsObj.BsonMapper));
        return services;
    }

    public static IServiceCollection AddCardRepository(this IServiceCollection services)
    {
        services.AddLiteDB(opts =>
        {
            opts.ConnectionString.Filename = Path.Combine(Path.GetDirectoryName(typeof(Card).Assembly.Location)!, "Assets", "Cards.db");
            opts.BsonMapper = opts.BsonMapper.UseCamelCase();
        });
        services.TryAddScoped<ICardRepository, CardRepository>();
        return services;
    }
}