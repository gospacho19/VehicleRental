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
        /// <summary>Returns true if car is free over the given date range.</summary>
        bool IsAvailable(int vehicleId, DateRange period);
    }
}
