using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Models
{
    public class Card
    {
        public int CardId { get; set; }

        // Stored as masked or tokenized in production!
        public string CardNumber { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Cvv { get; set; } = string.Empty;

        // FK back to Customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
    }
}
