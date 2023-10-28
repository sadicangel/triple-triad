using AspNetCore.Identity.Mongo.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TripleTriad.Caching;
using TripleTriad.Games;
using TripleTriad.Interfaces;
using TripleTriad.Repositories;
using TripleTriad.Services;
using TripleTriad.Users;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInMemoryRepositories();
        services.AddMongoRepositories(configuration);
        services.AddCaches(configuration);

        services.AddIdentity(configuration);

        return services;
    }

    private static IServiceCollection AddInMemoryRepositories(this IServiceCollection services)
    {
        services.AddSingleton<ICardRepository, CardRepository>(services =>
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Converters = { new JsonStringEnumConverter() }
            };
            var type = typeof(TripleTriad.Editions.IEditionAssemblyTypeMarker);
            var files = type.Assembly.GetManifestResourceNames();
            var cards = new List<Card>();
            foreach (var file in files)
            {
                using var stream = type.Assembly.GetManifestResourceStream(file)!;
                cards.AddRange(JsonSerializer.Deserialize<List<Card>>(stream, options)!);
            }
            return new CardRepository(cards);
        });
        return services;
    }

    private static IServiceCollection AddMongoRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        ConventionRegistry.Register("FolkLibrary", new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreIfDefaultConvention(ignoreIfDefault: true),
            new EnumRepresentationConvention(BsonType.String),
            new GuidAsStringRepresentationConvention()
        },
        type => true);
        services.AddSingleton<IMongoClient>(new MongoClient(configuration.GetConnectionString("Mongo")));
        services.AddSingleton<IMongoDatabase>(provider => provider.GetRequiredService<IMongoClient>().GetDatabase("tripletriad"));
        services.AddSingleton<ILobbyRepository, LobbyRepository>();
        services.AddSingleton<IGameRepository, GameRepository>();
        services.AddSingleton<IdentityErrorDescriber>();
        services.AddSingleton<IMongoCollection<User>>(provider => provider.GetRequiredService<IMongoDatabase>().GetCollection<User>("users"));
        services.AddSingleton<IMongoCollection<Role>>(provider => provider.GetRequiredService<IMongoDatabase>().GetCollection<Role>("roles"));
        return services;
    }

    private static IServiceCollection AddCaches(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(opts => opts.Configuration = configuration.GetConnectionString("Redis"));
        services.AddSingleton<IUserLobbyCache, UserLobbyCache>();
        return services;
    }

    private static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<SecurityKey>(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetRequiredSection("SecurityKey").Value!)));
        services.AddIdentityCore<User>()
            .AddRoles<Role>()
            .AddTokenProvider<JwtTokenProvider>("Default")
            .AddRoleStore<RoleStore<Role, string>>()
            .AddUserStore<UserStore<User, Role, string>>()
            .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<User>>();
        return services;
    }
}

file sealed class GuidAsStringRepresentationConvention : ConventionBase, IMemberMapConvention
{
    public void Apply(BsonMemberMap memberMap)
    {
        if (memberMap.MemberType == typeof(Guid))
            memberMap.SetSerializer(new GuidSerializer(BsonType.String));
        else if (memberMap.MemberType == typeof(Guid?))
            memberMap.SetSerializer(new NullableSerializer<Guid>(new GuidSerializer(BsonType.String)));
    }
}
