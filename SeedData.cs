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

            // 1) Define your catalog entries
            //    The Key must match the computed Name of each Vehicle
            var entries = new (string Key, Func<Vehicle> Factory, string Image)[]
            {
                ("2022 Toyota Camry",
                  () => new Car {
                      Make        = "Toyota",
                      Model       = "Camry",
                      Year        = 2022,
                      DailyRate   = new Money(49.99m, "USD"),
                      VehicleType = VehicleType.Car
                  },
                  "pack://application:,,,/LuxuryCarRental;component/Images/Vehicles/car1.jpg"
                ),
                ("2021 Honda Civic",
                  () => new Car {
                      Make        = "Honda",
                      Model       = "Civic",
                      Year        = 2021,
                      DailyRate   = new Money(44.99m, "USD"),
                      VehicleType = VehicleType.Car
                  },
                  "pack://application:,,,/LuxuryCarRental;component/Images/Vehicles/car1.jpg"
                ),
                ("Motorcycle 700cc",
                  () => new Motorcycle {
                      EngineCapacityCc = 700,
                      HasSidecar       = false,
                      DailyRate        = new Money(29.99m, "USD"),
                      VehicleType      = VehicleType.Motorcycle
                  },
                  "pack://application:,,,/LuxuryCarRental;component/Images/Vehicles/motorcycle1.jpg"
                ),
                ("Sidecar Motorcycle 1500cc",
                  () => new Motorcycle {
                      EngineCapacityCc = 1500,
                      HasSidecar       = true,
                      DailyRate        = new Money(34.99m, "USD"),
                      VehicleType      = VehicleType.Motorcycle
                  },
                  "pack://application:,,,/LuxuryCarRental;component/Images/Vehicles/motorcycle1.jpg"
                ),
                ("20.5m Yacht with 3 cabins",
                  () => new Yacht {
                      LengthInMeters = 20.5m,
                      CabinCount     = 3,
                      DailyRate      = new Money(999.99m, "USD"),
                      VehicleType    = VehicleType.Yacht
                  },
                  "pack://application:,,,/LuxuryCarRental;component/Images/Vehicles/yacht1.jpg"
                ),
                ("35m Yacht with 5 cabins",
                  () => new Yacht {
                      LengthInMeters = 35m,
                      CabinCount     = 5,
                      DailyRate      = new Money(1499.99m, "USD"),
                      VehicleType    = VehicleType.Yacht
                  },
                  "pack://application:,,,/LuxuryCarRental;component/Images/Vehicles/yacht1.jpg"
                ),
                ("2023 Mercedes-Benz S-Class",
                  () => new LuxuryCar {
                      Make        = "Mercedes-Benz",
                      Model       = "S-Class",
                      Year        = 2023,
                      DailyRate   = new Money(199.99m, "USD"),
                      VehicleType = VehicleType.LuxuryCar,
                      SecurityDeposit    = 500m,            // ← required
                      IncludesChauffeur  = false,           // ← required
                      OptionalFeatures   = "Leather seats"  // ← required
                  },
                  "pack://application:,,,/LuxuryCarRental;component/Images/Vehicles/luxurycar1.jpg"
                ),
                ("2024 Rolls-Royce Phantom",
                  () => new LuxuryCar {
                      Make        = "Rolls-Royce",
                      Model       = "Phantom",
                      Year        = 2024,
                      DailyRate   = new Money(299.99m, "USD"),
                      VehicleType = VehicleType.LuxuryCar,
                      SecurityDeposit    = 1000m,
                      IncludesChauffeur  = true,
                      OptionalFeatures   = "Champagne bar; Wi-Fi"
                  },
                  "pack://application:,,,/LuxuryCarRental;component/Images/Vehicles/luxurycar1.jpg"
                )
            };

            // 1a) Purge stale or empty rows
            var desiredKeys = entries.Select(e => e.Key).ToHashSet();
            var toDelete = context.Vehicles.Where(v => !desiredKeys.Contains(v.Name)).ToList();
            if (toDelete.Any())
            {
                context.Vehicles.RemoveRange(toDelete);
                context.SaveChanges();
            }

            // 2) Upsert each entry
            foreach (var (key, factory, imageUri) in entries)
            {
                var existing = context.Vehicles.FirstOrDefault(v => v.Name == key);
                if (existing is null)
                {
                    var v = factory();
                    v.ImagePath = imageUri;
                    context.Vehicles.Add(v);
                }
                else if (existing.ImagePath != imageUri)
                {
                    existing.ImagePath = imageUri;
                    context.Vehicles.Update(existing);
                }
            }

            // 3) Commit
            context.SaveChanges();
        }
    }
}
