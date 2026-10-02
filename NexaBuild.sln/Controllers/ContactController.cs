using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.DTOs;
using NexaBuild.Entities;

namespace NexaBuild.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ContactController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var setting = await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync() ?? new SiteSetting();
            ViewBag.Setting = setting;
            return View(new ContactMessageCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactMessageCreateDto model)
        {
            var setting = await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync() ?? new SiteSetting();
            ViewBag.Setting = setting;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var message = new ContactMessage
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Subject = model.Subject,
                    Message = model.Message,
                    Status = MessageStatus.New,
                    CreatedAt = DateTime.UtcNow
                };

                _db.ContactMessages.Add(message);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Mesajınız başarıyla iletildi. Müşteri temsilcimiz en kısa sürede size dönüş sağlayacaktır.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Mesaj gönderilirken hata oluştu: " + ex.Message);
                return View(model);
            }
        }
    }
}
