using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_jtadams5.Data;
using Fall2024_Assignment3_jtadams5.Models;
// Adding below includes for OMDB-API requests
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Fall2024_Assignment3_jtadams5.Services;
using System.Security.AccessControl;
using VaderSharp2;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;

namespace Fall2024_Assignment3_jtadams5.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly OpenAIService _openAIService;

        public MoviesController(ApplicationDbContext context, IConfiguration config, OpenAIService openAIService)
        {
            _context = context;
            _config = config; // Hope this works?
            _openAIService = openAIService;
        }
        
        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                .FirstOrDefaultAsync(m => m.MovieID == id);
             

            if (movie == null)
            {
                return NotFound();
            }

            if (movie.Reviews == null)
            {
                ViewBag.FakeTweets = new string[] { "Error" };
                ViewBag.Sentiment = "Error";
            }
            else
            {
                string[] tweetArray = movie.Reviews.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var anal = new SentimentIntensityAnalyzer();
                SentimentAnalysisResults sentiment = anal.PolarityScores(movie.Reviews); // I think we will just calculate it here always
                ViewBag.FakeTweets = tweetArray;
                ViewBag.Sentiment = sentiment.ToString();

            }

            ViewBag.BackgroundImage = movie.PosterURL;
            
            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MovieID,Title,Genre,ReleaseYear,PosterURL,imdbURL")] Movie movie)
        {
            var keyVaultName = "Fall2024-jtadams5-Vault";
            var kUri = $"https://{keyVaultName}.vault.azure.net";
            var client = new SecretClient(new Uri(kUri), new DefaultAzureCredential());

            var match = Regex.Match(movie.imdbURL, @"title/(tt\d+)/"); // Thank god for chatGPT jesus christ i hate regex
            if (match.Success)
            {
                // We can call our OMDB API
                string imdbID = match.Groups[1].Value;

                // Utilizing "USING" here because IDK? Maybe just makes it easier to encapsulate whats going on?
                using (var httpClient = new HttpClient())
                {
                    var APIKey = await client.GetSecretAsync("OMDBKey"); // Oksy I think this is how we grab the API key?
                    var OMBDURL = $"http://www.omdbapi.com/?i={imdbID}&apikey={APIKey.Value.Value}";
                    var response = await httpClient.GetStringAsync(OMBDURL); // Get Async is not the same as getStringAsync Lol
                    var jsonResponse = JObject.Parse(response);

                    // Error checking 
                    movie.PlotSummary = jsonResponse["Plot"]?.ToString() ?? "Not Found";

                    // Honestly IDK here
                    if (!Uri.IsWellFormedUriString(movie.PosterURL,UriKind.Absolute)) 
                    {
                        movie.PosterURL = jsonResponse["Poster"]?.ToString() ?? movie.PosterURL;
                    }

                    movie.ReleaseYear = jsonResponse["Year"]?.ToObject<int>() ?? movie.ReleaseYear;
                    movie.Genre = jsonResponse["Genre"]?.ToString() ?? movie.Genre;

                }
            } else
            {
                // Still try using the APi but with just the name
                // Need to replace all spaces with +-es
                string omdbSearch = movie.Title.Replace(" ","+");

                // Utilizing "USING" here because IDK? Maybe just makes it easier to encapsulate whats going on?
                using (var httpClient = new HttpClient())
                {
                    var APIKey = await client.GetSecretAsync("OMDBKey"); // Oksy I think this is how we grab the API key?
                    var OMBDURL = $"http://www.omdbapi.com/?t={omdbSearch}&apikey={APIKey.Value.Value}";
                    var response = await httpClient.GetStringAsync(OMBDURL); // Get Async is not the same as getStringAsync Lol
                    var jsonResponse = JObject.Parse(response);
                    // We may not find a match
                    if (jsonResponse["Response"]?.ToString() == "True")
                    {
                        // Error checking 
                        movie.PlotSummary = jsonResponse["Plot"]?.ToString() ?? "Not Found";

                        if (!Uri.IsWellFormedUriString(movie.PosterURL, UriKind.Absolute))
                        {
                            movie.PosterURL = jsonResponse["Poster"]?.ToString() ?? movie.PosterURL;
                        }

                        movie.ReleaseYear = jsonResponse["Year"]?.ToObject<int>() ?? movie.ReleaseYear;

                        movie.Title = jsonResponse["Title"]?.ToString() ?? movie.Title;
                        movie.Genre = jsonResponse["Genre"]?.ToString() ?? movie.Genre;
                    }

                }
            }
            // Lets do some OpenAI Stuff here
            var tweets = await _openAIService.GetFakeMovieTweetsAsync(movie.Title,movie.ReleaseYear); // Honestly no clue here what teh hell

            movie.Reviews = tweets;
            // Holy Toledo batman, let's pray this doesn't break my DB
            if (ModelState.IsValid)
            {

                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MovieID,Title,Genre,ReleaseYear,PosterURL,imdbURL,PlotSummary")] Movie movie)
        {
            if (id != movie.MovieID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var movieToUpdate = await _context.Movies.FindAsync(id);
                    if (movieToUpdate == null) return NotFound();

                    // Set the fields that can be updated
                    movieToUpdate.Title = movie.Title;
                    movieToUpdate.Genre = movie.Genre;
                    movieToUpdate.ReleaseYear = movie.ReleaseYear;
                    movieToUpdate.PosterURL = movie.PosterURL;

                    // Keep the existing Reviews value
                    _context.Update(movieToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.MovieID))
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
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieID == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.MovieID == id);
        }
    }
}
