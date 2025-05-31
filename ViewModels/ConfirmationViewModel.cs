using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Handlers.Interfaces;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Implementations; // for UserSessionService
using LuxuryCarRental.Services.Interfaces;

namespace LuxuryCarRental.ViewModels
{
    public partial class ConfirmationViewModel : ObservableObject
    {
        private readonly ICheckoutHandler _checkout;
        private readonly IMessenger _messenger;
        private readonly UserSessionService _session;   // ← We now need this

        public Money Total { get; private set; } = default!;
        public IEnumerable<CartItem> Items { get; private set; } = default!;
        public Card PaymentCard { get; private set; } = default!;

        public IRelayCommand ConfirmCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ConfirmationViewModel(
            ICheckoutHandler checkout,
            IMessenger messenger,
            UserSessionService session)        // ← Added session
        {
            _checkout = checkout;
            _messenger = messenger;
            _session = session;

            ConfirmCommand = new RelayCommand(OnConfirm);
            CancelCommand = new RelayCommand(() =>
                               _messenger.Send(new GoToCatalogMessage()));
        }

        /// <summary>Called by MainViewModel right after creating this VM:</summary>
        public void Initialize(Money total,
                               IEnumerable<CartItem> items,
                               Card paymentCard)
        {
            Total = total;
            Items = items;
            PaymentCard = paymentCard;

            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(PaymentCard));
        }

        private void OnConfirm()
        {
            var current = _session.CurrentCustomer
                         ?? throw new InvalidOperationException("No user logged in");

            var period = new DateRange(
                Items.First().StartDate,
                Items.First().EndDate
            );

            // Persist a “Rental” (deal) for this exact customer
            _checkout.Checkout(
                customerId: current.Id,
                period: period,
                paymentToken: "auth:" + PaymentCard.Id
            );

            // Tell the app to navigate to Deals
            _messenger.Send(new GoToDealsMessage());
        }
    }
}
