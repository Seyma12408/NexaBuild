using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.DTOs;
using NexaBuild.Entities;
using NexaBuild.Services;

namespace NexaBuild.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;
        private readonly ISlugService _slugService;

        public ServicesController(ApplicationDbContext db, IFileService fileService, ISlugService slugService)
        {
            _db = db;
            _fileService = fileService;
            _slugService = slugService;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _db.Services.OrderBy(s => s.DisplayOrder).ToListAsync();
            return View(services);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ServiceDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceDto model)
        {
            if (!ModelState.IsValid) return View(model);

            string imageUrl = "https://images.unsplash.com/photo-1541888946425-d0fbb18086f6?auto=format&fit=crop&w=800&q=80";
            if (model.ImageFile != null)
            {
                var uploaded = await _fileService.UploadImageAsync(model.ImageFile, "services");
                if (uploaded != null) imageUrl = uploaded;
            }

            var slug = _slugService.GenerateSlug(model.Title);

            var service = new Service
            {
                Title = model.Title,
                Slug = slug,
                Icon = model.Icon ?? "bi-buildings",
                Summary = model.Summary,
                Description = model.Description,
                ImageUrl = imageUrl,
                Features = model.Features,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive
            };

            _db.Services.Add(service);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hizmet başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _db.Services.FindAsync(id);
            if (service == null) return NotFound();

            var model = new ServiceDto
            {
                Id = service.Id,
                Title = service.Title,
                Slug = service.Slug,
                Icon = service.Icon,
                Summary = service.Summary,
                Description = service.Description,
                ImageUrl = service.ImageUrl,
                Features = service.Features,
                DisplayOrder = service.DisplayOrder,
                IsActive = service.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceDto model)
        {
            var service = await _db.Services.FindAsync(model.Id);
            if (service == null) return NotFound();

            if (!ModelState.IsValid) return View(model);

            if (model.ImageFile != null)
            {
                var uploaded = await _fileService.UploadImageAsync(model.ImageFile, "services");
                if (uploaded != null)
                {
                    _fileService.DeleteFile(service.ImageUrl);
                    service.ImageUrl = uploaded;
                }
            }

            service.Title = model.Title;
            service.Icon = model.Icon ?? "bi-buildings";
            service.Summary = model.Summary;
            service.Description = model.Description;
            service.Features = model.Features;
            service.DisplayOrder = model.DisplayOrder;
            service.IsActive = model.IsActive;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Hizmet bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _db.Services.FindAsync(id);
            if (service != null)
            {
                _fileService.DeleteFile(service.ImageUrl);
                _db.Services.Remove(service);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hizmet silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
