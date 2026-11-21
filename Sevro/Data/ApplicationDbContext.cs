using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sevro.Models;

namespace Sevro.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
        public DbSet<ProviderProfile> ProviderProfiles => Set<ProviderProfile>();
        public DbSet<ProviderService> ProviderServices => Set<ProviderService>();
        public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Review> Reviews => Set<Review>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ServiceCategory
            builder.Entity<ServiceCategory>(entity =>
            {
                entity.HasKey(c => c.CategoryId);
                entity.HasIndex(c => c.CategoryName).IsUnique();
            });

            // ProviderProfile (1:1 with ApplicationUser, Unique UserId)
            builder.Entity<ProviderProfile>(entity =>
            {
                entity.HasKey(p => p.ProviderProfileId);
                entity.HasIndex(p => p.UserId).IsUnique();
                entity.Property(p => p.IsVerified).HasDefaultValue(false);
                entity.Property(p => p.IsAvailable).HasDefaultValue(true);

                entity.HasOne(p => p.User)
                      .WithOne(u => u.ProviderProfile)
                      .HasForeignKey<ProviderProfile>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ProviderService
            builder.Entity<ProviderService>(entity =>
            {
                entity.HasKey(ps => ps.ProviderServiceId);
                entity.Property(ps => ps.Price).HasColumnType("decimal(10,2)");

                entity.HasOne(ps => ps.ProviderProfile)
                      .WithMany(p => p.Services)
                      .HasForeignKey(ps => ps.ProviderProfileId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ps => ps.Category)
                      .WithMany(c => c.ProviderServices)
                      .HasForeignKey(ps => ps.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // CustomerAddress
            builder.Entity<CustomerAddress>(entity =>
            {
                entity.HasKey(a => a.AddressId);

                entity.HasOne(a => a.Customer)
                      .WithMany(u => u.Addresses)
                      .HasForeignKey(a => a.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Booking
            builder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.BookingId);
                entity.Property(b => b.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);
                entity.Property(b => b.BookingDate)
                      .HasColumnType("date");

                entity.HasOne(b => b.Customer)
                      .WithMany(u => u.Bookings)
                      .HasForeignKey(b => b.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.ProviderService)
                      .WithMany(ps => ps.Bookings)
                      .HasForeignKey(b => b.ProviderServiceId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Address)
                      .WithMany(a => a.Bookings)
                      .HasForeignKey(b => b.AddressId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Review (1:1 optional with Booking, Unique BookingId, Rating 1-5)
            builder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.ReviewId);
                entity.HasIndex(r => r.BookingId).IsUnique();
                entity.ToTable(t => t.HasCheckConstraint("CK_Reviews_Rating", "`Rating` >= 1 AND `Rating` <= 5"));

                entity.HasOne(r => r.Booking)
                      .WithOne(b => b.Review)
                      .HasForeignKey<Review>(r => r.BookingId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Customer)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(r => r.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
