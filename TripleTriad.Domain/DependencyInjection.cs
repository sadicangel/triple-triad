using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using TripleTriad.Behaviors;
using TripleTriad.Interfaces;
using TripleTriad.Mapping;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services, IConfiguration configuration)
    {
        var domainAsm = typeof(IEntity<>).Assembly;
        var callingAsm = Assembly.GetCallingAssembly();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddMediatR(domainAsm, callingAsm);

        services.AddAutoMapper(config => config.AddProfile(new MappingProfile(domainAsm, callingAsm)));

        services.AddValidatorsFromAssembly(domainAsm);
        services.AddValidatorsFromAssembly(callingAsm);
        return services;
    }
}
