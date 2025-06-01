using LuxuryCarRental.Models;

namespace LuxuryCarRental.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with a username and plaintext password.
        /// Must throw if username already exists.
        /// </summary>
        Customer Register(string username, string password, string fullName, string driverLicenseNumber, ContactInfo contact);

        /// <summary>
        /// Attempts to authenticate with given username/password.
        /// Returns the Customer if success; otherwise null.
        /// </summary>
        Customer? Login(string username, string password);

        /// <summary>
        /// Logs out the given user (clears RememberMe).
        /// </summary>
        void Logout(Customer customer);

        /// <summary>
        /// Sets or clears the “Remember Me” boolean on that user.
        /// Returns nothing.
        /// </summary>
        void SetRememberMe(Customer customer, bool remember);

        /// <summary>
        /// Finds the one Customer whose RememberMe == true (or null if none).
        /// </summary>
        Customer? GetRememberedUser();
    }
}