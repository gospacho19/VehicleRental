using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Models
{
    public class LuxuryCar : Car
    {
        public LuxuryCar() { } 

        public required decimal SecurityDeposit { get; init; }
        public required bool IncludesChauffeur { get; init; }
        public required string OptionalFeatures { get; init; }
    }
}
