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
    public class CategoryViewModel : ObservableObject, IRecipient<CartUpdatedMessage>, IDisposable
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

            _messenger.Register<CartUpdatedMessage>(this);

            // Populate filter dropdowns
            Categories.Add("All");
            foreach (var vt in Enum.GetValues<VehicleType>().Cast<VehicleType>())
                Categories.Add(vt.ToString());
            SelectedCategory = "All";

            AvailabilityOptions.Add("All");
            AvailabilityOptions.Add("Available");
            AvailabilityOptions.Add("Not Available");
            SelectedAvailability = "Available";

            SortOptions.Add("None");
            SortOptions.Add("Name (A → Z)");
            SortOptions.Add("Name (Z → A)");
            SortOptions.Add("Price (Low → High)");
            SortOptions.Add("Price (High → Low)");
            SelectedSortOption = "None";

            // Commands
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            RentNowCommand = new RelayCommand<Vehicle?>(OnRentNow, CanRentNow);

            // Initial load
            RefreshCommand.Execute(null);
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
                    _ = RefreshAsync();
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
                    _ = RefreshAsync();
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
                    _ = RefreshAsync();
            }
        }

        // (D) The filtered + sorted list of vehicles shown in the DataGrid:
        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        // (E) “Refresh” button:
        public IAsyncRelayCommand RefreshCommand { get; }

        // (F) “Rent Now” button (one per row) takes a Vehicle as parameter:
        public IRelayCommand<Vehicle?> RentNowCommand { get; }

        // ────────── PRIVATE BACKING LIST ──────────
        // We always fetch all vehicles into this list, then filter/sort it for `Vehicles`.
        private List<Vehicle> _allVehicles = new();

        // ────────── REFRESH LOGIC ──────────
        private async Task RefreshAsync()
        {
            // 1) Fetch all vehicles from the database
            _allVehicles = _uow.Vehicles.GetAll().ToList();

            // 2) Determine “todayRange” once
            var todayRange = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));

            // 3) Batch‐fetch blocked IDs (overlapping rentals or other users’ carts)
            int? ignoreId = _session.CurrentCustomer?.Id;
            var blockedIds = await _availability.GetBlockedVehicleIdsAsync(todayRange, ignoreId);

            // 4) In‐memory filter + sort
            IEnumerable<Vehicle> temp = _allVehicles;

            // Filter by VehicleType
            if (!string.Equals(SelectedCategory, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<VehicleType>(SelectedCategory, out var vt))
                    temp = temp.Where(v => v.VehicleType == vt);
            }

            // Compute CurrentlyAvailable for each vehicle
            foreach (var v in temp)
            {
                bool freeNow = (v.Status == VehicleStatus.Available) && !blockedIds.Contains(v.Id);
                v.CurrentlyAvailable = freeNow;
            }

            // Filter by Availability
            if (string.Equals(SelectedAvailability, "Available", StringComparison.OrdinalIgnoreCase))
            {
                temp = temp.Where(v => v.CurrentlyAvailable);
            }
            else if (string.Equals(SelectedAvailability, "Not Available", StringComparison.OrdinalIgnoreCase))
            {
                temp = temp.Where(v => !v.CurrentlyAvailable);
            }
            // else “All” → no additional filter

            // Sort
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

            // Force DataGrid to rebind
            OnPropertyChanged(nameof(Vehicles));
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
            _ = RefreshAsync();
        }

        // IMessageHandler for CartUpdatedMessage
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
