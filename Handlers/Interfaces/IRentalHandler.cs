using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuxuryCarRental.Models;

namespace LuxuryCarRental.Handlers.Interfaces
{
    public interface IRentalHandler
    {
        Rental BookVehicle(int customerId, int VehicleId, DateRange period, IEnumerable<string> options);
        void StartRental(int rentalId);
        void CompleteRental(int rentalId);
        void CancelRental(int rentalId);
        IEnumerable<Rental> GetAllDeals();
    }
}
