using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LuxuryCarRental.Models;

namespace LuxuryCarRental.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Always read the JSON file on startup:
            var jsonPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", "vehicles.json"
            );
            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("Missing seed file", jsonPath);

            var json = File.ReadAllText(jsonPath);
            var seeds = JsonSerializer.Deserialize<List<DTO>>(json)
                        ?? throw new InvalidOperationException("Failed to parse vehicles.json");

            foreach (var s in seeds)
            {
                // Decide how to detect “already in DB.” 
                // Here, we treat DTO.Key as a unique name for vehicle.
                bool alreadyExists = context.Vehicles
                    .Any(v => v.Name == s.Key);

                if (alreadyExists)
                    continue;

                // Otherwise, create a new entity of the right subtype:
                Vehicle v = s.Type switch
                {
                    "Car" => new Car
                    {
                        Make = s.Make!,
                        Model = s.Model!,
                        Year = s.Year!.Value,
                        DailyRate = new Money(s.DailyRate!.Value, "USD"),
                        VehicleType = VehicleType.Car
                        // Note: Car.Name will be "{Year} {Make} {Model}"
                    },
                    "Motorcycle" => new Motorcycle
                    {
                        EngineCapacityCc = s.EngineCapacityCc!.Value,
                        HasSidecar = s.HasSidecar!.Value,
                        DailyRate = new Money(s.DailyRate!.Value, "USD"),
                        VehicleType = VehicleType.Motorcycle
                        // Motorcycle.Name is computed from VehicleType + engine size
                    },
                    "Yacht" => new Yacht
                    {
                        LengthInMeters = s.LengthInMeters!.Value,
                        CabinCount = s.CabinCount!.Value,
                        DailyRate = new Money(s.DailyRate!.Value, "USD"),
                        VehicleType = VehicleType.Yacht
                        // Yacht.Name is computed from length + cabin count
                    },
                    "LuxuryCar" => new LuxuryCar
                    {
                        Make = s.Make!,
                        Model = s.Model!,
                        Year = s.Year!.Value,
                        DailyRate = new Money(s.DailyRate!.Value, "USD"),
                        VehicleType = VehicleType.LuxuryCar,
                        SecurityDeposit = s.SecurityDeposit!.Value,
                        IncludesChauffeur = s.IncludesChauffeur!.Value,
                        OptionalFeatures = s.OptionalFeatures!
                        // LuxuryCar.Name inherits Car’s Name override
                    },
                    _ => throw new InvalidOperationException($"Unknown type {s.Type}")
                };

                // Set the “Key” into the .Name property (so we can detect duplicates by Name):
                v.Name = s.Key;
                // Store the image URI/path exactly as given in JSON:
                v.ImagePath = s.ImageUri;

                // Default Status remains VehicleStatus.Available:
                context.Vehicles.Add(v);
            }

            context.SaveChanges();


            // ───────────────────────────────────────
            // 2) Seed a “demo” user if none exists
            // ───────────────────────────────────────
            var demoCustomer = context.Customers
                   .FirstOrDefault(c => c.Username == "demo");

            if (demoCustomer == null)
            {
                demoCustomer = new Customer
                {
                    Username = "demo",
                    PasswordHash = "",      // blank for now
                    PasswordSalt = "",
                    FullName = "Demo User",
                    DriverLicenseNumber = "X1234567",
                    Contact = new ContactInfo
                    {
                        Email = "demo@example.com",
                        Phone = "+1-800-555-1234"
                    }
                };
                context.Customers.Add(demoCustomer);
                context.SaveChanges(); // demoCustomer.Id is now assigned by EF
            }

            // ───────────────────────────────────────
            // 3) Ensure DemoUser has a Basket
            // ───────────────────────────────────────
            if (!context.Baskets.Any(b => b.CustomerId == demoCustomer.Id))
            {
                var basket = new Basket(demoCustomer.Id, demoCustomer);
                context.Baskets.Add(basket);
                context.SaveChanges();
            }
        }
    }
}
