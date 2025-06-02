using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LuxuryCarRental.Messaging
{
    public class GoToProfileMessage : ValueChangedMessage<bool>
    {
        public GoToProfileMessage() : base(true) { }
    }
}
