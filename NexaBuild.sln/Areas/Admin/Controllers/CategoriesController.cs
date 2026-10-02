using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.Entities;
using NexaBuild.Services;

namespace NexaBuild.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ISlugService _slugService;

        public CategoriesController(ApplicationDbContext db, ISlugService slugService)
        {
            _db = db;
            _slugService = slugService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _db.ProjectCategories
                .Include(c => c.Projects)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string? description, int displayOrder)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Kategori adı zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            var slug = _slugService.GenerateSlug(name);
            _db.ProjectCategories.Add(new ProjectCategory
            {
                Name = name,
                Slug = slug,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = true
            });
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kategori başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.ProjectCategories.Include(c => c.Projects).FirstOrDefaultAsync(c => c.Id == id);
            if (category != null)
            {
                if (category.Projects.Any())
                {
                    TempData["ErrorMessage"] = "Bu kategoriye bağlı projeler bulunduğu için silinemez.";
                    return RedirectToAction(nameof(Index));
                }

                _db.ProjectCategories.Remove(category);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kategori silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
