using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using LuxuryCarRental.Data;
using LuxuryCarRental.Models;

namespace LuxuryCarRental
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (context.Vehicles.Any())
                return;

            // ctor-based Money helper
            static Money Rate(decimal amt) => new Money(amt, "USD");

            context.Vehicles.AddRange(
                // Cars
                new Car
                {
                    Make = "Toyota",
                    Model = "Camry",
                    Year = 2022,
                    DailyRate = Rate(49.99m),
                    VehicleType = VehicleType.Car
                },

                // Motorcycles
                new Motorcycle
                {
                    EngineCapacityCc = 700,
                    HasSidecar = false,
                    DailyRate = Rate(29.99m),
                    VehicleType = VehicleType.Motorcycle
                },
                new Motorcycle
                {
                    EngineCapacityCc = 1500,
                    HasSidecar = true,
                    DailyRate = Rate(34.99m),
                    VehicleType = VehicleType.Motorcycle
                },

                // Yachts
                new Yacht
                {
                    LengthInMeters = 20.5m,
                    CabinCount = 3,
                    DailyRate = Rate(999.99m),
                    VehicleType = VehicleType.Yacht
                },
                new Yacht
                {
                    LengthInMeters = 35m,
                    CabinCount = 5,
                    DailyRate = Rate(1499.99m),
                    VehicleType = VehicleType.Yacht
                }
            );

            context.SaveChanges();
        }
    }
}
