using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Linq;
using LuxuryCarRental.Data;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCarRental.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _ctx;
        private readonly IPricingService _pricing;

        public CartService(AppDbContext ctx, IPricingService pricing)
        {
            _ctx = ctx;
            _pricing = pricing;
        }

        // new – matches ICartService exactly
        public void AddToCart(int customerId, Vehicle vehicle, DateRange period, IEnumerable<string> options)
        {
            // 1) Find or create the basket
            var basket = _ctx.Baskets
                .Include(b => b.Items)
                .FirstOrDefault(b => b.CustomerId == customerId);

            if (basket == null)
            {
                // Ensure the customer exists
                var customer = _ctx.Customers.Find(customerId)
                    ?? throw new InvalidOperationException($"Customer {customerId} not found");

                // Use the ctor that fulfills the required nav
                basket = new Basket(customerId, customer);
                _ctx.Baskets.Add(basket);
                _ctx.SaveChanges();  // so basket.Id is set and EF is tracking it
            }

            // 2) Load the EF-tracked Vehicle instance by its Id
            var trackedVehicle = _ctx.Vehicles
                .FirstOrDefault(v => v.Id == vehicle.Id)
                ?? throw new InvalidOperationException($"Vehicle {vehicle.Id} not found");

            // 3) Create the CartItem with the tracked entities
            var item = new CartItem(
                basket: basket,
                vehicle: trackedVehicle,
                period: period,
                options: options
            );

            _ctx.CartItems.Add(item);

            // 4) Persist
            _ctx.SaveChanges();
        }



        public IEnumerable<CartItem> GetCartItems(int customerId)
            => _ctx.CartItems.Where(ci => ci.Basket.CustomerId == customerId).ToList();

        public Money GetCartTotal(int customerId)
        {
            var totalAmount = GetCartItems(customerId)
                .Sum(ci => ci.Subtotal.Amount);   // <-- use Subtotal.Amount (decimal)

            // 2) Wrap it into Money
            return new Money(totalAmount, "USD");
        }

        public void RemoveFromCart(int customerId, int cartItemId)
        {
            var item = _ctx.CartItems.Find(cartItemId);
            if (item != null)
            {
                _ctx.CartItems.Remove(item);
                _ctx.SaveChanges();
            }
        }

        public void ClearCart(int customerId)
        {
            var items = _ctx.CartItems.Where(ci => ci.Basket.CustomerId == customerId);
            _ctx.CartItems.RemoveRange(items);
            _ctx.SaveChanges();
        }
    }
}
