using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexaBuild.Entities;

namespace NexaBuild.DataAccess
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Veritabanının oluşturulması / migrasyonu
            await context.Database.EnsureCreatedAsync();

            // 1. ROLLER
            string[] roles = { "Admin", "Member" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. ADMIN KULLANICISI
            var adminEmail = "admin@nexabuild.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Başmimar Selim Aksoy",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    AvatarUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=400&q=80"
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!*");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. DEMO KULLANICI
            var demoEmail = "demo@nexabuild.com";
            var demoUser = await userManager.FindByEmailAsync(demoEmail);
            if (demoUser == null)
            {
                demoUser = new ApplicationUser
                {
                    UserName = demoEmail,
                    Email = demoEmail,
                    FullName = "Eren Yılmaz (Yatırımcı)",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    AvatarUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=400&q=80"
                };
                var result = await userManager.CreateAsync(demoUser, "User123!*");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(demoUser, "Member");
                }
            }

            // 4. SITE SETTINGS
            if (!await context.SiteSettings.AnyAsync())
            {
                context.SiteSettings.Add(new SiteSetting
                {
                    CompanyName = "NexaBuild Mimarlık & İnşaat",
                    LogoUrl = "/images/logo.png",
                    Phone = "+90 (212) 850 44 00",
                    Email = "info@nexabuild.com",
                    Address = "Büyükdere Caddesi No: 195, Levent, Beşiktaş / İstanbul",
                    WorkingHours = "Pazartesi - Cumartesi: 08:30 - 18:30",
                    HeroTitle = "ZAMANSIZ MİMARİ, KUSURSUZ MÜHENDİSLİK",
                    HeroSubtitle = "NexaBuild; lüks rezidanslardan dev sanayi yapılarına, kentsel dönüşümden butik villalara kadar modern dünyanın en prestijli yaşam ve ticaret alanlarını inşa eder.",
                    MetaDescription = "NexaBuild Mimarlık ve İnşaat - Türkiye'nin öncü lüks konut, ticari proje ve taahhüt platformu.",
                    FacebookUrl = "https://facebook.com",
                    InstagramUrl = "https://instagram.com",
                    LinkedInUrl = "https://linkedin.com",
                    YouTubeUrl = "https://youtube.com",
                    StatsProjectsCompleted = 164,
                    StatsYearsExperience = 26,
                    StatsExpertEngineers = 92,
                    StatsSatisfiedClients = 480
                });
                await context.SaveChangesAsync();
            }

            // 5. PROJECT CATEGORIES
            if (!await context.ProjectCategories.AnyAsync())
            {
                var categories = new List<ProjectCategory>
                {
                    new ProjectCategory { Name = "Lüks Konut & Rezidans", Slug = "luks-konut-rezidans", Description = "Modern yaşam standartlarında lüks konut ve rezidans projeleri.", DisplayOrder = 1, IsActive = true },
                    new ProjectCategory { Name = "Ticari & Karma Yaşam", Slug = "ticari-karma-yasam", Description = "A+ Ofis kuleleri, iş merkezleri ve karma yaşam alanları.", DisplayOrder = 2, IsActive = true },
                    new ProjectCategory { Name = "Villa & Özel Mimari", Slug = "villa-ozel-mimari", Description = "Doğayla bütünleşik, kişiye özel ultra lüks villa ve konaklar.", DisplayOrder = 3, IsActive = true },
                    new ProjectCategory { Name = "Endüstriyel Tesisler", Slug = "endustriyel-tesisler", Description = "Yüksek teknolojiye sahip fabrika ve lojistik tesisleri.", DisplayOrder = 4, IsActive = true },
                    new ProjectCategory { Name = "Kentsel Dönüşüm & Restorasyon", Slug = "kentsel-donusum-restorasyon", Description = "Tarihi mirasın korunması ve modern kentsel yenileme projeleri.", DisplayOrder = 5, IsActive = true }
                };
                context.ProjectCategories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // 6. PROJECTS & GALLERY & STAGES
            if (!await context.Projects.AnyAsync())
            {
                var catResidential = await context.ProjectCategories.FirstAsync(c => c.Slug == "luks-konut-rezidans");
                var catCommercial = await context.ProjectCategories.FirstAsync(c => c.Slug == "ticari-karma-yasam");
                var catVilla = await context.ProjectCategories.FirstAsync(c => c.Slug == "villa-ozel-mimari");
                var catIndustrial = await context.ProjectCategories.FirstAsync(c => c.Slug == "endustriyel-tesisler");
                var catRestoration = await context.ProjectCategories.FirstAsync(c => c.Slug == "kentsel-donusum-restorasyon");

                var projects = new List<Project>
                {
                    new Project
                    {
                        Title = "Mersin Marina Luxury Residence",
                        Slug = "mersin-marina-luxury-residence",
                        ShortDescription = "Akdeniz'in kalbinde, panoramik deniz manzaralı 24 katlı 180 ultra lüks daire ve marina yaşamı.",
                        Description = "Mersin Marina Luxury Residence projemiz, çağdaş mimarinin en seçkin örneklerinden biri olarak yükseliyor. Akdeniz'in eşsiz maviliğini kucaklayan geniş teraslar, akıllı ev otomasyon sistemleri, özel spa merkezi, infinity havuzu ve özel yat iskelesi bağlantısıyla bölgenin en prestijli konut projesidir. Projede deprem güvenliği için C50 sınıfı yüksek mukavemetli beton ve fore kazık zemin güçlendirme teknikleri uygulanmıştır.",
                        CoverImageUrl = "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=1400&q=85",
                        Location = "Marina Sahil Bulvarı, Mersin",
                        ProjectType = "Lüks Rezidans & Karma",
                        TotalArea = "42.500 m²",
                        NumberOfUnits = "180 Daire + 14 Ticari Ünite",
                        Status = ProjectStatus.Ongoing,
                        ProgressPercentage = 78,
                        IsFeatured = true,
                        DisplayOrder = 1,
                        CategoryId = catResidential.Id,
                        StartDate = new DateTime(2023, 3, 1),
                        EstimatedCompletionDate = new DateTime(2025, 8, 30),
                        Images = new List<ProjectImage>
                        {
                            new ProjectImage { ImageUrl = "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=1400&q=85", IsCover = true, Caption = "Ön Cephe ve Sahil Görünümü", DisplayOrder = 1 },
                            new ProjectImage { ImageUrl = "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=1200&q=85", IsCover = false, Caption = "İç Mekan Salon & Teras Tasarımı", DisplayOrder = 2 },
                            new ProjectImage { ImageUrl = "https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?auto=format&fit=crop&w=1200&q=85", IsCover = false, Caption = "Sonsuzluk Havuzu ve Sosyal Tesis", DisplayOrder = 3 },
                            new ProjectImage { ImageUrl = "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?auto=format&fit=crop&w=1200&q=85", IsCover = false, Caption = "Gece Aydınlatması ve Marina", DisplayOrder = 4 }
                        },
                        Stages = new List<ProjectStage>
                        {
                            new ProjectStage { Name = "Zemin İyileştirme & Fore Kazık", Order = 1, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "120 adet 32 metre derinliğinde fore kazık uygulaması tamamlandı." },
                            new ProjectStage { Name = "Temel & Radye Jeneratör Sistemi", Order = 2, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "C50 sınıfı su geçirimsiz beton dökümü başarıyla bitirildi." },
                            new ProjectStage { Name = "Kaba Yapı & Taşıyıcı Sistem", Order = 3, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "24 katın betonarme karkas imalatı tamamlandı." },
                            new ProjectStage { Name = "Elektromekanik & HVAC Tesisatı", Order = 4, Status = StageStatus.InProgress, ProgressPercentage = 85, Description = "Akıllı bina ve VRF iklimlendirme kanalları döşeniyor." },
                            new ProjectStage { Name = "Cephe & Isı Yalıtım Sistemleri", Order = 5, Status = StageStatus.InProgress, ProgressPercentage = 75, Description = "Schüco alüminyum cephe ve lamine ısıcam montajı sürüyor." },
                            new ProjectStage { Name = "İç Mimari & İnce İşçilik", Order = 6, Status = StageStatus.InProgress, ProgressPercentage = 50, Description = "İtalyan mermer zeminler ve mutfak mobilyaları montajda." },
                            new ProjectStage { Name = "Peyzaj, Sosyal Alanlar & Teslim", Order = 7, Status = StageStatus.Pending, ProgressPercentage = 15, Description = "Açık havuz ve botanik bahçe düzenlemeleri hazırlanıyor." }
                        }
                    },
                    new Project
                    {
                        Title = "Levent Sky Tower Karma Yaşam Merkezi",
                        Slug = "levent-sky-tower-karma-yasam-merkezi",
                        ShortDescription = "İstanbul finans merkezinde, LEED Platinum sertifikalı 38 katlı akıllı ofis ve ticari yaşam kulesi.",
                        Description = "Levent Sky Tower, modern iş dünyasının tüm ihtiyaçlarını sürdürülebilir yeşil bina standartlarıyla buluşturuyor. Kendi enerjisinin %35'ini güneş ve kinetik sistemlerle üreten, yağmur suyu geri kazanımlı ve sismik izolatörlü yapı teknolojisiyle İstanbul silüetinin en dikkat çeken anıt yapılarından biridir.",
                        CoverImageUrl = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?auto=format&fit=crop&w=1400&q=85",
                        Location = "Büyükdere Caddesi, Levent / İstanbul",
                        ProjectType = "A+ Ofis & Ticaret Kulesi",
                        TotalArea = "85.000 m²",
                        NumberOfUnits = "120 Ofis + 28 Premium Mağaza",
                        Status = ProjectStatus.Completed,
                        ProgressPercentage = 100,
                        IsFeatured = true,
                        DisplayOrder = 2,
                        CategoryId = catCommercial.Id,
                        StartDate = new DateTime(2021, 6, 15),
                        CompletionDate = new DateTime(2024, 2, 10),
                        Images = new List<ProjectImage>
                        {
                            new ProjectImage { ImageUrl = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?auto=format&fit=crop&w=1400&q=85", IsCover = true, Caption = "Kule Dış Cephe Perspektifi", DisplayOrder = 1 },
                            new ProjectImage { ImageUrl = "https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=1200&q=85", IsCover = false, Caption = "Lobi ve Karşılama Alanı", DisplayOrder = 2 },
                            new ProjectImage { ImageUrl = "https://images.unsplash.com/photo-1497366811353-6870744d04b2?auto=format&fit=crop&w=1200&q=85", IsCover = false, Caption = "Executive Yönetim Ofisleri", DisplayOrder = 3 }
                        },
                        Stages = new List<ProjectStage>
                        {
                            new ProjectStage { Name = "Zemin & Derin Kazı İksa", Order = 1, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "Diyafram duvar ve ankraj işlemleri." },
                            new ProjectStage { Name = "Karkas ve Çelik Konstrüksiyon", Order = 2, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "38 kat çelik-betonarme kompozit yapı." },
                            new ProjectStage { Name = "LEED Platinum Akıllı Cephe", Order = 3, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "Düşük emisyonlu cam cephe tamamlandı." },
                            new ProjectStage { Name = "Kullanıma Açılış ve Teslim", Order = 4, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "İskan belgesi alınarak firmalara teslim edildi." }
                        }
                    },
                    new Project
                    {
                        Title = "Bodrum Horizon Private Luxury Villas",
                        Slug = "bodrum-horizon-private-luxury-villas",
                        ShortDescription = "Yalıkavak koyunda, kesintisiz gün batımı manzaralı 12 adet müstakil havuzlu ve helipedli malikane.",
                        Description = "Doğal taş ve modern ahşabın harmonisiyle şekillenen Bodrum Horizon Villas; her biri 1.500 m² arsa payına sahip, özel koya direkt erişimi olan, akıllı otomasyonlu ve özel peyzajlı 12 adet ultra lüks malikaneden oluşmaktadır.",
                        CoverImageUrl = "https://images.unsplash.com/photo-1613977257363-707ba9348227?auto=format&fit=crop&w=1400&q=85",
                        Location = "Yalıkavak, Bodrum / Muğla",
                        ProjectType = "Müstakil Lüks Malikane",
                        TotalArea = "18.000 m²",
                        NumberOfUnits = "12 Müstakil Villa",
                        Status = ProjectStatus.Ongoing,
                        ProgressPercentage = 92,
                        IsFeatured = true,
                        DisplayOrder = 3,
                        CategoryId = catVilla.Id,
                        StartDate = new DateTime(2023, 10, 1),
                        EstimatedCompletionDate = new DateTime(2025, 4, 15),
                        Images = new List<ProjectImage>
                        {
                            new ProjectImage { ImageUrl = "https://images.unsplash.com/photo-1613977257363-707ba9348227?auto=format&fit=crop&w=1400&q=85", IsCover = true, Caption = "Özel Sonsuzluk Havuzu ve Villa Cephesi", DisplayOrder = 1 },
                            new ProjectImage { ImageUrl = "https://images.unsplash.com/photo-1600566753376-12c8ab7fb75b?auto=format&fit=crop&w=1200&q=85", IsCover = false, Caption = "Geniş Salon ve Deniz Manzarası", DisplayOrder = 2 }
                        },
                        Stages = new List<ProjectStage>
                        {
                            new ProjectStage { Name = "Arazi Teraslama & Kaba Yapı", Order = 1, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "Bodrum doğal taş duvar örgüsü ve betonarme." },
                            new ProjectStage { Name = "İnce İşçilik & Özel Mermer Kaplama", Order = 2, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "İthal traverten ve teak zemin kaplaması." },
                            new ProjectStage { Name = "Botanik Bahçe & Havuz Otomasyonu", Order = 3, Status = StageStatus.InProgress, ProgressPercentage = 85, Description = "Özel zeytin ağaçları ve peyzaj ışıklandırması." }
                        }
                    },
                    new Project
                    {
                        Title = "Bursa Sanayi & Yüksek Teknoloji Lojistik Kampüsü",
                        Slug = "bursa-sanayi-lojistik-kampusu",
                        ShortDescription = "Otomotiv yan sanayi ve yeşil lojistik için tasarlanmış 120.000 m² kapalı alana sahip akıllı tesis.",
                        Description = "Endüstri 4.0 altyapısına uygun ağır sanayi zemin yük kapasitesi, yüksek tavanlı otomatik depolama kuleleri, güneş enerji panelleri ve sıfır atık arıtma tesisleriyle donatılmış örnek endüstriyel proje.",
                        CoverImageUrl = "https://images.unsplash.com/photo-1581094794329-c8112a89af12?auto=format&fit=crop&w=1400&q=85",
                        Location = "Nilüfer OSB, Bursa",
                        ProjectType = "Ağır Sanayi & Lojistik",
                        TotalArea = "120.000 m²",
                        NumberOfUnits = "4 Bağımsız Fabrika Bloğu",
                        Status = ProjectStatus.Completed,
                        ProgressPercentage = 100,
                        IsFeatured = false,
                        DisplayOrder = 4,
                        CategoryId = catIndustrial.Id,
                        StartDate = new DateTime(2022, 1, 10),
                        CompletionDate = new DateTime(2023, 11, 20),
                        Images = new List<ProjectImage>
                        {
                            new ProjectImage { ImageUrl = "https://images.unsplash.com/photo-1581094794329-c8112a89af12?auto=format&fit=crop&w=1400&q=85", IsCover = true, Caption = "Tesis Genel Görünümü", DisplayOrder = 1 }
                        },
                        Stages = new List<ProjectStage>
                        {
                            new ProjectStage { Name = "Prefabrik ve Çelik İmalat", Order = 1, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "Öngerilmeli beton kiriş montajı." },
                            new ProjectStage { Name = "Çatı GES ve Mekanik", Order = 2, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "4 MW güneş enerjisi kurulumu." }
                        }
                    },
                    new Project
                    {
                        Title = "Karaköy Tarihi Liman Hanı Restorasyonu",
                        Slug = "karakoy-tarihi-liman-hani-restorasyonu",
                        ShortDescription = "19. yüzyıldan kalma tarihi tescilli taş yapının butik otel ve sanat merkezine dönüşümü.",
                        Description = "Kültür Varlıkları Koruma Kurulu onaylı restorasyon projemizde, tarihi taş ve tuğla dokular geleneksel horasan harcı ile güçlendirilmiş, çelik güçlendirme elemanlarıyla depreme dayanıklı butik bir sanat ve konaklama merkezine kavuşturulmuştur.",
                        CoverImageUrl = "https://images.unsplash.com/photo-1513694203232-719a280e022f?auto=format&fit=crop&w=1400&q=85",
                        Location = "Kemankeş Cad., Karaköy / İstanbul",
                        ProjectType = "Tarihi Eser Restorasyonu",
                        TotalArea = "6.800 m²",
                        NumberOfUnits = "35 Süit + Sanat Galerisi",
                        Status = ProjectStatus.Completed,
                        ProgressPercentage = 100,
                        IsFeatured = true,
                        DisplayOrder = 5,
                        CategoryId = catRestoration.Id,
                        StartDate = new DateTime(2022, 4, 1),
                        CompletionDate = new DateTime(2024, 1, 15),
                        Images = new List<ProjectImage>
                        {
                            new ProjectImage { ImageUrl = "https://images.unsplash.com/photo-1513694203232-719a280e022f?auto=format&fit=crop&w=1400&q=85", IsCover = true, Caption = "Tarihi Taş Avlu ve Restorasyon", DisplayOrder = 1 }
                        },
                        Stages = new List<ProjectStage>
                        {
                            new ProjectStage { Name = "Tarihi Duvar Enjeksiyonu & Askıya Alma", Order = 1, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "Mikro enjeksiyon sağlamlaştırma." },
                            new ProjectStage { Name = "Ahşap Çatı ve Tonoz Onarımı", Order = 2, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "Geleneksel tekniklerle rekonstrüksiyon." }
                        }
                    },
                    new Project
                    {
                        Title = "Çamlıca Panorama Konakları",
                        Slug = "camlica-panorama-konaklari",
                        ShortDescription = "Boğaz ve tarihi yarımada manzarasına hakim, yatay mimarili 64 prestijli lüks daire.",
                        Description = "İstanbul'un en köklü lokasyonlarından Çamlıca'da, 5 katı geçmeyen yatay mimari anlayışıyla, geniş bahçe ve çatı dublekslerinden oluşan projemiz doğa ve huzuru şehir merkezinde sunuyor.",
                        CoverImageUrl = "https://images.unsplash.com/photo-1512915922686-57c11dde9b6b?auto=format&fit=crop&w=1400&q=85",
                        Location = "Büyük Çamlıca, Üsküdar / İstanbul",
                        ProjectType = "Lüks Konut Sitesi",
                        TotalArea = "34.000 m²",
                        NumberOfUnits = "64 Özel Daire",
                        Status = ProjectStatus.Ongoing,
                        ProgressPercentage = 60,
                        IsFeatured = true,
                        DisplayOrder = 6,
                        CategoryId = catResidential.Id,
                        StartDate = new DateTime(2024, 1, 15),
                        EstimatedCompletionDate = new DateTime(2026, 3, 30),
                        Images = new List<ProjectImage>
                        {
                            new ProjectImage { ImageUrl = "https://images.unsplash.com/photo-1512915922686-57c11dde9b6b?auto=format&fit=crop&w=1400&q=85", IsCover = true, Caption = "Boğaz Manzaralı Bloklar", DisplayOrder = 1 }
                        },
                        Stages = new List<ProjectStage>
                        {
                            new ProjectStage { Name = "Hafriyat ve Temel İmalatı", Order = 1, Status = StageStatus.Completed, ProgressPercentage = 100, Description = "Radye jeneral temel tamamlandı." },
                            new ProjectStage { Name = "Kaba Yapı Karkas", Order = 2, Status = StageStatus.InProgress, ProgressPercentage = 80, Description = "4. kat tabliye beton dökümü yapılıyor." },
                            new ProjectStage { Name = "Dış Cephe & İnce İşler", Order = 3, Status = StageStatus.Pending, ProgressPercentage = 10, Description = "Tuğla duvar ve tesisat hazırlığı." }
                        }
                    }
                };

                context.Projects.AddRange(projects);
                await context.SaveChangesAsync();
            }

            // 7. SERVICES
            if (!await context.Services.AnyAsync())
            {
                var services = new List<Service>
                {
                    new Service
                    {
                        Title = "Lüks Konut & Rezidans İnşaatı",
                        Slug = "luks-konut-rezidans-insaati",
                        Icon = "bi-building",
                        Summary = "Modern mimarisi, akıllı ev teknolojileri ve yüksek yaşam kalitesiyle A+ lüks konut projeleri.",
                        Description = "NexaBuild olarak lüks konut anlayışını sadece estetik değil, deprem güvenliği, enerji verimliliği ve ergonomiyle harmanlıyoruz. Temelden anahtar teslimine kadar en kaliteli malzemeleri dünya standardında işçilikle buluşturuyoruz.",
                        ImageUrl = "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=800&q=80",
                        Features = "Sismik İzolasyon ve C50 Beton\nAkıllı Ev ve Bina Otomasyonu\nÖzel Akustik ve Termal Yalıtım\nSonsuzluk Havuzları ve Spa Alanları\n7/24 Entegre Güvenlik Sistemleri",
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new Service
                    {
                        Title = "Ticari Yapılar & Plaza Projeleri",
                        Slug = "ticari-yapilar-plaza-projeleri",
                        Icon = "bi-briefcase",
                        Summary = "İş dünyasının vizyonunu yansıtan A+ ofis kuleleri, iş merkezleri ve karma ticaret alanları.",
                        Description = "Ticari yapıların fonksiyonel tasarımı, çalışan verimliliğini ve marka prestijini doğrudan etkiler. LEED ve BREEAM yeşil bina kriterlerine uygun, esnek modüler ofis hacimleri ve yüksek katma değerli ticari merkezler inşa ediyoruz.",
                        ImageUrl = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?auto=format&fit=crop&w=800&q=80",
                        Features = "LEED Platinum Yeşil Bina Standartları\nModüler ve Esnek Kat Planları\nYüksek Hızlı Akıllı Asansör Sistemleri\nMerkezi VRF Havalandırma ve İklimlendirme\nKapalı Otopark ve Vale Altyapısı",
                        DisplayOrder = 2,
                        IsActive = true
                    },
                    new Service
                    {
                        Title = "Endüstriyel Tesisler & Lojistik",
                        Slug = "endustriyel-tesisler-lojistik",
                        Icon = "bi-gear-wide-connected",
                        Summary = "Ağır sanayi fabrikaları, yüksek teknoloji üretim tesisleri ve akıllı lojistik antrepolar.",
                        Description = "Endüstriyel inşaatta hız, statik dayanıklılık ve operasyonel verimlilik esastır. Çelik ve prekast yapı teknolojileriyle sıfır hata toleransında fabrikalar ve lojistik tesisler inşa ediyoruz.",
                        ImageUrl = "https://images.unsplash.com/photo-1581094794329-c8112a89af12?auto=format&fit=crop&w=800&q=80",
                        Features = "Ağır Tonajlı Endüstriyel Zemin Betonları\nÇatı Üstü GES (Güneş Enerjisi) Altyapısı\nOtomatik Yangın Sprinkler ve Algılama\nEndüstri 4.0 Otomasyon Kanalları\nGeniş Açıklıklı Çelik Konstrüksiyon",
                        DisplayOrder = 3,
                        IsActive = true
                    },
                    new Service
                    {
                        Title = "Mimari Tasarım & Proje Yönetimi",
                        Slug = "mimari-tasarim-proje-yonetimi",
                        Icon = "bi-compass",
                        Summary = "Konsept tasarımdan 3D görselleştirmeye, ruhsat projelerinden BIM modellemeye anahtar süreç.",
                        Description = "Tasarım ofisimiz, arazi analizinden başlayarak yapay zeka destekli parametrik modelleme, BIM (Building Information Modeling) koordinasyonu ve 3D fotogerçekçi görselleştirme hizmeti sunar.",
                        ImageUrl = "https://images.unsplash.com/photo-1503387762-592deb58ef4e?auto=format&fit=crop&w=800&q=80",
                        Features = "BIM ve Revit Entegre Modelleme\nFotogerçekçi 3D Render ve Animasyon\nStatik, Mekanik ve Elektrik Koordinasyonu\nBütçe ve Hakediş Yönetimi\nŞantiye Süpervizörlüğü ve Denetim",
                        DisplayOrder = 4,
                        IsActive = true
                    },
                    new Service
                    {
                        Title = "Kentsel Dönüşüm & Güçlendirme",
                        Slug = "kentsel-donusum-guclendirme",
                        Icon = "bi-shield-check",
                        Summary = "Riskli binaların yıkılıp depreme tam dayanıklı modern yaşam alanlarına dönüştürülmesi.",
                        Description = "Kentsel dönüşüm sürecinde mülk sahiplerinin haklarını güvence altına alan şeffaf sözleşme modellerimiz, kira yardımı süreçleri ve hızlı inşaat takvimimizle güvenli yuvalar inşa ediyoruz.",
                        ImageUrl = "https://images.unsplash.com/photo-1541888946425-d0fbb18086f6?auto=format&fit=crop&w=800&q=80",
                        Features = "Resmi Karot ve Zemin Etüt Raporları\nHukuki ve Noter Danışmanlığı\nAnahtar Teslim İnşaat Garantisi\nKira Desteği ve Taşınma Organizasyonu\n1. Sınıf Deprem Yönetmeliği Uyumu",
                        DisplayOrder = 5,
                        IsActive = true
                    },
                    new Service
                    {
                        Title = "Anahtar Teslim Taahhüt & Müteahhitlik",
                        Slug = "anahtar-teslim-taahhut-muteahhitlik",
                        Icon = "bi-key",
                        Summary = "Arsanızdan son vida montajına kadar tüm süreci tek elden yöneten taahhüt hizmeti.",
                        Description = "Bütçe aşımı ve gecikme risklerini ortadan kaldıran şeffaf maliyet-artı veya sabit fiyatlı taahhüt modellerimizle yatırımcılarımıza risksiz bir inşaat süreci sunuyoruz.",
                        ImageUrl = "https://images.unsplash.com/photo-1504307651254-35680f356dfd?auto=format&fit=crop&w=800&q=80",
                        Features = "Zamanında Teslim Cezai Şart Garantisi\nŞeffaf Hakediş ve Malzeme Faturalandırma\nTeknik Personel ve İSG Güvencesi\nMalzeme ve İmalat Garanti Sertifikaları\nİskan ve Ruhsat Takip Süreci",
                        DisplayOrder = 6,
                        IsActive = true
                    }
                };
                context.Services.AddRange(services);
                await context.SaveChangesAsync();
            }

            // 8. BLOG POSTS & CATEGORIES
            if (!await context.BlogCategories.AnyAsync())
            {
                var blogCat1 = new BlogCategory { Name = "Mimarlık & Trendler", Slug = "mimarlik-trendler", IsActive = true };
                var blogCat2 = new BlogCategory { Name = "İnşaat Teknolojileri", Slug = "insaat-teknolojileri", IsActive = true };
                var blogCat3 = new BlogCategory { Name = "Deprem & Güvenlik", Slug = "deprem-guvenlik", IsActive = true };
                context.BlogCategories.AddRange(blogCat1, blogCat2, blogCat3);
                await context.SaveChangesAsync();

                var blogPosts = new List<BlogPost>
                {
                    new BlogPost
                    {
                        Title = "2025'te Lüks Konut Mimarisinde Öne Çıkan 5 Temel Trend",
                        Slug = "2025-luks-konut-mimarisinde-5-temel-trend",
                        Summary = "Sürdürülebilirlik, biyofilik tasarım, akıllı enerji yönetimi ve zamansız brutalist detayların lüks yapılardaki yükselişi.",
                        Content = @"Lüks konut kavramı artık sadece pahalı kaplama malzemelerinden ibaret değil. Yeni nesil lüks; doğayla uyum, nefes alan yaşam alanları, düşük karbon ayak izi ve kusursuz teknolojik entegrasyonu ifade ediyor.
                        
                        ### 1. Biyofilik Mimari ve İç Bahçeler
                        Modern insan doğaya dönmek istiyor. Rezidans dairelerinde dahi tavana kadar uzanan iç avlular, dikey bahçeler ve doğal havalandırma koridorları mimarinin merkezinde yer alıyor.
                        
                        ### 2. Akıllı Enerji Bağımsızlığı
                        Güneş panelleriyle entegre çatı sistemleri, gri su arıtma üniteleri ve ısı geri kazanımlı iklimlendirme sistemleri sayesinde lüks konutlar kendi enerjisini üretiyor.
                        
                        ### 3. Ham Malzemelerin Zarafeti (Warm Brutalism)
                        Brüt beton, doğal masif ahşap ve traverten taşının sıcak tonlarla bir araya getirildiği 'Warm Brutalism' akımı, zamansız ve dingin bir atmosfer vadediyor.",
                        CoverImageUrl = "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=1200&q=80",
                        Author = "Y. Mimar Selim Aksoy",
                        CategoryId = blogCat1.Id,
                        IsPublished = true,
                        PublishedAt = DateTime.UtcNow.AddDays(-10),
                        ViewCount = 1420
                    },
                    new BlogPost
                    {
                        Title = "Sismik İzolasyon Sistemleri: Deprem Güvenliğinde En İleri Teknoloji",
                        Slug = "sismik-izolasyon-sistemleri-deprem-guvenligi",
                        Summary = "Deprem enerjisinin binaya aktarılmasını engelleyen kurşun çekirdekli kauçuk izolatörlerin çalışma prensipleri ve faydaları.",
                        Content = @"Deprem gerçeğiyle yaşayan ülkemizde, binaların sadece 'yıkılmaması' değil, deprem anında ve sonrasında fonksiyonunu kesintisiz sürdürmesi esastır. Sismik izolatörler, yapıyı temel seviyesinde yer hareketinden izole ederek sarsıntı hissini %80 oranında azaltır.",
                        CoverImageUrl = "https://images.unsplash.com/photo-1541888946425-d0fbb18086f6?auto=format&fit=crop&w=1200&q=80",
                        Author = "İnş. Yük. Müh. Hakan Çelik",
                        CategoryId = blogCat3.Id,
                        IsPublished = true,
                        PublishedAt = DateTime.UtcNow.AddDays(-18),
                        ViewCount = 2890
                    },
                    new BlogPost
                    {
                        Title = "BIM (Building Information Modeling) ile Şantiyede Sıfır Hata",
                        Slug = "bim-ile-santiyede-sifir-hata",
                        Summary = "3 boyutlu dijital ikiz modellemesiyle proje bütçesinde %15, şantiye süresinde %25 tasarruf sağlayan yeni nesil mühendislik yaklaşımı.",
                        Content = @"Şantiye aşamasında karşılaşılan boru-kiriş çakışmaları, metraj hataları ve revizyon maliyetleri BIM teknolojisi sayesinde daha çizim aşamasındayken simüle edilerek çözülüyor.",
                        CoverImageUrl = "https://images.unsplash.com/photo-1503387762-592deb58ef4e?auto=format&fit=crop&w=1200&q=80",
                        Author = "BIM Koordinatörü Ayşe Demir",
                        CategoryId = blogCat2.Id,
                        IsPublished = true,
                        PublishedAt = DateTime.UtcNow.AddDays(-25),
                        ViewCount = 980
                    }
                };
                context.BlogPosts.AddRange(blogPosts);
                await context.SaveChangesAsync();
            }

            // 9. TESTIMONIALS
            if (!await context.Testimonials.AnyAsync())
            {
                var testimonials = new List<Testimonial>
                {
                    new Testimonial
                    {
                        FullName = "Murat Gündoğdu",
                        Position = "Yönetim Kurulu Başkanı",
                        Company = "Gündoğdu Holding",
                        Comment = "NexaBuild ile Levent Sky Tower projemizde çalıştık. Şantiye disiplini, taahhüt edilen teslim süresine harfiyen uyulması ve mühendislik kalitesi gerçekten Türkiye standartlarının çok üzerinde.",
                        Rating = 5,
                        Status = ReviewStatus.Approved,
                        AvatarUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=200&q=80",
                        CreatedAt = DateTime.UtcNow.AddDays(-30)
                    },
                    new Testimonial
                    {
                        FullName = "Aylin & Cem Karaca",
                        Position = "Mülk Sahibi",
                        Company = "Bodrum Horizon Villas",
                        Comment = "Yalıkavak'taki villamızın anahtarını aldığımız gün duyduğumuz mutluluğu tarif edemeyiz. Malzeme seçimlerindeki titizlik ve mimari detaylar bizi hayran bıraktı. Teşekkürler NexaBuild ailesi!",
                        Rating = 5,
                        Status = ReviewStatus.Approved,
                        AvatarUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=200&q=80",
                        CreatedAt = DateTime.UtcNow.AddDays(-15)
                    },
                    new Testimonial
                    {
                        FullName = "Dr. Cihan Öztürk",
                        Position = "Yatırımcı",
                        Company = "Mersin Marina Residence",
                        Comment = "Devam eden projeyi şantiye kameralarından ve müşteri panelindeki haftalık progress raporlarından adım adım takip edebilmek büyük bir güven verdi.",
                        Rating = 5,
                        Status = ReviewStatus.Approved,
                        AvatarUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=200&q=80",
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    }
                };
                context.Testimonials.AddRange(testimonials);
                await context.SaveChangesAsync();
            }

            // 10. DEMO QUOTE REQUESTS & CONTACT MESSAGES
            if (!await context.QuoteRequests.AnyAsync())
            {
                context.QuoteRequests.Add(new QuoteRequest
                {
                    FullName = "Ahmet Yılmaz",
                    Email = "ahmet.yilmaz@globalinvestment.com",
                    Phone = "+90 532 111 22 33",
                    ProjectType = "Ticari & Plaza",
                    EstimatedBudget = "50.000.000 TL - 100.000.000 TL",
                    Message = "Maslak bölgesindeki 4.500 m² arsamız üzerinde 15 katlı akıllı ofis binası inşaatı için anahtar teslim teklif ve fizibilite çalışması talep ediyoruz.",
                    Status = QuoteStatus.New,
                    CreatedAt = DateTime.UtcNow.AddHours(-4)
                });
                context.QuoteRequests.Add(new QuoteRequest
                {
                    FullName = "Serkan Demir",
                    Email = "serkan@demirmakina.com",
                    Phone = "+90 544 222 33 44",
                    ProjectType = "Endüstriyel Tesis",
                    EstimatedBudget = "25.000.000 TL - 50.000.000 TL",
                    Message = "Gebze Dilovası OSB'de planlanan 12.000 m² kapalı alanlı makine fabrikamızın çelik konstrüksiyon ve saha beton işleri için keşif rica ederiz.",
                    Status = QuoteStatus.Reviewing,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    AdminNote = "Mühendislik ekibimiz zemin etüdü ve statik projeyi inceliyor."
                });
                await context.SaveChangesAsync();
            }

            if (!await context.ContactMessages.AnyAsync())
            {
                context.ContactMessages.Add(new ContactMessage
                {
                    FullName = "Zeynep Kaya",
                    Email = "zeynep@artmimarlik.com",
                    Phone = "+90 555 987 65 43",
                    Subject = "Kentsel Dönüşüm Danışmanlığı",
                    Message = "Kadıköy Bağdat Caddesi'ndeki 12 dairelik apartmanımızın kentsel dönüşüm süreci için görüşmek istiyoruz.",
                    Status = MessageStatus.New,
                    CreatedAt = DateTime.UtcNow.AddHours(-12)
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
