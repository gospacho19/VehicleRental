
using LuxuryCarRental.Models;
using System.Collections.Generic;

namespace LuxuryCarRental.Messaging
{
    public record CheckoutDraft(
        IList<CartItem> Items,
        DateRange Period,
        Money Total,
        Card CardToCharge);
}