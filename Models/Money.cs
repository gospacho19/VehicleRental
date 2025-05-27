using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCarRental.Models
{
    [Owned]
    public record Money(decimal Amount, string Currency)
    {
        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Currency mismatch");
            return new(a.Amount + b.Amount, a.Currency);
        }
        public static Money operator -(Money a, Money b) =>
            a + new Money(-b.Amount, b.Currency);

        public static Money operator *(Money a, int count) =>
            new(a.Amount * count, a.Currency);
    }
}
