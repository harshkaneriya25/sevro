using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sevro.Data;
using Sevro.Models;
using Sevro.Models.Enums;
using Sevro.ViewModels.Provider;

namespace Sevro.Controllers
{
    [Authorize(Roles = "Provider")]
    public class ProviderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProviderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<ProviderProfile?> GetCurrentProviderProfileAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.ProviderProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<IActionResult> Dashboard()
        {
            var profile = await GetCurrentProviderProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Account");

            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Address)
                .Include(b => b.ProviderService)
                    .ThenInclude(ps => ps.Category)
                .Where(b => b.ProviderService.ProviderProfileId == profile.ProviderProfileId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var serviceCount = await _context.ProviderServices
                .CountAsync(s => s.ProviderProfileId == profile.ProviderProfileId);

            var reviewRatings = await _context.Reviews
                .Where(r => r.Booking.ProviderService.ProviderProfileId == profile.ProviderProfileId)
                .Select(r => r.Rating)
                .ToListAsync();

            var model = new ProviderDashboardViewModel
            {
                Profile = profile,
                PendingRequests = bookings.Count(b => b.Status == BookingStatus.Pending),
                ActiveJobs = bookings.Count(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress),
                CompletedJobs = bookings.Count(b => b.Status == BookingStatus.Completed),
                ListedServices = serviceCount,
                AverageRating = reviewRatings.Any() ? reviewRatings.Average() : 0,
                ReviewCount = reviewRatings.Count,
                RecentBookings = bookings.Take(8).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var profile = await GetCurrentProviderProfileAsync();
            if (profile == null) return NotFound();

            var reviewRatings = await _context.Reviews
                .Where(r => r.Booking.ProviderService.ProviderProfileId == profile.ProviderProfileId)
                .Select(r => r.Rating)
                .ToListAsync();

            var model = new ProviderProfileViewModel
            {
                ProviderProfileId = profile.ProviderProfileId,
                FullName = profile.User.FullName,
                Email = profile.User.Email ?? string.Empty,
                PhoneNumber = profile.User.PhoneNumber ?? string.Empty,
                BusinessName = profile.BusinessName,
                ExperienceYears = profile.ExperienceYears,
                City = profile.City,
                Description = profile.Description,
                IsVerified = profile.IsVerified,
                IsAvailable = profile.IsAvailable,
                AverageRating = reviewRatings.Any() ? reviewRatings.Average() : 0,
                ReviewCount = reviewRatings.Count
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProviderProfileViewModel model)
        {
            var profile = await GetCurrentProviderProfileAsync();
            if (profile == null) return NotFound();

            model.Email = profile.User.Email ?? string.Empty;
            model.IsVerified = profile.IsVerified;
            model.IsAvailable = profile.IsAvailable;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            profile.User.FullName = model.FullName.Trim();
            profile.User.PhoneNumber = model.PhoneNumber.Trim();
            profile.BusinessName = model.BusinessName.Trim();
            profile.ExperienceYears = model.ExperienceYears;
            profile.City = model.City.Trim();
            profile.Description = model.Description?.Trim();

            await _userManager.UpdateAsync(profile.User);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Business profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(string? returnUrl = null)
        {
            var profile = await GetCurrentProviderProfileAsync();
            if (profile == null) return NotFound();

            profile.IsAvailable = !profile.IsAvailable;
            await _context.SaveChangesAsync();

            var statusLabel = profile.IsAvailable ? "Available" : "Unavailable";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, isAvailable = profile.IsAvailable, label = statusLabel });
            }

            TempData["Success"] = $"Availability status updated to {statusLabel}.";
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Dashboard));
        }

        // --- Provider Services CRUD ---
        [HttpGet]
        public async Task<IActionResult> Services()
        {
            var profile = await GetCurrentProviderProfileAsync();
            if (profile == null) return NotFound();

            var services = await _context.ProviderServices
                .Include(s => s.Category)
                .Include(s => s.Bookings)
                .Where(s => s.ProviderProfileId == profile.ProviderProfileId)
                .OrderByDescending(s => s.ProviderServiceId)
                .ToListAsync();

            ViewBag.IsVerified = profile.IsVerified;
            return View(services);
        }

        [HttpGet]
        public async Task<IActionResult> CreateService()
        {
            var model = new ProviderServiceFormViewModel
            {
                Categories = await GetCategorySelectListAsync()
            };
            return View("ServiceForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(ProviderServiceFormViewModel model)
        {
            var profile = await GetCurrentProviderProfileAsync();
            if (profile == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategorySelectListAsync();
                return View("ServiceForm", model);
            }

            var service = new ProviderService
            {
                ProviderProfileId = profile.ProviderProfileId,
                CategoryId = model.CategoryId,
                ServiceName = model.ServiceName.Trim(),
                Description = model.Description?.Trim(),
                Price = model.Price
            };

            _context.ProviderServices.Add(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service added to your menu.";
            return RedirectToAction(nameof(Services));
        }

        [HttpGet]
        public async Task<IActionResult> EditService(int id)
        {
            var profile = await GetCurrentProviderProfileAsync();
            if (profile == null) return NotFound();

            var service = await _context.ProviderServices
                .FirstOrDefaultAsync(s => s.ProviderServiceId == id && s.ProviderProfileId == profile.ProviderProfileId);

            if (service == null) return NotFound();

            var model = new ProviderServiceFormViewModel
            {
                ProviderServiceId = service.ProviderServiceId,
                CategoryId = service.CategoryId,
                ServiceName = service.ServiceName,
                Description = service.Description,
                Price = service.Price,
                Categories = await GetCategorySelectListAsync()
            };

            return View("ServiceForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(ProviderServiceFormViewModel model)
        {
            var profile = await GetCurrentProviderProfileAsync();
            if (profile == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategorySelectListAsync();
                return View("ServiceForm", model);
            }

            var service = await _context.ProviderServices
                .FirstOrDefaultAsync(s => s.ProviderServiceId == model.ProviderServiceId && s.ProviderProfileId == profile.ProviderProfileId);

            if (service == null) return NotFound();

            service.CategoryId = model.CategoryId;
            service.ServiceName = model.ServiceName.Trim();
            service.Description = model.Description?.Trim();
            service.Price = model.Price;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Service details updated.";
            return RedirectToAction(nameof(Services));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var profile = await GetCurrentProviderProfileAsync();
            if (profile == null) return NotFound();

            var service = await _context.ProviderServices
                .FirstOrDefaultAsync(s => s.ProviderServiceId == id && s.ProviderProfileId == profile.ProviderProfileId);

            if (service == null) return NotFound();

            var hasBookings = await _context.Bookings.AnyAsync(b => b.ProviderServiceId == id);
            if (hasBookings)
            {
                TempData["Error"] = "Cannot delete a service that has existing or historical bookings.";
                return RedirectToAction(nameof(Services));
            }

            _context.ProviderServices.Remove(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service removed from your menu.";
            return RedirectToAction(nameof(Services));
        }

        // --- Manage Incoming & Active Bookings ---
        [HttpGet]
        public async Task<IActionResult> Bookings(string? status = null)
        {
            var profile = await GetCurrentProviderProfileAsync();
            if (profile == null) return NotFound();

            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Address)
                .Include(b => b.ProviderService)
                    .ThenInclude(ps => ps.Category)
                .Include(b => b.Review)
                .Where(b => b.ProviderService.ProviderProfileId == profile.ProviderProfileId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var parsed))
            {
                query = query.Where(b => b.Status == parsed);
            }

            ViewBag.CurrentStatus = status;
            var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBookingStatus(int id, string nextStatus, string? returnUrl = null)
        {
            var profile = await GetCurrentProviderProfileAsync();
            if (profile == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.ProviderService)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.ProviderService.ProviderProfileId == profile.ProviderProfileId);

            if (booking == null) return NotFound();

            if (!Enum.TryParse<BookingStatus>(nextStatus, true, out var target))
            {
                TempData["Error"] = "Invalid status requested.";
                return RedirectToAction(nameof(Bookings));
            }

            // Enforce strict provider workflow transitions
            var isValidTransition = (booking.Status, target) switch
            {
                (BookingStatus.Pending, BookingStatus.Confirmed) => true,
                (BookingStatus.Pending, BookingStatus.Rejected) => true,
                (BookingStatus.Confirmed, BookingStatus.InProgress) => true,
                (BookingStatus.InProgress, BookingStatus.Completed) => true,
                _ => false
            };

            if (!isValidTransition)
            {
                TempData["Error"] = $"Cannot transition Booking #{booking.BookingId} from {booking.Status} to {target}.";
            }
            else
            {
                booking.Status = target;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Booking #{booking.BookingId} updated to {target}.";
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Bookings));
        }

        // --- Customer Reviews ---
        [HttpGet]
        public async Task<IActionResult> Reviews()
        {
            var profile = await GetCurrentProviderProfileAsync();
            if (profile == null) return NotFound();

            var reviews = await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.ProviderService)
                .Where(r => r.Booking.ProviderService.ProviderProfileId == profile.ProviderProfileId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.BusinessName = profile.BusinessName;
            ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;
            ViewBag.ReviewCount = reviews.Count;

            return View(reviews);
        }

        private async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            return await _context.ServiceCategories
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();
        }
    }
}
