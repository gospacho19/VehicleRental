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

        /// <summary>
        /// For each CartItem in the basket, create a Rental with the EXACT same date range (period).
        /// Then remove that CartItem from the basket, and return the list of created rentals.
        /// </summary>
        public IEnumerable<Rental> Checkout(int customerId, DateRange period, string paymentToken)
        {
            // 1) Load all CartItems
           
            var items = _cart.GetCartItems(customerId).ToList();
            var createdRentals = new List<Rental>();

            // 2) For each CartItem, book the vehicle exactly for the given(period):
            foreach (var item in items)
            {
                var rental = _rentalHandler.BookVehicle(
                    customerId: customerId,
                    VehicleId: item.VehicleId,
                    period: period,
                    options: Enumerable.Empty<string>()   // or pass real optional features
                );
                createdRentals.Add(rental);
            }

            // 3) Clear the cart—i.e., remove all CartItems from the basket
            _cart.ClearCart(customerId);

            return createdRentals;
        }
    }
}
