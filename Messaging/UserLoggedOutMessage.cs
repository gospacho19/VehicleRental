
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LuxuryCarRental.Messaging
{
    /// <summary>
    /// Sent when the user clicks “Log Out” in ProfileView.
    /// </summary>
    public class UserLoggedOutMessage : ValueChangedMessage<bool>
    {
        public UserLoggedOutMessage() : base(true) { }
    }
}
