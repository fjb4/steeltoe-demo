using System;
using FakeNewsWeb.Controllers;
using FakeNewsWeb.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Http.Discovery;
using Steeltoe.Discovery.Client;

namespace FakeNewsWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Steeltoe Discovery Client service
            services.AddDiscoveryClient(Configuration);

            services.AddTransient<DiscoveryHttpMessageHandler>();

            services.AddHttpClient("fake-news", client => {
                client.BaseAddress = new Uri("https://fake-news-service");
            }).AddHttpMessageHandler<DiscoveryHttpMessageHandler>();

            // Add Hystrix command GetRandomHeadline to Hystrix group
            services.AddHystrixCommand<GetHeadlinesCommand>("FakeNews", Configuration);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Add Hystrix metrics stream to enable monitoring 
            services.AddHystrixMetricsStream(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseHystrixRequestContext();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseDiscoveryClient();

            app.UseHystrixMetricsStream();
        }
    }
}