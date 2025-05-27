using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Models
{
    public class Motorcycle : Vehicle
    {
        // EF needs this for materialization
        public Motorcycle() { }

        // Motorcycle-specific properties
        public required int EngineCapacityCc { get; init; }
        public required bool HasSidecar { get; init; }

        // A friendly name override
        public override string Name
            => $"{(HasSidecar ? "Sidecar " : "")}{VehicleType} {EngineCapacityCc}cc";
    }
}

