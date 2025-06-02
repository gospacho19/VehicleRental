using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics.CodeAnalysis;

namespace LuxuryCarRental.Models
{
    public class Basket
    {
        public Basket() { }

        public int Id { get; set; }

        public int CustomerId { get; init; }
        public required Customer Customer { get; init; }


        [SetsRequiredMembers]   
        public Basket(int customerId, Customer customer)
        {
            Customer = customer ?? throw new ArgumentNullException(nameof(customer));
            CustomerId = customerId;
        }

        public ICollection<CartItem> Items { get; init; } = new List<CartItem>();


    }
}
