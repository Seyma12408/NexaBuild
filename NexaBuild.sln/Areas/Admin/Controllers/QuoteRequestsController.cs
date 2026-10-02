using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.Entities;

namespace NexaBuild.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class QuoteRequestsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public QuoteRequestsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(QuoteStatus? status)
        {
            var query = _db.QuoteRequests
                .Include(q => q.User)
                .OrderByDescending(q => q.CreatedAt)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(q => q.Status == status.Value);
            }

            var quotes = await query.ToListAsync();
            ViewBag.ActiveStatus = status;
            return View(quotes);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var quote = await _db.QuoteRequests
                .Include(q => q.User)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null) return NotFound();
            return View(quote);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, QuoteStatus status, string? adminNote)
        {
            var quote = await _db.QuoteRequests.FindAsync(id);
            if (quote == null) return NotFound();

            quote.Status = status;
            quote.AdminNote = adminNote;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Teklif talebi durumu başarıyla güncellendi.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var quote = await _db.QuoteRequests.FindAsync(id);
            if (quote != null)
            {
                _db.QuoteRequests.Remove(quote);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Teklif talebi silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
