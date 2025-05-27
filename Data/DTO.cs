using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Data
{
    public class DTO
    {
        public string Type { get; set; } = "";
        public string Key { get; set; } = ""; // your Name
        public string Make { get; set; } = ""; // for Car & LuxuryCar
        public string Model { get; set; } = "";
        public int? Year { get; set; }        // for Car & LuxuryCar
        public decimal? DailyRate { get; set; }
        public int? EngineCapacityCc { get; set; }       // for Motorcycle
        public bool? HasSidecar { get; set; }
        public decimal? LengthInMeters { get; set; }       // for Yacht
        public int? CabinCount { get; set; }
        public decimal? SecurityDeposit { get; set; }   // for LuxuryCar
        public bool? IncludesChauffeur { get; set; }
        public string? OptionalFeatures { get; set; }
        public string ImageUri { get; set; } = "";
    }
}
