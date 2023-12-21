using DOTNET5CORECRUD.Models;
using DOTNET5CORECRUD.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DOTNET5CORECRUD.Controllers
{
    public class MoviesController1 : Controller
    {
        private readonly ApplicatioDBContext _context;
       // private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1048576;
        public MoviesController1(ApplicatioDBContext context , IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;

        }
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.OrderByDescending(m=>m.Rate).ToListAsync();
            return View(movies);
        }

        public async Task<IActionResult> Create()
        {
            var ViewModel = new MovieformviewModel
            {

                Genres = await _context.Genres.OrderBy(b=>b.Name).ToListAsync()
            };
            return View("movieform", ViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieformviewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(b => b.Name).ToListAsync();
                return View("movieform",model);

            }
            var files = Request.Form.Files;
            if (!files.Any())
            {
                model.Genres = await _context.Genres.OrderBy(b => b.Name).ToListAsync();
                ModelState.AddModelError("Poster", "please select movie poster!");
                return View("movieform", model);
            }
            var poster = files.FirstOrDefault();
            if (!_allowedExtenstions.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Only .PNG, .JPG images are allowed!");
                return View("movieform", model);
            }
            if (poster.Length> _maxAllowedPosterSize)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "poster is over 1m");
                return View("movieform", model);

            }
            using var dataStream = new MemoryStream();//to return data to database

            await poster.CopyToAsync(dataStream);
            //identify all db
            var movies = new Movie
            {
                Title = model.Title,
                GenreId = model.GenreId,
                Year = model.Year,
                Rate = model.Rate,
                Storeline = model.Storeline,
                //array list
                Poster = dataStream.ToArray()
            };

            _context.Movies.Add(movies);
            _context.SaveChanges();

            _toastNotification.AddSuccessToastMessage("Movie created successfully");
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int ? id)
        {
            if (id==null)
            
                return BadRequest();
              
            var movie = await _context.Movies.FindAsync(id);
            if (movie==null)
            
                return NotFound();
            var viewModel = new MovieformviewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                GenreId = movie.GenreId,
                Rate = movie.Rate,
                Year = movie.Year,
                Storeline = movie.Storeline,
                Poster = movie.Poster,
                Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync()
            };
                return View("MovieForm", viewModel);




        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieformviewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View("MovieForm", model);
            }

            var movie = await _context.Movies.FindAsync(model.Id);

            if (movie == null)
                return NotFound();

            var files = Request.Form.Files;

            if (files.Any())
            {
                var poster = files.FirstOrDefault();

                using var dataStream = new MemoryStream();

                await poster.CopyToAsync(dataStream);

                model.Poster = dataStream.ToArray();

                if (!_allowedExtenstions.Contains(Path.GetExtension(poster.FileName).ToLower()))
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Only .PNG, .JPG images are allowed!");
                    return View("MovieForm", model);
                }

                if (poster.Length > _maxAllowedPosterSize)
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                    return View("MovieForm", model);
                }

                movie.Poster = model.Poster;
            }

            movie.Title = model.Title;
            movie.GenreId = model.GenreId;
            movie.Year = model.Year;
            movie.Rate = model.Rate;
            movie.Storeline = model.Storeline;

            _context.SaveChanges();

           _toastNotification.AddSuccessToastMessage("Movie updated successfully");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.Include(m => m.Genre).SingleOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            return Ok();
        }


    }
}
