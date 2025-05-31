using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Models
{
    public class Card
    {
        [Key]
        public int Id { get; set; }

        // Link back to Customer
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = default!;

        // You might normally use a token instead of raw numbers
        [Required, MaxLength(16)]
        public string CardNumber { get; set; } = string.Empty;

        [Range(1, 12)]
        public int ExpiryMonth { get; set; }

        [Range(2000, 2100)]
        public int ExpiryYear { get; set; }

        public string FormattedExpiry
        {
            get
            {
                // e.g. “03/25”
                return $"{ExpiryMonth:D2}/{ExpiryYear % 100:D2}";
            }
        }

        [Required, MaxLength(4)]
        public string Cvv { get; set; } = string.Empty;

        // A friendly description, e.g. “Visa ending 4242”
        [MaxLength(50)]
        public string? Nickname { get; set; }
    }
}
