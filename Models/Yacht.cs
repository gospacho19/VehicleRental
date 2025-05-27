using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Models
{
    public class Yacht : Vehicle
    {
        public Yacht() { }

        // Yacht-specific properties
        public required decimal LengthInMeters { get; init; }
        public required int CabinCount { get; init; }

        public override string Name
            => $"{LengthInMeters}m Yacht with {CabinCount} cabins";
    }
}
