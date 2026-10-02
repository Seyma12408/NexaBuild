using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.Entities;

namespace NexaBuild.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SettingsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = await _db.SiteSettings.FirstOrDefaultAsync() ?? new SiteSetting();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SiteSetting model)
        {
            var settings = await _db.SiteSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new SiteSetting();
                _db.SiteSettings.Add(settings);
            }

            settings.CompanyName = model.CompanyName;
            settings.Phone = model.Phone;
            settings.Email = model.Email;
            settings.Address = model.Address;
            settings.WorkingHours = model.WorkingHours;
            settings.MapEmbedUrl = model.MapEmbedUrl;
            settings.HeroTitle = model.HeroTitle;
            settings.HeroSubtitle = model.HeroSubtitle;
            settings.MetaDescription = model.MetaDescription;
            settings.FacebookUrl = model.FacebookUrl;
            settings.InstagramUrl = model.InstagramUrl;
            settings.LinkedInUrl = model.LinkedInUrl;
            settings.YouTubeUrl = model.YouTubeUrl;
            settings.StatsProjectsCompleted = model.StatsProjectsCompleted;
            settings.StatsYearsExperience = model.StatsYearsExperience;
            settings.StatsExpertEngineers = model.StatsExpertEngineers;
            settings.StatsSatisfiedClients = model.StatsSatisfiedClients;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Site ayarları başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
