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
            // TODO: Persist card (or token) if new:
            // e.g. _cardRepository.Add(card); _cardRepository.SaveChanges();

            // Integrate with real gateway here. For now, simulate:
            return Guid.NewGuid().ToString();
        }
    }


}
