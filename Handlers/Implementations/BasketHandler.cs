using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Linq;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;
using LuxuryCarRental.Handlers.Interfaces;

namespace LuxuryCarRental.Managers.Implementations
{
    public class BasketHandler : IBasketHandler
    {
        private readonly ICartService _cart;
        private readonly IRentalHandler _rental;

        public BasketHandler(ICartService cart, IRentalHandler rental)
        {
            _cart = cart;
            _rental = rental;
        }

        public IEnumerable<Rental> Checkout(int customerId, string paymentToken)
        {
            var items = _cart.GetCartItems(customerId).ToList();
            var rentals = new List<Rental>();

            foreach (var item in items)
            {
                var period = new DateRange(item.StartDate, item.EndDate);
                rentals.Add(
                    _rental.BookVehicle(
                     customerId,
                     item.VehicleId,
                     period,
                     Array.Empty<string>()  
                 )
             );
            }

            // clear cart
            _cart.ClearCart(customerId);
            return rentals;
        }
    }
}

