using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FakeNewsWeb.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Steeltoe.CircuitBreaker.Hystrix;

namespace FakeNewsWeb.Commands
{
    public class GetHeadlinesCommand : HystrixCommand<ICollection<Headline>>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GetHeadlinesCommand> _logger;
        private readonly IMemoryCache _cache;
        private int _count;

        private new const string CacheKey = "HEADLINES";

        public GetHeadlinesCommand(IHystrixCommandOptions options, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache,
            ILogger<GetHeadlinesCommand> logger)
            : base(options)
        {
            _httpClientFactory = httpClientFactory;
            _cache = memoryCache;
            _logger = logger;
            IsFallbackUserDefined = true;
        }

        public async Task<ICollection<Headline>> ExecuteAsync(int count)
        {
            _count = count;
            return await ExecuteAsync();
        }

        protected override async Task<ICollection<Headline>> RunAsync()
        {
            ICollection<Headline> headlines;
            
            using (var httpClient = _httpClientFactory.CreateClient("fake-news"))
            {
                var content = await httpClient.GetStringAsync($"http://fake-news-service/api/headline/random/{_count}");
                headlines = JsonConvert.DeserializeObject<List<Headline>>(content);
                SetCache(headlines);
            }

            _logger.LogDebug("Headlines returned from service");

            return headlines;
        }

        protected override Task<ICollection<Headline>> RunFallbackAsync()
        {
            _logger.LogWarning("Trying fallback");
            
            var fallbackHeadlines = TryGetCache();

            if (fallbackHeadlines == null)
            {
                var fallbackHeadline = new Headline
                {
                    Id = -1,
                    Text = "Study: Employees Happiest When Pretending To Work From Home"
                };

                fallbackHeadlines = new List<Headline> {fallbackHeadline};

                _logger.LogWarning("Headlines returning from hard-coded data");
            }
            else
            {
                _logger.LogWarning("Headlines returning from cache");
            }

            return Task.FromResult(fallbackHeadlines);
        }

        private ICollection<Headline> TryGetCache()
        {
            _cache.TryGetValue(CacheKey, out ICollection<Headline> headlines);
            return headlines;
        }

        private void SetCache(ICollection<Headline> headlines)
        {
            if (headlines != null)
            {
                _cache.Set(CacheKey, headlines, TimeSpan.FromHours(2));
            }
        }
    }
}