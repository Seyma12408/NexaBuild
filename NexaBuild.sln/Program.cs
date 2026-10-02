using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexaBuild.DataAccess;
using NexaBuild.Entities;
using NexaBuild.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE CONFIGURATION
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? "Data Source=nexabuild.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // SQLite varsayılan olarak her ortamda kurulumsuz ve stabil çalışır
    options.UseSqlite(connectionString);
});

// 2. IDENTITY CONFIGURATION
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. COOKIE AUTHENTICATION CONFIGURATION
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "NexaBuild.Auth";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// 4. BUSINESS SERVICES REGISTRATION
builder.Services.AddScoped<ISlugService, SlugService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ISiteSettingService, SiteSettingService>();

// 5. MVC CONTROLLERS & VIEWS
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 6. DATABASE SEEDING
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DbInitializer.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı başlatılırken ve tohumlama yapılırken bir hata oluştu.");
    }
}

// 7. PIPELINE & MIDDLEWARE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Home/ErrorStatus", "?code={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 8. ROUTING MAPS

// Admin Area Route (/admin, /admin/projects vb.)
app.MapAreaControllerRoute(
    name: "AdminArea",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

// Custom Friendly Public Routes
app.MapControllerRoute(
    name: "projectDetail",
    pattern: "projeler/{slug}",
    defaults: new { controller = "Projects", action = "Detail" });

app.MapControllerRoute(
    name: "projectsList",
    pattern: "projeler",
    defaults: new { controller = "Projects", action = "Index" });

app.MapControllerRoute(
    name: "serviceDetail",
    pattern: "hizmetler/{slug}",
    defaults: new { controller = "Services", action = "Detail" });

app.MapControllerRoute(
    name: "servicesList",
    pattern: "hizmetler",
    defaults: new { controller = "Services", action = "Index" });

app.MapControllerRoute(
    name: "blogDetail",
    pattern: "blog/{slug}",
    defaults: new { controller = "Blog", action = "Detail" });

app.MapControllerRoute(
    name: "blogList",
    pattern: "blog",
    defaults: new { controller = "Blog", action = "Index" });

app.MapControllerRoute(
    name: "quoteRoute",
    pattern: "teklif-al",
    defaults: new { controller = "Quote", action = "Index" });

app.MapControllerRoute(
    name: "contactRoute",
    pattern: "iletisim",
    defaults: new { controller = "Contact", action = "Index" });

app.MapControllerRoute(
    name: "aboutRoute",
    pattern: "hakkimizda",
    defaults: new { controller = "Home", action = "About" });

// Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
