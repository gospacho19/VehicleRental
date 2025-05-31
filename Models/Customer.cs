using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LuxuryCarRental.Models
{
    public class Customer : ObservableObject
    {
        public Customer() { }

        public int Id { get; set; }

        // These fields are now required (init‐only), set at registration:
        public required string Username { get; init; }
        public required string PasswordHash { get; set; }
        public required string PasswordSalt { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string DriverLicenseNumber { get; set; } = string.Empty;
        public ContactInfo Contact { get; set; } = new ContactInfo();
        public bool IsBlacklisted { get; set; }

        // Optional “Remember Me” token (nullable)
        private string? _rememberMeToken;
        public string? RememberMeToken
        {
            get => _rememberMeToken;
            set => SetProperty(ref _rememberMeToken, value);
        }
        // nav
        public ICollection<Rental> Rentals { get; init; } = new List<Rental>();
        public ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}
