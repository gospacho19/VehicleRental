using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace LuxuryCarRental.Models
{
    public record DateRange(DateTime Start, DateTime End)
    {
        public int Days => (End - Start).Days;

        public bool Overlaps(DateRange other) =>
            Start < other.End && End > other.Start;
    }
}