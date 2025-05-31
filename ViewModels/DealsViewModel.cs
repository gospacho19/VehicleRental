using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Handlers.Interfaces;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Implementations;
using System.Collections.ObjectModel;

namespace LuxuryCarRental.ViewModels
{
    public partial class DealsViewModel : ObservableObject
    {
        private readonly IRentalHandler _rentalHandler;
        private readonly UserSessionService _session;

        public DealsViewModel(
            IRentalHandler rentalHandler,
            UserSessionService session)
        {
            _rentalHandler = rentalHandler;
            _session = session;

            CancelRentalCommand = new RelayCommand<Rental?>(OnCancelRental,
                                                            CanCancelRental);
            RefreshCommand = new RelayCommand(LoadMyDeals);

            // Whenever someone sends GoToDealsMessage, reload
            WeakReferenceMessenger.Default.Register<GoToDealsMessage>(
                this, (_, _) => LoadMyDeals());

            // Initial load
            LoadMyDeals();
        }

        public ObservableCollection<Rental> MyDeals { get; } = new();

        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand<Rental?> CancelRentalCommand { get; }

        private bool CanCancelRental(Rental? r)
            => r is { Status: RentalStatus.Active or RentalStatus.Booked };

        private void OnCancelRental(Rental? r)
        {
            if (r == null) return;

            _rentalHandler.CancelRental(r.Id);   // also sets Vehicle.Status = Available
            LoadMyDeals();                       // refresh list & button states
        }

        private void LoadMyDeals()
        {
            MyDeals.Clear();

            var current = _session.CurrentCustomer;
            if (current == null)
            {
                // Optionally: show a message or do nothing if not logged in
                return;
            }
            int customerId = current.Id;

            foreach (var r in _rentalHandler.GetAllDeals()
                                            .Where(r => r.CustomerId == customerId)
                                            .OrderByDescending(r => r.StartDate))
            {
                MyDeals.Add(r);
            }

            // Re-evaluate "Cancel" button state
            CancelRentalCommand.NotifyCanExecuteChanged();
        }
    }
}