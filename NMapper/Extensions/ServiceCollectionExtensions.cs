using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NMapper
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Type MappingMarkerType = typeof(IMapping);
        private static readonly Type MappingGenericType = typeof(IMapping<,>);
        private static readonly Type MappingWithContextGenericType = typeof(IMappingWithContext<,>);

        /// <summary>
        /// Registers mapping service <see cref="IMapper"/> in the dependency injection container.
        /// All mappings found in the specified assemblies (see <see cref="MappingOptions.MappingAssemblies"/>) will be used in the mapper.
        /// </summary>
        public static IServiceCollection AddMapping(this IServiceCollection services, Action<MappingOptions>? options = null)
        {
            var mappingOptions = new MappingOptions();
            options?.Invoke(mappingOptions);

            services.AddScoped<IMapper, Mapper>();

            var registerGenericMappingTypes = mappingOptions.RegisterGenericMappingTypes;

            foreach (var assembly in mappingOptions.MappingAssemblies)
            {
                services.AddMappers(assembly, registerGenericMappingTypes);
            }

            if (mappingOptions.MappingAssembly != null)
            {
                services.AddMappers(mappingOptions.MappingAssembly, registerGenericMappingTypes);
            }

            if (mappingOptions.Mappings != null)
            {
                var mappingTypes = mappingOptions.Mappings.Select(m => m.GetType()).ToArray();
                services.AddMappers(mappingTypes, registerGenericMappingTypes);
            }

            if (mappingOptions.MappingTypes != null)
            {
                var mappingTypes = mappingOptions.MappingTypes;
                services.AddMappers(mappingTypes, registerGenericMappingTypes);
            }

            return services;
        }

        public static IServiceCollection AddMappers(this IServiceCollection services, Assembly assembly, bool registerGenericMappingTypes)
        {
            var mappingTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                              i.GetGenericTypeDefinition() is Type t && (t == MappingGenericType || t == MappingWithContextGenericType)));

            services.AddMappers(mappingTypes, registerGenericMappingTypes);

            return services;
        }

        private static void AddMappers(this IServiceCollection services, IEnumerable<Type> mappingTypes, bool registerGenericMappingTypes)
        {
            foreach (var mappingType in mappingTypes)
            {
                if (registerGenericMappingTypes)
                {
                    var mappingInterfaces = mappingType.GetInterfaces()
                        .Where(i => i.IsGenericType &&
                                    i.GetGenericTypeDefinition() is Type t && (t == MappingGenericType || t == MappingWithContextGenericType))
                        .ToArray();

                    foreach (var mappingInterface in mappingInterfaces)
                    {
                        services.AddScoped(mappingInterface, mappingType);
                    }
                }


                services.AddScoped(MappingMarkerType, mappingType);
            }
        }
    }
}