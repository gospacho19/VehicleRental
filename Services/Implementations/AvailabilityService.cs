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
            var vehicle = _ctx.Set<Vehicle>().Find(vehicleId);
            if (vehicle?.Status != VehicleStatus.Available) return false;

            return !_ctx.Rentals
                .Where(r => r.VehicleId == vehicleId && r.Status == RentalStatus.Active)
                .Any(r => new DateRange(r.StartDate, r.EndDate).Overlaps(period));
        }
    }
}

