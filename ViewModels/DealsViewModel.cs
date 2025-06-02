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

            WeakReferenceMessenger.Default.Register<GoToDealsMessage>(
                this, (_, _) => LoadMyDeals());

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

            _rentalHandler.CancelRental(r.Id); 
            LoadMyDeals();                   
        }

        private void LoadMyDeals()
        {
            MyDeals.Clear();

            var current = _session.CurrentCustomer;
            if (current == null)
            {
                return;
            }
            int customerId = current.Id;

            foreach (var r in _rentalHandler.GetAllDeals()
                                            .Where(r => r.CustomerId == customerId)
                                            .OrderByDescending(r => r.StartDate))
            {
                MyDeals.Add(r);
            }

            CancelRentalCommand.NotifyCanExecuteChanged();
        }
    }
}