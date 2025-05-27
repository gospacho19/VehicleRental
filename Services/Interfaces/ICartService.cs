using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuxuryCarRental.Models;
using System.Collections.Generic;

namespace LuxuryCarRental.Services.Interfaces
{
    public interface ICartService
    {
        void AddToCart(int customerId, Vehicle vehicle, DateRange period, IEnumerable<string> options);
        void RemoveFromCart(int customerId, int cartItemId);
        IEnumerable<CartItem> GetCartItems(int customerId);
        Money GetCartTotal(int customerId);
        void ClearCart(int customerId);
    }
}
