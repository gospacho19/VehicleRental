using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Models
{
    public class Car : Vehicle
    {
        // EF needs this for materialization
        protected Car() { }

        // Car-specific
        public required string Make { get; init; }
        public required string Model { get; init; }
        public required int Year { get; init; }

        // Build a human-friendly Name from Make/Model/Year
        public override string Name
        {
            get => $"{Year} {Make} {Model}";
            set => base.Name = value;  // if you ever need to set it
        }
    }
}