using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.Entities;

namespace NexaBuild.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TestimonialsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TestimonialsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var testimonials = await _db.Testimonials
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(testimonials);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var t = await _db.Testimonials.FindAsync(id);
            if (t != null)
            {
                t.Status = ReviewStatus.Approved;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yorum onaylandı ve public sitede yayına alındı.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var t = await _db.Testimonials.FindAsync(id);
            if (t != null)
            {
                t.Status = ReviewStatus.Rejected;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yorum reddedildi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _db.Testimonials.FindAsync(id);
            if (t != null)
            {
                _db.Testimonials.Remove(t);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yorum silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
