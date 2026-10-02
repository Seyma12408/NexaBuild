using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.DTOs;
using NexaBuild.Entities;

namespace NexaBuild.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProjectsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? category, string? status, string? search)
        {
            var query = _db.Projects
                .Include(p => p.Category)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category != null && p.Category.Slug == category);
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<ProjectStatus>(status, true, out var parsedStatus))
                {
                    query = query.Where(p => p.Status == parsedStatus);
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(s) || p.Location.ToLower().Contains(s));
            }

            var projects = await query
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Favori kontrolü
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<int> userFavIds = new();
            if (!string.IsNullOrEmpty(userId))
            {
                userFavIds = await _db.Favorites
                    .Where(f => f.UserId == userId)
                    .Select(f => f.ProjectId)
                    .ToListAsync();
            }

            var projectDtos = projects.Select(p => new ProjectListDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                CoverImageUrl = p.CoverImageUrl,
                Location = p.Location,
                CategoryName = p.Category?.Name ?? "",
                CategoryId = p.CategoryId,
                ProjectType = p.ProjectType,
                Status = p.Status,
                ProgressPercentage = p.ProgressPercentage,
                IsFeatured = p.IsFeatured,
                IsFavorited = userFavIds.Contains(p.Id)
            }).ToList();

            var categories = await _db.ProjectCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.ActiveCategory = category;
            ViewBag.ActiveStatus = status;
            ViewBag.Search = search;

            return View(projectDtos);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();

            var project = await _db.Projects
                .Include(p => p.Category)
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .Include(p => p.Stages.OrderBy(s => s.Order))
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (project == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isFav = false;
            if (!string.IsNullOrEmpty(userId))
            {
                isFav = await _db.Favorites.AnyAsync(f => f.UserId == userId && f.ProjectId == project.Id);
            }

            var relatedProjects = await _db.Projects
                .Include(p => p.Category)
                .Where(p => p.Id != project.Id && (p.CategoryId == project.CategoryId || p.IsFeatured))
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .Select(p => new ProjectListDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    ShortDescription = p.ShortDescription,
                    CoverImageUrl = p.CoverImageUrl,
                    Location = p.Location,
                    CategoryName = p.Category != null ? p.Category.Name : "",
                    Status = p.Status,
                    ProgressPercentage = p.ProgressPercentage
                })
                .ToListAsync();

            var model = new ProjectDetailDto
            {
                Id = project.Id,
                Title = project.Title,
                Slug = project.Slug,
                ShortDescription = project.ShortDescription,
                Description = project.Description,
                CoverImageUrl = project.CoverImageUrl,
                Location = project.Location,
                CategoryName = project.Category?.Name ?? "",
                ProjectType = project.ProjectType,
                TotalArea = project.TotalArea,
                NumberOfUnits = project.NumberOfUnits,
                Status = project.Status,
                ProgressPercentage = project.ProgressPercentage,
                StartDate = project.StartDate,
                EstimatedCompletionDate = project.EstimatedCompletionDate,
                CompletionDate = project.CompletionDate,
                IsFavorited = isFav,
                Images = project.Images.Select(i => new ProjectImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    Caption = i.Caption,
                    IsCover = i.IsCover,
                    DisplayOrder = i.DisplayOrder
                }).ToList(),
                Stages = project.Stages.Select(s => new ProjectStageDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Order = s.Order,
                    Status = s.Status,
                    ProgressPercentage = s.ProgressPercentage,
                    CompletionDate = s.CompletionDate,
                    Description = s.Description
                }).ToList(),
                RelatedProjects = relatedProjects
            };

            return View(model);
        }
    }
}
