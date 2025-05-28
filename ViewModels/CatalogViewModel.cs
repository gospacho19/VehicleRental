using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LuxuryCarRental.Models;
using LuxuryCarRental.Repositories.Interfaces;
using LuxuryCarRental.Services.Interfaces;
using System.Linq;

namespace LuxuryCarRental.ViewModels
{
    public class CatalogViewModel : ObservableObject
    {

        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        public IEnumerable<Vehicle> Cars => Vehicles.Where(v => v.VehicleType == VehicleType.Car);
        public IEnumerable<Vehicle> LuxuryCars => Vehicles.Where(v => v.VehicleType == VehicleType.LuxuryCar);
        public IEnumerable<Vehicle> Motorcycles => Vehicles.Where(v => v.VehicleType == VehicleType.Motorcycle);
        public IEnumerable<Vehicle> Yachts => Vehicles.Where(v => v.VehicleType == VehicleType.Yacht);

        private readonly IUnitOfWork _uow;           // still used for raw fetch
        private readonly IAvailabilityService _availability;
        private readonly IPricingService _pricing;
        private readonly ICartService _cart;

        public CatalogViewModel(
            IUnitOfWork uow,
            IAvailabilityService availability,
            IPricingService pricing,
            ICartService cart)
        {
            _uow = uow;
            _availability = availability;
            _pricing = pricing;
            _cart = cart;

            RefreshCommand = new RelayCommand(Refresh);
            AddToCartCommand = new RelayCommand<Vehicle?>(OnAddToCart);
            Refresh();
        }


        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand<Vehicle?> AddToCartCommand { get; }

        private void Refresh()
        {
            Vehicles.Clear();

            var day = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));

            // ONLY query the DB for AVAILABLE vehicles
            var available = _uow.Vehicles
                                .GetAll()
                                .Where(v => v.Status == VehicleStatus.Available);

            foreach (var v in available)
            {
                if (_availability.IsAvailable(v.Id, day))
                    Vehicles.Add(v);
            }

            // tell the UI all four collections have changed
            OnPropertyChanged(nameof(Cars));
            OnPropertyChanged(nameof(LuxuryCars));
            OnPropertyChanged(nameof(Motorcycles));
            OnPropertyChanged(nameof(Yachts));
        }

        private void OnAddToCart(Vehicle? vehicle)
        {
            if (vehicle is null) return;

            // 1) Add to your cart service
            var period = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));
            _cart.AddToCart(1, vehicle, period, Array.Empty<string>());

            // 2) Mark it as no longer available in the DB
            vehicle.Status = VehicleStatus.Rented;

            // DON’T call _uow.Vehicles.Update(vehicle) here—
            // EF is already tracking `vehicle`.
            _uow.Commit();    // this will flush the Status change

            // 3) Remove locally for instant feedback
            Vehicles.Remove(vehicle);
            OnPropertyChanged(nameof(Cars));
            OnPropertyChanged(nameof(LuxuryCars));
            OnPropertyChanged(nameof(Motorcycles));
            OnPropertyChanged(nameof(Yachts));
        }
    }

}