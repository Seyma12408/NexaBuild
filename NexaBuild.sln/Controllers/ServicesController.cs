using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.DTOs;
using NexaBuild.Entities;

namespace NexaBuild.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ServicesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var services = await _db.Services
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            return View(services);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();

            var service = await _db.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Slug == slug && s.IsActive);

            if (service == null) return NotFound();

            var otherServices = await _db.Services
                .AsNoTracking()
                .Where(s => s.Id != service.Id && s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            ViewBag.OtherServices = otherServices;
            return View(service);
        }
    }
}
