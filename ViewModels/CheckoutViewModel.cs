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
        // ────────── Dependencies ──────────
        private readonly ICartService _cart;
        private readonly IPaymentService _payments;
        private readonly IMessenger _messenger;
        private readonly AppDbContext _ctx;
        private readonly UserSessionService _session;

        // ────────── Constructor ──────────
        public CheckoutViewModel(
            ICartService cart,
            IPaymentService payments,
            IMessenger messenger,
            AppDbContext ctx,
            UserSessionService session)
        {
            _cart = cart;
            _payments = payments;
            _messenger = messenger;
            _ctx = ctx;
            _session = session;

            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1);

            // Initialize the ReadOnly wrapper around our internal ObservableCollection:
            _savedCardsRO = new ReadOnlyObservableCollection<Card>(_savedCardsOC);

            // Commands:
            RefreshCommand = new RelayCommand(LoadAll);
            PayCommand = new RelayCommand(OnPay, CanPay);
            NavigateToPaymentInfoCommand = new RelayCommand(() =>
                _messenger.Send(new GoToPaymentInfoMessage()));
            BackToCartCommand = new RelayCommand(() =>
            _messenger.Send(new GoToCartMessage())
            );

            // Any time StartDate/EndDate changes, update the cart item date ranges:
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(StartDate) or nameof(EndDate))
                    UpdateCartItemDates();
            };

            // Immediately load (will handle null‐user gracefully):
            LoadAll();
        }

        // ────────── Date Range Properties ──────────
        [ObservableProperty]
        private DateTime _startDate;

        partial void OnStartDateChanged(DateTime oldValue, DateTime newValue)
        {
            // Ensure EndDate is always at least one day after StartDate and no more than 30 days:
            if (EndDate <= newValue) EndDate = newValue.AddDays(1);
            if ((EndDate - StartDate).TotalDays > 30)
                EndDate = StartDate.AddDays(30);

            OnPropertyChanged(nameof(DurationDays));
        }

        [ObservableProperty]
        private DateTime _endDate;

        partial void OnEndDateChanged(DateTime oldValue, DateTime newValue)
        {
            if (newValue <= StartDate) EndDate = StartDate.AddDays(1);
            if ((newValue - StartDate).TotalDays > 30)
                EndDate = StartDate.AddDays(30);

            OnPropertyChanged(nameof(DurationDays));
        }

        public int DurationDays => (int)Math.Ceiling((EndDate - StartDate).TotalDays);


        // ────────── Manual‐entry Card Fields ──────────
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private string _cardNumber = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private string _expiry = string.Empty; // e.g. "01/25"

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private string _cvv = string.Empty;

        [ObservableProperty]
        private bool _rememberCard;


        // ────────── Saved Cards Collection ──────────
        // Backing ObservableCollection:
        private readonly ObservableCollection<Card> _savedCardsOC = new();

        // Read‐only wrapper exposed publicly:
        private readonly ReadOnlyObservableCollection<Card> _savedCardsRO;
        public ReadOnlyObservableCollection<Card> SavedCards => _savedCardsRO;

        // The currently‐selected card from the ComboBox.
        // If its Id == -1, that means “Use a new card” placeholder.
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private Card? _selectedSavedCard = null!;


        // ────────── Cart Items & Total ──────────
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


        // ────────── Commands ──────────
        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public IRelayCommand PayCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand NavigateToPaymentInfoCommand { get; }
        public IRelayCommand BackToCartCommand { get; }


        // ────────── Public Methods ──────────

        /// <summary>
        /// Refresh both cart items and saved cards.
        /// If no user is logged in, we simply show empty data rather than throwing.
        /// </summary>
        private void LoadAll()
        {
            LoadCartItems();
            LoadSavedCards();
        }

        /// <summary>
        /// Loads CartItems for the current customer, or empties the list if no one is logged in.
        /// </summary>
        public void LoadCartItems()
        {
            var current = _session.CurrentCustomer;
            if (current == null)
            {
                CartItems = new List<CartItem>();
                return;
            }

            CartItems = _cart.GetCartItems(current.Id).ToList();
            UpdateCartItemDates();
        }

        /// <summary>
        /// When StartDate or EndDate changes, update each CartItem’s StartDate/EndDate
        /// so the DataGrid subtotal reflects the new duration.
        /// </summary>
        private void UpdateCartItemDates()
        {
            foreach (var item in CartItems)
            {
                item.StartDate = StartDate;
                item.EndDate = EndDate;
            }
            OnPropertyChanged(nameof(TotalCost));
            PayCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Loads SavedCards for the current customer with a dummy placeholder (Id = -1).
        /// If no user is logged in, we nevertheless add the dummy so the user can still type a new card.
        /// </summary>
        private void LoadSavedCards()
        {
            _savedCardsOC.Clear();

            var current = _session.CurrentCustomer;
            if (current == null)
            {
                // Add only the placeholder:
                var dummyNoUser = new Card
                {
                    Id = -1,
                    Nickname = "(Use a new card)",
                    CardNumber = string.Empty,
                    ExpiryMonth = 0,
                    ExpiryYear = 0,
                    Cvv = string.Empty,
                    CustomerId = -1
                };
                _savedCardsOC.Add(dummyNoUser);
                SelectedSavedCard = dummyNoUser;
                return;
            }

            // 1) First the placeholder for “Use a new card”:
            var placeholder = new Card
            {
                Id = -1,
                Nickname = "(Use a new card)",
                CardNumber = string.Empty,
                ExpiryMonth = 0,
                ExpiryYear = 0,
                Cvv = string.Empty,
                CustomerId = current.Id
            };
            _savedCardsOC.Add(placeholder);

            // 2) Then load real cards from the database:
            var realCards = _ctx.Cards
                                .Where(c => c.CustomerId == current.Id)
                                .OrderBy(c => c.ExpiryYear)
                                .ThenBy(c => c.ExpiryMonth);

            foreach (var c in realCards)
                _savedCardsOC.Add(c);

            // 3) Set the placeholder as the default selection:
            SelectedSavedCard = placeholder;
            PayCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Called whenever the ComboBox selection changes.
        /// If SelectedSavedCard.Id >= 0, it's a real card—prefill the manual fields for clarity.
        /// If SelectedSavedCard.Id == -1, it's our “new card” placeholder—clear the manual fields.
        /// </summary>
        partial void OnSelectedSavedCardChanged(Card? oldValue, Card? newValue)
        {
            if (newValue != null && newValue.Id >= 0)
            {
                CardNumber = newValue.CardNumber;
                Expiry = $"{newValue.ExpiryMonth:D2}/{newValue.ExpiryYear % 100:D2}";
                Cvv = newValue.Cvv;
            }
            else
            {
                CardNumber = string.Empty;
                Expiry = string.Empty;
                Cvv = string.Empty;
            }

            PayCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Determines whether the “Confirm & Pay” button is enabled:
        /// - If any saved card (Id>=0) is selected, we just require that its CardNumber isn't empty.
        /// - If the placeholder is selected (Id==-1), we require valid manual fields (CardNumber, Expiry, CVV).
        /// - Also requires at least one cart item.
        /// </summary>
        private bool CanPay()
        {
            // 1) Must have at least one cart item
            if (!CartItems.Any()) return false;

            // 2) If a saved card is selected (Id >= 0), check that it has a nonempty CardNumber
            if (SelectedSavedCard != null && SelectedSavedCard.Id >= 0)
            {
                return !string.IsNullOrWhiteSpace(SelectedSavedCard.CardNumber);
            }

            // 3) Otherwise, we’re in “new card” mode: all three manual fields must be nonblank
            if (string.IsNullOrWhiteSpace(CardNumber)
                || string.IsNullOrWhiteSpace(Expiry)
                || string.IsNullOrWhiteSpace(Cvv))
            {
                return false;
            }

            // 4) Validate expiry format “MM/YY” and CVV format 3–4 digits
            bool okExpiry = Regex.IsMatch(Expiry, @"^(0[1-9]|1[0-2])/[0-9]{2}$");
            bool okCvv = Regex.IsMatch(Cvv, @"^\d{3,4}$");
            return okExpiry && okCvv;
        }

        /// <summary>
        /// Called when the user clicks Confirm &amp; Pay:
        /// - If a real saved card is selected, charge it.
        /// - Otherwise build a new Card from the manual fields (auto‐generate nickname),
        ///   save it (if RememberCard==true), then charge it.
        /// Then navigate to the confirmation screen and clear out the form.
        /// </summary>
        private void OnPay()
        {
            ErrorMessage = string.Empty;

            var current = _session.CurrentCustomer;
            if (current == null)
            {
                ErrorMessage = "No user is currently logged in.";
                return;
            }

            Card cardToCharge;

            if (SelectedSavedCard != null && SelectedSavedCard.Id >= 0)
            {
                // 1) Charge the existing saved card
                cardToCharge = SelectedSavedCard;
            }
            else
            {
                // 2) Build a brand‐new Card from the manual fields
                //    We already validated “MM/YY” and CVV in CanPay()
                var parts = Expiry.Split('/');
                int m = int.Parse(parts[0]);
                int y = 2000 + int.Parse(parts[1]);

                // Generate nickname “Card ending ####”
                string last4 = CardNumber.Trim().Substring(CardNumber.Trim().Length - 4);
                string nickname = "Card ending " + last4;

                cardToCharge = new Card
                {
                    CustomerId = current.Id,
                    CardNumber = CardNumber.Trim(),
                    ExpiryMonth = m,
                    ExpiryYear = y,
                    Cvv = Cvv.Trim(),
                    Nickname = nickname
                };

                if (RememberCard)
                {
                    _ctx.Cards.Add(cardToCharge);
                    _ctx.SaveChanges();

                    // Refresh so the new card appears in the dropdown
                    LoadSavedCards();
                }
            }

            // 3) Pre‐authorize / Charge
            var totalMoney = new Money(TotalCost, "USD");
            _payments.Charge(cardToCharge, totalMoney);

            // 4) Navigate to confirmation (passing total, cart items, and the card used)
            _messenger.Send(new GoToConfirmationMessage(
                totalMoney,
                CartItems.ToList(),
                cardToCharge));



            // 5) Clear the form for next time:
            CardNumber = string.Empty;
            Expiry = string.Empty;
            Cvv = string.Empty;
            RememberCard = false;

            // Reset to the dummy “Use a new card” placeholder
            var placeholder = _savedCardsOC.FirstOrDefault(c => c.Id == -1);
            if (placeholder != null)
            {
                SelectedSavedCard = placeholder;
            }
        }
    }
}
