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

        // turn all items in the customer's cart into Rentals
        IEnumerable<Rental> Checkout(int customerId, DateRange period, string paymentToken);
    }
}
