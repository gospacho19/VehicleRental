using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LuxuryCarRental.Models
{
    public class Customer : ObservableObject
    {
        public Customer() { }

        public int Id { get; set; }

        public required string Username { get; init; }
        public required string PasswordHash { get; set; }
        public required string PasswordSalt { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string DriverLicenseNumber { get; set; } = string.Empty;
        public ContactInfo Contact { get; set; } = new ContactInfo();
        public bool IsBlacklisted { get; set; }

        // “Remember Me” bool
        public bool RememberMe { get; set; } = false;

        // nav
        public ICollection<Rental> Rentals { get; init; } = new List<Rental>();
        public ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}
