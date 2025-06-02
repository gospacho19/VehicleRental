using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuxuryCarRental.Data;
using LuxuryCarRental.Handlers.Interfaces;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCarRental.Handlers.Implementations
{
    public class RentalHandler : IRentalHandler
    {
        private readonly AppDbContext _ctx;
        private readonly IPricingService _pricing;
        private readonly IAvailabilityService _availability;

        public RentalHandler(
            AppDbContext ctx,
            IPricingService pricing,
            IAvailabilityService availability)
        {
            _ctx = ctx;
            _pricing = pricing;
            _availability = availability;
        }

        public Rental BookVehicle(int customerId,
                                  int vehicleId,               
                                  DateRange period,
                                  IEnumerable<string> options)
        {
            // availability guard
            if (!_availability.IsAvailable(vehicleId, period, customerId))
                throw new InvalidOperationException("Car not available.");

            var customer = _ctx.Customers.Find(customerId)
                           ?? throw new InvalidOperationException("Customer not found.");

            var vehicle = _ctx.Vehicles.Find(vehicleId)
                           ?? throw new InvalidOperationException("Vehicle not found.");

            var cost = _pricing.CalculateTotal(vehicle, period, options);
            var rental = new Rental(customer, vehicle, period, cost);

            vehicle.Status = VehicleStatus.Rented;
            rental.Status = RentalStatus.Active;

            _ctx.Rentals.Add(rental);
            _ctx.SaveChanges();

            return rental;
        }


        public void StartRental(int rentalId)
        {
            var r = _ctx.Rentals.Find(rentalId) ?? throw new ArgumentException("Not found");
            r.Status = RentalStatus.Active;
            _ctx.SaveChanges();
        }

        public void CompleteRental(int rentalId)
        {
            var r = _ctx.Rentals.Find(rentalId) ?? throw new ArgumentException("Not found");
            r.Status = RentalStatus.Completed;
            _ctx.SaveChanges();
        }

        public void CancelRental(int rentalId)
        {
            var r = _ctx.Rentals.Find(rentalId) ?? throw new ArgumentException("Not found");
            r.Status = RentalStatus.Cancelled;
            r.Vehicle.Status = VehicleStatus.Available;
            _ctx.SaveChanges();
        }

        public IEnumerable<Rental> GetAllDeals()
            => _ctx.Rentals
                   .Include(r => r.Vehicle)
                   .Include(r => r.Customer)
                   .ToList();
    }
}
