using System.Net.Http;
using System.Threading.Tasks;
using HeadlinesShared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Steeltoe.CircuitBreaker.Hystrix;

namespace HeadlinesWeb.Controllers
{
    public class GetRandomHeadlineCommand : HystrixCommand<Headline>
    {
        private readonly ILogger<GetRandomHeadlineCommand> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public GetRandomHeadlineCommand(IHystrixCommandOptions options, IHttpClientFactory httpClientFactory,
            ILogger<GetRandomHeadlineCommand> logger)
            : base(options)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        protected override async Task<Headline> RunAsync()
        {
            using (var httpClient = _httpClientFactory.CreateClient("headlines"))
            {
                var content = await httpClient.GetStringAsync("http://headlines-service/api/headline/random");
                return JsonConvert.DeserializeObject<Headline>(content);
            }
        }

        protected override async Task<Headline> RunFallbackAsync()
        {
            var fallbackHeadline = new Headline
            {
                Id = -1,
                Text = "Study: Employees Happiest When Pretending To Work From Home"
            };

            return await Task.FromResult(fallbackHeadline);
        }
    }
}