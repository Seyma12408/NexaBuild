using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NexaBuild.Entities;

namespace NexaBuild.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectCategory> ProjectCategories => Set<ProjectCategory>();
        public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
        public DbSet<ProjectStage> ProjectStages => Set<ProjectStage>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
        public DbSet<QuoteRequest> QuoteRequests => Set<QuoteRequest>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<Testimonial> Testimonials => Set<Testimonial>();
        public DbSet<Favorite> Favorites => Set<Favorite>();
        public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Project - Category
            builder.Entity<Project>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project - Images
            builder.Entity<ProjectImage>()
                .HasOne(pi => pi.Project)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project - Stages
            builder.Entity<ProjectStage>()
                .HasOne(ps => ps.Project)
                .WithMany(p => p.Stages)
                .HasForeignKey(ps => ps.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Blog - Category
            builder.Entity<BlogPost>()
                .HasOne(b => b.Category)
                .WithMany(c => c.BlogPosts)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Favorite - User & Project
            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Favorite>()
                .HasOne(f => f.Project)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuoteRequest - User
            builder.Entity<QuoteRequest>()
                .HasOne(q => q.User)
                .WithMany(u => u.QuoteRequests)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Testimonial - User
            builder.Entity<Testimonial>()
                .HasOne(t => t.User)
                .WithMany(u => u.Testimonials)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes for Slugs
            builder.Entity<Project>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            builder.Entity<BlogPost>()
                .HasIndex(b => b.Slug)
                .IsUnique();

            builder.Entity<ProjectCategory>()
                .HasIndex(c => c.Slug);

            builder.Entity<BlogCategory>()
                .HasIndex(c => c.Slug);

            builder.Entity<Service>()
                .HasIndex(s => s.Slug);

            // Composite Index for Favorites
            builder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.ProjectId })
                .IsUnique();
        }
    }
}
