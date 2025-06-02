using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;


namespace LuxuryCarRental.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        public string Charge(Card card, Money amount)
        {
            return Guid.NewGuid().ToString();
        }
    }
}
