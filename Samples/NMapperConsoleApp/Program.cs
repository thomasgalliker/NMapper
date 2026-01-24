using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NMapper;
using NMapper.TestData;
using NMapper.TestData.Mappings;

namespace NMapperConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(o =>
            {
                o.ClearProviders();
                o.AddSimpleConsole();
                o.SetMinimumLevel(LogLevel.Debug);
            });

            services.AddMapping(o =>
            {
                o.Mappings.ScanAssembly(typeof(Person).Assembly);
                //o.Mappings.Add(new PersonMapping());
                //o.Mappings.Add(new PersonMapping(), new VenueMapping());
                //o.Mappings.Add(new IMapping[] { new PersonMapping(), new VenueMapping() });
                o.ServiceLifetime = ServiceLifetime.Singleton;
            });

            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                var person = new Person
                {
                    Id = 1,
                    Name = "John Doe",
                };
                var personDto = mapper.Map<PersonDto>(person);

                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.WriteLine();
                Console.WriteLine($"{ObjectDumper.Dump(person, DumpStyle.CSharp)}");

                Console.WriteLine();
                Console.WriteLine("↓ ↓ ↓ mapped to ↓ ↓ ↓");
                Console.WriteLine();

                Console.WriteLine($"{ObjectDumper.Dump(personDto, DumpStyle.CSharp)}");
                Console.WriteLine();
            }

            Console.ReadKey();
        }
    }
}
