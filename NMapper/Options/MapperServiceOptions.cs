using Microsoft.Extensions.DependencyInjection;

namespace NMapper
{
    public class MapperServiceOptions : MapperOptions
    {
        public MapperServiceOptions()
        {
            this.Mappings = new MappingOptionsMappingCollection();
            this.ServiceLifetime = ServiceLifetime.Singleton;
        }


        /// <summary>
        /// Configures the mappings to be used in <see cref="IMapper"/> when a new instance is created.
        /// </summary>
        public new MappingOptionsMappingCollection Mappings { get; set; }

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
}
