using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using LuxuryCarRental.Models;

namespace LuxuryCarRental.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { }

        // ctor for DI
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSets
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Motorcycle> Motorcycles { get; set; }
        public DbSet<Yacht> Yachts { get; set; }
        public DbSet<LuxuryCar> LuxuryCars { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Card> Cards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlite("Data Source=LuxuryRental.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .OwnsOne(c => c.Contact);

            modelBuilder.Entity<Vehicle>()
                   .HasDiscriminator(v => v.VehicleType) 
                   .HasValue<Car>(VehicleType.Car)
                   .HasValue<Motorcycle>(VehicleType.Motorcycle)
                   .HasValue<Yacht>(VehicleType.Yacht)
                   .HasValue<LuxuryCar>(VehicleType.LuxuryCar);

            modelBuilder.Entity<Vehicle>()
                .OwnsOne(v => v.DailyRate, mb =>
                {
                    mb.Property(m => m.Amount).HasColumnName("DailyRate");
                    mb.Property(m => m.Currency).HasColumnName("Currency");
                });

            modelBuilder.Entity<Card>()
                .HasOne(c => c.Customer)
                .WithMany(cu => cu.Cards)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}