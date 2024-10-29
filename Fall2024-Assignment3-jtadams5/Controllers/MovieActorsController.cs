using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_jtadams5.Data;
using Fall2024_Assignment3_jtadams5.Models;

namespace Fall2024_Assignment3_jtadams5.Controllers
{
    public class MovieActorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieActorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MovieActors
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.MovieActors.Include(m => m.Actor).Include(m => m.Movie);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: MovieActors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movieActor = await _context.MovieActors
                .Include(m => m.Actor)
                .Include(m => m.Movie)
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movieActor == null)
            {
                return NotFound();
            }

            return View(movieActor);
        }

        /// GET: MovieActor/Create
        public IActionResult Create()
        {
            ViewBag.Actors = _context.Actors.ToList();
            ViewBag.Movies = _context.Movies.ToList();
            return View();
        }

        // POST: MovieActor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int actorId, int movieId)
        {
            if (ModelState.IsValid)
            {
                // Check if the relationship already exists
                var existingLink = _context.MovieActors
                    .FirstOrDefault(ma => ma.ActorId == actorId && ma.MovieId == movieId);

                if (existingLink == null)
                {
                    var movieActor = new MovieActor
                    {
                        ActorId = actorId,
                        MovieId = movieId
                    };
                    _context.MovieActors.Add(movieActor);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index", "Movies");  // Redirect to the movies list or any other relevant page
            }

            ViewBag.Actors = _context.Actors.ToList();
            ViewBag.Movies = _context.Movies.ToList();
            return View();
        }

        // GET: MovieActors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movieActor = await _context.MovieActors.FindAsync(id);
            if (movieActor == null)
            {
                return NotFound();
            }
            ViewData["ActorId"] = new SelectList(_context.Actors, "ActorID", "ActorID", movieActor.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieID", "MovieID", movieActor.MovieId);
            return View(movieActor);
        }

        // POST: MovieActors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MovieId,ActorId")] MovieActor movieActor)
        {
            if (id != movieActor.MovieId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movieActor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieActorExists(movieActor.MovieId))
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
            ViewData["ActorId"] = new SelectList(_context.Actors, "ActorID", "ActorID", movieActor.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieID", "MovieID", movieActor.MovieId);
            return View(movieActor);
        }

        // GET: MovieActors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movieActor = await _context.MovieActors
                .Include(m => m.Actor)
                .Include(m => m.Movie)
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movieActor == null)
            {
                return NotFound();
            }

            return View(movieActor);
        }

        // POST: MovieActors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movieActor = await _context.MovieActors.FindAsync(id);
            if (movieActor != null)
            {
                _context.MovieActors.Remove(movieActor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieActorExists(int id)
        {
            return _context.MovieActors.Any(e => e.MovieId == id);
        }
    }
}
