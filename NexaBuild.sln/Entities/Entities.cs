using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace NexaBuild.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<QuoteRequest> QuoteRequests { get; set; } = new List<QuoteRequest>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();
    }

    public enum ProjectStatus
    {
        Planning = 0,    // Planlama
        Ongoing = 1,     // Devam Eden
        Completed = 2,   // Tamamlanan
        Paused = 3       // Duraklatıldı
    }

    public class ProjectCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    }

    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string ProjectType { get; set; } = string.Empty; // Konut, Ticari, vb.
        public string? TotalArea { get; set; } // Örn: 45.000 m²
        public string? NumberOfUnits { get; set; } // Örn: 180 Daire + 12 Ticari
        public ProjectStatus Status { get; set; } = ProjectStatus.Ongoing;
        public int ProgressPercentage { get; set; } = 0; // 0 - 100
        public bool IsFeatured { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CategoryId { get; set; }
        public virtual ProjectCategory? Category { get; set; }

        public virtual ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();
        public virtual ICollection<ProjectStage> Stages { get; set; } = new List<ProjectStage>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }

    public class ProjectImage
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public virtual Project? Project { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public bool IsCover { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
    }

    public enum StageStatus
    {
        Pending = 0,     // Beklemede
        InProgress = 1,  // Devam Ediyor
        Completed = 2    // Tamamlandı
    }

    public class ProjectStage
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public virtual Project? Project { get; set; }
        public string Name { get; set; } = string.Empty; // Temel, Kaba İnşaat vb.
        public int Order { get; set; } = 0;
        public StageStatus Status { get; set; } = StageStatus.Pending;
        public int ProgressPercentage { get; set; } = 0; // 0-100
        public DateTime? CompletionDate { get; set; }
        public string? Description { get; set; }
    }

    public class Service
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Icon { get; set; } // Bi-icon or svg class
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? Features { get; set; } // JSON or newline-separated features
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    public class BlogCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }

    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public string Author { get; set; } = "NexaBuild Mimarlık";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsPublished { get; set; } = true;
        public DateTime? PublishedAt { get; set; } = DateTime.UtcNow;
        public int ViewCount { get; set; } = 0;

        public int CategoryId { get; set; }
        public virtual BlogCategory? Category { get; set; }
    }

    public enum QuoteStatus
    {
        New = 0,        // Yeni Talep
        Reviewing = 1,  // İnceleniyor
        Contacted = 2,  // İletişime Geçildi
        Completed = 3,  // Teklif Sunuldu / Tamamlandı
        Rejected = 4    // İptal / Reddedildi
    }

    public class QuoteRequest
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ProjectType { get; set; } = string.Empty;
        public string? EstimatedBudget { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AttachmentPath { get; set; }
        public QuoteStatus Status { get; set; } = QuoteStatus.New;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? AdminNote { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }

    public enum MessageStatus
    {
        New = 0,
        Read = 1,
        Replied = 2
    }

    public class ContactMessage
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public MessageStatus Status { get; set; } = MessageStatus.New;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? AdminReply { get; set; }
        public DateTime? RepliedAt { get; set; }
    }

    public enum ReviewStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public class Testimonial
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; } // Örn: Mülk Sahibi, Yatırımcı
        public string? Company { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; } = 5; // 1-5
        public string? AvatarUrl { get; set; }
        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }

    public class Favorite
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }

        public int ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SiteSetting
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "NexaBuild İnşaat & Mimarlık";
        public string? LogoUrl { get; set; }
        public string Phone { get; set; } = "+90 (212) 850 44 00";
        public string Email { get; set; } = "info@nexabuild.com";
        public string Address { get; set; } = "Büyükdere Cad. No: 195, Levent, İstanbul";
        public string? MapEmbedUrl { get; set; } = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3007.6974100684615!2d29.009477676643224!3d41.07560597134105!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x14cab65d6484e93d%3A0x868b4ad13e61a343!2sLevent%2C%20B%C3%BCy%C3%BCkdere%20Cd.%2C%20Be%C5%9Fikta%C5%9F%2F%C4%B0stanbul!5e0!3m2!1str!2str!4v1700000000000!5m2!1str!2str";
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? YouTubeUrl { get; set; }
        public string HeroTitle { get; set; } = "GELECEĞİN MİMARİSİ, KUSURSUZ İNŞAAT";
        public string HeroSubtitle { get; set; } = "Modern mühendislik, sürdürülebilir lüks ve zamansız estetikle geleceğin yaşam ve iş alanlarını inşa ediyoruz.";
        public string WorkingHours { get; set; } = "Pzt - Cmt: 08:30 - 18:30";
        public string MetaDescription { get; set; } = "NexaBuild - Premium İnşaat, Mimari Tasarım ve Taahhüt Çözümleri";

        // İstatistikler
        public int StatsProjectsCompleted { get; set; } = 148;
        public int StatsYearsExperience { get; set; } = 24;
        public int StatsExpertEngineers { get; set; } = 85;
        public int StatsSatisfiedClients { get; set; } = 320;
    }
}
