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

        /// <summary>
        /// Returns all vehicle IDs that are booked (Status=Booked/Active) or in a cart
        /// (excluding any cart items for ignoreCustomerId), overlapping the given period.
        /// </summary>
        Task<HashSet<int>> GetBlockedVehicleIdsAsync(DateRange period, int? ignoreCustomerId = null);
    }
}
