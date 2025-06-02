
using System;
using System.Linq;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Handlers.Interfaces;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Implementations; 
using LuxuryCarRental.Services.Interfaces;

namespace LuxuryCarRental.ViewModels
{
    public partial class ConfirmationViewModel : ObservableObject
    {
        private readonly ICheckoutHandler _checkout;
        private readonly IMessenger _messenger;
        private readonly UserSessionService _session;

        public Money Total { get; private set; } = default!;
        public IEnumerable<CartItem> Items { get; private set; } = Enumerable.Empty<CartItem>();
        public Card PaymentCard { get; private set; } = default!;

        public IRelayCommand ConfirmCommand { get; }
        public IRelayCommand BackToCartCommand { get; }

        public ConfirmationViewModel(
            ICheckoutHandler checkout,
            IMessenger messenger,
            UserSessionService session)
        {
            _checkout = checkout ?? throw new ArgumentNullException(nameof(checkout));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _session = session ?? throw new ArgumentNullException(nameof(session));

            ConfirmCommand = new RelayCommand(OnConfirm);
            BackToCartCommand = new RelayCommand(OnBackToCart);
        }


        public void Initialize(Money total,
                               IEnumerable<CartItem> items,
                               Card paymentCard)
        {
            Total = total;
            Items = items ?? Enumerable.Empty<CartItem>();
            PaymentCard = paymentCard ?? throw new ArgumentNullException(nameof(paymentCard));

            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(PaymentCard));
        }

        private void OnConfirm()
        {
            var current = _session.CurrentCustomer
                         ?? throw new InvalidOperationException("No user is currently logged in.");


            var firstItem = Items.FirstOrDefault()
                            ?? throw new InvalidOperationException("Cart is empty.");

            var period = new DateRange(
                firstItem.StartDate,
                firstItem.EndDate
            );

            // deal for this customer
            _checkout.Checkout(
                customerId: current.Id,
                period: period,
                paymentToken: "auth:" + PaymentCard.Id
            );

            _messenger.Send(new GoToDealsMessage());
        }

        private void OnBackToCart()
        {
            _messenger.Send(new GoToCartMessage());
        }
    }
}
