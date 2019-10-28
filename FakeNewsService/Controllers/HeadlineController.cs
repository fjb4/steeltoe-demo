using System;
using System.Collections.Generic;
using System.Linq;
using FakeNewsService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FakeNewsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HeadlineController : ControllerBase
    {
        private static readonly Headline[] Headlines =
        {
            new Headline {Id = 1, Text = "Study Reveals: Babies Are Stupid"},
            new Headline {Id = 2, Text = "World Death Rate Holding Steady at 100 Percent"},
            new Headline {Id = 3, Text = "CIA Realizes It's Been Using Black Highlighters All These Years"},
            new Headline {Id = 4, Text = "Drugs Win Drug War"},
            new Headline {Id = 5, Text = "Winner Didn't Even Know It Was Pie-Eating Contest"},
            new Headline {Id = 6, Text = "Child Bankrupts Make-A-Wish Foundation With Wish For Unlimited Wishes"},
            new Headline {Id = 7, Text = "Study: Dolphins Not So Intelligent On Land"},
            new Headline {Id = 8, Text = "Annual Ninja Parade Once Again Passes Through Town Unnoticed"},
            new Headline {Id = 9, Text = "Woman Who Left Room Crying Earlier Expects To Jump Back Into Party Just Like That"},
            new Headline {Id = 10, Text = "Members Of Twisted Sister Now Willing To Take It"},
            new Headline {Id = 11, Text = "Man From Canada Insists He Is Not Cold"},
            new Headline {Id = 12, Text = "Roadkill Squirrel Remembered As Frantic, Indecisive"},
            new Headline {Id = 13, Text = "Scientists Trace Heat Wave To Massive Star At Center Of Solar System"},
            new Headline {Id = 14, Text = "Science Guy Bill Nye Killed In Massive Vinegar/Baking-Soda Explosion"},
            new Headline {Id = 15, Text = "Romantic Gesture Too Expensive To Waste On Current Girlfriend"},
            new Headline {Id = 16, Text = "Miracle Of Birth Occurs For 83 Billionth Time "},
            new Headline {Id = 17, Text = "I’m Like A Chocoholic, But For Booze"},
            new Headline {Id = 18, Text = "Wealthy Teen Nearly Experiences Consequence"},
            new Headline {Id = 19, Text = "World's Scientists Admit They Just Don't Like Mice"},
            new Headline {Id = 20, Text = "Zombie Nutritionist Recommends All-Brain Diet"}
        };

        private readonly ILogger<HeadlineController> _logger;

        private static readonly Random _random = new Random();
        private static readonly object _randomLock = new object();

        public HeadlineController(ILogger<HeadlineController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Headline> Get()
        {
            return Headlines;
        }

        [HttpGet("{id}")]
        public Headline Get(int id)
        {
            return Headlines.FirstOrDefault(h => h.Id == id);
        }

        [HttpGet("random/{count?}")]
        public IEnumerable<Headline> Random(int count = 1)
        {
            if (count > Headlines.Length)
            {
                count = Headlines.Length;
            }
            else if (count < 1)
            {
                count = 1;
            }

            var randomHeadlines = new List<Headline>(count);

            lock (_randomLock)
            {
                for (var i = 0; i < count; i++)
                {
                    Headline headline;

                    do
                    {
                        var index = _random.Next(0, Headlines.Length);
                        headline = Headlines.ElementAt(index);
                    } while (randomHeadlines.Any(h => h.Id == headline.Id));

                    randomHeadlines.Add(headline);
                }
            }

            return randomHeadlines;
        }
    }
}