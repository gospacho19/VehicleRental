using System;
namespace LuxuryCarRental.Messaging
{
    public class GoToVehicleDetailMessage
    {
        public int VehicleId { get; }

        public GoToVehicleDetailMessage(int vehicleId)
        {
            VehicleId = vehicleId;
        }
    }
}
