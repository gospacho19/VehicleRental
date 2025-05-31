using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Data;
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
        private readonly AppDbContext _ctx;

        private const int DemoCustomerId = 1;

        public CheckoutViewModel(
            ICartService cart,
            IPaymentService payments,
            ICheckoutHandler checkoutHandler,
            IMessenger messenger,
            AppDbContext ctx)
        {
            _cart = cart;
            _payments = payments;
            _checkoutHandler = checkoutHandler;
            _messenger = messenger;
            _ctx = ctx;

            // Initialize Start/End dates
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1);

            // Prepare SavedCards collection
            _savedCardsRO = new ReadOnlyObservableCollection<Card>(_savedCardsOC);
            LoadSavedCards();

            // Wire up commands
            PayCommand = new RelayCommand(OnPay, CanPay);
            RefreshCommand = new RelayCommand(LoadCartItems);
            NavigateToPaymentInfoCommand = new RelayCommand(OnNavigateToPaymentInfo);

            // Whenever the user picks or clears a saved card, re‐evaluate CanEditCard and PayCommand
            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedSavedCard))
                {
                    OnPropertyChanged(nameof(CanEditCard));
                    PayCommand.NotifyCanExecuteChanged();
                }
            };

            // Whenever the user changes StartDate or EndDate, push those dates into each CartItem
            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(StartDate) ||
                    e.PropertyName == nameof(EndDate))
                {
                    UpdateCartItemDates();
                    OnPropertyChanged(nameof(TotalCost)); // total cost may have changed
                }
            };

            // Initial load
            RememberCard = false;
            LoadCartItems();
            PrefillCardIfExists();
        }

        // ─────────────────────────────────────────────────────────
        // 1) Date picker properties & validation
        // ─────────────────────────────────────────────────────────

        [ObservableProperty]
        private DateTime _startDate;
        partial void OnStartDateChanged(DateTime oldValue, DateTime newValue)
        {
            if (EndDate <= newValue)
                EndDate = newValue.AddDays(1);
            if ((EndDate - StartDate).TotalDays > 30)
                EndDate = StartDate.AddDays(30);

            OnPropertyChanged(nameof(DurationDays));
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
            OnPropertyChanged(nameof(DurationDays));
        }

        public int DurationDays => (int)Math.Ceiling((EndDate - StartDate).TotalDays);

        // ─────────────────────────────────────────────────────────
        // 2) Manual card entry fields
        // ─────────────────────────────────────────────────────────

        // *** ADDED [NotifyCanExecuteChangedFor(nameof(PayCommand))] ***
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private string _cardNumber = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private string _expiry = string.Empty; // format “MM/YY”

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private string _cvv = string.Empty;

        [ObservableProperty]
        private bool _rememberCard;

        // ─────────────────────────────────────────────────────────
        // 3) Saved cards collection and selection
        // ─────────────────────────────────────────────────────────

        public ReadOnlyObservableCollection<Card> SavedCards => _savedCardsRO;
        private readonly ObservableCollection<Card> _savedCardsOC = new();
        private readonly ReadOnlyObservableCollection<Card> _savedCardsRO;

        // Already had [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private Card? _selectedSavedCard;

        public bool CanEditCard => SelectedSavedCard == null;

        private void LoadSavedCards()
        {
            _savedCardsOC.Clear();
            var cards = _ctx.Cards
                            .Where(c => c.CustomerId == DemoCustomerId)
                            .OrderBy(c => c.ExpiryYear)
                            .ThenBy(c => c.ExpiryMonth)
                            .ToList();
            foreach (var c in cards)
                _savedCardsOC.Add(c);
        }

        private void PrefillCardIfExists()
        {
            var recent = _ctx.Cards
                             .Where(c => c.CustomerId == DemoCustomerId)
                             .OrderByDescending(c => c.Id)
                             .FirstOrDefault();
            if (recent != null)
            {
                SelectedSavedCard = recent;
                CardNumber = recent.CardNumber;
                Expiry = $"{recent.ExpiryMonth:D2}/{recent.ExpiryYear % 100:D2}";
                Cvv = recent.Cvv;
            }
        }

        // ─────────────────────────────────────────────────────────
        // 4) Cart items & TotalCost
        // ─────────────────────────────────────────────────────────

        private List<CartItem> _cartItems = new();
        public List<CartItem> CartItems
        {
            get => _cartItems;
            private set
            {
                SetProperty(ref _cartItems, value);
                OnPropertyChanged(nameof(TotalCost));
                PayCommand.NotifyCanExecuteChanged();
            }
        }

        public decimal TotalCost =>
            CartItems.Sum(ci => ci.Vehicle.DailyRate.Amount * DurationDays);

        public void LoadCartItems()
        {
            CartItems = _cart.GetCartItems(DemoCustomerId).ToList();
            UpdateCartItemDates();
        }

        private void UpdateCartItemDates()
        {
            foreach (var item in CartItems)
            {
                item.StartDate = StartDate;
                item.EndDate = EndDate;
            }
            OnPropertyChanged(nameof(CartItems));
            OnPropertyChanged(nameof(TotalCost));
            PayCommand.NotifyCanExecuteChanged();
        }

        // ─────────────────────────────────────────────────────────
        // 5) Commands
        // ─────────────────────────────────────────────────────────

        public IRelayCommand PayCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand NavigateToPaymentInfoCommand { get; }

        private bool CanPay()
        {
            if (SelectedSavedCard != null)
                return CartItems.Any();

            if (!CartItems.Any())
                return false;

            if (string.IsNullOrWhiteSpace(CardNumber)) return false;
            if (string.IsNullOrWhiteSpace(Expiry)) return false;
            if (string.IsNullOrWhiteSpace(Cvv)) return false;

            bool okExpiry = Regex.IsMatch(Expiry, @"^(0[1-9]|1[0-2])/[0-9]{2}$");
            bool okCvv = Regex.IsMatch(Cvv, @"^\d{3,4}$");

            return okExpiry && okCvv;
        }

        private void OnPay()
        {
            Card cardToCharge;

            if (SelectedSavedCard != null)
            {
                cardToCharge = SelectedSavedCard;
            }
            else
            {
                var parts = Expiry.Split('/');
                int month = int.Parse(parts[0]);
                int year = 2000 + int.Parse(parts[1]);

                cardToCharge = new Card
                {
                    CustomerId = DemoCustomerId,
                    CardNumber = CardNumber,
                    ExpiryMonth = month,
                    ExpiryYear = year,
                    Cvv = Cvv,
                    Nickname = $"Card ending {CardNumber[^4..]}"
                };

                if (RememberCard)
                {
                    _ctx.Cards.Add(cardToCharge);
                    _ctx.SaveChanges();
                    _savedCardsOC.Add(cardToCharge);
                }
            }

            var totalMoney = new Money(TotalCost, "USD");
            var transactionId = _payments.Charge(cardToCharge, totalMoney);
            var period = new DateRange(StartDate, EndDate);

            var rentals = _checkoutHandler.Checkout(DemoCustomerId, period, transactionId);

            _messenger.Send(new GoToConfirmationMessage(totalMoney, CartItems, cardToCharge));

            CardNumber = string.Empty;
            Expiry = string.Empty;
            Cvv = string.Empty;
            RememberCard = false;
            SelectedSavedCard = null;

            LoadCartItems();
        }

        private void OnNavigateToPaymentInfo()
        {
            _messenger.Send(new GoToPaymentInfoMessage());
        }
    }
}
