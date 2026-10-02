using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.DTOs;
using NexaBuild.Entities;

namespace NexaBuild.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BlogController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? category, string? search)
        {
            var query = _db.BlogPosts
                .Include(b => b.Category)
                .AsNoTracking()
                .Where(b => b.IsPublished)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b => b.Category != null && b.Category.Slug == category);
            }

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(s) || b.Summary.ToLower().Contains(s));
            }

            var posts = await query
                .OrderByDescending(b => b.PublishedAt)
                .ToListAsync();

            var categories = await _db.BlogCategories
                .Where(c => c.IsActive)
                .ToListAsync();

            var recentPosts = await _db.BlogPosts
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .Take(4)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.RecentPosts = recentPosts;
            ViewBag.ActiveCategory = category;
            ViewBag.Search = search;

            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();

            var post = await _db.BlogPosts
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished);

            if (post == null) return NotFound();

            // Okunma sayısını artır
            post.ViewCount++;
            await _db.SaveChangesAsync();

            var recentPosts = await _db.BlogPosts
                .Where(b => b.Id != post.Id && b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .Take(4)
                .ToListAsync();

            var categories = await _db.BlogCategories
                .Where(c => c.IsActive)
                .ToListAsync();

            ViewBag.RecentPosts = recentPosts;
            ViewBag.Categories = categories;

            return View(post);
        }
    }
}
