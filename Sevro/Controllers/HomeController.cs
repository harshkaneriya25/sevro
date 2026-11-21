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
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            var servicesQuery = await _context.ProviderServices
                .Include(s => s.Category)
                .Include(s => s.ProviderProfile)
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.Review)
                .Where(s => s.ProviderProfile.IsVerified)
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
                        ServiceName = s.ServiceName,
                        Description = s.Description,
                        Price = s.Price,
                        CategoryId = s.CategoryId,
                        CategoryName = s.Category.CategoryName,
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

            var model = new HomeViewModel
            {
                Categories = categories,
                FeaturedServices = featuredServices,
                TotalVerifiedProviders = await _context.ProviderProfiles.CountAsync(p => p.IsVerified),
                TotalServices = servicesQuery.Count,
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
