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
            // if available
            var vehicle = _ctx.Vehicles.Find(vehicleId);
            if (vehicle is null || vehicle.Status == VehicleStatus.Maintenance)
                return false;

            bool rentalClash = _ctx.Rentals.Any(r =>
                r.VehicleId == vehicleId &&
                (r.Status == RentalStatus.Booked || r.Status == RentalStatus.Active) &&
                r.StartDate < period.End &&
                period.Start < r.EndDate);

            if (rentalClash)
                return false;

            bool cartClash = _ctx.CartItems.Any(ci =>
                ci.VehicleId == vehicleId &&
                (ignoreCustomerId == null || ci.Basket.CustomerId != ignoreCustomerId.Value) &&
                ci.StartDate < period.End &&
                period.Start < ci.EndDate);

            return !cartClash;
        }

        public async Task<HashSet<int>> GetBlockedVehicleIdsAsync(DateRange period, int? ignoreCustomerId = null)
        {

            var rentalQuery = _ctx.Rentals
                .Where(r =>
                    (r.Status == RentalStatus.Booked || r.Status == RentalStatus.Active) &&
                    r.StartDate < period.End &&
                    period.Start < r.EndDate)
                .Select(r => r.VehicleId);

            var cartQuery = _ctx.CartItems
                .Where(ci =>
                    ci.StartDate < period.End &&
                    period.Start < ci.EndDate &&
                    (ignoreCustomerId == null || ci.Basket.CustomerId != ignoreCustomerId.Value))
                .Select(ci => ci.VehicleId);

            var rentalIdsTask = rentalQuery.ToListAsync();  
            var cartIdsTask = cartQuery.ToListAsync();    

            await Task.WhenAll(rentalIdsTask, cartIdsTask);

            var blocked = new HashSet<int>(rentalIdsTask.Result);
            foreach (var id in cartIdsTask.Result)
                blocked.Add(id);

            return blocked;
        }
    }
}
