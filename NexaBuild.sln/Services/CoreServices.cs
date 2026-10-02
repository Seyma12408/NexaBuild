using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.Entities;

namespace NexaBuild.Services
{
    // ==================== SLUG SERVICE ====================
    public interface ISlugService
    {
        string GenerateSlug(string title);
    }

    public class SlugService : ISlugService
    {
        public string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Guid.NewGuid().ToString("N").Substring(0, 8);

            string str = title.ToLowerInvariant();

            // Türkçe karakter dönüşümleri
            str = str.Replace("ç", "c")
                     .Replace("ğ", "g")
                     .Replace("ı", "i")
                     .Replace("ö", "o")
                     .Replace("ş", "s")
                     .Replace("ü", "u");

            // Geçersiz karakterleri temizle
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // Birden fazla boşluğu tek boşluğa indir
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // Boşlukları tire yap
            str = Regex.Replace(str, @"\s", "-");
            // Birden fazla tireyi tek tire yap
            str = Regex.Replace(str, @"-+", "-");

            return string.IsNullOrEmpty(str) ? Guid.NewGuid().ToString("N").Substring(0, 8) : str;
        }
    }

    // ==================== FILE SERVICE ====================
    public interface IFileService
    {
        Task<string?> UploadImageAsync(IFormFile? file, string subFolder = "uploads");
        Task<string?> UploadFileAsync(IFormFile? file, string subFolder = "attachments");
        void DeleteFile(string? relativePath);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".svg" };
        private readonly string[] _allowedDocExtensions = { ".pdf", ".docx", ".doc", ".zip", ".rar", ".jpg", ".png" };
        private const long MaxImageSize = 8 * 1024 * 1024; // 8 MB
        private const long MaxDocSize = 25 * 1024 * 1024; // 25 MB

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string?> UploadImageAsync(IFormFile? file, string subFolder = "uploads")
        {
            if (file == null || file.Length == 0)
                return null;

            if (file.Length > MaxImageSize)
                throw new InvalidOperationException("Görsel boyutu en fazla 8MB olabilir.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedImageExtensions.Contains(ext))
                throw new InvalidOperationException("Desteklenmeyen görsel formatı! (İzin verilen: .jpg, .jpeg, .png, .webp, .svg)");

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsDir = Path.Combine(webRoot, "uploads", subFolder);

            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsDir, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{subFolder}/{uniqueFileName}";
        }

        public async Task<string?> UploadFileAsync(IFormFile? file, string subFolder = "attachments")
        {
            if (file == null || file.Length == 0)
                return null;

            if (file.Length > MaxDocSize)
                throw new InvalidOperationException("Dosya boyutu en fazla 25MB olabilir.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedDocExtensions.Contains(ext))
                throw new InvalidOperationException("Desteklenmeyen dosya formatı!");

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsDir = Path.Combine(webRoot, "uploads", subFolder);

            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsDir, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{subFolder}/{uniqueFileName}";
        }

        public void DeleteFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            // Güvenlik: path traversal engelleme
            if (relativePath.Contains("..")) return;

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var cleanPath = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webRoot, cleanPath);

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch
                {
                    // Log or ignore
                }
            }
        }
    }

    // ==================== SITE SETTINGS SERVICE ====================
    public interface ISiteSettingService
    {
        Task<SiteSetting> GetSettingsAsync();
    }

    public class SiteSettingService : ISiteSettingService
    {
        private readonly ApplicationDbContext _db;

        public SiteSettingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SiteSetting> GetSettingsAsync()
        {
            var setting = await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new SiteSetting();
            }
            return setting;
        }
    }
}
