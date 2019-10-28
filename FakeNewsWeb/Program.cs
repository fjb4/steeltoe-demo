using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Configuration.ConfigServer;

namespace FakeNewsWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            // retrieve configuration from Spring Cloud Config Server
            builder = builder.AddConfigServer(GetLoggerFactory());

            // use the "PORT" env variable to set the port to listen on
            builder = CloudFoundryHostBuilderExtensions.UseCloudFoundryHosting(builder);

            // add VCAP_* configuration data
            builder = CloudFoundryHostBuilderExtensions.AddCloudFoundry(builder);

            return builder.UseStartup<Startup>().Build();
        }

        private static ILoggerFactory GetLoggerFactory()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));
            serviceCollection.AddLogging(builder => builder.AddConsole((opts) => { opts.DisableColors = true; }));
            serviceCollection.AddLogging(builder => builder.AddDebug());
            return serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
        }
    }
}