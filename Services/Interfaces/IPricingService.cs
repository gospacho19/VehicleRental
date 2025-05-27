using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuxuryCarRental.Models;
using System.Collections.Generic;

namespace LuxuryCarRental.Services.Interfaces
{
    public interface IPricingService
    {
        /// <summary>Calculate total cost for renting <paramref name="car"/> over <paramref name="period"/>, including any optional features.</summary>
        Money CalculateTotal(Vehicle vehicle, DateRange period, IEnumerable<string> options);
    }
}