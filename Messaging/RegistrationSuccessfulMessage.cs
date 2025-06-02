using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LuxuryCarRental.Messaging
{
    public class RegistrationSuccessfulMessage : ValueChangedMessage<string>
    {
        public RegistrationSuccessfulMessage(string username)
            : base(username)
        {
        }
    }
}
