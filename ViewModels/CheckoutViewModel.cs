using System;
using System.Collections.Generic;
using System.Linq;
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

            // Initialize default dates:
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1);

            PayCommand = new RelayCommand(OnPay, CanPay);

            // Preload any existing cart items (if you wish, though we’ll ignore their dates)
            LoadCartItems();
        }

        // 1) Two new properties for date selection:
        [ObservableProperty]
        private DateTime _startDate;

        partial void OnStartDateChanged(DateTime oldValue, DateTime newValue)
        {
            if (EndDate <= newValue)
                EndDate = newValue.AddDays(1);

            if ((EndDate - StartDate).TotalDays > 30)
                EndDate = StartDate.AddDays(30);

            // 1) Tell WPF to re-read DurationDays
            OnPropertyChanged(nameof(DurationDays));

            // 2) Tell WPF to re-compute TotalCost (if you show that too)
            OnPropertyChanged(nameof(TotalCost));
        }

        [ObservableProperty]
        private DateTime _endDate;

        partial void OnEndDateChanged(DateTime oldValue, DateTime newValue)
        {
            if (newValue <= StartDate)
            {
                EndDate = StartDate.AddDays(1);
                return;
            }

            if ((newValue - StartDate).TotalDays > 30)
            {
                EndDate = StartDate.AddDays(30);
                return;
            }

            // 1) DurationDays changed
            OnPropertyChanged(nameof(DurationDays));

            // 2) TotalCost changed
            OnPropertyChanged(nameof(TotalCost));
        }

        /// <summary>
        /// Number of days (always ≥ 1, ≤ 30).
        /// This is used to compute TotalCost if your PricingService multiplies daily rate by days.
        /// </summary>
        public int DurationDays => (int)Math.Ceiling((EndDate - StartDate).TotalDays);

        // 2) Re‐use your existing card‐fields + validation:
        [ObservableProperty] private string cardNumber = string.Empty;
        [ObservableProperty] private string expiry = string.Empty;
        [ObservableProperty] private string cvv = string.Empty;

        partial void OnCardNumberChanged(string? oldValue, string newValue)
            => PayCommand.NotifyCanExecuteChanged();

        partial void OnExpiryChanged(string? oldValue, string newValue)
            => PayCommand.NotifyCanExecuteChanged();

        partial void OnCvvChanged(string? oldValue, string newValue)
            => PayCommand.NotifyCanExecuteChanged();

        public IRelayCommand PayCommand { get; }

        private bool CanPay()
        {
            if (string.IsNullOrWhiteSpace(CardNumber)) return false;
            if (string.IsNullOrWhiteSpace(Expiry)) return false;
            if (string.IsNullOrWhiteSpace(Cvv)) return false;

            bool okExpiry = Regex.IsMatch(Expiry, @"^(0[1-9]|1[0-2])/[0-9]{2}$");
            bool okCvv = Regex.IsMatch(Cvv, @"^\d{3,4}$");

            return okExpiry && okCvv;
        }

        // 3) We still want to show WHAT is in the cart while user chooses dates:
        public List<CartItem> CartItems { get; private set; } = new();

        public void LoadCartItems()
        {
            CartItems = _cart.GetCartItems(1).ToList();  // 1 = demo customer
            OnPropertyChanged(nameof(CartItems));
            OnPropertyChanged(nameof(TotalCost));
        }

        /// <summary>
        /// Total cost for ALL vehicles in cart, given the chosen date range.
        /// We assume your PricingService.CalculateTotal(...) does something like:
        ///     (vehicle.DailyRate × DurationDays) + any extras.
        /// If you don’t have a pricing‐per‐day logic yet, you can compute manually:
        ///     CartItems.Sum(ci => ci.Vehicle.DailyRate.Amount) * DurationDays
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                // If you already have a PricingService.CalculateTotal for a single vehicle:
                // return CartItems.Sum(ci =>
                //     _pricing.CalculateTotal(ci.Vehicle,
                //                             new DateRange(StartDate, EndDate),
                //                             Enumerable.Empty<string>())
                //             .Amount);

                // Otherwise, do a simple: daily rate * days:
                return CartItems.Sum(ci =>
                    ci.Vehicle.DailyRate.Amount * DurationDays
                );
            }
        }

        /// <summary>
        /// When PayCommand is clicked, we:
        ///  1) Charge the card via _payments.Charge(...)
        ///  2) Call _checkoutHandler.Checkout(customerId, chosenPeriod, transactionId)
        ///  3) Send confirmation message
        ///  4) Clear out card fields and (optionally) clear local cart‐view
        /// </summary>
        private void OnPay()
        {
            const int customerId = 1;

            // 1) Charge the card
            var parts = Expiry.Split('/');
            var card = new Card
            {
                CustomerId = customerId,
                CardNumber = CardNumber,
                ExpiryMonth = int.Parse(parts[0]),
                ExpiryYear = 2000 + int.Parse(parts[1]),
                Cvv = Cvv
            };

            var totalMoney = new Money(TotalCost, "USD");
            var transactionId = _payments.Charge(card, totalMoney);

            // 2) Build a DateRange from the user’s chosen dates:
            var chosenRange = new DateRange(StartDate, EndDate);

            // 3) Call the updated Checkout method (we’ll modify the interface shortly)
            var rentals = _checkoutHandler.Checkout(customerId, chosenRange, transactionId);

            // 4) Notify the ConfirmationView, passing along total & items & card
            _messenger.Send(new GoToConfirmationMessage(totalMoney, CartItems, card));

            // 5) Optionally, clear local fields:
            CardNumber = Expiry = Cvv = string.Empty;
            LoadCartItems();   // reload in case Cart was cleared
        }
    }
}
