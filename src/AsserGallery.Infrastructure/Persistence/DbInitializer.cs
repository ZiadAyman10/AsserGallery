using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using AsserGallery.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AsserGallery.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedDatabaseAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ILogger logger)
    {
        try
        {
            // Seed Admin Role & User
            const string adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            var adminEmail = configuration["AdminSeed:Email"] ?? "admin@assergallery.com";
            var adminPassword = configuration["AdminSeed:Password"] ?? "Admin@123456";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Asser Gallery Admin",
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                    logger.LogInformation("AsserGallery Admin user seeded successfully ({Email})", adminEmail);
                }
                else
                {
                    logger.LogWarning("Failed to seed admin user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }

            // Seed Store Settings
            if (!await context.StoreSettings.AnyAsync())
            {
                context.StoreSettings.AddRange(
                    new StoreSetting { Key = "StoreName", Value = "Asser Gallery", Description = "Store Brand Name (EN)" },
                    new StoreSetting { Key = "StoreArabicName", Value = "آسر جاليري", Description = "Store Brand Name (AR)" },
                    new StoreSetting { Key = "WhatsAppNumber", Value = "201012345678", Description = "Primary WhatsApp Order Number" },
                    new StoreSetting { Key = "MessengerUsername", Value = "assergallery.eg", Description = "Facebook Messenger handle" },
                    new StoreSetting { Key = "Currency", Value = "EGP", Description = "Default Currency Code" },
                    new StoreSetting { Key = "CurrencyArabic", Value = "ج.م", Description = "Default Currency Arabic Symbol" },
                    new StoreSetting { Key = "HideOutOfStock", Value = "false", Description = "Whether to completely hide out-of-stock items on catalog" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Colors
            if (!await context.Colors.AnyAsync())
            {
                context.Colors.AddRange(
                    new Color { Name = "Black", ArabicName = "أسود", HexCode = "#111827" },
                    new Color { Name = "Pure White", ArabicName = "أبيض ناصع", HexCode = "#F9FAFB" },
                    new Color { Name = "Navy Blue", ArabicName = "كحلي داكن", HexCode = "#1E3A8A" },
                    new Color { Name = "Sky Blue", ArabicName = "سماوي / بيبي بلو", HexCode = "#60A5FA" },
                    new Color { Name = "Dusty Rose / Pink", ArabicName = "وردي / بينك", HexCode = "#F472B6" },
                    new Color { Name = "Olive Green", ArabicName = "زيتي / زتوني", HexCode = "#4D7C0F" },
                    new Color { Name = "Beige / Sand", ArabicName = "بيج / رملي", HexCode = "#D7B58B" },
                    new Color { Name = "Burgundy", ArabicName = "نبيتي / عنابي", HexCode = "#881337" },
                    new Color { Name = "Charcoal Gray", ArabicName = "رمادي فحمي", HexCode = "#4B5563" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Categories & SubCategories
            if (!await context.Categories.AnyAsync())
            {
                var menCategory = new Category
                {
                    Name = "Men's Collection",
                    ArabicName = "تشكيلة الرجال",
                    Description = "Modern casual and formal clothing for men",
                    ArabicDescription = "أحدث صيحات الملابس الكاجوال والرسمية للرجال",
                    DisplayOrder = 1,
                    IsActive = true,
                    SubCategories = new List<SubCategory>
                    {
                        new() { Name = "Casual Shirts & Polos", ArabicName = "قمصان وبولو كاجوال", DisplayOrder = 1, IsActive = true },
                        new() { Name = "Formal & Suits", ArabicName = "بدل وملابس رسمية", DisplayOrder = 2, IsActive = true },
                        new() { Name = "Pajamas & Loungewear", ArabicName = "بيجامات وملابس منزلية", DisplayOrder = 3, IsActive = true },
                        new() { Name = "Jackets & Winterwear", ArabicName = "جواكت ومعاطف شتوية", DisplayOrder = 4, IsActive = true }
                    }
                };

                var womenCategory = new Category
                {
                    Name = "Women's Collection",
                    ArabicName = "تشكيلة النساء",
                    Description = "Elegant dresses, modest fashion, and loungewear",
                    ArabicDescription = "فساتين أنيقة، ملابس محتشمة، وملابس منزلية مريحة",
                    DisplayOrder = 2,
                    IsActive = true,
                    SubCategories = new List<SubCategory>
                    {
                        new() { Name = "Casual Tops & Dresses", ArabicName = "فساتين وتوبات كاجوال", DisplayOrder = 1, IsActive = true },
                        new() { Name = "Loungewear & Sleepwear", ArabicName = "بيجامات وملابس نوم", DisplayOrder = 2, IsActive = true },
                        new() { Name = "Abayas & Modest Wear", ArabicName = "عبايات وملابس محتشمة", DisplayOrder = 3, IsActive = true },
                        new() { Name = "Cardigans & Jackets", ArabicName = "كارديجان وجواكت", DisplayOrder = 4, IsActive = true }
                    }
                };

                var kidsCategory = new Category
                {
                    Name = "Kids & Teens",
                    ArabicName = "أزياء الأطفال والناشئين",
                    Description = "High quality comfortable clothes for boys and girls",
                    ArabicDescription = "ملابس مريحة وعالية الجودة للأولاد والبنات",
                    DisplayOrder = 3,
                    IsActive = true,
                    SubCategories = new List<SubCategory>
                    {
                        new() { Name = "Boys Casual Sets", ArabicName = "أطقم أولادي كاجوال", DisplayOrder = 1, IsActive = true },
                        new() { Name = "Girls Dresses & Sets", ArabicName = "فساتين وأطقم بناتي", DisplayOrder = 2, IsActive = true }
                    }
                };

                context.Categories.AddRange(menCategory, womenCategory, kidsCategory);
                await context.SaveChangesAsync();
            }

            // Seed Sample Products
            if (!await context.Products.AnyAsync())
            {
                var subCategories = await context.SubCategories.ToListAsync();
                var colors = await context.Colors.ToListAsync();

                var menShirtSub = subCategories.FirstOrDefault(s => s.Name.Contains("Casual Shirts")) ?? subCategories.First();
                var menJacketSub = subCategories.FirstOrDefault(s => s.Name.Contains("Jackets")) ?? subCategories.First();
                var womenDressSub = subCategories.FirstOrDefault(s => s.Name.Contains("Casual Tops")) ?? subCategories.Last();
                var womenPajamaSub = subCategories.FirstOrDefault(s => s.Name.Contains("Loungewear")) ?? subCategories.Last();

                var black = colors.First(c => c.Name == "Black");
                var white = colors.First(c => c.Name == "Pure White");
                var navy = colors.First(c => c.Name == "Navy Blue");
                var skyBlue = colors.First(c => c.Name == "Sky Blue");
                var pink = colors.First(c => c.Name == "Dusty Rose / Pink");
                var olive = colors.First(c => c.Name == "Olive Green");
                var beige = colors.First(c => c.Name == "Beige / Sand");

                var sampleProducts = new List<Product>
                {
                    new()
                    {
                        Name = "Premium Breathable Linen Shirt",
                        ArabicName = "قميص كتان صيفي فاخر",
                        Description = "100% natural Egyptian flax linen shirt with refined tailored fit. Breathable, comfortable, and perfect for warm weather or layering.",
                        ArabicDescription = "قميص مصنوع من الكتان الطبيعي الفاخر بنسبة 100%. قماش خفيف ومسامي يمنحك راحة فائقة وأناقة مميزة في الصيف.",
                        Price = 650m,
                        DiscountedPrice = 490m,
                        SubCategoryId = menShirtSub.Id,
                        IsFeatured = true,
                        DisplayOrder = 1,
                        DateAdded = DateTime.UtcNow.AddDays(-10),
                        Images = new List<ProductImage>
                        {
                            new() { ImageUrl = "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=800&auto=format&fit=crop&q=80", ImageType = ImageType.AiEnhanced, IsPrimary = true, DisplayOrder = 1 },
                            new() { ImageUrl = "https://images.unsplash.com/photo-1602810318383-e386cc2a3ccf?w=800&auto=format&fit=crop&q=80", ImageType = ImageType.Original, IsPrimary = false, DisplayOrder = 2 }
                        },
                        Variants = new List<ProductVariant>
                        {
                            new() { ColorId = white.Id, Quantity = 8 },
                            new() { ColorId = skyBlue.Id, Quantity = 4 },
                            new() { ColorId = beige.Id, Quantity = 5 },
                            new() { ColorId = navy.Id, Quantity = 0 }
                        }
                    },
                    new()
                    {
                        Name = "Urban Waterproof Windbreaker Jacket",
                        ArabicName = "جاكيت وندبريكر مقاوم للماء",
                        Description = "Minimalist lightweight windbreaker jacket with fleece lining and waterproof finish. Essential outer layer for chilly evenings.",
                        ArabicDescription = "جاكيت خفيف مقاوم للرياح والأمطار مبطن بطبقة داخلية ناعمة. مثالي للإطلالات الكاجوال اليومية والطقس المتقلب.",
                        Price = 1200m,
                        DiscountedPrice = 950m,
                        SubCategoryId = menJacketSub.Id,
                        IsFeatured = true,
                        DisplayOrder = 2,
                        DateAdded = DateTime.UtcNow.AddDays(-6),
                        Images = new List<ProductImage>
                        {
                            new() { ImageUrl = "https://images.unsplash.com/photo-1544441893-675973e31985?w=800&auto=format&fit=crop&q=80", ImageType = ImageType.AiEnhanced, IsPrimary = true, DisplayOrder = 1 },
                            new() { ImageUrl = "https://images.unsplash.com/photo-1548883354-7622d03aca27?w=800&auto=format&fit=crop&q=80", ImageType = ImageType.Original, IsPrimary = false, DisplayOrder = 2 }
                        },
                        Variants = new List<ProductVariant>
                        {
                            new() { ColorId = black.Id, Quantity = 6 },
                            new() { ColorId = olive.Id, Quantity = 2 },
                            new() { ColorId = navy.Id, Quantity = 1 }
                        }
                    },
                    new()
                    {
                        Name = "Flowy Floral Tiered Midi Dress",
                        ArabicName = "فستان صيفي ميدي بنقشة زهور",
                        Description = "Charming tiered silhouette midi dress with breathable soft chiffon and cotton lining. Elegant for both daily outings and special gatherings.",
                        ArabicDescription = "فستان ميدي أنيق بقصة طبقات جذابة ونقشة زهور رقيقة. خامة شيفون ناعمة مبطنة بقطن مريح جداً.",
                        Price = 850m,
                        DiscountedPrice = 680m,
                        SubCategoryId = womenDressSub.Id,
                        IsFeatured = true,
                        DisplayOrder = 3,
                        DateAdded = DateTime.UtcNow.AddDays(-4),
                        Images = new List<ProductImage>
                        {
                            new() { ImageUrl = "https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?w=800&auto=format&fit=crop&q=80", ImageType = ImageType.AiEnhanced, IsPrimary = true, DisplayOrder = 1 },
                            new() { ImageUrl = "https://images.unsplash.com/photo-1515372039744-b8f02a3ae446?w=800&auto=format&fit=crop&q=80", ImageType = ImageType.Original, IsPrimary = false, DisplayOrder = 2 }
                        },
                        Variants = new List<ProductVariant>
                        {
                            new() { ColorId = pink.Id, Quantity = 7 },
                            new() { ColorId = skyBlue.Id, Quantity = 3 },
                            new() { ColorId = beige.Id, Quantity = 0 }
                        }
                    },
                    new()
                    {
                        Name = "Silky Satin Luxury Pajama Set",
                        ArabicName = "طقم بيجاما ستان حريري فاخر",
                        Description = "Ultra-soft premium satin 2-piece lounge set with contrast piping. Delivers hotel-grade luxury and relaxing night comfort.",
                        ArabicDescription = "طقم بيجاما قطعتين من الستان الحريري فائق النعومة مع حواف مميزة. يمنحك شعوراً بالفخامة والراحة طوال الليل.",
                        Price = 580m,
                        DiscountedPrice = null,
                        SubCategoryId = womenPajamaSub.Id,
                        IsFeatured = false,
                        DisplayOrder = 4,
                        DateAdded = DateTime.UtcNow.AddDays(-2),
                        Images = new List<ProductImage>
                        {
                            new() { ImageUrl = "https://images.unsplash.com/photo-1582533561751-ef6f6ab93a2e?w=800&auto=format&fit=crop&q=80", ImageType = ImageType.AiEnhanced, IsPrimary = true, DisplayOrder = 1 },
                            new() { ImageUrl = "https://images.unsplash.com/photo-1583743814966-8936f5b7be1a?w=800&auto=format&fit=crop&q=80", ImageType = ImageType.Original, IsPrimary = false, DisplayOrder = 2 }
                        },
                        Variants = new List<ProductVariant>
                        {
                            new() { ColorId = pink.Id, Quantity = 5 },
                            new() { ColorId = black.Id, Quantity = 3 },
                            new() { ColorId = navy.Id, Quantity = 4 }
                        }
                    }
                };

                foreach (var prod in sampleProducts)
                {
                    prod.UpdateStatusFromStock();
                }

                context.Products.AddRange(sampleProducts);
                await context.SaveChangesAsync();
            }

            // Seed Initial Facebook Destinations
            if (!await context.FacebookDestinations.AnyAsync())
            {
                context.FacebookDestinations.AddRange(
                    new FacebookDestination
                    {
                        Name = "Asser Gallery Official Page",
                        DestinationType = DestinationType.Page,
                        TargetIdOrUrl = "109283746501928",
                        AccessToken = "MOCK_FB_PAGE_TOKEN_ASSER_GALLERY",
                        IsActive = true
                    },
                    new FacebookDestination
                    {
                        Name = "Cairo Fashion & Clothing Deals Group",
                        DestinationType = DestinationType.Group,
                        TargetIdOrUrl = "cairofashiondeals",
                        AccessToken = null,
                        IsActive = true
                    },
                    new FacebookDestination
                    {
                        Name = "Alexandria VIP Outfits Group",
                        DestinationType = DestinationType.Group,
                        TargetIdOrUrl = "alexandriavipoutfits",
                        AccessToken = null,
                        IsActive = true
                    }
                );
                await context.SaveChangesAsync();
            }

            // Seed Initial Financial Transactions
            if (!await context.FinancialTransactions.AnyAsync())
            {
                context.FinancialTransactions.AddRange(
                    new FinancialTransaction
                    {
                        Title = "Summer Fabric & Workshop Stock Purchase",
                        Description = "Purchased high-thread linen rolls and satin rolls from Al-Azhar textile market.",
                        Amount = 14500m,
                        Type = TransactionType.Expense,
                        Category = "StockPurchase",
                        Date = DateTime.UtcNow.AddDays(-15)
                    },
                    new FinancialTransaction
                    {
                        Title = "Custom Branded Bags & Box Packaging",
                        Description = "Printed 500 premium glossy bags with Asser Gallery logo.",
                        Amount = 1800m,
                        Type = TransactionType.Expense,
                        Category = "Packaging",
                        Date = DateTime.UtcNow.AddDays(-12)
                    },
                    new FinancialTransaction
                    {
                        Title = "Facebook & Instagram Boost Ads Campaign",
                        Description = "Targeted sponsored posts for Summer Linen Shirt launch.",
                        Amount = 1200m,
                        Type = TransactionType.Expense,
                        Category = "Advertising",
                        Date = DateTime.UtcNow.AddDays(-8)
                    },
                    new FinancialTransaction
                    {
                        Title = "First Batch Sales Revenue",
                        Description = "Direct sales from group posts and WhatsApp orders.",
                        Amount = 24800m,
                        Type = TransactionType.Income,
                        Category = "SalesRevenue",
                        Date = DateTime.UtcNow.AddDays(-2)
                    }
                );
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the Asser Gallery database.");
            throw;
        }
    }
}
