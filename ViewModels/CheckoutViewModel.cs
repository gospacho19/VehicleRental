// LuxuryCarRental/ViewModels/CheckoutViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Data;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;
using LuxuryCarRental.Services.Implementations; // for UserSessionService

namespace LuxuryCarRental.ViewModels
{
    public partial class CheckoutViewModel : ObservableObject
    {
        private readonly ICartService _cart;
        private readonly IPaymentService _payments;
        private readonly IMessenger _messenger;
        private readonly AppDbContext _ctx;
        private readonly UserSessionService _session;

        public CheckoutViewModel(
            ICartService cart,
            IPaymentService payments,
            IMessenger messenger,
            AppDbContext ctx,
            UserSessionService session)      // inject the session here
        {
            _cart = cart;
            _payments = payments;
            _messenger = messenger;
            _ctx = ctx;
            _session = session;

            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1);

            _savedCardsRO = new ReadOnlyObservableCollection<Card>(_savedCardsOC);
            // LoadSavedCards();

            PayCommand = new RelayCommand(OnPay, CanPay);
            RefreshCommand = new RelayCommand(LoadCartItems);
            NavigateToPaymentInfoCommand = new RelayCommand(() =>
                                           _messenger.Send(new GoToPaymentInfoMessage()));

            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(StartDate) or nameof(EndDate))
                    UpdateCartItemDates();
            };

            // LoadCartItems();
            // PrefillCardIfExists();
        }

        // ───── Dates ───────────────────────────────────────────
        [ObservableProperty] private DateTime _startDate;
        partial void OnStartDateChanged(DateTime oldValue, DateTime newValue)
        {
            if (EndDate <= newValue) EndDate = newValue.AddDays(1);
            if ((EndDate - StartDate).TotalDays > 30)
                EndDate = StartDate.AddDays(30);
            OnPropertyChanged(nameof(DurationDays));
        }

        [ObservableProperty] private DateTime _endDate;
        partial void OnEndDateChanged(DateTime oldValue, DateTime newValue)
        {
            if (newValue <= StartDate) EndDate = StartDate.AddDays(1);
            if ((newValue - StartDate).TotalDays > 30)
                EndDate = StartDate.AddDays(30);
            OnPropertyChanged(nameof(DurationDays));
        }

        public int DurationDays => (int)Math.Ceiling((EndDate - StartDate).TotalDays);

        // ───── Manual-entry card fields ───────────────────────
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private string _cardNumber = string.Empty;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private string _expiry = string.Empty;               // MM/YY

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private string _cvv = string.Empty;

        [ObservableProperty] private bool _rememberCard;

        // ───── Saved cards ────────────────────────────────────
        private readonly ObservableCollection<Card> _savedCardsOC = new();
        private readonly ReadOnlyObservableCollection<Card> _savedCardsRO;
        public ReadOnlyObservableCollection<Card> SavedCards => _savedCardsRO;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private Card? _selectedSavedCard;
        public bool CanEditCard => SelectedSavedCard == null;

        private void LoadSavedCards()
        {
            _savedCardsOC.Clear();

            var current = _session.CurrentCustomer
                         ?? throw new InvalidOperationException("No user logged in");

            foreach (var c in _ctx.Cards
                                  .Where(c => c.CustomerId == current.Id)
                                  .OrderBy(c => c.ExpiryYear)
                                  .ThenBy(c => c.ExpiryMonth))
            {
                _savedCardsOC.Add(c);
            }
        }

        private void PrefillCardIfExists()
        {
            var current = _session.CurrentCustomer
                         ?? throw new InvalidOperationException("No user logged in");

            var recent = _ctx.Cards
                             .Where(c => c.CustomerId == current.Id)
                             .OrderByDescending(c => c.Id)
                             .FirstOrDefault();
            if (recent == null) return;

            SelectedSavedCard = recent;
            CardNumber = recent.CardNumber;
            Expiry = $"{recent.ExpiryMonth:D2}/{recent.ExpiryYear % 100:D2}";
            Cvv = recent.Cvv;
        }

        // ───── Cart & totals ──────────────────────────────────
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
            var current = _session.CurrentCustomer
                         ?? throw new InvalidOperationException("No user logged in");

            CartItems = _cart.GetCartItems(current.Id).ToList();
            UpdateCartItemDates();
        }

        private void UpdateCartItemDates()
        {
            foreach (var i in CartItems)
            {
                i.StartDate = StartDate;
                i.EndDate = EndDate;
            }
            OnPropertyChanged(nameof(TotalCost));
            PayCommand.NotifyCanExecuteChanged();
        }

        // ───── Commands ───────────────────────────────────────
        [ObservableProperty] private string _errorMessage = string.Empty;

        public IRelayCommand PayCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand NavigateToPaymentInfoCommand { get; }

        private bool CanPay()
        {
            if (!CartItems.Any()) return false;

            if (SelectedSavedCard != null)
                return !string.IsNullOrWhiteSpace(SelectedSavedCard.CardNumber);

            if (string.IsNullOrWhiteSpace(CardNumber) ||
                string.IsNullOrWhiteSpace(Expiry) ||
                string.IsNullOrWhiteSpace(Cvv))
                return false;

            bool okExpiry = Regex.IsMatch(Expiry, @"^(0[1-9]|1[0-2])/[0-9]{2}$");
            bool okCvv = Regex.IsMatch(Cvv, @"^\d{3,4}$");
            return okExpiry && okCvv;
        }

        private void OnPay()
        {
            ErrorMessage = string.Empty;

            var current = _session.CurrentCustomer
                         ?? throw new InvalidOperationException("No user logged in");

            // 1) choose / build Card
            Card cardToCharge = SelectedSavedCard ?? BuildNewCard(current);

            // 2) pre-authorise
            var totalMoney = new Money(TotalCost, "USD");
            _payments.Charge(cardToCharge, totalMoney);

            // 3) navigate to confirmation
            _messenger.Send(new GoToConfirmationMessage(
                totalMoney,
                CartItems.ToList(),
                cardToCharge));

            // 4) clear manual fields
            CardNumber = string.Empty;
            Expiry = string.Empty;
            Cvv = string.Empty;
            RememberCard = false;
            SelectedSavedCard = null;
        }

        private Card BuildNewCard(Customer current)
        {
            var parts = Expiry.Split('/');
            int m = int.Parse(parts[0]);
            int y = 2000 + int.Parse(parts[1]);

            var card = new Card
            {
                CustomerId = current.Id,
                CardNumber = CardNumber,
                ExpiryMonth = m,
                ExpiryYear = y,
                Cvv = Cvv,
                Nickname = $"Card ending {CardNumber[^4..]}"
            };

            if (RememberCard)
            {
                _ctx.Cards.Add(card);
                _ctx.SaveChanges();
                _savedCardsOC.Add(card);
            }
            return card;
        }
    }
}
