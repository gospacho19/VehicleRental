using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Models
{
    public class Car : Vehicle
    {
        public Car() { }

        public required string Make { get; init; }
        public required string Model { get; init; }
        public required int Year { get; init; }

        // human-friendly Name
        public override string Name
        {
            get => $"{Year} {Make} {Model}";
            set => base.Name = value;  
        }
    }
}