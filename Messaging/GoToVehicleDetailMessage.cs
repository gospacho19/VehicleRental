using System;
namespace LuxuryCarRental.Messaging
{
    /// <summary>
    /// Sent when the user clicks “Learn More” on a Vehicle in CategoryView.
    /// Carries the Vehicle’s ID so the detail view can load that specific record.
    /// </summary>
    public class GoToVehicleDetailMessage
    {
        public int VehicleId { get; }

        public GoToVehicleDetailMessage(int vehicleId)
        {
            VehicleId = vehicleId;
        }
    }
}
