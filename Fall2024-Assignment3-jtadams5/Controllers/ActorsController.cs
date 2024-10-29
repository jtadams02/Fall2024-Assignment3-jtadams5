using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_jtadams5.Data;
using Fall2024_Assignment3_jtadams5.Models;
using VaderSharp2;
using Fall2024_Assignment3_jtadams5.Services;

namespace Fall2024_Assignment3_jtadams5.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly OpenAIService _openAIService;

        public ActorsController(ApplicationDbContext context, IConfiguration config, OpenAIService openAIService)
        {
            _context = context;
            _config = config; // Hope this works?
            _openAIService = openAIService;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors.Include(m => m.MovieActors).ThenInclude(mm => mm.Movie)
                .FirstOrDefaultAsync(m => m.ActorID == id);
            

            if (actor == null)
            {
                return NotFound();
            }
            if (actor.Reviews == null)
            {
                ViewBag.FakeTweets = new string[] { "Error" };
                ViewBag.Sentiment = "Error";
            } else
            {
                string[] tweetArray = actor.Reviews.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var anal = new SentimentIntensityAnalyzer();
                SentimentAnalysisResults sentiment = anal.PolarityScores(actor.Reviews); // I think we will just calculate it here always
                ViewBag.FakeTweets = tweetArray;
                ViewBag.Sentiment = sentiment.ToString();
            }

            return View(actor);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ActorID,Name,Gender,Age,imdbURL,photoURL")] Actor actor)
        {

            var tweets = await _openAIService.GetFakeActorTweetsAsync(actor.Name);

            actor.Reviews = tweets;
            if (ModelState.IsValid)
            {
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ActorID,Name,Gender,Age,imdbURL,photoURL")] Actor actor)
        {
            if (id != actor.ActorID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    var actorToUpdate = await _context.Actors.FindAsync(id);
                    if (actorToUpdate == null) return NotFound();

                    // Set the fields that can be updated
                    actorToUpdate.Name = actor.Name;
                    actorToUpdate.Gender = actor.Gender;
                    actorToUpdate.Age = actor.Age;
                    actorToUpdate.photoURL = actor.photoURL;
                    actorToUpdate.imdbURL = actor.imdbURL;

                    // Keep the existing Reviews value
                    _context.Update(actorToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.ActorID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.ActorID == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actors.Any(e => e.ActorID == id);
        }
    }
}
