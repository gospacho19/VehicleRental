using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Models
{
    public class Customer
    {
        public Customer() { }

        public int Id { get; set; }

        // Remove `init;` and replace with a normal setter:
        public string FullName { get; set; } = string.Empty;
        public string DriverLicenseNumber { get; set; } = string.Empty;

        // If ContactInfo’s properties were init‐only, also change Contact.
        public ContactInfo Contact { get; set; } = new ContactInfo();

        public bool IsBlacklisted { get; set; }

        // nav
        public ICollection<Rental> Rentals { get; init; } = new List<Rental>();
        public ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}