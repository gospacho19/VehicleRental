using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class CatalogViewModel : ObservableObject
    {
        // 1) Backing collection of vehicles
        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        // 2) Filtered views
        public IEnumerable<Vehicle> Cars => Vehicles.Where(v => v.VehicleType == VehicleType.Car);
        public IEnumerable<Vehicle> LuxuryCars => Vehicles.Where(v => v.VehicleType == VehicleType.LuxuryCar);
        public IEnumerable<Vehicle> Motorcycles => Vehicles.Where(v => v.VehicleType == VehicleType.Motorcycle);
        public IEnumerable<Vehicle> Yachts => Vehicles.Where(v => v.VehicleType == VehicleType.Yacht);

        // 3) Dependencies
        private readonly IUnitOfWork _uow;           // for raw fetch and commit
        private readonly IAvailabilityService _availability;  // check overlaps
        private readonly IPricingService _pricing;       // if you need price logic
        private readonly ICartService _cart;          // to add to cart
        private readonly IMessenger _messenger;     // for CartUpdatedMessage, navigation
        private readonly UserSessionService _session;       // to get CurrentCustomer

        // 4) Commands
        public IRelayCommand RefreshCommand { get; }
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

            RefreshCommand = new RelayCommand(Refresh);
            AddToCartCommand = new RelayCommand<Vehicle?>(OnAddToCart);

            // Whenever the cart changes for this user, re‐refresh the catalog
            _messenger.Register<CartUpdatedMessage>(this, (r, msg) =>
            {
                // Only refresh if the message matches the current user
                var current = _session.CurrentCustomer;
                if (current is not null && msg.CustomerId == current.Id)
                {
                    Refresh();
                }
            });

            // Initial load
            Refresh();
        }

        /// <summary>
        /// Re‐loads the list of available vehicles (only those with Status=Available and no overlapping rentals).
        /// </summary>
        private void Refresh()
        {
            Vehicles.Clear();

            var day = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));

            // 1) Fetch all vehicles that are “Available” in the DB
            var available = _uow.Vehicles
                                .GetAll()
                                .Where(v => v.Status == VehicleStatus.Available);

            // 2) Only add to the UI if they truly have no overlapping active rentals
            foreach (var v in available)
            {
                if (_availability.IsAvailable(v.Id, day))
                {
                    Vehicles.Add(v);
                }
            }

            // 3) Notify the UI that the filtered collections changed
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

    }
}
