using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sevro.Data;
using Sevro.Models.Enums;
using Sevro.ViewModels.Catalog;

namespace Sevro.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            int? categoryId,
            string? city,
            bool availableOnly = false,
            List<string>? priceRanges = null,
            string sortBy = "rating")
        {
            priceRanges ??= new List<string>();

            var categories = await _context.ServiceCategories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            var cities = await _context.ProviderProfiles
                .Where(p => p.IsVerified && !string.IsNullOrWhiteSpace(p.City))
                .Select(p => p.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var query = _context.ProviderServices
                .Include(s => s.Category)
                .Include(s => s.ProviderProfile)
                .Where(s => s.ProviderProfile.IsVerified)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(s =>
                    s.ServiceName.ToLower().Contains(term) ||
                    (s.Description != null && s.Description.ToLower().Contains(term)) ||
                    s.ProviderProfile.BusinessName.ToLower().Contains(term) ||
                    s.Category.CategoryName.ToLower().Contains(term));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(s => s.ProviderProfile.City == city);
            }

            if (availableOnly)
            {
                query = query.Where(s => s.ProviderProfile.IsAvailable);
            }

            var rawServices = await query.ToListAsync();

            if (priceRanges.Any())
            {
                rawServices = rawServices.Where(s =>
                    (priceRanges.Contains("under500") && s.Price < 500m) ||
                    (priceRanges.Contains("500to1500") && s.Price >= 500m && s.Price <= 1500m) ||
                    (priceRanges.Contains("above1500") && s.Price > 1500m)
                ).ToList();
            }

            var providerRatings = await _context.Reviews
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

            var cards = rawServices.Select(s =>
            {
                providerRatings.TryGetValue(s.ProviderProfileId, out var stat);
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
            });

            cards = sortBy switch
            {
                "price_asc" => cards.OrderBy(c => c.Price).ThenByDescending(c => c.AverageRating),
                "price_desc" => cards.OrderByDescending(c => c.Price).ThenByDescending(c => c.AverageRating),
                _ => cards.OrderByDescending(c => c.AverageRating).ThenByDescending(c => c.ReviewCount).ThenBy(c => c.Price)
            };

            var model = new ServiceCatalogViewModel
            {
                Search = search,
                CategoryId = categoryId,
                City = city,
                AvailableOnly = availableOnly,
                PriceRanges = priceRanges,
                SortBy = sortBy,
                Categories = categories,
                Cities = cities,
                Services = cards.ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Providers(string? search, string? city, bool availableOnly = false)
        {
            var cities = await _context.ProviderProfiles
                .Where(p => p.IsVerified && !string.IsNullOrWhiteSpace(p.City))
                .Select(p => p.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var query = _context.ProviderProfiles
                .Include(p => p.User)
                .Include(p => p.Services)
                    .ThenInclude(s => s.Category)
                .Where(p => p.IsVerified)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p =>
                    p.BusinessName.ToLower().Contains(term) ||
                    p.User.FullName.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(p => p.City == city);
            }

            if (availableOnly)
            {
                query = query.Where(p => p.IsAvailable);
            }

            var profiles = await query.ToListAsync();

            var providerRatings = await _context.Reviews
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

            var items = profiles.Select(p =>
            {
                providerRatings.TryGetValue(p.ProviderProfileId, out var stat);
                return new ProviderCardItem
                {
                    ProviderProfileId = p.ProviderProfileId,
                    BusinessName = p.BusinessName,
                    OwnerName = p.User.FullName,
                    City = p.City,
                    ExperienceYears = p.ExperienceYears,
                    Description = p.Description,
                    IsVerified = p.IsVerified,
                    IsAvailable = p.IsAvailable,
                    ServiceCount = p.Services.Count,
                    AverageRating = stat?.Average ?? 0,
                    ReviewCount = stat?.Count ?? 0,
                    Categories = p.Services.Select(s => s.Category.CategoryName).Distinct().OrderBy(n => n).ToList()
                };
            })
            .OrderByDescending(p => p.IsAvailable)
            .ThenByDescending(p => p.AverageRating)
            .ThenByDescending(p => p.ExperienceYears)
            .ToList();

            var model = new ProviderSearchViewModel
            {
                Search = search,
                City = city,
                AvailableOnly = availableOnly,
                Cities = cities,
                Providers = items
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ProviderDetails(int id)
        {
            var profile = await _context.ProviderProfiles
                .Include(p => p.User)
                .Include(p => p.Services)
                    .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(p => p.ProviderProfileId == id && p.IsVerified);

            if (profile == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.ProviderService)
                .Where(r => r.Booking.ProviderService.ProviderProfileId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var completedJobs = await _context.Bookings
                .CountAsync(b => b.ProviderService.ProviderProfileId == id && b.Status == BookingStatus.Completed);

            var avgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            var model = new ProviderDetailsViewModel
            {
                Profile = profile,
                Reviews = reviews,
                AverageRating = avgRating,
                ReviewCount = reviews.Count,
                CompletedJobsCount = completedJobs,
                Services = profile.Services.OrderBy(s => s.Price).Select(s => new ServiceCardViewModel
                {
                    ProviderServiceId = s.ProviderServiceId,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    Price = s.Price,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category.CategoryName,
                    ProviderProfileId = profile.ProviderProfileId,
                    BusinessName = profile.BusinessName,
                    City = profile.City,
                    IsVerified = profile.IsVerified,
                    IsAvailable = profile.IsAvailable,
                    AverageRating = avgRating,
                    ReviewCount = reviews.Count
                }).ToList()
            };

            return View(model);
        }
    }
}
