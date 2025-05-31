using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LuxuryCarRental.Messaging
{
    public class GoToPaymentInfoMessage : ValueChangedMessage<bool>
    {
        public GoToPaymentInfoMessage() : base(true) { }
    }
}
