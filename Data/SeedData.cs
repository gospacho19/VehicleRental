using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using LuxuryCarRental.Models;
using System.IO;
using System.Text.Json;

namespace LuxuryCarRental.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();


            // -- Load the JSON file --
            var jsonPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", "vehicles.json"
            );
            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("Missing seed file", jsonPath);

            var json = File.ReadAllText(jsonPath);
            var seeds = JsonSerializer.Deserialize<List<DTO>>(json)
                           ?? new List<DTO>();

            // -- Purge any vehicles not in the JSON list --
            var desiredKeys = seeds.Select(s => s.Key).ToHashSet();
            var toDelete = context.Vehicles
                                     .Where(v => !desiredKeys.Contains(v.Name))
                                     .ToList();
            if (toDelete.Any())
            {
                context.Vehicles.RemoveRange(toDelete);
                context.SaveChanges();
            }

            // -- Upsert each seed entry --
            foreach (var s in seeds)
            {
                var existing = context.Vehicles
                                      .FirstOrDefault(v => v.Name == s.Key);

                // Build the proper Vehicle subclass
                Vehicle build() => s.Type switch
                {
                    "Car" => new Car
                    {
                        Make = s.Make!,
                        Model = s.Model!,
                        Year = s.Year!.Value,
                        DailyRate = new Money(s.DailyRate!.Value, "USD"),
                        VehicleType = VehicleType.Car
                    },
                    "Motorcycle" => new Motorcycle
                    {
                        EngineCapacityCc = s.EngineCapacityCc!.Value,
                        HasSidecar = s.HasSidecar!.Value,
                        DailyRate = new Money(s.DailyRate!.Value, "USD"),
                        VehicleType = VehicleType.Motorcycle
                    },
                    "Yacht" => new Yacht
                    {
                        LengthInMeters = s.LengthInMeters!.Value,
                        CabinCount = s.CabinCount!.Value,
                        DailyRate = new Money(s.DailyRate!.Value, "USD"),
                        VehicleType = VehicleType.Yacht
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
                    },
                    _ => throw new InvalidOperationException($"Unknown type {s.Type}")
                };

                if (existing == null)
                {
                    var v = build();
                    v.ImagePath = s.ImageUri;
                    context.Vehicles.Add(v);
                }
                else if (existing.ImagePath != s.ImageUri)
                {
                    existing.ImagePath = s.ImageUri;
                    context.Vehicles.Update(existing);
                }
            }

            context.SaveChanges();
        }
    }
}
