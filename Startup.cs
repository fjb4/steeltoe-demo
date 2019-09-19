using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Steeltoe.Common.Discovery;
using Steeltoe.Common.LoadBalancer;
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
            services.AddDiscoveryClient(Configuration);
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

                var upstreamContent = await GetUpstreamContent(app.ApplicationServices, upstreamHost);

                var message = $"<p>{applicationName}{GetAppInstanceIndex()} => {upstreamHost}</p>";
                message += upstreamContent;

                context.Response.Headers.Add("Content-Type", "text/html");
                context.Response.Headers.Add("Cache-Control", "no-cache");
                await context.Response.WriteAsync(message);
            });

            app.UseDiscoveryClient();
        }

        private async Task<string> GetUpstreamContent(IServiceProvider serviceProvider, string hostname)
        {
            try
            {
                using (var httpClient = GetClient(serviceProvider))
                {
                    var result = await httpClient.GetStringAsync($"http://{hostname}/");
                    return result ?? "<No response>";
                }
            }
            catch (HttpRequestException ex)
            {
                return "Error: " + ex;
            }
        }

        private HttpClient GetClient(IServiceProvider serviceProvider)
        {
            var client = serviceProvider.GetService<IDiscoveryClient>();
            var handler = new DiscoveryHttpClientHandler(client, Logger);
            return new HttpClient(handler, false);
        }

        private static string GetAppInstanceIndex()
        {
            var index = Environment.GetEnvironmentVariable("CF_INSTANCE_INDEX");
            return !string.IsNullOrWhiteSpace(index) ? $" ({index})" : string.Empty;
        }
    }
}