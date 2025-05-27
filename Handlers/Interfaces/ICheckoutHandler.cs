using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using LuxuryCarRental.Models;

namespace LuxuryCarRental.Handlers.Interfaces
{
    public interface ICheckoutHandler
    {
        /// <summary>
        /// Turns all items in the customer's cart into Rentals, processes payment, and returns the new Rental objects.
        /// </summary>
        IEnumerable<Rental> Checkout(int customerId, string paymentToken);
    }
}
