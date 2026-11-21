using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sevro.Data;
using Sevro.Models;
using Sevro.Models.Enums;
using Sevro.ViewModels.Admin;

namespace Sevro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var pendingProviders = await _context.ProviderProfiles
                .Include(p => p.User)
                .Where(p => !p.IsVerified)
                .OrderByDescending(p => p.ProviderProfileId)
                .ToListAsync();

            var recentBookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.ProviderService)
                    .ThenInclude(ps => ps.ProviderProfile)
                .Include(b => b.ProviderService)
                    .ThenInclude(ps => ps.Category)
                .OrderByDescending(b => b.CreatedAt)
                .Take(8)
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalProviders = await _context.ProviderProfiles.CountAsync(),
                PendingVerifications = pendingProviders.Count,
                TotalBookings = await _context.Bookings.CountAsync(),
                TotalCategories = await _context.ServiceCategories.CountAsync(),
                PendingProviders = pendingProviders.Take(5).ToList(),
                RecentBookings = recentBookings
            };

            return View(model);
        }

        // --- Manage Users ---
        [HttpGet]
        public async Task<IActionResult> Users(string? role = null, string? search = null)
        {
            var users = await _context.Users
                .Include(u => u.ProviderProfile)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var result = new List<UserListItemViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var primaryRole = roles.FirstOrDefault() ?? "Customer";

                if (!string.IsNullOrWhiteSpace(role) && !string.Equals(primaryRole, role, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim().ToLower();
                    var matches = u.FullName.ToLower().Contains(term)
                        || (u.Email != null && u.Email.ToLower().Contains(term))
                        || (u.ProviderProfile?.BusinessName.ToLower().Contains(term) ?? false);
                    if (!matches) continue;
                }

                result.Add(new UserListItemViewModel
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber,
                    Role = primaryRole,
                    CreatedAt = u.CreatedAt,
                    IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow,
                    IsProviderVerified = u.ProviderProfile?.IsVerified,
                    BusinessName = u.ProviderProfile?.BusinessName
                });
            }

            ViewBag.RoleFilter = role;
            ViewBag.Search = search;
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Admin accounts cannot be suspended.";
                return RedirectToAction(nameof(Users));
            }

            var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
            if (isLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["Success"] = $"{user.FullName}'s account has been reactivated.";
            }
            else
            {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(10));
                TempData["Success"] = $"{user.FullName}'s account has been suspended.";
            }

            return RedirectToAction(nameof(Users));
        }

        // --- Manage Service Categories (CRUD) ---
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.ServiceCategories
                .Include(c => c.ProviderServices)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View("CategoryForm", new CategoryFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("CategoryForm", model);
            }

            var name = model.CategoryName.Trim();
            var exists = await _context.ServiceCategories
                .AnyAsync(c => c.CategoryName.ToLower() == name.ToLower());

            if (exists)
            {
                ModelState.AddModelError(nameof(model.CategoryName), "A category with this name already exists.");
                return View("CategoryForm", model);
            }

            _context.ServiceCategories.Add(new ServiceCategory
            {
                CategoryName = name,
                Description = model.Description?.Trim()
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Category '{name}' created.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.ServiceCategories.FindAsync(id);
            if (category == null) return NotFound();

            return View("CategoryForm", new CategoryFormViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("CategoryForm", model);
            }

            var category = await _context.ServiceCategories.FindAsync(model.CategoryId);
            if (category == null) return NotFound();

            var name = model.CategoryName.Trim();
            var duplicate = await _context.ServiceCategories
                .AnyAsync(c => c.CategoryId != model.CategoryId && c.CategoryName.ToLower() == name.ToLower());

            if (duplicate)
            {
                ModelState.AddModelError(nameof(model.CategoryName), "Another category already uses this name.");
                return View("CategoryForm", model);
            }

            category.CategoryName = name;
            category.Description = model.Description?.Trim();
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Category '{name}' updated.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.ServiceCategories
                .Include(c => c.ProviderServices)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound();

            if (category.ProviderServices.Any())
            {
                TempData["Error"] = $"Cannot delete '{category.CategoryName}' because {category.ProviderServices.Count} provider service(s) are assigned to it.";
                return RedirectToAction(nameof(Categories));
            }

            _context.ServiceCategories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Category '{category.CategoryName}' deleted.";
            return RedirectToAction(nameof(Categories));
        }

        // --- Verify / Reject Service Providers ---
        [HttpGet]
        public async Task<IActionResult> Providers(string? filter = null)
        {
            var query = _context.ProviderProfiles
                .Include(p => p.User)
                .Include(p => p.Services)
                .AsQueryable();

            if (string.Equals(filter, "pending", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => !p.IsVerified);
            }
            else if (string.Equals(filter, "verified", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.IsVerified);
            }

            ViewBag.Filter = filter;
            var providers = await query
                .OrderBy(p => p.IsVerified)
                .ThenByDescending(p => p.ProviderProfileId)
                .ToListAsync();

            return View(providers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyProvider(int id, string? returnUrl = null)
        {
            var provider = await _context.ProviderProfiles.FindAsync(id);
            if (provider == null) return NotFound();

            provider.IsVerified = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{provider.BusinessName} has been verified and is now live in the marketplace.";
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Providers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectProvider(int id, string? returnUrl = null)
        {
            var provider = await _context.ProviderProfiles.FindAsync(id);
            if (provider == null) return NotFound();

            provider.IsVerified = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Verification removed/rejected for {provider.BusinessName}.";
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Providers));
        }

        // --- Manage All Platform Bookings ---
        [HttpGet]
        public async Task<IActionResult> Bookings(string? status = null)
        {
            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Address)
                .Include(b => b.ProviderService)
                    .ThenInclude(ps => ps.ProviderProfile)
                .Include(b => b.ProviderService)
                    .ThenInclude(ps => ps.Category)
                .Include(b => b.Review)
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
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Rejected)
            {
                TempData["Error"] = $"Booking #{booking.BookingId} is already {booking.Status} and cannot be cancelled.";
                return RedirectToAction(nameof(Bookings));
            }

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Booking #{booking.BookingId} has been cancelled by Admin.";
            return RedirectToAction(nameof(Bookings));
        }
    }
}
