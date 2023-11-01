using Microsoft.Extensions.DependencyInjection;

namespace TripleTriad.Infrastructure;
public static class ApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
