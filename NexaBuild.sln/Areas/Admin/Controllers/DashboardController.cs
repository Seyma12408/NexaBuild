using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.DTOs;
using NexaBuild.Entities;

namespace NexaBuild.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var totalProjects = await _db.Projects.CountAsync();
            var ongoingProjects = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.Ongoing);
            var completedProjects = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.Completed);
            var totalUsers = await _db.Users.CountAsync();
            var totalQuotes = await _db.QuoteRequests.CountAsync();
            var pendingQuotes = await _db.QuoteRequests.CountAsync(q => q.Status == QuoteStatus.New);
            var unreadMessages = await _db.ContactMessages.CountAsync(m => m.Status == MessageStatus.New);
            var pendingReviews = await _db.Testimonials.CountAsync(t => t.Status == ReviewStatus.Pending);

            var recentQuotes = await _db.QuoteRequests
                .Include(q => q.User)
                .OrderByDescending(q => q.CreatedAt)
                .Take(5)
                .Select(q => new QuoteRequestListDto
                {
                    Id = q.Id,
                    FullName = q.FullName,
                    Email = q.Email,
                    Phone = q.Phone,
                    ProjectType = q.ProjectType,
                    EstimatedBudget = q.EstimatedBudget,
                    Status = q.Status,
                    CreatedAt = q.CreatedAt,
                    UserName = q.User != null ? q.User.FullName : null
                })
                .ToListAsync();

            var recentMessages = await _db.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .ToListAsync();

            var latestProjects = await _db.Projects
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new ProjectListDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    CoverImageUrl = p.CoverImageUrl,
                    CategoryName = p.Category != null ? p.Category.Name : "",
                    Status = p.Status,
                    ProgressPercentage = p.ProgressPercentage
                })
                .ToListAsync();

            var recentUsers = await _db.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Status Distribution Chart Data
            var planningCount = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.Planning);
            var pausedCount = await _db.Projects.CountAsync(p => p.Status == ProjectStatus.Paused);

            var model = new AdminDashboardDto
            {
                TotalProjects = totalProjects,
                OngoingProjects = ongoingProjects,
                CompletedProjects = completedProjects,
                TotalUsers = totalUsers,
                TotalQuotes = totalQuotes,
                PendingQuotes = pendingQuotes,
                UnreadMessages = unreadMessages,
                PendingReviews = pendingReviews,
                RecentQuotes = recentQuotes,
                RecentMessages = recentMessages,
                LatestProjects = latestProjects,
                RecentUsers = recentUsers,
                MonthlyLabels = new List<string> { "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim" },
                MonthlyQuotesData = new List<int> { 8, 14, 19, 24, 31, totalQuotes > 0 ? totalQuotes : 12 },
                ProjectStatusDistribution = new List<int> { planningCount, ongoingProjects, completedProjects, pausedCount }
            };

            return View(model);
        }
    }
}
