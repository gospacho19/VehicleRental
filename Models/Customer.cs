using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Models
{
    public class Customer
    {
        protected Customer() { } 

        public int Id { get; set; }
        public required string FullName { get; init; }
        public required string DriverLicenseNumber { get; init; }
        public required ContactInfo Contact { get; init; }
        public bool IsBlacklisted { get; set; }

        // nav
        public ICollection<Rental> Rentals { get; init; } = new List<Rental>();

        public ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}