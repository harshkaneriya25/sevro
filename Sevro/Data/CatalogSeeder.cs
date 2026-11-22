using Microsoft.EntityFrameworkCore;
using Sevro.Helpers;
using Sevro.Models;
using Sevro.Models.Enums;

namespace Sevro.Data
{
    public static class CatalogSeeder
    {
        private sealed record CategorySeed(
            string Name,
            string IconClass,
            string Description,
            int Order,
            ServiceSeed[] Services);

        private sealed record ServiceSeed(
            string Name,
            decimal? Price,
            PricingType PricingType = PricingType.Fixed,
            string Unit = "Per job",
            string Duration = "30–60 mins",
            bool IsPopular = false,
            string? Description = null);

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var catalog = GetSeedCatalog();
            var existingCategories = await context.ServiceCategories.ToListAsync();
            var categoryByName = existingCategories
                .ToDictionary(c => c.CategoryName.Trim().ToLowerInvariant(), c => c);

            // Backfill slug/icon on any pre-existing categories
            bool categoriesUpdated = false;
            foreach (var existing in existingCategories)
            {
                if (string.IsNullOrWhiteSpace(existing.Slug))
                {
                    existing.Slug = PricingFormatter.Slugify(existing.CategoryName);
                    categoriesUpdated = true;
                }
                if (string.IsNullOrWhiteSpace(existing.IconClass))
                {
                    existing.IconClass = "bi-grid-fill";
                    categoriesUpdated = true;
                }
            }

            // 1. Insert missing categories (idempotent)
            foreach (var cat in catalog)
            {
                var key = cat.Name.Trim().ToLowerInvariant();
                if (!categoryByName.TryGetValue(key, out var entity))
                {
                    entity = new ServiceCategory
                    {
                        CategoryName = cat.Name,
                        Slug = PricingFormatter.Slugify(cat.Name),
                        IconClass = cat.IconClass,
                        Description = cat.Description,
                        IsActive = true,
                        DisplayOrder = cat.Order
                    };
                    context.ServiceCategories.Add(entity);
                    categoryByName[key] = entity;
                    categoriesUpdated = true;
                }
                else
                {
                    if (entity.IconClass == "bi-grid-fill" && !string.IsNullOrWhiteSpace(cat.IconClass))
                    {
                        entity.IconClass = cat.IconClass;
                        categoriesUpdated = true;
                    }
                    if (entity.DisplayOrder == 0)
                    {
                        entity.DisplayOrder = cat.Order;
                        categoriesUpdated = true;
                    }
                }
            }

            if (categoriesUpdated)
            {
                await context.SaveChangesAsync();
            }

            // 2. Insert missing services (idempotent by unique Slug)
            var existingSlugs = new HashSet<string>(
                await context.CatalogServices.Select(s => s.Slug).ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            var now = DateTime.UtcNow;
            var toInsert = new List<CatalogService>();

            foreach (var cat in catalog)
            {
                var catEntity = categoryByName[cat.Name.Trim().ToLowerInvariant()];
                var catSlug = string.IsNullOrWhiteSpace(catEntity.Slug)
                    ? PricingFormatter.Slugify(catEntity.CategoryName)
                    : catEntity.Slug;

                foreach (var svc in cat.Services)
                {
                    var slug = $"{catSlug}-{PricingFormatter.Slugify(svc.Name)}";
                    if (slug.Length > 180) slug = slug[..180].TrimEnd('-');
                    if (existingSlugs.Contains(slug)) continue;

                    existingSlugs.Add(slug);
                    toInsert.Add(new CatalogService
                    {
                        CategoryId = catEntity.CategoryId,
                        Name = svc.Name,
                        Slug = slug,
                        Description = svc.Description ?? BuildDefaultDescription(svc.Name, cat.Name, svc.PricingType),
                        Price = svc.PricingType == PricingType.QuoteRequired ? null : svc.Price,
                        PricingType = svc.PricingType,
                        Unit = svc.Unit,
                        EstimatedDuration = svc.Duration,
                        IsActive = true,
                        IsPopular = svc.IsPopular,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
            }

            if (toInsert.Count > 0)
            {
                context.CatalogServices.AddRange(toInsert);
                await context.SaveChangesAsync();
            }
        }

        private static string BuildDefaultDescription(string serviceName, string categoryName, PricingType type)
        {
            return type switch
            {
                PricingType.QuoteRequired => $"{serviceName} under {categoryName}. Submit your job details to receive a transparent local quotation after inspection.",
                PricingType.PerVisit => $"{serviceName} by a verified local specialist. Covers technician visit and inspection; any repair estimate is shared upfront.",
                PricingType.Hourly => $"{serviceName} provided by verified local experts billed transparently per hour.",
                PricingType.StartingFrom => $"Affordable {serviceName.ToLowerInvariant()} under {categoryName} with transparent starting price and no hidden fees.",
                _ => $"Reliable {serviceName.ToLowerInvariant()} by admin-verified local professionals at an affordable fixed rate."
            };
        }

        // 12 Core Categories — 5 to 6 curated, affordable INR services per category (70 total)
        private static CategorySeed[] GetSeedCatalog() => new[]
        {
            // 1. Electrical Services (6)
            new CategorySeed("Electrical Services", "bi-lightning-charge-fill", "Fans, lights, switchboards, inverter setups, and electrical inspections by certified local electricians.", 1, new[]
            {
                new ServiceSeed("Fan installation", 199, PricingType.Fixed, "Per job", "30–45 mins", true),
                new ServiceSeed("Fan repair", 149, PricingType.Fixed, "Per job", "30–45 mins", true),
                new ServiceSeed("Light installation", 99, PricingType.Fixed, "Per job", "20–30 mins"),
                new ServiceSeed("Switchboard repair", 199, PricingType.Fixed, "Per job", "30–45 mins", true),
                new ServiceSeed("Inverter installation", 399, PricingType.Fixed, "Per job", "60–90 mins"),
                new ServiceSeed("Minor electrical inspection", 149, PricingType.PerVisit, "Per visit", "30 mins"),
            }),

            // 2. Plumbing Services (6)
            new CategorySeed("Plumbing Services", "bi-droplet-fill", "Taps, water leakages, drain blockages, toilet flush fixes, and water motor installations.", 2, new[]
            {
                new ServiceSeed("Tap repair", 99, PricingType.Fixed, "Per job", "20–30 mins", true),
                new ServiceSeed("Tap installation", 149, PricingType.Fixed, "Per job", "30 mins"),
                new ServiceSeed("Water leakage repair", 199, PricingType.StartingFrom, "Per job", "45–60 mins", true),
                new ServiceSeed("Drain blockage removal", 199, PricingType.Fixed, "Per job", "30–45 mins", true),
                new ServiceSeed("Toilet flush repair", 199, PricingType.Fixed, "Per job", "30–45 mins"),
                new ServiceSeed("Water motor installation", 399, PricingType.Fixed, "Per job", "60–90 mins"),
            }),

            // 3. AC & Home Appliance Repair (6)
            new CategorySeed("AC & Home Appliance Repair", "bi-snow2", "AC servicing & deep cleaning, refrigerator, washing machine, and RO water purifier maintenance.", 3, new[]
            {
                new ServiceSeed("AC basic servicing", 499, PricingType.Fixed, "Per service", "45–60 mins", true),
                new ServiceSeed("AC deep cleaning", 699, PricingType.Fixed, "Per service", "60–90 mins", true),
                new ServiceSeed("AC inspection", 199, PricingType.PerVisit, "Per visit", "30 mins"),
                new ServiceSeed("Refrigerator repair", 399, PricingType.StartingFrom, "Per job", "45–90 mins", true),
                new ServiceSeed("Washing machine repair", 299, PricingType.StartingFrom, "Per job", "45–60 mins"),
                new ServiceSeed("Water purifier servicing", 299, PricingType.Fixed, "Per service", "45 mins"),
            }),

            // 4. Home Cleaning (6)
            new CategorySeed("Home Cleaning", "bi-stars", "Bathroom, kitchen, sofa, ceiling fan, full-home deep cleaning, and post-renovation cleanup.", 4, new[]
            {
                new ServiceSeed("1 bathroom cleaning", 199, PricingType.Fixed, "Per service", "45 mins", true),
                new ServiceSeed("Kitchen cleaning", 399, PricingType.Fixed, "Per service", "60–90 mins", true),
                new ServiceSeed("Sofa cleaning", 499, PricingType.StartingFrom, "Per sofa", "60 mins", true),
                new ServiceSeed("Full home deep cleaning", 1499, PricingType.StartingFrom, "Per home", "4–5 hours"),
                new ServiceSeed("Fan cleaning", 99, PricingType.PerUnit, "Per fan", "15 mins"),
                new ServiceSeed("Post-renovation cleaning", null, PricingType.QuoteRequired, "Custom", "Inspection required"),
            }),

            // 5. Carpentry (6)
            new CategorySeed("Carpentry", "bi-hammer", "Door hinges, locks, shelves, furniture assembly, hourly carpentry, and custom woodwork.", 5, new[]
            {
                new ServiceSeed("Door hinge repair", 149, PricingType.Fixed, "Per job", "30 mins", true),
                new ServiceSeed("Door lock installation", 199, PricingType.Fixed, "Per job", "30–45 mins", true),
                new ServiceSeed("Furniture assembly", 299, PricingType.StartingFrom, "Per item", "45–90 mins", true),
                new ServiceSeed("Shelf installation", 199, PricingType.Fixed, "Per job", "30 mins"),
                new ServiceSeed("Minor carpenter work", 249, PricingType.Hourly, "Per hour", "1 hour"),
                new ServiceSeed("Major furniture work", null, PricingType.QuoteRequired, "Custom", "Inspection required"),
            }),

            // 6. Painting & Waterproofing (6)
            new CategorySeed("Painting & Waterproofing", "bi-brush-fill", "Wall touch-ups, room & door painting, wallpaper installation, and full-home painting quotes.", 6, new[]
            {
                new ServiceSeed("Wall touch-up", 299, PricingType.Fixed, "Per job", "45–60 mins", true),
                new ServiceSeed("One wall painting", 499, PricingType.StartingFrom, "Per wall", "2–3 hours", true),
                new ServiceSeed("One room painting", 1499, PricingType.StartingFrom, "Per room", "1 day"),
                new ServiceSeed("Door painting", 399, PricingType.Fixed, "Per door", "60–90 mins"),
                new ServiceSeed("Wallpaper installation", 499, PricingType.StartingFrom, "Per wall", "2 hours"),
                new ServiceSeed("Full home painting", null, PricingType.QuoteRequired, "Custom", "Site inspection"),
            }),

            // 7. Vehicle Services (6)
            new CategorySeed("Vehicle Services", "bi-car-front-fill", "Bike & car servicing, puncture repair, battery jump-start, exterior wash, and roadside help.", 7, new[]
            {
                new ServiceSeed("Bike servicing", 499, PricingType.StartingFrom, "Per service", "60–90 mins", true),
                new ServiceSeed("Car servicing", 999, PricingType.StartingFrom, "Per service", "2–3 hours", true),
                new ServiceSeed("Battery jump-start", 199, PricingType.Fixed, "Per visit", "30 mins", true),
                new ServiceSeed("Bike puncture repair", 99, PricingType.Fixed, "Per tire", "20 mins"),
                new ServiceSeed("Car exterior wash", 299, PricingType.Fixed, "Per wash", "45 mins"),
                new ServiceSeed("Roadside mechanic visit", 299, PricingType.PerVisit, "Per visit", "30–60 mins"),
            }),

            // 8. Computer & Technology (6)
            new CategorySeed("Computer & Technology", "bi-laptop", "Laptop inspection & cleaning, SSD upgrades, Windows setup, Wi-Fi routers, and CCTV cameras.", 8, new[]
            {
                new ServiceSeed("Laptop inspection", 199, PricingType.PerVisit, "Per visit", "30 mins"),
                new ServiceSeed("Laptop cleaning", 299, PricingType.Fixed, "Per service", "45 mins"),
                new ServiceSeed("SSD installation", 199, PricingType.Fixed, "Per job", "30 mins", true),
                new ServiceSeed("Windows installation", 399, PricingType.Fixed, "Per job", "60 mins", true),
                new ServiceSeed("Wi-Fi router setup", 199, PricingType.Fixed, "Per job", "30 mins", true),
                new ServiceSeed("CCTV installation", 499, PricingType.StartingFrom, "Per camera", "60 mins"),
            }),

            // 9. Pest Control (5)
            new CategorySeed("Pest Control", "bi-bug-fill", "Safe cockroach, bed bug, termite inspection, and 1BHK/2BHK residential pest treatments.", 9, new[]
            {
                new ServiceSeed("Cockroach control", 499, PricingType.StartingFrom, "Per service", "45–60 mins", true),
                new ServiceSeed("Termite inspection", 199, PricingType.PerVisit, "Per visit", "30–45 mins"),
                new ServiceSeed("1BHK pest control", 799, PricingType.Fixed, "Per home", "60 mins"),
                new ServiceSeed("2BHK pest control", 999, PricingType.Fixed, "Per home", "75 mins", true),
                new ServiceSeed("Bed bug treatment", 999, PricingType.StartingFrom, "Per service", "90 mins"),
            }),

            // 10. Beauty & Grooming (6)
            new CategorySeed("Beauty & Grooming", "bi-scissors", "At-home haircuts, beard grooming, facials, pedicure, waxing, and home salon visits.", 10, new[]
            {
                new ServiceSeed("Men's haircut", 149, PricingType.Fixed, "Per service", "30 mins", true),
                new ServiceSeed("Haircut and beard", 199, PricingType.Fixed, "Per service", "45 mins", true),
                new ServiceSeed("Basic facial", 399, PricingType.Fixed, "Per service", "45–60 mins", true),
                new ServiceSeed("Pedicure", 299, PricingType.Fixed, "Per service", "45 mins"),
                new ServiceSeed("Waxing", 199, PricingType.StartingFrom, "Per service", "30–60 mins"),
                new ServiceSeed("Home salon visit", 199, PricingType.PerVisit, "Per visit", "Visit fee"),
            }),

            // 11. Laundry & Dry Cleaning (5)
            new CategorySeed("Laundry & Dry Cleaning", "bi-basket2-fill", "Affordable ironing bundles, per-kg wash & fold, wash & iron, blankets, and suit dry cleaning.", 11, new[]
            {
                new ServiceSeed("5 clothes ironing", 99, PricingType.Fixed, "Per bundle", "Same day", true),
                new ServiceSeed("Regular laundry", 99, PricingType.PerUnit, "Per kg", "24 hours", true),
                new ServiceSeed("Wash and iron", 129, PricingType.PerUnit, "Per kg", "24 hours"),
                new ServiceSeed("Blanket cleaning", 199, PricingType.StartingFrom, "Per blanket", "48 hours"),
                new ServiceSeed("Suit dry cleaning", 299, PricingType.StartingFrom, "Per suit", "48 hours"),
            }),

            // 12. Moving & Shifting (6)
            new CategorySeed("Moving & Shifting", "bi-truck", "Single-item moves, hourly packing & loading help, local mini-trucks, 1BHK & office shifting.", 12, new[]
            {
                new ServiceSeed("Single-item shifting", 299, PricingType.StartingFrom, "Per item", "1 hour"),
                new ServiceSeed("Packing assistance", 249, PricingType.Hourly, "Per hour", "1 hour"),
                new ServiceSeed("Loading assistance", 199, PricingType.Hourly, "Per hour", "1 hour"),
                new ServiceSeed("Local mini-truck booking", 499, PricingType.StartingFrom, "Per trip", "1–2 hours", true),
                new ServiceSeed("1BHK shifting", 1999, PricingType.StartingFrom, "Per home", "3–5 hours", true),
                new ServiceSeed("Office shifting", null, PricingType.QuoteRequired, "Custom", "Site inspection"),
            }),
        };
    }
}
