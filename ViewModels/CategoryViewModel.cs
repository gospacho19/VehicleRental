// LuxuryCarRental/ViewModels/CategoryViewModel.cs

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

            // Populate filter dropdowns …
            Categories.Add("All");
            foreach (var vt in Enum.GetValues(typeof(VehicleType)).Cast<VehicleType>())
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

            // *** Change here: LearnMoreCommand must be RelayCommand<Vehicle?>, not RelayCommand<Vehicle> ***
            LearnMoreCommand = new RelayCommand<Vehicle?>(OnLearnMore);

            // Initial load
            RefreshCommand.Execute(null);
        }

        // ────────── PUBLIC COLLECTIONS & PROPERTIES ──────────

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

        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        public IAsyncRelayCommand RefreshCommand { get; }
        public IRelayCommand<Vehicle?> RentNowCommand { get; }

        // *** Change here: use Vehicle? in the type argument ***
        public IRelayCommand<Vehicle?> LearnMoreCommand { get; }

        // ────────── PRIVATE BACKING LIST ──────────
        private List<Vehicle> _allVehicles = new();

        private async Task RefreshAsync()
        {
            Vehicles.Clear();

            // 1) Fetch all vehicles from the database
            _allVehicles = _uow.Vehicles.GetAll().ToList();

            // 2) Determine “todayRange”
            var todayRange = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));
            int? ignoreId = _session.CurrentCustomer?.Id;

            // 3) Batch‐fetch blocked IDs
            var blockedIds = await _availability.GetBlockedVehicleIdsAsync(todayRange, ignoreId);

            // 4) Filter by Category + Availability + Sort
            IEnumerable<Vehicle> temp = _allVehicles;

            if (!string.Equals(SelectedCategory, "All", StringComparison.OrdinalIgnoreCase) &&
                Enum.TryParse<VehicleType>(SelectedCategory, out var vt))
            {
                temp = temp.Where(v => v.VehicleType == vt);
            }

            // Compute CurrentlyAvailable
            foreach (var v in temp)
            {
                bool freeNow = (v.Status == VehicleStatus.Available) && !blockedIds.Contains(v.Id);
                v.CurrentlyAvailable = freeNow;
            }

            // Filter by Availability selection
            if (string.Equals(SelectedAvailability, "Available", StringComparison.OrdinalIgnoreCase))
            {
                temp = temp.Where(v => v.CurrentlyAvailable);
            }
            else if (string.Equals(SelectedAvailability, "Not Available", StringComparison.OrdinalIgnoreCase))
            {
                temp = temp.Where(v => !v.CurrentlyAvailable);
            }

            // Sort
            temp = SelectedSortOption switch
            {
                "Name (A → Z)" => temp.OrderBy(v => v.Name),
                "Name (Z → A)" => temp.OrderByDescending(v => v.Name),
                "Price (Low → High)" => temp.OrderBy(v => v.DailyRate.Amount),
                "Price (High → Low)" => temp.OrderByDescending(v => v.DailyRate.Amount),
                _ => temp
            };

            // 5) Push results into the ObservableCollection
            Vehicles.Clear();
            foreach (var v in temp)
                Vehicles.Add(v);

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

        // *** Change here: Accept Vehicle? instead of Vehicle ***
        private void OnLearnMore(Vehicle? vehicle)
        {
            if (vehicle == null) return;

            _messenger.Send(new GoToVehicleDetailMessage(vehicle.Id));
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
