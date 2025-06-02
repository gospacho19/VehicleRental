using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LuxuryCarRental.Data;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCarRental.Services.Implementations
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly AppDbContext _ctx;
        public AvailabilityService(AppDbContext ctx) => _ctx = ctx;

        public bool IsAvailable(int vehicleId,
                                DateRange period,
                                int? ignoreCustomerId = null)
        {
            // 0) Vehicle must not be out of service / under repair
            var vehicle = _ctx.Vehicles.Find(vehicleId);
            if (vehicle is null || vehicle.Status == VehicleStatus.Maintenance)
                return false;

            // 1) any overlap with confirmed (=Booked/Active) rentals?
            bool rentalClash = _ctx.Rentals.Any(r =>
                r.VehicleId == vehicleId &&
                (r.Status == RentalStatus.Booked || r.Status == RentalStatus.Active) &&
                r.StartDate < period.End &&
                period.Start < r.EndDate);

            if (rentalClash)
                return false;

            // 2) any overlap with *other people’s* cart items?
            //    We must refer to ci.Basket.CustomerId because CartItem itself doesn’t expose CustomerId directly.
            bool cartClash = _ctx.CartItems.Any(ci =>
                ci.VehicleId == vehicleId &&
                (ignoreCustomerId == null || ci.Basket.CustomerId != ignoreCustomerId.Value) &&
                ci.StartDate < period.End &&
                period.Start < ci.EndDate);

            return !cartClash;
        }

        public async Task<HashSet<int>> GetBlockedVehicleIdsAsync(DateRange period, int? ignoreCustomerId = null)
        {
            // 1) All rentals (Booked or Active) that overlap ‘period’:
            //    .Select(r => r.VehicleId) produces IEnumerable<int>
            var rentalQuery = _ctx.Rentals
                .Where(r =>
                    (r.Status == RentalStatus.Booked || r.Status == RentalStatus.Active) &&
                    r.StartDate < period.End &&
                    period.Start < r.EndDate)
                .Select(r => r.VehicleId);

            // 2) All cart items (other customers) that overlap ‘period’:
            //    Notice the use of ci.Basket.CustomerId → this is how we get the FK to Customer.
            //    .Select(ci => ci.VehicleId) again yields IEnumerable<int>
            var cartQuery = _ctx.CartItems
                .Where(ci =>
                    ci.StartDate < period.End &&
                    period.Start < ci.EndDate &&
                    (ignoreCustomerId == null || ci.Basket.CustomerId != ignoreCustomerId.Value))
                .Select(ci => ci.VehicleId);

            // Run these two queries in parallel:
            var rentalIdsTask = rentalQuery.ToListAsync();   // List<int>
            var cartIdsTask = cartQuery.ToListAsync();     // List<int>

            await Task.WhenAll(rentalIdsTask, cartIdsTask);

            // Combine them into a single HashSet<int>
            var blocked = new HashSet<int>(rentalIdsTask.Result);
            foreach (var id in cartIdsTask.Result)
                blocked.Add(id);

            return blocked;
        }
    }
}
