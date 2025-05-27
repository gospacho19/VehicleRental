using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Models
{
    public abstract class Vehicle
    {
        public int Id { get; set; }
        public virtual string Name { get; set; } = "";
        public required Money DailyRate { get; set; }
        // … any other shared props …
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
        public VehicleType VehicleType { get; set; }
    }

    public enum VehicleType
    {
        Car,
        Motorcycle,
        Yacht,
        LuxuryCar
    }
}

