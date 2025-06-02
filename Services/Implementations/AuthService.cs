
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
            if (_ctx.Customers.Any(c => c.Username == username))
                throw new InvalidOperationException("Username already exists");

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
                RememberMe = false   // initially false
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

            var hash = HashPassword(password, customer.PasswordSalt);
            if (hash != customer.PasswordHash)
                return null;

            return customer;
        }

        public void Logout(Customer customer)
        {
            // on logout clear RememberMe 
            customer.RememberMe = false;
            _ctx.SaveChanges();
        }

        public void SetRememberMe(Customer customer, bool remember)
        {
            customer.RememberMe = remember;
            _ctx.SaveChanges();
        }

        public Customer? GetRememberedUser()
        {
            // find the user where RememberMe == true
            return _ctx.Customers
                       .Include(c => c.Cards)
                       .Include(c => c.Rentals)
                       .FirstOrDefault(c => c.RememberMe);
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
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] toHash = saltBytes.Concat(passwordBytes).ToArray();
            var hashBytes = sha256.ComputeHash(toHash);
            return Convert.ToBase64String(hashBytes);
        }

        #endregion
    }
}
