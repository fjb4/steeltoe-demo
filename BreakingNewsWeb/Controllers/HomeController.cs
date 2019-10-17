using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BreakingNewsWeb.Models;

namespace BreakingNewsWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly GetRandomHeadlineCommand _getRandomHeadlineCommand;
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration, GetRandomHeadlineCommand getRandomHeadlineCommand)
        {
            _configuration = configuration;
            _getRandomHeadlineCommand = getRandomHeadlineCommand;
        }

        public async Task<IActionResult> Index()
        {
            var headline = await _getRandomHeadlineCommand.ExecuteAsync();

            var model = new HomeViewModel
            {
                Title = _configuration["title"] ?? "<No Title>",
                ButtonText = _configuration["buttonText"] ?? "<No Button Text>",
                ButtonColor = _configuration["buttonColor"] ?? string.Empty,
                ButtonBackgroundColor = _configuration["buttonBackgroundColor"] ?? string.Empty,
                HeadlineText = headline?.Text
            };

            return View(model);
        }
    }
}