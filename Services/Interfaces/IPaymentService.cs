using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuxuryCarRental.Models;

namespace LuxuryCarRental.Services.Interfaces
{
    public interface IPaymentService
    {
        // process payment for the given amount
        string Charge(Card card, Money amount);

    }
}
