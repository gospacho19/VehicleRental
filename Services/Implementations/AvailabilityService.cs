using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuxuryCarRental.Data;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;

namespace LuxuryCarRental.Services.Implementations
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly AppDbContext _ctx;
        public AvailabilityService(AppDbContext ctx) => _ctx = ctx;

        // Services/Implementations/AvailabilityService.cs
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
                (r.Status == RentalStatus.Booked || r.Status == RentalStatus.Active) &&   // ← plain OR
                r.StartDate < period.End &&
                period.Start < r.EndDate);

            if (rentalClash) return false;

            // 2) any overlap with *other people’s* cart items?
            bool cartClash = _ctx.CartItems.Any(ci =>
                ci.VehicleId == vehicleId &&
                (ignoreCustomerId == null || ci.Basket.CustomerId != ignoreCustomerId) &&
                ci.StartDate < period.End &&
                period.Start < ci.EndDate);

            return !cartClash;
        }


    }
}

