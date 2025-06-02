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
        public string Key { get; set; } = ""; // Name
        public string Make { get; set; } = ""; 
        public string Model { get; set; } = "";
        public int? Year { get; set; }       
        public decimal? DailyRate { get; set; }
        public int? EngineCapacityCc { get; set; }    
        public bool? HasSidecar { get; set; }
        public decimal? LengthInMeters { get; set; }    
        public int? CabinCount { get; set; }
        public decimal? SecurityDeposit { get; set; }   
        public bool? IncludesChauffeur { get; set; }
        public string? OptionalFeatures { get; set; }
        public string ImageUri { get; set; } = "";
    }
}
