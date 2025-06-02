using System.Collections.Generic;
using System.Linq;
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

        // For each CartItem in the basket, create a Rental with the same date range 
        // Then remove that CartItem from the basket, and return the list of created rentals

        public IEnumerable<Rental> Checkout(int customerId, DateRange period, string paymentToken)
        {
           
            var items = _cart.GetCartItems(customerId).ToList();
            var createdRentals = new List<Rental>();

            foreach (var item in items)
            {
                var rental = _rentalHandler.BookVehicle(
                    customerId: customerId,
                    VehicleId: item.VehicleId,
                    period: period,
                    options: Enumerable.Empty<string>()   
                );
                createdRentals.Add(rental);
            }
            _cart.ClearCart(customerId);

            return createdRentals;
        }
    }
}
