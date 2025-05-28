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
        public Basket() { }

        public int Id { get; set; }

        public int CustomerId { get; init; }
        public required Customer Customer { get; init; }


        [SetsRequiredMembers]   // tells the compiler “this ctor fulfills the required props”
        public Basket(int customerId, Customer customer)
        {
            Customer = customer ?? throw new ArgumentNullException(nameof(customer));
            CustomerId = customerId;
        }

        // for EF tooling

        public ICollection<CartItem> Items { get; init; } = new List<CartItem>();


    }
}
