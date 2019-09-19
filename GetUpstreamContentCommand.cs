using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Discovery;
using Microsoft.Extensions.Logging;

namespace SteeltoeDemo
{
    public class GetUpstreamContentCommand : HystrixCommand<string>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GetUpstreamContentCommand> _logger;

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
                return result ?? "<No response>";
            }
        }

        protected override async Task<string> RunFallbackAsync()
        {
            return await Task.FromResult("FALLBACK MESSAGE");
        }

        private HttpClient GetClient()
        {
            var client = _serviceProvider.GetService<IDiscoveryClient>();
            var handler = new DiscoveryHttpClientHandler(client);
            return new HttpClient(handler, false);
        }
    }
}