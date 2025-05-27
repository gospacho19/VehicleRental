using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Models/Basket.cs
using System.Diagnostics.CodeAnalysis;

namespace LuxuryCarRental.Models
{
    public class Basket
    {
        protected Basket() { }

        public int Id { get; set; }

        public required int CustomerId { get; init; }
        public required Customer Customer { get; init; }

        public ICollection<CartItem> Items { get; init; } = new List<CartItem>();

        [SetsRequiredMembers]   // tells the compiler “this ctor fulfills the required props”
        public Basket(int customerId, Customer customer)
        {
            CustomerId = customerId;
            Customer = customer;
            // Items is already initialized above
        }
    }
}
