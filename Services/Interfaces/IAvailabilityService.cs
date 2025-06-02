using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuxuryCarRental.Models;

namespace LuxuryCarRental.Services.Interfaces
{
    public interface IAvailabilityService
    {
        bool IsAvailable(int vehicleId, DateRange period, int? ignoreCustomerId = null);

        // returns all vehicle IDs that are booked or in a cart
        Task<HashSet<int>> GetBlockedVehicleIdsAsync(DateRange period, int? ignoreCustomerId = null);
    }
}
