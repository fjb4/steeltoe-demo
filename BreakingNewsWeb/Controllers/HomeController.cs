using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BreakingNewsWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly GetRandomHeadlineCommand _getRandomHeadlineCommand;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, GetRandomHeadlineCommand getRandomHeadlineCommand)
        {
            _logger = logger;
            _getRandomHeadlineCommand = getRandomHeadlineCommand;
        }

        public async Task<IActionResult> Index()
        {
            var headline = await _getRandomHeadlineCommand.ExecuteAsync();
            return View(headline);
        }
    }
}