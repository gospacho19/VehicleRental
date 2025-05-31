using LuxuryCarRental.Models;

namespace LuxuryCarRental.Services.Implementations
{

    public class UserSessionService
    {
        public Customer? CurrentCustomer { get; private set; }

        public void SetCurrentCustomer(Customer customer)
        {
            CurrentCustomer = customer;
        }

        public void ClearCurrentCustomer() => CurrentCustomer = null;
    }
}
