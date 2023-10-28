using AutoMapper;
using System.Reflection;
using TripleTriad.Interfaces;

namespace TripleTriad.Mapping;
internal sealed class MappingProfile : Profile
{
    public MappingProfile(params Assembly[] handlerAssemblyMarkerTypes)
    {
        foreach (var assembly in handlerAssemblyMarkerTypes)
            ApplyMappingsFromAssembly(assembly);
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        const string mapFromName = nameof(IMapFrom<object>.MapFrom);
        const string mapToName = nameof(IMapTo<object>.MapTo);

        var mapArgs = new object[] { this };
        foreach (var type in assembly.GetExportedTypes())
        {
            var instance = default(object);
            if (type.GetInterface("IMapFrom`1") is Type mapFromType)
            {
                instance ??= Activator.CreateInstance(type);
                var mapFromMethod = type.GetMethod(mapFromName) ?? mapFromType.GetMethod(mapFromName);
                mapFromMethod!.Invoke(instance, mapArgs);
            }
            if (type.GetInterface("IMapTo`1") is Type mapToType)
            {
                instance ??= Activator.CreateInstance(type);
                var mapToMethod = type.GetMethod(mapToName) ?? mapToType.GetMethod(mapToName);
                mapToMethod!.Invoke(instance, mapArgs);
            }
        }

        foreach (var type in new List<Type>())
        {
            var instance = Activator.CreateInstance(type);

            var methodInfo = type.GetMethod(nameof(IMapFrom<object>.MapFrom)) ??
                             type.GetInterface("IMapFrom`1")!.GetMethod(nameof(IMapFrom<object>.MapFrom));

            methodInfo?.Invoke(instance, new object[] { this });
        }
    }
}
