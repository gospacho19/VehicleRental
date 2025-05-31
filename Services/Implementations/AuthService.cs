using LuxuryCarRental.Data;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LuxuryCarRental.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _ctx;

        public AuthService(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public Customer Register(
            string username,
            string password,
            string fullName,
            string driverLicenseNumber,
            ContactInfo contact)
        {
            // 1) Check if username is already taken
            if (_ctx.Customers.Any(c => c.Username == username))
                throw new InvalidOperationException("Username already exists");

            // 2) Generate salt + hash
            var salt = GenerateSalt();
            var hash = HashPassword(password, salt);

            var newCustomer = new Customer
            {
                Username = username,
                PasswordSalt = salt,
                PasswordHash = hash,
                FullName = fullName,
                DriverLicenseNumber = driverLicenseNumber,
                Contact = contact,
                // RememberMeToken is initially null
            };
            _ctx.Customers.Add(newCustomer);
            _ctx.SaveChanges();

            return newCustomer;
        }

        public Customer? Login(string username, string password)
        {
            var customer = _ctx.Customers
                               .Include(c => c.Cards)
                               .Include(c => c.Rentals)
                               .FirstOrDefault(c => c.Username == username);
            if (customer == null) return null;

            // Recompute hash with stored salt
            var hash = HashPassword(password, customer.PasswordSalt);
            if (hash != customer.PasswordHash)
                return null;

            return customer;
        }

        public void Logout(Customer customer)
        {
            // Just clear the remember‐me token
            customer.RememberMeToken = null;
            _ctx.SaveChanges();
        }

        public string? SetRememberMe(Customer customer, bool remember)
        {
            if (remember)
            {
                // Generate a new token (GUID)
                var token = Guid.NewGuid().ToString("N");

                // Save to DB
                customer.RememberMeToken = token;
                _ctx.SaveChanges();
                return token;
            }
            else
            {
                customer.RememberMeToken = null;
                _ctx.SaveChanges();
                return null;
            }
        }

        public Customer? LoginWithToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var customer = _ctx.Customers
                               .Include(c => c.Cards)
                               .Include(c => c.Rentals)
                               .FirstOrDefault(c => c.RememberMeToken == token);
            return customer;
        }

        #region ─── Private helpers ───────────────────────────────────

        private static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        private static string HashPassword(string password, string saltBase64)
        {
            var saltBytes = Convert.FromBase64String(saltBase64);
            using var sha256 = SHA256.Create();
            // Combine salt + password bytes
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] toHash = saltBytes.Concat(passwordBytes).ToArray();

            var hashBytes = sha256.ComputeHash(toHash);
            return Convert.ToBase64String(hashBytes);
        }

        #endregion
    }
}
