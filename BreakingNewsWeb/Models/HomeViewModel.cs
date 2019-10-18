using System.Collections.Generic;

namespace BreakingNewsWeb.Models
{
    public class HomeViewModel
    {
        public string Title { get; set; }
        public ICollection<string> Headlines { get; set; }
    }
}