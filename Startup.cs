using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Discovery.Client;

namespace SteeltoeDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        private IConfiguration Configuration { get; set; }
        private ILogger<Startup> Logger { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Steeltoe Discovery Client service
            services.AddDiscoveryClient(Configuration);

            // Add Hystrix command GetUpstreamContent to Hystrix group "GetUpstreamContent"
            services.AddHystrixCommand<GetUpstreamContentCommand>("GetUpstreamContent", Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                var applicationName = Configuration["spring:application:name"];

                var upstreamHost = Configuration["upstreamHost"];

                if (string.IsNullOrWhiteSpace(upstreamHost))
                {
                    upstreamHost = "date.jsontest.com";
                }

                var command = app.ApplicationServices.GetService<GetUpstreamContentCommand>();
                var upstreamContent = await command.ExecuteAsync(upstreamHost);

                var message = $"<p>{applicationName}{GetAppInstanceIndex()} => {upstreamHost}</p>";
                message += upstreamContent;

                context.Response.Headers.Add("Content-Type", "text/html");
                context.Response.Headers.Add("Cache-Control", "no-cache");
                await context.Response.WriteAsync(message);
            });

            app.UseDiscoveryClient();
        }

        private static string GetAppInstanceIndex()
        {
            var index = Environment.GetEnvironmentVariable("CF_INSTANCE_INDEX");
            return !string.IsNullOrWhiteSpace(index) ? $" ({index})" : string.Empty;
        }
    }
}