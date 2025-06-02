
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LuxuryCarRental.Messaging
{
    public class UserLoggedOutMessage : ValueChangedMessage<bool>
    {
        public UserLoggedOutMessage() : base(true) { }
    }
}
