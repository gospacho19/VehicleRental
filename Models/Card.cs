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

        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = default!;

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
                // like 03/25
                return $"{ExpiryMonth:D2}/{ExpiryYear % 100:D2}";
            }
        }

        [Required, MaxLength(4)]
        public string Cvv { get; set; } = string.Empty;

        // friendly description
        [MaxLength(50)]
        public string? Nickname { get; set; }
    }
}
