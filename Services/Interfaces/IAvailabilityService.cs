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
        /// <summary>
        /// Returns true when <paramref name="vehicleId"/> is free.
        /// When <paramref name="ignoreCustomerId"/> is supplied, any
        /// CartItems that belong to that customer are ignored.
        /// </summary>
        bool IsAvailable(int vehicleId,
                         DateRange period,
                         int? ignoreCustomerId = null);
    }
}
