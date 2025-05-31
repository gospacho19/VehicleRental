using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LuxuryCarRental.Models
{
    /// <summary>
    /// Make CartItem inherit ObservableObject so we can raise PropertyChanged.
    /// </summary>
    public class CartItem : ObservableObject
    {
        protected CartItem() { }

        public int Id { get; set; }

        public required int BasketId { get; init; }
        public required Basket Basket { get; init; }

        public required int VehicleId { get; init; }
        public required Vehicle Vehicle { get; init; }

        //
        // Change StartDate/EndDate to use private backing fields + SetProperty,
        // so that setting them raises a PropertyChanged event.
        //

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        // Subtotal was `init;` only, which is fine, since you never need to reassign it.
        public required Money Subtotal { get; init; }

        /// <summary>
        /// Public constructor that builds a CartItem from basket, vehicle, and period.
        /// </summary>
        [SetsRequiredMembers]
        public CartItem(Basket basket, Vehicle vehicle, DateRange period, IEnumerable<string> options)
        {
            // 1) Link to basket
            BasketId = basket.Id;
            Basket = basket;

            // 2) Link to vehicle
            VehicleId = vehicle.Id;
            Vehicle = vehicle;

            // 3) Set both dates via the backing fields so that the first assignment also raises a change notification
            StartDate = period.Start;
            EndDate = period.End;

            // 4) Compute subtotal at creation (we assume daily‐rate × number of days)
            var days = period.Days;
            Subtotal = vehicle.DailyRate * days;
        }
    }
}
