using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Discovery;

namespace SteeltoeDemo
{
    public class GetUpstreamContentCommand : HystrixCommand<string>
    {
        private readonly ILogger<GetUpstreamContentCommand> _logger;
        private readonly IServiceProvider _serviceProvider;

        private string _upstreamHost;

        public GetUpstreamContentCommand(IHystrixCommandOptions options, IServiceProvider serviceProvider,
            ILogger<GetUpstreamContentCommand> logger)
            : base(options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<string> ExecuteAsync(string upstreamHost)
        {
            _upstreamHost = upstreamHost;
            return await ExecuteAsync();
        }

        protected override async Task<string> RunAsync()
        {
            using (var httpClient = GetClient())
            {
                var result = await httpClient.GetStringAsync($"http://{_upstreamHost}/");
                return result ?? "[No response]";
            }
        }

        protected override async Task<string> RunFallbackAsync()
        {
            const string color = "yellow";
            var html = Startup.BuildResponseHtml(_upstreamHost, string.Empty, color, string.Empty, string.Empty);
            return await Task.FromResult(html);
        }

        private HttpClient GetClient()
        {
            var client = _serviceProvider.GetService<IDiscoveryClient>();
            var handler = new DiscoveryHttpClientHandler(client);
            return new HttpClient(handler, false);
        }
    }
}