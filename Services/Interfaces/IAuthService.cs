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
        /// Logs out the given user (clears RememberMeToken).
        /// </summary>
        void Logout(Customer customer);

        /// <summary>
        /// Generates or clears a “remember‐me” token for that user.
        /// If remember = true, create a new random token and save to DB.
        /// If remember = false, clear the token from DB.
        /// Returns the new token (or null if cleared).
        /// </summary>
        string? SetRememberMe(Customer customer, bool remember);

        /// <summary>
        /// Tries to find a customer by their RememberMeToken. Returns null if not found or token is invalid.
        /// </summary>
        Customer? LoginWithToken(string token);
    }
}
