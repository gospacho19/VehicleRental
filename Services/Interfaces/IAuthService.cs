using LuxuryCarRental.Models;

namespace LuxuryCarRental.Services.Interfaces
{
    public interface IAuthService
    {
        // registers a new user 
        Customer Register(string username, string password, string fullName, string driverLicenseNumber, ContactInfo contact);


        // authenticate with given username/password
        Customer? Login(string username, string password);


        // log out the given user
        void Logout(Customer customer);


        // sets or clears the “Remember Me” boolean on that user
        void SetRememberMe(Customer customer, bool remember);

        Customer? GetRememberedUser();
    }
}