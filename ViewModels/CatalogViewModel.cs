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
        // 1) Backing collection of vehicles
        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        // 2) Filtered views
        public IEnumerable<Vehicle> Cars => Vehicles.Where(v => v.VehicleType == VehicleType.Car);
        public IEnumerable<Vehicle> LuxuryCars => Vehicles.Where(v => v.VehicleType == VehicleType.LuxuryCar);
        public IEnumerable<Vehicle> Motorcycles => Vehicles.Where(v => v.VehicleType == VehicleType.Motorcycle);
        public IEnumerable<Vehicle> Yachts => Vehicles.Where(v => v.VehicleType == VehicleType.Yacht);

        // 3) Dependencies
        private readonly IUnitOfWork _uow;              // for raw fetch and commit
        private readonly IAvailabilityService _availability; // check overlaps
        private readonly IPricingService _pricing;      // if you need price logic
        private readonly ICartService _cart;            // to add to cart
        private readonly IMessenger _messenger;         // for CartUpdatedMessage, navigation
        private readonly UserSessionService _session;   // to get CurrentCustomer

        // 4) Commands
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

            // Use an AsyncRelayCommand so UI stays responsive
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            AddToCartCommand = new RelayCommand<Vehicle?>(OnAddToCart);

            // Whenever the cart changes for this user, re‐refresh the catalog
            _messenger.Register<CartUpdatedMessage>(this);

            // Initial load
            RefreshCommand.Execute(null);
        }

        private async Task RefreshAsync()
        {
            Vehicles.Clear();

            // 1) Build today’s date range
            var day = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));

            // 2) Fetch all vehicles that are “Available” in the DB
            //    (synchronously retrieving here; if you prefer async: await _uow.Vehicles.GetAll().ToListAsync())
            var allAvailable = _uow.Vehicles
                                   .GetAll()
                                   .Where(v => v.Status == VehicleStatus.Available)
                                   .ToList();

            // 3) Batch‐fetch all “blocked” IDs (overlapping rentals or other users’ carts)
            var ignoreId = _session.CurrentCustomer?.Id;
            var blockedIds = await _availability.GetBlockedVehicleIdsAsync(day, ignoreId);

            // 4) Add only those vehicles whose ID is not in blockedIds
            foreach (var v in allAvailable)
            {
                bool isFreeNow = !blockedIds.Contains(v.Id);
                v.CurrentlyAvailable = isFreeNow;
                if (isFreeNow)
                {
                    Vehicles.Add(v);
                }
            }

            // 5) Notify the UI that the filtered collections changed
            OnPropertyChanged(nameof(Cars));
            OnPropertyChanged(nameof(LuxuryCars));
            OnPropertyChanged(nameof(Motorcycles));
            OnPropertyChanged(nameof(Yachts));
        }

        /// <summary>
        /// Adds the selected vehicle to the current user's cart, marks it rented,
        /// commits the change, and then removes from the local list for instant feedback.
        /// </summary>
        private void OnAddToCart(Vehicle? vehicle)
        {
            if (vehicle is null)
                return;

            // 1) Grab current user from the shared session
            var current = _session.CurrentCustomer;
            if (current == null)
            {
                // If nobody is logged in, send the user to the Login screen:
                _messenger.Send(new GoToLoginMessage());
                return;
            }

            // 2) Now that we know "current" is non-null, safely add to cart:
            var period = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));
            _cart.AddToCart(current.Id, vehicle, period, Array.Empty<string>());

            // 3) Mark the vehicle as Rented
            vehicle.Status = VehicleStatus.Rented;
            _uow.Commit();

            // 4) Remove it from the local collection:
            Vehicles.Remove(vehicle);
            OnPropertyChanged(nameof(Cars));
            OnPropertyChanged(nameof(LuxuryCars));
            OnPropertyChanged(nameof(Motorcycles));
            OnPropertyChanged(nameof(Yachts));

            // 5) Broadcast to anyone listening that this user's cart changed
            _messenger.Send(new CartUpdatedMessage(current.Id));
        }

        // IMessageHandler for CartUpdatedMessage
        public void Receive(CartUpdatedMessage message)
        {
            var current = _session.CurrentCustomer;
            if (current is not null && message.CustomerId == current.Id)
            {
                // Fire the async refresh again
                _ = RefreshAsync();
            }
        }

        public void Dispose()
        {
            _messenger.UnregisterAll(this);
        }
    }
}
