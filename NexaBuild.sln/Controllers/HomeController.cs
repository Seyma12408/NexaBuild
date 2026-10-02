using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.Entities;
using NexaBuild.Models;

namespace NexaBuild.Controllers
{
    public class HomeViewModel
    {
        public SiteSetting Setting { get; set; } = new();
        public List<Project> FeaturedProjects { get; set; } = new();
        public List<Project> OngoingProjects { get; set; } = new();
        public List<Service> Services { get; set; } = new();
        public List<BlogPost> LatestPosts { get; set; } = new();
        public List<Testimonial> Testimonials { get; set; } = new();
        public List<ProjectCategory> Categories { get; set; } = new();
    }

    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var setting = await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync() ?? new SiteSetting();
            
            var featuredProjects = await _db.Projects
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.IsFeatured)
                .OrderBy(p => p.DisplayOrder)
                .Take(6)
                .ToListAsync();

            var ongoingProjects = await _db.Projects
                .Include(p => p.Category)
                .Include(p => p.Stages)
                .AsNoTracking()
                .Where(p => p.Status == ProjectStatus.Ongoing)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            var services = await _db.Services
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .Take(6)
                .ToListAsync();

            var latestPosts = await _db.BlogPosts
                .Include(b => b.Category)
                .AsNoTracking()
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .Take(3)
                .ToListAsync();

            var testimonials = await _db.Testimonials
                .AsNoTracking()
                .Where(t => t.Status == ReviewStatus.Approved)
                .OrderByDescending(t => t.CreatedAt)
                .Take(4)
                .ToListAsync();

            var categories = await _db.ProjectCategories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var model = new HomeViewModel
            {
                Setting = setting,
                FeaturedProjects = featuredProjects,
                OngoingProjects = ongoingProjects,
                Services = services,
                LatestPosts = latestPosts,
                Testimonials = testimonials,
                Categories = categories
            };

            return View(model);
        }

        [Route("hakkimizda")]
        [Route("about")]
        [Route("Home/About")]
        public async Task<IActionResult> About()
        {
            var setting = await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync() ?? new SiteSetting();
            var testimonials = await _db.Testimonials
                .AsNoTracking()
                .Where(t => t.Status == ReviewStatus.Approved)
                .OrderByDescending(t => t.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.Setting = setting;
            ViewBag.Testimonials = testimonials;
            return View();
        }

        [Route("Home/ErrorStatus")]
        public IActionResult ErrorStatus(int code)
        {
            ViewBag.StatusCode = code;
            if (code == 404)
            {
                ViewBag.Title = "Sayfa Bulunamadı (404)";
                ViewBag.Message = "Aradığınız sayfa taşınmış, silinmiş veya hiç var olmamış olabilir.";
            }
            else if (code == 403)
            {
                ViewBag.Title = "Erişim Reddedildi (403)";
                ViewBag.Message = "Bu alana erişmek için gerekli yetkilere sahip değilsiniz.";
            }
            else
            {
                ViewBag.Title = "Bir Hata Oluştu";
                ViewBag.Message = "İşleminiz gerçekleştirilirken beklenmeyen bir hata meydana geldi.";
            }
            return View("Error");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
