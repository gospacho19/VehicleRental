using LuxuryCarRental.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LuxuryCarRental.Messaging
{
    public class LoginSuccessfulMessage : ValueChangedMessage<Customer>
    {
        public LoginSuccessfulMessage(Customer customer) : base(customer) { }
    }
}
