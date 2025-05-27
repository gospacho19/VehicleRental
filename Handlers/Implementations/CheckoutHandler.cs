using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuxuryCarRental.Handlers.Interfaces;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;

namespace LuxuryCarRental.Handlers.Implementations
{
    public class CheckoutHandler : ICheckoutHandler
    {
        private readonly ICartService _cart;
        private readonly IRentalHandler _rentalHandler;

        public CheckoutHandler(ICartService cart, IRentalHandler rentalHandler)
        {
            _cart = cart;
            _rentalHandler = rentalHandler;
        }

        public IEnumerable<Rental> Checkout(int customerId, string paymentToken)
        {
            // You may want to call a payment service here...
            var items = _cart.GetCartItems(customerId).ToList();
            var rentals = new List<Rental>();
            foreach (var item in items)
            {
                var period = new DateRange(item.StartDate, item.EndDate);
                rentals.Add(_rentalHandler.BookVehicle(customerId, item.VehicleId, period, Enumerable.Empty<string>()));
            }
            _cart.ClearCart(customerId);
            return rentals;
        }
    }
}

