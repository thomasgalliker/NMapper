using System.Reflection;

namespace ObjectMapper.Extensions
{
    public class MappingOptions
    {
        public MappingOptions()
        {
            this.MappingAssemblies = Array.Empty<Assembly>();
            this.Mappings = Array.Empty<IMapping>();
            this.MappingTypes = Array.Empty<Type>();
        }

        public Assembly[] MappingAssemblies { get; set; }

        public Assembly? MappingAssembly { get; set; }

        public IMapping[] Mappings { get; set; }

        public Type[] MappingTypes { get; set; }

        public bool RegisterGenericMappingTypes { get; set; }
    }
}
