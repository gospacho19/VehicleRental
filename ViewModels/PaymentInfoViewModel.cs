// LuxuryCarRental/ViewModels/PaymentInfoViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LuxuryCarRental.Data;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Implementations; // for UserSessionService

namespace LuxuryCarRental.ViewModels
{
    public partial class PaymentInfoViewModel : ObservableObject
    {
        private readonly AppDbContext _ctx;
        private readonly UserSessionService _session;

        public ObservableCollection<Card> SavedCards { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteCardCommand))]
        [NotifyCanExecuteChangedFor(nameof(BeginEditCardCommand))]
        private Card? _selectedCard;


        // “Add/Edit Card” panel state & fields 

        [ObservableProperty] private bool _isAddingOrEditingCard;
        [ObservableProperty] private bool _isEditingExistingCard;


        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCardCommand))]
        private string _newCardNumber = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCardCommand))]
        private string _newExpiry = string.Empty; 

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCardCommand))]
        private string _newCvv = string.Empty;

        [ObservableProperty] private string _errorMessage = string.Empty;

        // Commands
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BeginAddCardCommand { get; }
        public IRelayCommand BeginEditCardCommand { get; }
        public IRelayCommand SaveCardCommand { get; }
        public IRelayCommand CancelCardCommand { get; }
        public IRelayCommand DeleteCardCommand { get; }

        public PaymentInfoViewModel(AppDbContext ctx, UserSessionService session)
        {
            _ctx = ctx;
            _session = session;

            RefreshCommand = new RelayCommand(LoadSavedCards);
            BeginAddCardCommand = new RelayCommand(OnBeginAddCard);
            BeginEditCardCommand = new RelayCommand(OnBeginEditCard, CanEditOrDelete);
            SaveCardCommand = new RelayCommand(OnSaveCard, CanSaveCard);
            CancelCardCommand = new RelayCommand(OnCancelCard);
            DeleteCardCommand = new RelayCommand(OnDeleteCard, CanEditOrDelete);

            LoadSavedCards();
        }

        private void LoadSavedCards()
        {
            SavedCards.Clear();
            var current = _session.CurrentCustomer;
            if (current == null) return;

            var cards = _ctx.Cards
                            .Where(c => c.CustomerId == current.Id)
                            .OrderBy(c => c.ExpiryYear)
                            .ThenBy(c => c.ExpiryMonth)
                            .ToList();

            foreach (var c in cards)
                SavedCards.Add(c);
        }

        private bool CanEditOrDelete() => SelectedCard != null;

        private void OnBeginAddCard()
        {
            IsEditingExistingCard = false;
            NewCardNumber = string.Empty;
            NewExpiry = string.Empty;
            NewCvv = string.Empty;
            ErrorMessage = string.Empty;

            IsAddingOrEditingCard = true;
        }

        private void OnBeginEditCard()
        {
            var card = SelectedCard;
            if (card == null) return;

            IsEditingExistingCard = true;
            NewCardNumber = card.CardNumber;
            NewExpiry = $"{card.ExpiryMonth:D2}/{card.ExpiryYear % 100:D2}";
            NewCvv = card.Cvv;
            ErrorMessage = string.Empty;

            IsAddingOrEditingCard = true;
        }

        private bool CanSaveCard()
        {
            return
                !string.IsNullOrWhiteSpace(NewCardNumber) &&
                !string.IsNullOrWhiteSpace(NewExpiry) &&
                !string.IsNullOrWhiteSpace(NewCvv);
        }

        private void OnSaveCard()
        {
            ErrorMessage = string.Empty;
            var current = _session.CurrentCustomer;
            if (current == null)
            {
                ErrorMessage = "No user is currently logged in.";
                return;
            }

            // exactly 16 digits
            var number = NewCardNumber.Trim();
            if (!Regex.IsMatch(number, @"^\d{16}$"))
            {
                ErrorMessage = "Card number must be exactly 16 digits.";
                return;
            }

            // expiry “MM/YY” 
            var parts = NewExpiry.Trim().Split('/');
            if (parts.Length != 2
                || !int.TryParse(parts[0], out int m)
                || !int.TryParse(parts[1], out int yPart))
            {
                ErrorMessage = "Expiry must be in MM/YY or MM/YYYY format.";
                return;
            }

            int yr = (yPart < 100) ? 2000 + yPart : yPart;
            if (m < 1 || m > 12)
            {
                ErrorMessage = "Expiry month must be between 01 and 12.";
                return;
            }

            var today = DateTime.Today;
            var expiryDate = new DateTime(yr, m, 1).AddMonths(1).AddDays(-1);
            if (expiryDate < today)
            {
                ErrorMessage = "This card has already expired.";
                return;
            }

            // CVV: 3 or 4 digits
            if (!Regex.IsMatch(NewCvv.Trim(), @"^\d{3,4}$"))
            {
                ErrorMessage = "CVV must be 3 or 4 digits.";
                return;
            }

            // Compute the nickname from the last 4 digits
            var last4 = number.Substring(number.Length - 4);
            var nickname = "Card ending " + last4;

            if (IsEditingExistingCard && SelectedCard != null)
            {
                SelectedCard.Nickname = nickname;
                SelectedCard.CardNumber = number;
                SelectedCard.ExpiryMonth = m;
                SelectedCard.ExpiryYear = yr;
                SelectedCard.Cvv = NewCvv.Trim();

                _ctx.Cards.Update(SelectedCard);
                _ctx.SaveChanges();
            }
            else
            {
                var card = new Card
                {
                    CustomerId = current.Id,
                    Nickname = nickname,
                    CardNumber = number,
                    ExpiryMonth = m,
                    ExpiryYear = yr,
                    Cvv = NewCvv.Trim()
                };
                _ctx.Cards.Add(card);
                _ctx.SaveChanges();
            }

            LoadSavedCards();
            IsAddingOrEditingCard = false;
        }

        private void OnCancelCard()
        {
            ErrorMessage = string.Empty;
            IsAddingOrEditingCard = false;
        }

        private void OnDeleteCard()
        {
            if (SelectedCard == null) return;
            _ctx.Cards.Remove(SelectedCard);
            _ctx.SaveChanges();
            LoadSavedCards();
        }
    }
}
