using System;
using System.Collections.Generic;
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
    public partial class CategoryViewModel : ObservableObject
    {
        // ────────── Dependencies ──────────
        private readonly IUnitOfWork _uow;
        private readonly ICartService _cartService;
        private readonly IAvailabilityService _availability;
        private readonly IMessenger _messenger;
        private readonly UserSessionService _session;

        // ────────── Constructor ──────────
        public CategoryViewModel(
            IUnitOfWork uow,
            ICartService cartService,
            IAvailabilityService availability,
            IMessenger messenger,
            UserSessionService session)
        {
            _uow = uow;
            _cartService = cartService;
            _availability = availability;
            _messenger = messenger;
            _session = session;

            _messenger.Register<CartUpdatedMessage>(this, (_, __) => Refresh());

            Categories.Add("All");
            foreach (var vt in Enum.GetValues(typeof(VehicleType)).Cast<VehicleType>())
                Categories.Add(vt.ToString());
            SelectedCategory = "All";

            AvailabilityOptions.Add("All");
            AvailabilityOptions.Add("Available");
            AvailabilityOptions.Add("Not Available");
            SelectedAvailability = "Available"; // default

            SortOptions.Add("None");
            SortOptions.Add("Name (A → Z)");
            SortOptions.Add("Name (Z → A)");
            SortOptions.Add("Price (Low → High)");
            SortOptions.Add("Price (High → Low)");
            SelectedSortOption = "None";

            RefreshCommand = new RelayCommand(Refresh);
            RentNowCommand = new RelayCommand<Vehicle?>(OnRentNow, CanRentNow);

            Refresh();
        }
        

        // ────────── PUBLIC COLLECTIONS & PROPERTIES ──────────

        // (A) Vehicle Type dropdown:
        public ObservableCollection<string> Categories { get; } = new();
        private string _selectedCategory = string.Empty;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                    ApplyFilterSortAndRefresh();
            }
        }

        // (B) Availability filter dropdown:
        public ObservableCollection<string> AvailabilityOptions { get; } = new();
        private string _selectedAvailability = string.Empty;
        public string SelectedAvailability
        {
            get => _selectedAvailability;
            set
            {
                if (SetProperty(ref _selectedAvailability, value))
                    ApplyFilterSortAndRefresh();
            }
        }

        // (C) Sort order dropdown:
        public ObservableCollection<string> SortOptions { get; } = new();
        private string _selectedSortOption = string.Empty;
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (SetProperty(ref _selectedSortOption, value))
                    ApplyFilterSortAndRefresh();
            }
        }

        // (D) The filtered + sorted list of vehicles shown in the DataGrid:
        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        // (E) “Refresh” button:
        public IRelayCommand RefreshCommand { get; }

        // (F) “Rent Now” button (one per row) takes a Vehicle as parameter:
        public IRelayCommand<Vehicle?> RentNowCommand { get; }


        // ────────── PRIVATE BACKING LIST ──────────
        // We always fetch all vehicles into this list, then filter/sort it for `Vehicles`.
        private List<Vehicle> _allVehicles = new();


        // ────────── REFRESH LOGIC ──────────
        private void Refresh()
        {
            // 1) Fetch all vehicles from the database (including their Status, ImagePath, etc.)
            _allVehicles = _uow.Vehicles.GetAll().ToList();

            // 2) Apply the current Category + Availability + Sort filters
            ApplyFilterSortAndRefresh();
        }

        private void ApplyFilterSortAndRefresh()
        {
            IEnumerable<Vehicle> temp = _allVehicles;

            // 1) Filter by VehicleType
            if (!string.Equals(SelectedCategory, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<VehicleType>(SelectedCategory, out var vt))
                    temp = temp.Where(v => v.VehicleType == vt);
            }

            // 2) Determine “todayRange” once
            var todayRange = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));

            // 3) Filter by Availability (using status AND IAvailabilityService)
            if (string.Equals(SelectedAvailability, "Available", StringComparison.OrdinalIgnoreCase))
            {
                temp = temp.Where(v =>
                {
                    bool freeNow = (v.Status == VehicleStatus.Available)
                                   && _availability.IsAvailable(v.Id, todayRange);
                    v.CurrentlyAvailable = freeNow;
                    return freeNow;
                });
            }
            else if (string.Equals(SelectedAvailability, "Not Available", StringComparison.OrdinalIgnoreCase))
            {
                temp = temp.Where(v =>
                {
                    bool freeNow = (v.Status == VehicleStatus.Available)
                                   && _availability.IsAvailable(v.Id, todayRange);
                    v.CurrentlyAvailable = freeNow;
                    return !freeNow;
                });
            }
            else
            {
                // “All” → include everything, but still set CurrentlyAvailable flag
                temp = temp.Select(v =>
                {
                    v.CurrentlyAvailable = (v.Status == VehicleStatus.Available)
                                            && _availability.IsAvailable(v.Id, todayRange);
                    return v;
                });
            }

            // 4) Sort
            temp = SelectedSortOption switch
            {
                "Name (A → Z)" => temp.OrderBy(v => v.Name),
                "Name (Z → A)" => temp.OrderByDescending(v => v.Name),
                "Price (Low → High)" => temp.OrderBy(v => v.DailyRate.Amount),
                "Price (High → Low)" => temp.OrderByDescending(v => v.DailyRate.Amount),
                _ => temp
            };

            // 5) Push into ObservableCollection
            Vehicles.Clear();
            foreach (var v in temp)
                Vehicles.Add(v);
        }

        private bool CanRentNow(Vehicle? vehicle)
        {
            return vehicle != null && vehicle.CurrentlyAvailable;
        }

        private void OnRentNow(Vehicle? vehicle)
        {
            if (vehicle == null) return;

            var current = _session.CurrentCustomer;
            if (current == null)
            {
                _messenger.Send(new GoToLoginMessage());
                return;
            }

            var period = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));
            _cartService.AddToCart(current.Id, vehicle, period, Array.Empty<string>());

            vehicle.Status = VehicleStatus.Rented;
            _uow.Commit();

            _messenger.Send(new CartUpdatedMessage(current.Id));
            Refresh();
        }

    }
}
