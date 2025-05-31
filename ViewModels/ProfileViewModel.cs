// LuxuryCarRental/ViewModels/ProfileViewModel.cs
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Data;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Implementations; // for UserSessionService
using LuxuryCarRental.Services.Interfaces;

namespace LuxuryCarRental.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly IAuthService _auth;
        private readonly AppDbContext _ctx;
        private readonly IMessenger _messenger;
        private readonly UserSessionService _session;

        // 2) Saved Cards backing fields
        private readonly ObservableCollection<Card> _savedCardsOC = new();
        private readonly ReadOnlyObservableCollection<Card> _savedCardsRO;

        // 3) Commands
        public IRelayCommand SaveProfileCommand { get; }
        public IRelayCommand AddCardCommand { get; }
        public IRelayCommand<Card?> DeleteCardCommand { get; }
        public IRelayCommand LogoutCommand { get; }

        public ProfileViewModel(
            IAuthService auth,
            AppDbContext ctx,
            IMessenger messenger,
            UserSessionService session)
        {
            _auth = auth;
            _ctx = ctx;
            _messenger = messenger;
            _session = session;

            // Initialize the ReadOnly wrapper around our card collection
            _savedCardsRO = new ReadOnlyObservableCollection<Card>(_savedCardsOC);

            // Prefill fields if a user is already logged in
            if (_session.CurrentCustomer is not null)
            {
                var c = _session.CurrentCustomer!;
                _fullName = c.FullName;
                _driverLicenseNumber = c.DriverLicenseNumber;
                _email = c.Contact.Email;
                _phone = c.Contact.Phone;
                LoadCards();
            }

            // Initialize commands (all non-nullable)
            SaveProfileCommand = new RelayCommand(OnSaveProfile, CanSaveProfile);
            AddCardCommand = new RelayCommand(OnAddCard);
            DeleteCardCommand = new RelayCommand<Card?>(OnDeleteCard);
            LogoutCommand = new RelayCommand(OnLogout);
        }

        // ─────────────────────────────────────────────────────
        // 1) Personal Info fields, bound to the UI
        // ─────────────────────────────────────────────────────
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
        private string _fullName = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
        private string _driverLicenseNumber = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
        private string _email = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
        private string _phone = string.Empty;

        [ObservableProperty]
        private Card? _selectedCardToDelete;

        private bool CanSaveProfile()
        {
            // If no user is logged in, disallow.
            var customer = _session.CurrentCustomer;
            if (customer is null) return false;

            // Otherwise, ensure none of the fields are blank
            return !string.IsNullOrWhiteSpace(FullName)
                && !string.IsNullOrWhiteSpace(DriverLicenseNumber)
                && !string.IsNullOrWhiteSpace(Email)
                && !string.IsNullOrWhiteSpace(Phone);
        }

        private void OnSaveProfile()
        {
            var customer = _session.CurrentCustomer;
            if (customer is null)
            {
                // No logged-in user → do nothing
                return;
            }

            // Otherwise, update that customer’s fields
            customer.FullName = FullName;
            customer.DriverLicenseNumber = DriverLicenseNumber;
            customer.Contact.Email = Email;
            customer.Contact.Phone = Phone;

            _ctx.Customers.Update(customer);
            _ctx.SaveChanges();
        }

        // ─────────────────────────────────────────────────────
        // 2) Saved Cards
        // ─────────────────────────────────────────────────────
        public ReadOnlyObservableCollection<Card> SavedCards => _savedCardsRO;

        private void LoadCards()
        {
            _savedCardsOC.Clear();

            var customer = _session.CurrentCustomer;
            if (customer is null) return;

            var cards = _ctx.Cards
                            .Where(c => c.CustomerId == customer.Id)
                            .ToList();
            foreach (var c in cards)
                _savedCardsOC.Add(c);
        }

        private void OnAddCard()
        {
            var customer = _session.CurrentCustomer;
            if (customer is null) return;

            var newCard = new Card
            {
                CustomerId = customer.Id,
                CardNumber = "0000111122223333",
                ExpiryMonth = 12,
                ExpiryYear = 2027,
                Cvv = "123",
                Nickname = "New Card"
            };

            _ctx.Cards.Add(newCard);
            _ctx.SaveChanges();
            _savedCardsOC.Add(newCard);
        }

        private void OnDeleteCard(Card? card)
        {
            if (card == null) return;

            _ctx.Cards.Remove(card);
            _ctx.SaveChanges();
            _savedCardsOC.Remove(card);
        }

        // ─────────────────────────────────────────────────────
        // 3) Logout
        // ─────────────────────────────────────────────────────
        private void OnLogout()
        {
            var customer = _session.CurrentCustomer;
            if (customer is null)
            {
                // No user → do nothing
                return;
            }

            // Clear “Remember Me” token in the database
            _auth.Logout(customer);

            // Broadcast that the user has logged out, so MainViewModel can navigate to LoginView
            _messenger.Send(new UserLoggedOutMessage());
        }
    }
}
