using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NMapper;

namespace NMapperConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(o =>
            {
                o.ClearProviders();
                o.AddSimpleConsole();
                o.SetMinimumLevel(LogLevel.Debug);
            });

            serviceCollection.AddMapping(o =>
            {
                o.Mappings.ScanAssembly(typeof(Program).Assembly);
                o.ServiceLifetime = ServiceLifetime.Transient;
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();

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
