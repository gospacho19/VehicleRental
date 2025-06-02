
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
using LuxuryCarRental.Services.Implementations; 

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
            UserSessionService session)
        {
            _cart = cart;
            _payments = payments;
            _messenger = messenger;
            _ctx = ctx;
            _session = session;

            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1);

            _savedCardsRO = new ReadOnlyObservableCollection<Card>(_savedCardsOC);

            // Commands:
            RefreshCommand = new RelayCommand(LoadAll);
            PayCommand = new RelayCommand(OnPay, CanPay);
            NavigateToPaymentInfoCommand = new RelayCommand(() =>
                _messenger.Send(new GoToPaymentInfoMessage()));
            BackToCartCommand = new RelayCommand(() =>
            _messenger.Send(new GoToCartMessage())
            );

            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(StartDate) or nameof(EndDate))
                    UpdateCartItemDates();
            };

            LoadAll();
        }

        // Date Range Properties 
        [ObservableProperty]
        private DateTime _startDate;

        partial void OnStartDateChanged(DateTime oldValue, DateTime newValue)
        {
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


        // Manual‐entry Card Fields
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private string _cardNumber = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private string _expiry = string.Empty; 

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private string _cvv = string.Empty;

        [ObservableProperty]
        private bool _rememberCard;



        private readonly ObservableCollection<Card> _savedCardsOC = new();

        private readonly ReadOnlyObservableCollection<Card> _savedCardsRO;
        public ReadOnlyObservableCollection<Card> SavedCards => _savedCardsRO;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PayCommand))]
        private Card? _selectedSavedCard = null!;


        //Cart Items and Total 
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


        // Commands 
        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public IRelayCommand PayCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand NavigateToPaymentInfoCommand { get; }
        public IRelayCommand BackToCartCommand { get; }


        // Public Methods
        private void LoadAll()
        {
            LoadCartItems();
            LoadSavedCards();
        }


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

        private void LoadSavedCards()
        {
            _savedCardsOC.Clear();

            var current = _session.CurrentCustomer;
            if (current == null)
            {
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

            // load real cards from the database
            var realCards = _ctx.Cards
                                .Where(c => c.CustomerId == current.Id)
                                .OrderBy(c => c.ExpiryYear)
                                .ThenBy(c => c.ExpiryMonth);

            foreach (var c in realCards)
                _savedCardsOC.Add(c);

            SelectedSavedCard = placeholder;
            PayCommand.NotifyCanExecuteChanged();
        }


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


        // determines whether the “Confirm & Pay” button is enabled
        private bool CanPay()
        {
            // must have at least one cart item
            if (!CartItems.Any()) return false;

            if (SelectedSavedCard != null && SelectedSavedCard.Id >= 0)
            {
                return !string.IsNullOrWhiteSpace(SelectedSavedCard.CardNumber);
            }

            if (string.IsNullOrWhiteSpace(CardNumber)
                || string.IsNullOrWhiteSpace(Expiry)
                || string.IsNullOrWhiteSpace(Cvv))
            {
                return false;
            }

            // validate expiry format 
            bool okExpiry = Regex.IsMatch(Expiry, @"^(0[1-9]|1[0-2])/[0-9]{2}$");
            bool okCvv = Regex.IsMatch(Cvv, @"^\d{3,4}$");
            return okExpiry && okCvv;
        }



        // if a real saved card is selected, charge it

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
                cardToCharge = SelectedSavedCard;
            }
            else
            {
                // Build a brand‐new Card 
                var parts = Expiry.Split('/');
                int m = int.Parse(parts[0]);
                int y = 2000 + int.Parse(parts[1]);

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

                    LoadSavedCards();
                }
            }

            var totalMoney = new Money(TotalCost, "USD");
            _payments.Charge(cardToCharge, totalMoney);

            _messenger.Send(new GoToConfirmationMessage(
                totalMoney,
                CartItems.ToList(),
                cardToCharge));



            CardNumber = string.Empty;
            Expiry = string.Empty;
            Cvv = string.Empty;
            RememberCard = false;

            var placeholder = _savedCardsOC.FirstOrDefault(c => c.Id == -1);
            if (placeholder != null)
            {
                SelectedSavedCard = placeholder;
            }
        }
    }
}
