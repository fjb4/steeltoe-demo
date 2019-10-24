using System.Collections.Generic;

namespace FakeNewsWeb.Models
{
    public class HomeViewModel
    {
        public string Title { get; set; }
        public ICollection<string> Headlines { get; set; }
    }
}