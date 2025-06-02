using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LuxuryCarRental.Models
{
    public class CartItem : ObservableObject
    {
        protected CartItem() { }

        public int Id { get; set; }

        public required int BasketId { get; init; }
        public required Basket Basket { get; init; }

        public required int VehicleId { get; init; }
        public required Vehicle Vehicle { get; init; }


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

        public required Money Subtotal { get; init; }


        // constructor that builds a CartItem 
        [SetsRequiredMembers]
        public CartItem(Basket basket, Vehicle vehicle, DateRange period, IEnumerable<string> options)
        {
            // Link to basket
            BasketId = basket.Id;
            Basket = basket;

            // Link to vehicle
            VehicleId = vehicle.Id;
            Vehicle = vehicle;

            StartDate = period.Start;
            EndDate = period.End;

            var days = period.Days;
            Subtotal = vehicle.DailyRate * days;
        }
    }
}
