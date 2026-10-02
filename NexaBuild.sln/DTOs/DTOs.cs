using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using NexaBuild.Entities;

namespace NexaBuild.DTOs
{
    // ==================== PROJECT DTOs ====================
    public class ProjectCreateDto
    {
        [Required(ErrorMessage = "Proje adı zorunludur.")]
        [StringLength(200, ErrorMessage = "Proje adı en fazla 200 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kısa açıklama zorunludur.")]
        [StringLength(500, ErrorMessage = "Kısa açıklama en fazla 500 karakter olabilir.")]
        public string ShortDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Proje detay açıklaması zorunludur.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lokasyon bilgisi zorunludur.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori seçilmelidir.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Proje türü zorunludur.")]
        public string ProjectType { get; set; } = "Konut";

        public string? TotalArea { get; set; }
        public string? NumberOfUnits { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Ongoing;
        public int ProgressPercentage { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;

        public DateTime? StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public IFormFile? CoverImageFile { get; set; }
        public List<IFormFile>? GalleryFiles { get; set; }
    }

    public class ProjectUpdateDto : ProjectCreateDto
    {
        public int Id { get; set; }
        public string? ExistingCoverImageUrl { get; set; }
    }

    public class ProjectListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string ProjectType { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; }
        public int ProgressPercentage { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsFavorited { get; set; }
    }

    public class ProjectDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string ProjectType { get; set; } = string.Empty;
        public string? TotalArea { get; set; }
        public string? NumberOfUnits { get; set; }
        public ProjectStatus Status { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public bool IsFavorited { get; set; }

        public List<ProjectImageDto> Images { get; set; } = new();
        public List<ProjectStageDto> Stages { get; set; } = new();
        public List<ProjectListDto> RelatedProjects { get; set; } = new();
    }

    public class ProjectImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public bool IsCover { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ProjectStageDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public StageStatus Status { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? Description { get; set; }
    }

    // ==================== SERVICE DTOs ====================
    public class ServiceDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet başlığı zorunludur.")]
        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;
        public string? Icon { get; set; }

        [Required(ErrorMessage = "Özet bilgi zorunludur.")]
        public string Summary { get; set; } = string.Empty;

        [Required(ErrorMessage = "Detaylı açıklama zorunludur.")]
        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }

        public string? Features { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    // ==================== BLOG DTOs ====================
    public class BlogPostDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yazı başlığı zorunludur.")]
        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "Özet zorunludur.")]
        public string Summary { get; set; } = string.Empty;

        [Required(ErrorMessage = "İçerik zorunludur.")]
        public string Content { get; set; } = string.Empty;

        public string? CoverImageUrl { get; set; }
        public IFormFile? CoverImageFile { get; set; }

        public string Author { get; set; } = "NexaBuild Mimarlık";

        [Required(ErrorMessage = "Kategori seçiniz.")]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; }
    }

    // ==================== QUOTE REQUEST DTOs ====================
    public class QuoteRequestCreateDto
    {
        [Required(ErrorMessage = "Adınız ve soyadınız gereklidir.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası gereklidir.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Proje türünü seçiniz.")]
        public string ProjectType { get; set; } = "Konut";

        public string? EstimatedBudget { get; set; }

        [Required(ErrorMessage = "Mesajınız / proje detayınız gereklidir.")]
        public string Message { get; set; } = string.Empty;

        public IFormFile? AttachmentFile { get; set; }
    }

    public class QuoteRequestListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ProjectType { get; set; } = string.Empty;
        public string? EstimatedBudget { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AttachmentPath { get; set; }
        public QuoteStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AdminNote { get; set; }
        public string? UserName { get; set; }
    }

    // ==================== CONTACT DTOs ====================
    public class ContactMessageCreateDto
    {
        [Required(ErrorMessage = "Adınız ve soyadınız gereklidir.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required(ErrorMessage = "Konu gereklidir.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mesajınız gereklidir.")]
        public string Message { get; set; } = string.Empty;
    }

    // ==================== TESTIMONIAL DTOs ====================
    public class TestimonialCreateDto
    {
        [Required(ErrorMessage = "Ad ve Soyad gereklidir.")]
        public string FullName { get; set; } = string.Empty;

        public string? Position { get; set; }
        public string? Company { get; set; }

        [Required(ErrorMessage = "Yorumunuz gereklidir.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Yorum 10 ile 1000 karakter arasında olmalıdır.")]
        public string Comment { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Rating { get; set; } = 5;

        public IFormFile? AvatarFile { get; set; }
    }

    // ==================== ACCOUNT DTOs ====================
    public class LoginDto
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
        public string? ReturnUrl { get; set; }
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Ad Soyad gereklidir.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ProfileUpdateDto
    {
        [Required(ErrorMessage = "Ad Soyad gereklidir.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public string? CurrentAvatarUrl { get; set; }

        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
    }

    // ==================== ADMIN DASHBOARD DTOs ====================
    public class AdminDashboardDto
    {
        public int TotalProjects { get; set; }
        public int OngoingProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int TotalUsers { get; set; }
        public int TotalQuotes { get; set; }
        public int PendingQuotes { get; set; }
        public int UnreadMessages { get; set; }
        public int PendingReviews { get; set; }

        public List<QuoteRequestListDto> RecentQuotes { get; set; } = new();
        public List<ContactMessage> RecentMessages { get; set; } = new();
        public List<ProjectListDto> LatestProjects { get; set; } = new();
        public List<ApplicationUser> RecentUsers { get; set; } = new();

        // Chart Data
        public List<string> MonthlyLabels { get; set; } = new();
        public List<int> MonthlyQuotesData { get; set; } = new();
        public List<int> ProjectStatusDistribution { get; set; } = new(); // Planning, Ongoing, Completed, Paused
    }
}
