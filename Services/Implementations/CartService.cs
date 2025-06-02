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

        public void AddToCart(int customerId, Vehicle vehicle, DateRange period, IEnumerable<string> options)
        {
            // find or create the basket
            var basket = _ctx.Baskets
                .Include(b => b.Items)
                .FirstOrDefault(b => b.CustomerId == customerId);

            if (basket == null)
            {
                // if customer exists
                var customer = _ctx.Customers.Find(customerId)
                    ?? throw new InvalidOperationException($"Customer {customerId} not found");

                basket = new Basket(customerId, customer);
                _ctx.Baskets.Add(basket);
                _ctx.SaveChanges(); 
            }

            var trackedVehicle = _ctx.Vehicles
                .FirstOrDefault(v => v.Id == vehicle.Id)
                ?? throw new InvalidOperationException($"Vehicle {vehicle.Id} not found");

            // create the CartItem
            var item = new CartItem(
                basket: basket,
                vehicle: trackedVehicle,
                period: period,
                options: options
            );

            _ctx.CartItems.Add(item);

            _ctx.SaveChanges();
        }



        public IEnumerable<CartItem> GetCartItems(int customerId)
            => _ctx.CartItems
                   .Include(ci => ci.Vehicle)  
                   .Where(ci => ci.Basket.CustomerId == customerId)
                   .ToList();

        public Money GetCartTotal(int customerId)
        {
            var totalAmount = GetCartItems(customerId)
                .Sum(ci => ci.Subtotal.Amount);  

            return new Money(totalAmount, "USD");
        }

        public void RemoveFromCart(int customerId, int cartItemId)
        {
            var item = _ctx.CartItems
                           .Include(ci => ci.Vehicle)
                           .FirstOrDefault(ci => ci.Id == cartItemId);
            if (item != null)
            {
                item.Vehicle.Status = VehicleStatus.Available;

                _ctx.CartItems.Remove(item);

                _ctx.SaveChanges();
            }
        }


        public void ClearCart(int customerId)
        {
            var items = _ctx.CartItems
                            .Include(ci => ci.Vehicle)
                            .Where(ci => ci.Basket.CustomerId == customerId)
                            .ToList();

            // mark each vehicle as Available
            foreach (var ci in items)
                ci.Vehicle.Status = VehicleStatus.Available;

            _ctx.CartItems.RemoveRange(items);

            _ctx.SaveChanges();
        }

    }
}
