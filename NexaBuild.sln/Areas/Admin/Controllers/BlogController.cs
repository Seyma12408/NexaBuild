using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.DTOs;
using NexaBuild.Entities;
using NexaBuild.Services;

namespace NexaBuild.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;
        private readonly ISlugService _slugService;

        public BlogController(ApplicationDbContext db, IFileService fileService, ISlugService slugService)
        {
            _db = db;
            _fileService = fileService;
            _slugService = slugService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _db.BlogPosts
                .Include(b => b.Category)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _db.BlogCategories.Where(c => c.IsActive).ToListAsync(), "Id", "Name");
            return View(new BlogPostDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogPostDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _db.BlogCategories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            string coverImageUrl = "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=1200&q=80";
            if (model.CoverImageFile != null)
            {
                var uploaded = await _fileService.UploadImageAsync(model.CoverImageFile, "blog");
                if (uploaded != null) coverImageUrl = uploaded;
            }

            var slug = _slugService.GenerateSlug(model.Title);
            if (await _db.BlogPosts.AnyAsync(b => b.Slug == slug))
            {
                slug = $"{slug}-{Guid.NewGuid().ToString("N").Substring(0, 4)}";
            }

            var post = new BlogPost
            {
                Title = model.Title,
                Slug = slug,
                Summary = model.Summary,
                Content = model.Content,
                Author = string.IsNullOrWhiteSpace(model.Author) ? "NexaBuild Mimarlık" : model.Author,
                CategoryId = model.CategoryId,
                CoverImageUrl = coverImageUrl,
                IsPublished = model.IsPublished,
                CreatedAt = DateTime.UtcNow,
                PublishedAt = model.IsPublished ? DateTime.UtcNow : null
            };

            _db.BlogPosts.Add(post);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Blog yazısı başarıyla yayınlandı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _db.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();

            var model = new BlogPostDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Summary = post.Summary,
                Content = post.Content,
                Author = post.Author,
                CategoryId = post.CategoryId,
                CoverImageUrl = post.CoverImageUrl,
                IsPublished = post.IsPublished
            };

            ViewBag.Categories = new SelectList(await _db.BlogCategories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", post.CategoryId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BlogPostDto model)
        {
            var post = await _db.BlogPosts.FindAsync(model.Id);
            if (post == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _db.BlogCategories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            if (model.CoverImageFile != null)
            {
                var uploaded = await _fileService.UploadImageAsync(model.CoverImageFile, "blog");
                if (uploaded != null)
                {
                    _fileService.DeleteFile(post.CoverImageUrl);
                    post.CoverImageUrl = uploaded;
                }
            }

            post.Title = model.Title;
            post.Summary = model.Summary;
            post.Content = model.Content;
            post.Author = model.Author;
            post.CategoryId = model.CategoryId;
            post.IsPublished = model.IsPublished;
            post.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Blog yazısı güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _db.BlogPosts.FindAsync(id);
            if (post != null)
            {
                _fileService.DeleteFile(post.CoverImageUrl);
                _db.BlogPosts.Remove(post);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Blog yazısı silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
