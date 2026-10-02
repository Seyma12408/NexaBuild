using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NexaBuild.DataAccess;
using NexaBuild.DTOs;
using NexaBuild.Entities;
using NexaBuild.Services;

namespace NexaBuild.Controllers
{
    public class QuoteController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        public QuoteController(ApplicationDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new QuoteRequestCreateDto();
            if (User.Identity?.IsAuthenticated == true)
            {
                // Giriş yapmış kullanıcının adını/emailini formda hazır getirelim
                model.FullName = User.Identity.Name ?? "";
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(QuoteRequestCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string? attachmentPath = null;
                if (model.AttachmentFile != null)
                {
                    attachmentPath = await _fileService.UploadFileAsync(model.AttachmentFile, "quote_attachments");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var quote = new QuoteRequest
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    ProjectType = model.ProjectType,
                    EstimatedBudget = model.EstimatedBudget,
                    Message = model.Message,
                    AttachmentPath = attachmentPath,
                    Status = QuoteStatus.New,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                _db.QuoteRequests.Add(quote);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Teklif talebiniz başarıyla alındı! Uzman mühendislerimiz en geç 24 saat içinde sizinle iletişime geçecektir.";
                
                if (User.Identity?.IsAuthenticated == true)
                {
                    return RedirectToAction("Quotes", "Account");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Talep iletilirken bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }
    }
}
