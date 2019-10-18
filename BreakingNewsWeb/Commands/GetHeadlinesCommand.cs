using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BreakingNewsWeb.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Steeltoe.CircuitBreaker.Hystrix;

namespace BreakingNewsWeb.Commands
{
    public class GetHeadlinesCommand : HystrixCommand<ICollection<Headline>>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GetHeadlinesCommand> _logger;
        private int _count;

        public GetHeadlinesCommand(IHystrixCommandOptions options, IHttpClientFactory httpClientFactory,
            ILogger<GetHeadlinesCommand> logger)
            : base(options)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public Task<ICollection<Headline>> ExecuteAsync(int count)
        {
            _count = count;
            return ExecuteAsync();
        }

        protected override async Task<ICollection<Headline>> RunAsync()
        {
            using (var httpClient = _httpClientFactory.CreateClient("breaking-news"))
            {
                var content = await httpClient.GetStringAsync($"http://breaking-news-service/api/headline/random/{_count}");
                return JsonConvert.DeserializeObject<List<Headline>>(content);
            }
        }

        protected override async Task<ICollection<Headline>> RunFallbackAsync()
        {
            var fallbackHeadline = new Headline
            {
                Id = -1,
                Text = "Study: Employees Happiest When Pretending To Work From Home"
            };

            return await Task.FromResult(new List<Headline> {fallbackHeadline});
        }
    }
}