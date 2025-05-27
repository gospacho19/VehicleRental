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

        public bool IsAvailable(int vehicleId, DateRange period)
        {
            // 1) Quick out if the vehicle isn’t even “Available”
            var vehicle = _ctx.Set<Vehicle>().Find(vehicleId);
            if (vehicle?.Status != VehicleStatus.Available)
                return false;

            // 2) Check for any overlapping Active rentals
            bool hasOverlap = _ctx.Rentals
                .Where(r => r.VehicleId == vehicleId
                         && r.Status == RentalStatus.Active)
                .Any(r => r.StartDate < period.End
                       && r.EndDate > period.Start);

            // 3) If there’s an overlap, it’s NOT available
            return !hasOverlap;
        }

    }
}

