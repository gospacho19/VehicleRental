using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuxuryCarRental.Models;

namespace LuxuryCarRental.Services.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>Process payment for the given amount. Returns transaction ID or throws on failure.</summary>
        /// <summary>
        /// Charge the given card for the amount, returns a transaction ID.
        /// </summary>
        string Charge(Card card, Money amount);

    }
}
