using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Extensions.Configuration.ConfigServer;

namespace BreakingNewsWeb
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
            builder = builder.AddConfigServer();

            return CloudFoundryHostBuilderExtensions.UseCloudFoundryHosting(builder)
                .AddCloudFoundry()
                .UseStartup<Startup>()
                .Build();
        }
    }
}