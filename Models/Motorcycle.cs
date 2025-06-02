using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Models
{
    public class Motorcycle : Vehicle
    {
        public Motorcycle() { }

        public required int EngineCapacityCc { get; init; }
        public required bool HasSidecar { get; init; }

        public override string Name
            => $"{(HasSidecar ? "Sidecar " : "")}{VehicleType} {EngineCapacityCc}cc";
    }
}

