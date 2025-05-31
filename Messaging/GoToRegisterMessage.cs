using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LuxuryCarRental.Messaging
{
    public class GoToRegisterMessage : ValueChangedMessage<bool>
    {
        public GoToRegisterMessage() : base(true) { }
    }
}