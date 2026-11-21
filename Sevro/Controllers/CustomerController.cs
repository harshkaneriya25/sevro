using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sevro.Data;
using Sevro.Models;
using Sevro.Models.Enums;
using Sevro.ViewModels.Customer;

namespace Sevro.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string CurrentUserId => _userManager.GetUserId(User)!;

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var bookings = await _context.Bookings
                .Include(b => b.ProviderService)
                    .ThenInclude(ps => ps.ProviderProfile)
                .Include(b => b.ProviderService)
                    .ThenInclude(ps => ps.Category)
                .Include(b => b.Address)
                .Include(b => b.Review)
                .Where(b => b.CustomerId == user.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var addressCount = await _context.CustomerAddresses.CountAsync(a => a.CustomerId == user.Id);

            var model = new CustomerDashboardViewModel
            {
                FullName = user.FullName,
                TotalBookings = bookings.Count,
                ActiveBookings = bookings.Count(b =>
                    b.Status == BookingStatus.Pending ||
                    b.Status == BookingStatus.Confirmed ||
                    b.Status == BookingStatus.InProgress),
                CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
                SavedAddresses = addressCount,
                RecentBookings = bookings.Take(6).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new CustomerProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(CustomerProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            model.Email = user.Email ?? string.Empty;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Your profile has been updated.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // --- Saved Addresses CRUD ---
        [HttpGet]
        public async Task<IActionResult> Addresses()
        {
            var addresses = await _context.CustomerAddresses
                .Where(a => a.CustomerId == CurrentUserId)
                .OrderByDescending(a => a.AddressId)
                .ToListAsync();

            return View(addresses);
        }

        [HttpGet]
        public IActionResult AddAddress(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("AddressForm", new AddressViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(AddressViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View("AddressForm", model);
            }

            var address = new CustomerAddress
            {
                CustomerId = CurrentUserId,
                Address = model.Address.Trim(),
                City = model.City.Trim(),
                Pincode = model.Pincode.Trim()
            };

            _context.CustomerAddresses.Add(address);
            await _context.SaveChangesAsync();

            TempData["Success"] = "New service address added.";
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Addresses));
        }

        [HttpGet]
        public async Task<IActionResult> EditAddress(int id)
        {
            var address = await _context.CustomerAddresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.CustomerId == CurrentUserId);

            if (address == null) return NotFound();

            var model = new AddressViewModel
            {
                AddressId = address.AddressId,
                Address = address.Address,
                City = address.City,
                Pincode = address.Pincode
            };

            return View("AddressForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(AddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("AddressForm", model);
            }

            var address = await _context.CustomerAddresses
                .FirstOrDefaultAsync(a => a.AddressId == model.AddressId && a.CustomerId == CurrentUserId);

            if (address == null) return NotFound();

            address.Address = model.Address.Trim();
            address.City = model.City.Trim();
            address.Pincode = model.Pincode.Trim();

            await _context.SaveChangesAsync();
            TempData["Success"] = "Address updated successfully.";
            return RedirectToAction(nameof(Addresses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var address = await _context.CustomerAddresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.CustomerId == CurrentUserId);

            if (address == null) return NotFound();

            var isUsedInBooking = await _context.Bookings.AnyAsync(b => b.AddressId == id);
            if (isUsedInBooking)
            {
                TempData["Error"] = "This address is linked to past or active bookings and cannot be deleted.";
                return RedirectToAction(nameof(Addresses));
            }

            _context.CustomerAddresses.Remove(address);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Address deleted.";
            return RedirectToAction(nameof(Addresses));
        }

        // --- Book a Service ---
        [HttpGet]
        public async Task<IActionResult> Book(int id)
        {
            var service = await _context.ProviderServices
                .Include(s => s.Category)
                .Include(s => s.ProviderProfile)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.ProviderServiceId == id);

            if (service == null || !service.ProviderProfile.IsVerified)
            {
                TempData["Error"] = "This service is not available for booking.";
                return RedirectToAction("Index", "Services");
            }

            if (!service.ProviderProfile.IsAvailable)
            {
                TempData["Error"] = $"{service.ProviderProfile.BusinessName} is currently marked Unavailable. Please check back later.";
                return RedirectToAction("ProviderDetails", "Services", new { id = service.ProviderProfileId });
            }

            var addresses = await _context.CustomerAddresses
                .Where(a => a.CustomerId == CurrentUserId)
                .OrderByDescending(a => a.AddressId)
                .ToListAsync();

            var reviews = await _context.Reviews
                .Where(r => r.Booking.ProviderService.ProviderProfileId == service.ProviderProfileId)
                .Select(r => r.Rating)
                .ToListAsync();

            var model = new CreateBookingViewModel
            {
                ProviderServiceId = service.ProviderServiceId,
                Service = service,
                SavedAddresses = addresses,
                AddressId = addresses.FirstOrDefault()?.AddressId ?? 0,
                BookingDate = DateTime.Today.AddDays(1),
                ProviderAverageRating = reviews.Any() ? reviews.Average() : 0,
                ProviderReviewCount = reviews.Count
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(CreateBookingViewModel model)
        {
            var service = await _context.ProviderServices
                .Include(s => s.Category)
                .Include(s => s.ProviderProfile)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.ProviderServiceId == model.ProviderServiceId);

            if (service == null || !service.ProviderProfile.IsVerified || !service.ProviderProfile.IsAvailable)
            {
                TempData["Error"] = "This service is currently unavailable for booking.";
                return RedirectToAction("Index", "Services");
            }

            var addresses = await _context.CustomerAddresses
                .Where(a => a.CustomerId == CurrentUserId)
                .OrderByDescending(a => a.AddressId)
                .ToListAsync();

            // Enforce Address Ownership integrity rule
            var ownsAddress = addresses.Any(a => a.AddressId == model.AddressId);
            if (!ownsAddress)
            {
                ModelState.AddModelError(nameof(model.AddressId), "Please select one of your own saved addresses.");
            }

            if (model.BookingDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.BookingDate), "Booking date cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                var reviews = await _context.Reviews
                    .Where(r => r.Booking.ProviderService.ProviderProfileId == service.ProviderProfileId)
                    .Select(r => r.Rating)
                    .ToListAsync();

                model.Service = service;
                model.SavedAddresses = addresses;
                model.ProviderAverageRating = reviews.Any() ? reviews.Average() : 0;
                model.ProviderReviewCount = reviews.Count;
                return View(model);
            }

            var booking = new Booking
            {
                CustomerId = CurrentUserId,
                ProviderServiceId = service.ProviderServiceId,
                AddressId = model.AddressId,
                BookingDate = model.BookingDate.Date,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Booking #{booking.BookingId} submitted! Track status below while {service.ProviderProfile.BusinessName} reviews your request.";
            return RedirectToAction(nameof(Bookings));
        }

        // --- My Bookings & Cancellation ---
        [HttpGet]
        public async Task<IActionResult> Bookings(string? status = null)
        {
            var query = _context.Bookings
                .Include(b => b.ProviderService)
                    .ThenInclude(ps => ps.ProviderProfile)
                        .ThenInclude(p => p.User)
                .Include(b => b.ProviderService)
                    .ThenInclude(ps => ps.Category)
                .Include(b => b.Address)
                .Include(b => b.Review)
                .Where(b => b.CustomerId == CurrentUserId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(b => b.Status == parsedStatus);
            }

            ViewBag.CurrentStatus = status;
            var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == id && b.CustomerId == CurrentUserId);

            if (booking == null) return NotFound();

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] = "Only bookings in Pending status can be cancelled.";
                return RedirectToAction(nameof(Bookings));
            }

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Booking #{booking.BookingId} has been cancelled.";
            return RedirectToAction(nameof(Bookings));
        }

        // --- Reviews ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(CreateReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please provide a valid rating between 1 and 5 stars.";
                return RedirectToAction(nameof(Bookings));
            }

            var booking = await _context.Bookings
                .Include(b => b.Review)
                .FirstOrDefaultAsync(b => b.BookingId == model.BookingId && b.CustomerId == CurrentUserId);

            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction(nameof(Bookings));
            }

            if (booking.Status != BookingStatus.Completed)
            {
                TempData["Error"] = "Reviews can only be submitted after the booking is marked Completed.";
                return RedirectToAction(nameof(Bookings));
            }

            if (booking.Review != null)
            {
                TempData["Error"] = "You have already submitted a review for this booking.";
                return RedirectToAction(nameof(Bookings));
            }

            var review = new Review
            {
                BookingId = booking.BookingId,
                CustomerId = CurrentUserId,
                Rating = model.Rating,
                Comment = model.Comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you! Your review has been published.";
            return RedirectToAction(nameof(Reviews));
        }

        [HttpGet]
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Booking)
                    .ThenInclude(b => b.ProviderService)
                        .ThenInclude(ps => ps.ProviderProfile)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.ProviderService)
                        .ThenInclude(ps => ps.Category)
                .Where(r => r.CustomerId == CurrentUserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }
    }
}
