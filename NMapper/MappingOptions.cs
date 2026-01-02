using System.Reflection;
using NMapper.Extensions;

namespace NMapper
{
    public class MappingOptions
    {
        public MappingOptions()
        {
            this.MappingAssemblies = Array.Empty<Assembly>();
            this.Mappings = new MappingOptionsMappingCollection();
        }

        public Assembly[] MappingAssemblies { get; set; }

        public Assembly? MappingAssembly { get; set; }

        public MappingOptionsMappingCollection Mappings { get; }
    }

    public class MappingOptionsMappingCollection
    {
        internal List<IMapping> Mappings { get; } = new List<IMapping>();

        internal List<Type> MappingTypes { get; } = new List<Type>();

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
