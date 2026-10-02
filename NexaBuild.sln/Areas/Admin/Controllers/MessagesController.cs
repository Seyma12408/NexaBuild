using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.Entities;

namespace NexaBuild.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MessagesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _db.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var msg = await _db.ContactMessages.FindAsync(id);
            if (msg != null)
            {
                msg.Status = MessageStatus.Read;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mesaj okundu olarak işaretlendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var msg = await _db.ContactMessages.FindAsync(id);
            if (msg != null)
            {
                _db.ContactMessages.Remove(msg);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mesaj silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
