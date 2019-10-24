using System;
using System.Linq;
using System.Threading.Tasks;
using FakeNewsWeb.Commands;
using FakeNewsWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FakeNewsWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly GetHeadlinesCommand _getHeadlinesCommand;

        public HomeController(IConfiguration configuration, GetHeadlinesCommand getHeadlinesCommand)
        {
            _configuration = configuration;
            _getHeadlinesCommand = getHeadlinesCommand;
        }

        public async Task<IActionResult> Index()
        {
            var columnCount = GetColumnCount();
            var headlines = await _getHeadlinesCommand.ExecuteAsync(columnCount);

            var model = new HomeViewModel
            {
                Title = GetNewspaperTitle(),
                Headlines = headlines.Select(h => h.Text).ToList()
            };

            return View(model);
        }

        private string GetNewspaperTitle()
        {
            return _configuration["title"] ?? "<No Title>";
        }

        private int GetColumnCount()
        {
            var columnCountStr = _configuration["columnCount"];
            return !string.IsNullOrWhiteSpace(columnCountStr) ? Convert.ToInt32(columnCountStr) : 1;
        }
    }
}