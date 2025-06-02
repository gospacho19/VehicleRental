using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;

namespace LuxuryCarRental.Services.Implementations
{
    public class PricingService : IPricingService
    {
        public Money CalculateTotal(Vehicle vehicle, DateRange period, IEnumerable<string> options)
        {
            var days = period.Days;
            var cost = vehicle.DailyRate * period.Days;

            if (options?.Contains("Chauffeur", StringComparer.OrdinalIgnoreCase) == true)
                cost += new Money(200m * days, "USD");

            return cost;
        }
    }
}
