using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sevro.Data;
using Sevro.Helpers;
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
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
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
                TotalCatalogServices = await _context.CatalogServices.CountAsync(),
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
                    continue;

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
        public async Task<IActionResult> ToggleUserLock(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
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

        // --- Manage Service Categories (CRUD + Icon/Image + Active Toggle) ---
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.ServiceCategories
                .Include(c => c.CatalogServices)
                .Include(c => c.ProviderServices)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View("CategoryForm", new CategoryFormViewModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("CategoryForm", model);

            var name = model.CategoryName.Trim();
            var exists = await _context.ServiceCategories
                .AnyAsync(c => c.CategoryName.ToLower() == name.ToLower());

            if (exists)
            {
                ModelState.AddModelError(nameof(model.CategoryName), "A category with this name already exists.");
                return View("CategoryForm", model);
            }

            string? imagePath = await SaveUploadedImageAsync(model.IconImageFile, "categories");

            _context.ServiceCategories.Add(new ServiceCategory
            {
                CategoryName = name,
                Slug = PricingFormatter.Slugify(name),
                Description = model.Description?.Trim(),
                IconClass = string.IsNullOrWhiteSpace(model.IconClass) ? "bi-grid-fill" : model.IconClass.Trim(),
                ImagePath = imagePath,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder
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
                Description = category.Description,
                IconClass = category.IconClass,
                ExistingImagePath = category.ImagePath,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("CategoryForm", model);

            var category = await _context.ServiceCategories.FindAsync(model.CategoryId);
            if (category == null) return NotFound();

            var name = model.CategoryName.Trim();
            var duplicate = await _context.ServiceCategories
                .AnyAsync(c => c.CategoryId != model.CategoryId && c.CategoryName.ToLower() == name.ToLower());

            if (duplicate)
            {
                ModelState.AddModelError(nameof(model.CategoryName), "Another category already uses this name.");
                model.ExistingImagePath = category.ImagePath;
                return View("CategoryForm", model);
            }

            if (model.IconImageFile != null && model.IconImageFile.Length > 0)
            {
                category.ImagePath = await SaveUploadedImageAsync(model.IconImageFile, "categories");
            }

            category.CategoryName = name;
            category.Slug = PricingFormatter.Slugify(name);
            category.Description = model.Description?.Trim();
            category.IconClass = string.IsNullOrWhiteSpace(model.IconClass) ? "bi-grid-fill" : model.IconClass.Trim();
            category.IsActive = model.IsActive;
            category.DisplayOrder = model.DisplayOrder;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Category '{name}' updated.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategoryStatus(int id)
        {
            var category = await _context.ServiceCategories.FindAsync(id);
            if (category == null) return NotFound();

            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Category '{category.CategoryName}' is now {(category.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.ServiceCategories
                .Include(c => c.CatalogServices)
                .Include(c => c.ProviderServices)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound();

            if (category.ProviderServices.Any() || category.CatalogServices.Any())
            {
                TempData["Error"] = $"Cannot delete '{category.CategoryName}' because {category.CatalogServices.Count} catalog service(s) and {category.ProviderServices.Count} provider service(s) are linked to it. Deactivate it instead.";
                return RedirectToAction(nameof(Categories));
            }

            _context.ServiceCategories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Category '{category.CategoryName}' deleted.";
            return RedirectToAction(nameof(Categories));
        }

        // --- Master Service Catalog Management (CRUD + Search + Filter + Bulk Price Update) ---
        [HttpGet]
        public async Task<IActionResult> Services(string? search = null, int? categoryId = null, string? pricingType = null, string? status = null)
        {
            var categories = await _context.ServiceCategories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

            var query = _context.CatalogServices
                .Include(cs => cs.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(cs =>
                    cs.Name.ToLower().Contains(term) ||
                    (cs.Description != null && cs.Description.ToLower().Contains(term)) ||
                    cs.Category.CategoryName.ToLower().Contains(term));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(cs => cs.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(pricingType) && Enum.TryParse<PricingType>(pricingType, true, out var parsedType))
            {
                query = query.Where(cs => cs.PricingType == parsedType);
            }

            if (string.Equals(status, "active", StringComparison.OrdinalIgnoreCase))
                query = query.Where(cs => cs.IsActive);
            else if (string.Equals(status, "inactive", StringComparison.OrdinalIgnoreCase))
                query = query.Where(cs => !cs.IsActive);
            else if (string.Equals(status, "popular", StringComparison.OrdinalIgnoreCase))
                query = query.Where(cs => cs.IsPopular);

            var services = await query
                .OrderBy(cs => cs.Category.DisplayOrder)
                .ThenBy(cs => cs.Name)
                .ToListAsync();

            var providerCounts = await _context.ProviderServices
                .Include(ps => ps.ProviderProfile)
                .Where(ps => ps.CatalogServiceId.HasValue && ps.IsActive && ps.ProviderProfile.IsVerified)
                .GroupBy(ps => ps.CatalogServiceId!.Value)
                .Select(g => new { CatalogServiceId = g.Key, Count = g.Select(x => x.ProviderProfileId).Distinct().Count() })
                .ToDictionaryAsync(x => x.CatalogServiceId, x => x.Count);

            var model = new AdminCatalogIndexViewModel
            {
                Search = search,
                CategoryId = categoryId,
                PricingType = pricingType,
                StatusFilter = status,
                Categories = categories,
                TotalCatalogServices = await _context.CatalogServices.CountAsync(),
                ActiveServicesCount = await _context.CatalogServices.CountAsync(cs => cs.IsActive),
                PopularServicesCount = await _context.CatalogServices.CountAsync(cs => cs.IsPopular),
                QuoteServicesCount = await _context.CatalogServices.CountAsync(cs => cs.PricingType == PricingType.QuoteRequired),
                Items = services.Select(s => new AdminCatalogListItem
                {
                    Service = s,
                    VerifiedProviderCount = providerCounts.TryGetValue(s.CatalogServiceId, out var c) ? c : 0
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateService(int? categoryId = null)
        {
            return View("ServiceForm", new CatalogServiceFormViewModel
            {
                CategoryId = categoryId ?? 0,
                PricingType = PricingType.Fixed,
                Unit = "Per job",
                IsActive = true,
                Categories = await GetCategorySelectListAsync()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(CatalogServiceFormViewModel model)
        {
            ValidateCatalogPrice(model);
            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategorySelectListAsync();
                return View("ServiceForm", model);
            }

            var category = await _context.ServiceCategories.FindAsync(model.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Selected category was not found.");
                model.Categories = await GetCategorySelectListAsync();
                return View("ServiceForm", model);
            }

            var baseSlug = $"{PricingFormatter.Slugify(category.CategoryName)}-{PricingFormatter.Slugify(model.Name)}";
            var slug = await EnsureUniqueServiceSlugAsync(baseSlug);

            var entity = new CatalogService
            {
                CategoryId = model.CategoryId,
                Name = model.Name.Trim(),
                Slug = slug,
                Description = model.Description?.Trim(),
                Price = model.PricingType == PricingType.QuoteRequired ? null : model.Price,
                PricingType = model.PricingType,
                Unit = string.IsNullOrWhiteSpace(model.Unit) ? "Per job" : model.Unit.Trim(),
                EstimatedDuration = model.EstimatedDuration?.Trim(),
                IsActive = model.IsActive,
                IsPopular = model.IsPopular,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CatalogServices.Add(entity);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Catalog service '{entity.Name}' added.";
            return RedirectToAction(nameof(Services), new { categoryId = entity.CategoryId });
        }

        [HttpGet]
        public async Task<IActionResult> EditService(int id)
        {
            var service = await _context.CatalogServices.FindAsync(id);
            if (service == null) return NotFound();

            return View("ServiceForm", new CatalogServiceFormViewModel
            {
                CatalogServiceId = service.CatalogServiceId,
                CategoryId = service.CategoryId,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                PricingType = service.PricingType,
                Unit = service.Unit,
                EstimatedDuration = service.EstimatedDuration,
                IsActive = service.IsActive,
                IsPopular = service.IsPopular,
                Categories = await GetCategorySelectListAsync()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(CatalogServiceFormViewModel model)
        {
            ValidateCatalogPrice(model);
            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategorySelectListAsync();
                return View("ServiceForm", model);
            }

            var service = await _context.CatalogServices.FindAsync(model.CatalogServiceId);
            if (service == null) return NotFound();

            service.CategoryId = model.CategoryId;
            service.Name = model.Name.Trim();
            service.Description = model.Description?.Trim();
            service.Price = model.PricingType == PricingType.QuoteRequired ? null : model.Price;
            service.PricingType = model.PricingType;
            service.Unit = string.IsNullOrWhiteSpace(model.Unit) ? "Per job" : model.Unit.Trim();
            service.EstimatedDuration = model.EstimatedDuration?.Trim();
            service.IsActive = model.IsActive;
            service.IsPopular = model.IsPopular;
            service.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Catalog service '{service.Name}' updated.";
            return RedirectToAction(nameof(Services), new { categoryId = service.CategoryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleServiceStatus(int id, int? categoryId = null)
        {
            var service = await _context.CatalogServices.FindAsync(id);
            if (service == null) return NotFound();

            service.IsActive = !service.IsActive;
            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{service.Name}' is now {(service.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction(nameof(Services), new { categoryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleServicePopular(int id, int? categoryId = null)
        {
            var service = await _context.CatalogServices.FindAsync(id);
            if (service == null) return NotFound();

            service.IsPopular = !service.IsPopular;
            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{service.Name}' {(service.IsPopular ? "marked as Popular" : "removed from Popular")}.";
            return RedirectToAction(nameof(Services), new { categoryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.CatalogServices.FindAsync(id);
            if (service == null) return NotFound();

            var catId = service.CategoryId;
            _context.CatalogServices.Remove(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Catalog service '{service.Name}' deleted.";
            return RedirectToAction(nameof(Services), new { categoryId = catId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdatePrices(int? categoryId, string mode, decimal amount)
        {
            if (amount == 0)
            {
                TempData["Error"] = "Enter a non-zero price adjustment amount.";
                return RedirectToAction(nameof(Services), new { categoryId });
            }

            var query = _context.CatalogServices
                .Where(cs => cs.PricingType != PricingType.QuoteRequired && cs.Price.HasValue);

            if (categoryId.HasValue)
            {
                query = query.Where(cs => cs.CategoryId == categoryId.Value);
            }

            var targets = await query.ToListAsync();
            if (!targets.Any())
            {
                TempData["Error"] = "No priced services found for bulk update.";
                return RedirectToAction(nameof(Services), new { categoryId });
            }

            var now = DateTime.UtcNow;
            foreach (var svc in targets)
            {
                var current = svc.Price!.Value;
                var updated = string.Equals(mode, "percent", StringComparison.OrdinalIgnoreCase)
                    ? Math.Round(current * (1m + (amount / 100m)), 0)
                    : current + amount;

                // Prevent negative or zero prices on priced services
                svc.Price = Math.Max(10m, updated);
                svc.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            var label = string.Equals(mode, "percent", StringComparison.OrdinalIgnoreCase) ? $"{amount:+#;-#;0}%" : $"₹{amount:+#;-#;0}";
            TempData["Success"] = $"Updated prices for {targets.Count} service(s) by {label}.";
            return RedirectToAction(nameof(Services), new { categoryId });
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
                query = query.Where(p => !p.IsVerified);
            else if (string.Equals(filter, "verified", StringComparison.OrdinalIgnoreCase))
                query = query.Where(p => p.IsVerified);

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
                return Redirect(returnUrl);
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
                return Redirect(returnUrl);
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
                query = query.Where(b => b.Status == parsed);

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

        private void ValidateCatalogPrice(CatalogServiceFormViewModel model)
        {
            if (model.PricingType == PricingType.QuoteRequired)
            {
                model.Price = null;
                return;
            }

            if (!model.Price.HasValue || model.Price.Value <= 0)
            {
                ModelState.AddModelError(nameof(model.Price), "Enter a valid positive price in ₹ (or choose Quote Required).");
            }
        }

        private async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            return await _context.ServiceCategories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();
        }

        private async Task<string> EnsureUniqueServiceSlugAsync(string baseSlug)
        {
            var candidate = baseSlug.Length > 170 ? baseSlug[..170].TrimEnd('-') : baseSlug;
            var suffix = 1;
            while (await _context.CatalogServices.AnyAsync(cs => cs.Slug == candidate))
            {
                candidate = $"{baseSlug}-{suffix++}";
            }
            return candidate;
        }

        private async Task<string?> SaveUploadedImageAsync(IFormFile? file, string subfolder)
        {
            if (file == null || file.Length == 0) return null;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp", ".svg" };
            if (!allowed.Contains(ext) || file.Length > 3 * 1024 * 1024) return null;

            var dir = Path.Combine(_env.WebRootPath, "uploads", subfolder);
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(dir, fileName);
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{subfolder}/{fileName}";
        }
    }
}
