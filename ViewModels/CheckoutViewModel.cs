using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Handlers.Interfaces;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;

namespace LuxuryCarRental.ViewModels
{
    public partial class CheckoutViewModel : ObservableObject
    {
        private readonly ICartService _cart;
        private readonly IPaymentService _payments;
        private readonly ICheckoutHandler _checkoutHandler;
        private readonly IMessenger _messenger;

        public CheckoutViewModel(
            ICartService cart,
            IPaymentService payments,
            ICheckoutHandler checkoutHandler,
            IMessenger messenger)
        {
            _cart = cart;
            _payments = payments;
            _checkoutHandler = checkoutHandler;
            _messenger = messenger;
            PayCommand = new RelayCommand(OnPay, CanPay);
        }

        public IRelayCommand PayCommand { get; }

        // <– Generator will create public CardNumber/Expiry/Cvv properties for you
        [ObservableProperty] private string cardNumber = string.Empty;
        [ObservableProperty] private string expiry = string.Empty;
        [ObservableProperty] private string cvv = string.Empty;

        // <– these hooks run whenever the generator’s setters fire
        partial void OnCardNumberChanged(string? oldValue, string newValue)
            => PayCommand.NotifyCanExecuteChanged();

        partial void OnExpiryChanged(string? oldValue, string newValue)
            => PayCommand.NotifyCanExecuteChanged();

        partial void OnCvvChanged(string? oldValue, string newValue)
            => PayCommand.NotifyCanExecuteChanged();

        private bool CanPay()
        {
            // Make sure we have non-empty values
            if (string.IsNullOrWhiteSpace(CardNumber)) return false;
            if (string.IsNullOrWhiteSpace(Expiry)) return false;
            if (string.IsNullOrWhiteSpace(Cvv)) return false;

            // Validate format
            bool okExpiry = Regex.IsMatch(Expiry, @"^(0[1-9]|1[0-2])/[0-9]{2}$");
            bool okCvv = Regex.IsMatch(Cvv, @"^\d{3,4}$");

            return okExpiry && okCvv;
        }

        private void OnPay()
        {
            const int customerId = 1;

            // Gather cart data
            var items = _cart.GetCartItems(customerId);
            var total = _cart.GetCartTotal(customerId);

            // Build Card object
            var parts = Expiry.Split('/');
            var card = new Card
            {
                CustomerId = customerId,
                CardNumber = CardNumber,
                ExpiryMonth = int.Parse(parts[0]),
                ExpiryYear = 2000 + int.Parse(parts[1]),
                Cvv = Cvv
            };

            // Charge & persist rentals
            var transactionId = _payments.Charge(card, total);
            _checkoutHandler.Checkout(customerId, transactionId);

            // Send payload to confirmation
            _messenger.Send(new GoToConfirmationMessage(total, items, card));

            // Clear fields
            CardNumber = Expiry = Cvv = "";
        }
    }
}
