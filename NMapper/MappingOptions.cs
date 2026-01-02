using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NMapper.Extensions;

namespace NMapper
{
    public class MappingOptions
    {
        public MappingOptions()
        {
            this.Mappings = new MappingOptionsMappingCollection();
            this.ServiceLifetime = ServiceLifetime.Singleton;
        }

        public MappingOptionsMappingCollection Mappings { get; }

        /// <summary>
        /// Configures the service lifetime for the registered <see cref="IMapper"/> and <see cref="IMapping"/> registrations.
        /// Default: <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        /// <remarks>
        /// Be cautious when using different service lifetimes.
        /// There are strict rules regarding service lifetimes of chained dependencies in Microsoft.Extensions.DependencyInjection.
        /// </remarks>
        public ServiceLifetime ServiceLifetime { get; set; }
    }

    public class MappingOptionsMappingCollection
    {
        internal List<Assembly> MappingAssemblies { get; } = new List<Assembly>();

        internal List<IMapping> Mappings { get; } = new List<IMapping>();

        internal List<Type> MappingTypes { get; } = new List<Type>();

        public void ScanAssembly(params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            this.MappingAssemblies.AddRange(assemblies);
        }

        public void Add(IEnumerable<IMapping> mappings)
        {
            if (mappings == null)
            {
                throw new ArgumentNullException(nameof(mappings));
            }

            this.Mappings.AddRange(mappings);
        }

        public void Add(params IMapping[] mappings)
        {
            if (mappings == null)
            {
                throw new ArgumentNullException(nameof(mappings));
            }

            this.Mappings.AddRange(mappings);
        }

        public void Add<TSource, TTarget>(Func<TSource, TTarget> mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException(nameof(mapping));
            }

            this.Mappings.Add(new DelegateMapping<TSource, TTarget>(mapping));
        }

        public void Add(params Type[] mappingTypes)
        {
            if (mappingTypes == null)
            {
                throw new ArgumentNullException(nameof(mappingTypes));
            }

            foreach (var mappingType in mappingTypes)
            {
                if (!typeof(IMapping).IsAssignableFrom(mappingType))
                {
                    throw new ArgumentException($"Type '{mappingType.GetFormattedName()}' does not implement '{typeof(IMapping).GetFormattedName()}'.", nameof(mappingType));
                }

                this.MappingTypes.Add(mappingType);
            }
        }
    }
}
