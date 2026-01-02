using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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

            var serviceLifetime = mappingOptions.ServiceLifetime;

            foreach (var assembly in mappingOptions.Mappings.MappingAssemblies)
            {
                services.AddMappers(assembly, serviceLifetime);
            }

            if (mappingOptions.Mappings.MappingTypes.Any())
            {
                var mappingTypes = mappingOptions.Mappings.MappingTypes;
                services.AddMappings(mappingTypes, serviceLifetime);
            }

            services.Add(new ServiceDescriptor(typeof(IMapper), s =>
            {
                var logger = s.GetService<ILogger<Mapper>>() ?? new NullLogger<Mapper>();
                var registeredMappings = s.GetServices<IMapping>() ?? Array.Empty<IMapping>();
                var mappings = mappingOptions.Mappings.Mappings.Union(registeredMappings);
                return new Mapper(logger, mappings);
            }, serviceLifetime));

            return services;
        }

        public static IServiceCollection AddMappers(this IServiceCollection services, Assembly assembly, ServiceLifetime serviceLifetime, bool registerGenericMappingTypes = false)
        {
            var mappingTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                              i.GetGenericTypeDefinition() is Type t && (t == MappingGenericType || t == MappingWithContextGenericType)));

            services.AddMappings(mappingTypes, serviceLifetime, registerGenericMappingTypes);

            return services;
        }

        private static void AddMappings(this IServiceCollection services, IEnumerable<Type> mappingTypes, ServiceLifetime serviceLifetime, bool registerGenericMappingTypes = false)
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
                        services.Add(new ServiceDescriptor(mappingInterface, mappingType, serviceLifetime));
                    }
                }

                services.Add(new ServiceDescriptor(MappingMarkerType, mappingType, serviceLifetime));
            }
        }
    }
}