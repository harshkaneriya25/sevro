using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sevro.Data;
using Sevro.Models;
using Sevro.Models.Enums;
using Sevro.ViewModels.Catalog;

namespace Sevro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.ServiceCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

            var servicesQuery = await _context.ProviderServices
                .Include(s => s.Category)
                .Include(s => s.ProviderProfile)
                .Where(s => s.IsActive && s.Category.IsActive && s.ProviderProfile.IsVerified)
                .ToListAsync();

            var providerReviewStats = await _context.Reviews
                .Include(r => r.Booking)
                    .ThenInclude(b => b.ProviderService)
                .GroupBy(r => r.Booking.ProviderService.ProviderProfileId)
                .Select(g => new
                {
                    ProviderProfileId = g.Key,
                    Average = g.Average(r => r.Rating),
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.ProviderProfileId);

            var featuredServices = servicesQuery
                .Select(s =>
                {
                    providerReviewStats.TryGetValue(s.ProviderProfileId, out var stat);
                    return new ServiceCardViewModel
                    {
                        ProviderServiceId = s.ProviderServiceId,
                        CatalogServiceId = s.CatalogServiceId,
                        ServiceName = s.ServiceName,
                        Description = s.Description,
                        Price = s.Price,
                        PricingType = s.PricingType,
                        Unit = s.Unit,
                        EstimatedDuration = s.EstimatedDuration,
                        CategoryId = s.CategoryId,
                        CategoryName = s.Category.CategoryName,
                        CategoryIconClass = s.Category.IconClass,
                        CategoryImagePath = s.Category.ImagePath,
                        ProviderProfileId = s.ProviderProfileId,
                        BusinessName = s.ProviderProfile.BusinessName,
                        City = s.ProviderProfile.City,
                        IsVerified = s.ProviderProfile.IsVerified,
                        IsAvailable = s.ProviderProfile.IsAvailable,
                        AverageRating = stat?.Average ?? 0,
                        ReviewCount = stat?.Count ?? 0
                    };
                })
                .OrderByDescending(s => s.IsAvailable)
                .ThenByDescending(s => s.AverageRating)
                .ThenByDescending(s => s.ReviewCount)
                .Take(6)
                .ToList();

            var popularCatalogRaw = await _context.CatalogServices
                .Include(cs => cs.Category)
                .Where(cs => cs.IsActive && cs.Category.IsActive && cs.IsPopular)
                .OrderBy(cs => cs.Category.DisplayOrder)
                .ThenBy(cs => cs.Price)
                .Take(9)
                .ToListAsync();

            var providersByCatalogId = servicesQuery
                .Where(s => s.CatalogServiceId.HasValue)
                .GroupBy(s => s.CatalogServiceId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var popularCatalog = popularCatalogRaw.Select(cs =>
            {
                providersByCatalogId.TryGetValue(cs.CatalogServiceId, out var linked);
                var avail = linked?.FirstOrDefault(x => x.ProviderProfile.IsAvailable) ?? linked?.FirstOrDefault();
                return new CatalogRateItemViewModel
                {
                    CatalogServiceId = cs.CatalogServiceId,
                    CategoryId = cs.CategoryId,
                    CategoryName = cs.Category.CategoryName,
                    CategoryIconClass = cs.Category.IconClass,
                    CategoryImagePath = cs.Category.ImagePath,
                    Name = cs.Name,
                    Slug = cs.Slug,
                    Description = cs.Description,
                    Price = cs.Price,
                    PricingType = cs.PricingType,
                    Unit = cs.Unit,
                    EstimatedDuration = cs.EstimatedDuration,
                    IsPopular = cs.IsPopular,
                    AvailableProvidersCount = linked?.Count(x => x.ProviderProfile.IsAvailable) ?? 0,
                    DefaultProviderServiceId = avail?.ProviderServiceId
                };
            }).ToList();

            var totalCatalogServices = await _context.CatalogServices
                .CountAsync(cs => cs.IsActive && cs.Category.IsActive);

            var model = new HomeViewModel
            {
                Categories = categories,
                FeaturedServices = featuredServices,
                PopularCatalogServices = popularCatalog,
                TotalVerifiedProviders = await _context.ProviderProfiles.CountAsync(p => p.IsVerified),
                TotalServices = servicesQuery.Count,
                TotalCatalogServices = totalCatalogServices,
                TotalCategories = categories.Count,
                TotalCompletedBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Completed)
            };

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
