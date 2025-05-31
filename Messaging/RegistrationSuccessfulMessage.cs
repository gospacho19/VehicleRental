using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LuxuryCarRental.Messaging
{
    /// <summary>
    /// Sent after a new user has successfully registered.
    /// Carries the new username as its Value.
    /// </summary>
    public class RegistrationSuccessfulMessage : ValueChangedMessage<string>
    {
        public RegistrationSuccessfulMessage(string username)
            : base(username)
        {
        }
    }
}
