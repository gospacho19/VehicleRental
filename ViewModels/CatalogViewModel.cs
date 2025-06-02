using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;
using LuxuryCarRental.Repositories.Interfaces;
using LuxuryCarRental.Services.Implementations;
using LuxuryCarRental.Services.Interfaces;

namespace LuxuryCarRental.ViewModels
{
    public class CatalogViewModel : ObservableObject, IRecipient<CartUpdatedMessage>, IDisposable
    {
        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        // filtered views
        public IEnumerable<Vehicle> Cars => Vehicles.Where(v => v.VehicleType == VehicleType.Car);
        public IEnumerable<Vehicle> LuxuryCars => Vehicles.Where(v => v.VehicleType == VehicleType.LuxuryCar);
        public IEnumerable<Vehicle> Motorcycles => Vehicles.Where(v => v.VehicleType == VehicleType.Motorcycle);
        public IEnumerable<Vehicle> Yachts => Vehicles.Where(v => v.VehicleType == VehicleType.Yacht);


        private readonly IUnitOfWork _uow;             
        private readonly IAvailabilityService _availability; 
        private readonly IPricingService _pricing;      
        private readonly ICartService _cart;          
        private readonly IMessenger _messenger;      
        private readonly UserSessionService _session; 

        // commands
        public IAsyncRelayCommand RefreshCommand { get; }
        public IRelayCommand<Vehicle?> AddToCartCommand { get; }

        public CatalogViewModel(
            IUnitOfWork uow,
            IAvailabilityService availability,
            IPricingService pricing,
            ICartService cart,
            IMessenger messenger,
            UserSessionService session)
        {
            _uow = uow;
            _availability = availability;
            _pricing = pricing;
            _cart = cart;
            _messenger = messenger;
            _session = session;

            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            AddToCartCommand = new RelayCommand<Vehicle?>(OnAddToCart);

            _messenger.Register<CartUpdatedMessage>(this);

            RefreshCommand.Execute(null);
        }

        private async Task RefreshAsync()
        {
            Vehicles.Clear();

            var day = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));

            var allAvailable = _uow.Vehicles
                                   .GetAll()
                                   .Where(v => v.Status == VehicleStatus.Available)
                                   .ToList();

            var ignoreId = _session.CurrentCustomer?.Id;
            var blockedIds = await _availability.GetBlockedVehicleIdsAsync(day, ignoreId);

            foreach (var v in allAvailable)
            {
                bool isFreeNow = !blockedIds.Contains(v.Id);
                v.CurrentlyAvailable = isFreeNow;
                if (isFreeNow)
                {
                    Vehicles.Add(v);
                }
            }

            OnPropertyChanged(nameof(Cars));
            OnPropertyChanged(nameof(LuxuryCars));
            OnPropertyChanged(nameof(Motorcycles));
            OnPropertyChanged(nameof(Yachts));
        }

        // Add selected vehicle to the current user's cart
        private void OnAddToCart(Vehicle? vehicle)
        {
            if (vehicle is null)
                return;

            var current = _session.CurrentCustomer;
            if (current == null)
            {
                _messenger.Send(new GoToLoginMessage());
                return;
            }

            var period = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));
            _cart.AddToCart(current.Id, vehicle, period, Array.Empty<string>());

            vehicle.Status = VehicleStatus.Rented;
            _uow.Commit();

            Vehicles.Remove(vehicle);
            OnPropertyChanged(nameof(Cars));
            OnPropertyChanged(nameof(LuxuryCars));
            OnPropertyChanged(nameof(Motorcycles));
            OnPropertyChanged(nameof(Yachts));

            _messenger.Send(new CartUpdatedMessage(current.Id));
        }

        public void Receive(CartUpdatedMessage message)
        {
            var current = _session.CurrentCustomer;
            if (current is not null && message.CustomerId == current.Id)
            {
                _ = RefreshAsync();
            }
        }

        public void Dispose()
        {
            _messenger.UnregisterAll(this);
        }
    }
}
