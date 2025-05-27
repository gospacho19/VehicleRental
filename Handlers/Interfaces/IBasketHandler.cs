using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuxuryCarRental.Models;

namespace LuxuryCarRental.Handlers.Interfaces
{
    public interface IBasketHandler
    {
        IEnumerable<Rental> Checkout(int customerId, string paymentToken);
    }
}

