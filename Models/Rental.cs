using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;


namespace LuxuryCarRental.Models
{
    public class Rental
    {
        protected Rental() { } 

        public int Id { get; set; }
        public required int CustomerId { get; init; }
        public required Customer Customer { get; init; }
        public required int VehicleId { get; init; }
        public required Vehicle Vehicle { get; init; }
        public required DateTime StartDate { get; init; }
        public required DateTime EndDate { get; init; }
        public required Money TotalCost { get; init; }
        public RentalStatus Status { get; set; } = RentalStatus.Booked;

        [SetsRequiredMembers]
        public Rental(Customer customer, Vehicle vehicle, DateRange period, Money totalCost)
        {
            Customer = customer;
            CustomerId = customer.Id;
            Vehicle = vehicle;
            VehicleId = vehicle.Id;
            StartDate = period.Start;
            EndDate = period.End;
            TotalCost = totalCost;
            Status = RentalStatus.Booked;
        }

    }
}