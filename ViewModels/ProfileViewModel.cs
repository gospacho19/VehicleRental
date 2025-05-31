// LuxuryCarRental/ViewModels/ProfileViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Data;
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

        // ─────────────────────────────────────────────────────
        // 2) Commands
        // ─────────────────────────────────────────────────────
        public IRelayCommand SaveProfileCommand { get; }
        public IRelayCommand AddCardCommand { get; }
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

            // If a user is already logged in, prefill their info:
            if (_session.CurrentCustomer is not null)
            {
                var c = _session.CurrentCustomer!;
                _fullName = c.FullName;
                _driverLicenseNumber = c.DriverLicenseNumber;
                _email = c.Contact.Email;
                _phone = c.Contact.Phone;
            }

            SaveProfileCommand = new RelayCommand(OnSaveProfile, CanSaveProfile);
            AddCardCommand = new RelayCommand(OnAddCard);
            LogoutCommand = new RelayCommand(OnLogout);
        }

        private bool CanSaveProfile()
        {
            var customer = _session.CurrentCustomer;
            if (customer is null) return false;

            return !string.IsNullOrWhiteSpace(FullName)
                && !string.IsNullOrWhiteSpace(DriverLicenseNumber)
                && !string.IsNullOrWhiteSpace(Email)
                && !string.IsNullOrWhiteSpace(Phone);
        }

        private void OnSaveProfile()
        {
            var customer = _session.CurrentCustomer;
            if (customer is null) return;

            // Update the Customer’s details
            customer.FullName = FullName;
            customer.DriverLicenseNumber = DriverLicenseNumber;
            customer.Contact.Email = Email;
            customer.Contact.Phone = Phone;

            _ctx.Customers.Update(customer);
            _ctx.SaveChanges();
        }

        private void OnAddCard()
        {
            // Simply navigate to the Payment Info screen:
            _messenger.Send(new GoToPaymentInfoMessage());
        }

        private void OnLogout()
        {
            var customer = _session.CurrentCustomer;
            if (customer is null) return;

            // Clear “Remember Me” / token in the database if needed
            _auth.Logout(customer);

            // Clear session
            _session.ClearCurrentCustomer();

            // Tell the app to go back to the Login screen
            _messenger.Send(new GoToLoginMessage());
        }
    }
}
