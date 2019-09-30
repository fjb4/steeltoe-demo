using System;
using System.Text;
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

        private IConfiguration Configuration { get; }
        private ILogger<Startup> Logger { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add Steeltoe Discovery Client service
            services.AddDiscoveryClient(Configuration);

            // Add Hystrix command GetUpstreamContent to Hystrix group "GetUpstreamContent"
            services.AddHystrixCommand<GetUpstreamContentCommand>("GetUpstreamContent", Configuration);

            // Add Hystrix metrics stream to enable monitoring 
            services.AddHystrixMetricsStream(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseHystrixRequestContext();

            app.Run(async context =>
            {
                var appName = Configuration["spring:application:name"];
                var appInstanceIndex = GetAppInstanceIndex();

                // get this application's configuration from config server
                var color = Configuration["color"];
                var upstreamHost = Configuration["upstreamHost"];

                // retrieve content from upstream host
                var command = app.ApplicationServices.GetService<GetUpstreamContentCommand>();
                var upstreamContent = await command.ExecuteAsync(upstreamHost);

                context.Response.Headers.Add("Content-Type", "text/html");
                context.Response.Headers.Add("Cache-Control", "no-cache");

                var html = BuildResponseHtml(appName, appInstanceIndex, color, upstreamHost, upstreamContent);
                await context.Response.WriteAsync(html);
            });

            app.UseDiscoveryClient();

            app.UseHystrixMetricsStream();
        }

        public static string BuildResponseHtml(string appName, string appInstanceIndex, string color, string upstreamHost, string upstreamContent)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"<div style='border-style: solid; border-color: {color}; border-width: 1em; padding: 0.5em;'>");
            sb.AppendLine($"<h1 style='margin-top: 0;'>{appName}{appInstanceIndex}</h1>");
            sb.AppendLine($"Color: <strong>{color}</strong><br/>");

            if (!string.IsNullOrWhiteSpace(upstreamHost)) sb.AppendLine($"Upstream Host: <strong>{upstreamHost}</strong><br/>");

            sb.AppendLine("</div>");

            if (!string.IsNullOrWhiteSpace(upstreamContent))
            {
                sb.AppendLine("<h1 style='text-align: center; margin-top: 0.5em; margin-bottom: 0.5em;'>&#x25BC;</h1>");
                sb.Append(upstreamContent);
            }

            return sb.ToString();
        }

        private static string GetAppInstanceIndex()
        {
            var index = Environment.GetEnvironmentVariable("CF_INSTANCE_INDEX");
            return !string.IsNullOrWhiteSpace(index) ? $" ({index})" : string.Empty;
        }
    }
}