using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.DTOs;
using NexaBuild.Entities;
using NexaBuild.Services;

namespace NexaBuild.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db,
            IFileService fileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _fileService = fileService;
        }

        // ==================== AUTH ACTIONS ====================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginDto { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("", "E-posta adresi veya şifre hatalı.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                return RedirectToAction("Profile");
            }

            ModelState.AddModelError("", "E-posta adresi veya şifre hatalı.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanımda.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Member");
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = "Hesabınız başarıyla oluşturuldu! Hoş geldiniz.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "Güvenli çıkış yapıldı.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ==================== USER PANEL ACTIONS ====================

        [Authorize]
        [HttpGet("profil")]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var model = new ProfileUpdateDto
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                CurrentAvatarUrl = user.AvatarUrl
            };

            return View(model);
        }

        [Authorize]
        [HttpPost("profil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileUpdateDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            model.CurrentAvatarUrl = user.AvatarUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Profil bilgilerini güncelle
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            // Avatar yükleme
            if (model.AvatarFile != null)
            {
                var avatarPath = await _fileService.UploadImageAsync(model.AvatarFile, "avatars");
                if (avatarPath != null)
                {
                    user.AvatarUrl = avatarPath;
                }
            }

            // Şifre değiştirme istenmişse
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Şifre değiştirmek için mevcut şifrenizi girmelisiniz.");
                    return View(model);
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                return RedirectToAction("Profile");
            }

            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [Authorize]
        [HttpGet("profil/tekliflerim")]
        public async Task<IActionResult> Quotes()
        {
            var userId = _userManager.GetUserId(User);
            var quotes = await _db.QuoteRequests
                .Where(q => q.UserId == userId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(quotes);
        }

        [Authorize]
        [HttpGet("profil/favoriler")]
        public async Task<IActionResult> Favorites()
        {
            var userId = _userManager.GetUserId(User);
            var favorites = await _db.Favorites
                .Include(f => f.Project)
                    .ThenInclude(p => p.Category)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(favorites);
        }

        [Authorize]
        [HttpGet("profil/yorumlarim")]
        public async Task<IActionResult> Reviews()
        {
            var userId = _userManager.GetUserId(User);
            var reviews = await _db.Testimonials
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        [Authorize]
        [HttpPost("profil/yorum-gonder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(TestimonialCreateDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm zorunlu alanları doldurunuz.";
                return RedirectToAction("Reviews");
            }

            string? avatarUrl = user.AvatarUrl;
            if (model.AvatarFile != null)
            {
                avatarUrl = await _fileService.UploadImageAsync(model.AvatarFile, "reviews");
            }

            var testimonial = new Testimonial
            {
                FullName = model.FullName,
                Position = model.Position,
                Company = model.Company,
                Comment = model.Comment,
                Rating = model.Rating,
                AvatarUrl = avatarUrl,
                Status = ReviewStatus.Pending, // Admin onayı bekleyecek
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            };

            _db.Testimonials.Add(testimonial);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Değerlendirmeniz alındı! Yönetici onayından sonra web sitesinde yayınlanacaktır.";
            return RedirectToAction("Reviews");
        }

        // ==================== FAVORITE AJAX ENDPOINT ====================
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int projectId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var project = await _db.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound(new { success = false, message = "Proje bulunamadı." });
            }

            var fav = await _db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ProjectId == projectId);
            bool isFavorited;

            if (fav != null)
            {
                _db.Favorites.Remove(fav);
                isFavorited = false;
            }
            else
            {
                _db.Favorites.Add(new Favorite
                {
                    UserId = userId,
                    ProjectId = projectId,
                    CreatedAt = DateTime.UtcNow
                });
                isFavorited = true;
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, isFavorited });
        }
    }
}
