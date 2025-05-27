using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LuxuryCarRental.Models;
using LuxuryCarRental.Repositories.Interfaces;
using LuxuryCarRental.Services.Interfaces;

namespace LuxuryCarRental.ViewModels
{
    public class CatalogViewModel : ObservableObject
    {

        public ObservableCollection<Vehicle> Vehicles { get; } = new();

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
            // example uses a 1-day default; you can replace with real date picker
            var defaultPeriod = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));

            foreach (var vehicle in _uow.Vehicles.GetAll())
            {
                if (_availability.IsAvailable(vehicle.Id, defaultPeriod))
                    Vehicles.Add(vehicle);
            }
        }

        private void OnAddToCart(Vehicle? vehicle)
        {
            if (vehicle is null) return;

            var period = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));
            _cart.AddToCart(1, vehicle, period, Array.Empty<string>());
        }
    }

}