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
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;
        private readonly ISlugService _slugService;

        public ProjectsController(ApplicationDbContext db, IFileService fileService, ISlugService slugService)
        {
            _db = db;
            _fileService = fileService;
            _slugService = slugService;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _db.Projects
                .Include(p => p.Category)
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(projects);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _db.ProjectCategories.Where(c => c.IsActive).ToListAsync(), "Id", "Name");
            return View(new ProjectCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _db.ProjectCategories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            string coverImageUrl = "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=1400&q=85";
            if (model.CoverImageFile != null)
            {
                var uploaded = await _fileService.UploadImageAsync(model.CoverImageFile, "projects");
                if (uploaded != null) coverImageUrl = uploaded;
            }

            var slug = _slugService.GenerateSlug(model.Title);
            // Benzersiz slug kontrolü
            if (await _db.Projects.AnyAsync(p => p.Slug == slug))
            {
                slug = $"{slug}-{Guid.NewGuid().ToString("N").Substring(0, 4)}";
            }

            var project = new Project
            {
                Title = model.Title,
                Slug = slug,
                ShortDescription = model.ShortDescription,
                Description = model.Description,
                Location = model.Location,
                CategoryId = model.CategoryId,
                ProjectType = model.ProjectType,
                TotalArea = model.TotalArea,
                NumberOfUnits = model.NumberOfUnits,
                Status = model.Status,
                ProgressPercentage = model.ProgressPercentage,
                IsFeatured = model.IsFeatured,
                DisplayOrder = model.DisplayOrder,
                StartDate = model.StartDate,
                EstimatedCompletionDate = model.EstimatedCompletionDate,
                CompletionDate = model.CompletionDate,
                CoverImageUrl = coverImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            // Çoklu Galeri Görselleri
            if (model.GalleryFiles != null && model.GalleryFiles.Any())
            {
                int order = 1;
                foreach (var file in model.GalleryFiles)
                {
                    var path = await _fileService.UploadImageAsync(file, "projects");
                    if (path != null)
                    {
                        project.Images.Add(new ProjectImage
                        {
                            ImageUrl = path,
                            DisplayOrder = order++,
                            IsCover = false
                        });
                    }
                }
            }

            // Varsayılan Temel Şantiye Aşamaları
            project.Stages.Add(new ProjectStage { Name = "Zemin İyileştirme & Hafriyat", Order = 1, Status = StageStatus.Completed, ProgressPercentage = 100 });
            project.Stages.Add(new ProjectStage { Name = "Kaba Yapı & Taşıyıcı Sistem", Order = 2, Status = StageStatus.InProgress, ProgressPercentage = 75 });
            project.Stages.Add(new ProjectStage { Name = "Dış Cephe & İnce İşçilik", Order = 3, Status = StageStatus.Pending, ProgressPercentage = 10 });
            project.Stages.Add(new ProjectStage { Name = "Peyzaj & Anahtar Teslim", Order = 4, Status = StageStatus.Pending, ProgressPercentage = 0 });

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yeni proje başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var model = new ProjectUpdateDto
            {
                Id = project.Id,
                Title = project.Title,
                ShortDescription = project.ShortDescription,
                Description = project.Description,
                Location = project.Location,
                CategoryId = project.CategoryId,
                ProjectType = project.ProjectType,
                TotalArea = project.TotalArea,
                NumberOfUnits = project.NumberOfUnits,
                Status = project.Status,
                ProgressPercentage = project.ProgressPercentage,
                IsFeatured = project.IsFeatured,
                DisplayOrder = project.DisplayOrder,
                StartDate = project.StartDate,
                EstimatedCompletionDate = project.EstimatedCompletionDate,
                CompletionDate = project.CompletionDate,
                ExistingCoverImageUrl = project.CoverImageUrl
            };

            ViewBag.Categories = new SelectList(await _db.ProjectCategories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", project.CategoryId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectUpdateDto model)
        {
            var project = await _db.Projects.FindAsync(model.Id);
            if (project == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _db.ProjectCategories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            if (model.CoverImageFile != null)
            {
                var uploaded = await _fileService.UploadImageAsync(model.CoverImageFile, "projects");
                if (uploaded != null)
                {
                    _fileService.DeleteFile(project.CoverImageUrl);
                    project.CoverImageUrl = uploaded;
                }
            }

            project.Title = model.Title;
            project.ShortDescription = model.ShortDescription;
            project.Description = model.Description;
            project.Location = model.Location;
            project.CategoryId = model.CategoryId;
            project.ProjectType = model.ProjectType;
            project.TotalArea = model.TotalArea;
            project.NumberOfUnits = model.NumberOfUnits;
            project.Status = model.Status;
            project.ProgressPercentage = model.ProgressPercentage;
            project.IsFeatured = model.IsFeatured;
            project.DisplayOrder = model.DisplayOrder;
            project.StartDate = model.StartDate;
            project.EstimatedCompletionDate = model.EstimatedCompletionDate;
            project.CompletionDate = model.CompletionDate;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Proje bilgileri başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _db.Projects
                .Include(p => p.Images)
                .Include(p => p.Stages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();

            _fileService.DeleteFile(project.CoverImageUrl);
            foreach (var img in project.Images)
            {
                _fileService.DeleteFile(img.ImageUrl);
            }

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Proje ve ilişkili tüm veriler başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        // ==================== GALLERY MANAGEMENT ====================
        [HttpGet]
        public async Task<IActionResult> Gallery(int id)
        {
            var project = await _db.Projects
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadGalleryImage(int projectId, IFormFile imageFile, string? caption)
        {
            if (imageFile != null)
            {
                var uploaded = await _fileService.UploadImageAsync(imageFile, "projects");
                if (uploaded != null)
                {
                    var maxOrder = await _db.ProjectImages.Where(i => i.ProjectId == projectId).MaxAsync(i => (int?)i.DisplayOrder) ?? 0;
                    _db.ProjectImages.Add(new ProjectImage
                    {
                        ProjectId = projectId,
                        ImageUrl = uploaded,
                        Caption = caption,
                        DisplayOrder = maxOrder + 1,
                        IsCover = false
                    });
                    await _db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Görsel galeriye eklendi.";
                }
            }
            return RedirectToAction(nameof(Gallery), new { id = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGalleryImage(int imageId, int projectId)
        {
            var img = await _db.ProjectImages.FindAsync(imageId);
            if (img != null)
            {
                _fileService.DeleteFile(img.ImageUrl);
                _db.ProjectImages.Remove(img);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Görsel galeriden silindi.";
            }
            return RedirectToAction(nameof(Gallery), new { id = projectId });
        }

        // ==================== STAGES MANAGEMENT ====================
        [HttpGet]
        public async Task<IActionResult> Stages(int id)
        {
            var project = await _db.Projects
                .Include(p => p.Stages.OrderBy(s => s.Order))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStage(int projectId, string name, int progressPercentage, StageStatus status, string? description)
        {
            var maxOrder = await _db.ProjectStages.Where(s => s.ProjectId == projectId).MaxAsync(s => (int?)s.Order) ?? 0;
            _db.ProjectStages.Add(new ProjectStage
            {
                ProjectId = projectId,
                Name = name,
                ProgressPercentage = progressPercentage,
                Status = status,
                Description = description,
                Order = maxOrder + 1
            });
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yeni şantiye aşaması eklendi.";
            return RedirectToAction(nameof(Stages), new { id = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStage(int stageId, int projectId)
        {
            var stage = await _db.ProjectStages.FindAsync(stageId);
            if (stage != null)
            {
                _db.ProjectStages.Remove(stage);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Aşama silindi.";
            }
            return RedirectToAction(nameof(Stages), new { id = projectId });
        }
    }
}
