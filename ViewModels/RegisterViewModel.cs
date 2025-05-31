// LuxuryCarRental/ViewModels/RegisterViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;
using LuxuryCarRental.Services.Implementations; // for UserSessionService
using System;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace LuxuryCarRental.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IAuthService _auth;
        private readonly IMessenger _messenger;
        private readonly UserSessionService _session;   // add UserSessionService

        public RegisterViewModel(
            IAuthService auth,
            IMessenger messenger,
            UserSessionService session)       // inject it here
        {
            _auth = auth;
            _messenger = messenger;
            _session = session;               // assign it

            RegisterCommand = new RelayCommand(OnRegister, CanRegister);
            CancelCommand = new RelayCommand(OnCancel);
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _username = string.Empty;

        // Bound by View’s code-behind from PasswordBox
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _password = string.Empty;

        // Bound by View’s code-behind from ConfirmPasswordBox
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _fullName = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _driverLicenseNumber = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _email = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _phone = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public IRelayCommand RegisterCommand { get; }
        public IRelayCommand CancelCommand { get; }

        private bool CanRegister()
        {
            // All fields must be non‐empty, and Password == ConfirmPassword
            if (string.IsNullOrWhiteSpace(Username))
            {
                Debug.WriteLine("Username is blank");
                return false;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                Debug.WriteLine("Password is blank");
                return false;
            }
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                Debug.WriteLine("ConfirmPassword is blank");
                return false;
            }
            if (Password != ConfirmPassword)
            {
                Debug.WriteLine("Passwords do not match");
                return false;
            }
            if (string.IsNullOrWhiteSpace(FullName))
            {
                Debug.WriteLine("FullName is blank");
                return false;
            }
            if (string.IsNullOrWhiteSpace(DriverLicenseNumber))
            {
                Debug.WriteLine("DriverLicenseNumber is blank");
                return false;
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                Debug.WriteLine("Email is blank");
                return false;
            }
            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                Debug.WriteLine("Email does not match pattern");
                return false;
            }
            if (string.IsNullOrWhiteSpace(Phone))
            {
                Debug.WriteLine("Phone is blank");
                return false;
            }

            return true;
        }

        private void OnRegister()
        {
            ErrorMessage = string.Empty;

            // Build ContactInfo
            var contact = new ContactInfo
            {
                Email = Email,
                Phone = Phone
            };

            try
            {
                // 1) Perform actual registration via the IAuthService
                //    Assume Register(...) returns a Customer object if successful
                var newCustomer = _auth.Register(
                    Username.Trim(),
                    Password,
                    FullName.Trim(),
                    DriverLicenseNumber.Trim(),
                    contact);

                if (newCustomer == null)
                {
                    ErrorMessage = "Registration failed";
                    return;
                }

                // 2) Immediately set this new customer into session
                _session.SetCurrentCustomer(newCustomer);

                // 3) Now that “bob” (for example) is in session, navigate directly to Catalog
                //    so that Catalog/CART/DEALS etc. will all use newCustomer.Id.
                _messenger.Send(new LoginSuccessfulMessage(newCustomer));
            }
            catch (Exception ex)
            {
                // If the underlying service throws (e.g. username taken), display that
                ErrorMessage = ex.Message;
            }
        }

        private void OnCancel()
        {
            // If the user cancels registration, go back to the Login screen
            _messenger.Send(new GoToLoginMessage());
        }
    }
}
