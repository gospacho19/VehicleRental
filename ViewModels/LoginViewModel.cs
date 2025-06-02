using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;
using LuxuryCarRental.Services.Interfaces;
using LuxuryCarRental.Services.Implementations; 

namespace LuxuryCarRental.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _auth;
        private readonly IMessenger _messenger;
        private readonly UserSessionService _session;  // store the session dependency

        public LoginViewModel(
            IAuthService auth,
            IMessenger messenger,
            UserSessionService session)   
        {
            _auth = auth;
            _messenger = messenger;
            _session = session;  // assign it

            LoginCommand = new RelayCommand(OnLogin, CanLogin);
            NavigateToRegisterCommand = new RelayCommand(OnNavigateToRegister);
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _username = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public IRelayCommand LoginCommand { get; }
        public IRelayCommand NavigateToRegisterCommand { get; }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Password);
        }

        private void OnLogin()
        {
            ErrorMessage = string.Empty;

            var customer = _auth.Login(Username, Password);
            if (customer == null)
            {
                ErrorMessage = "Invalid username or password";
                return;
            }

            // store in session
            _session.SetCurrentCustomer(customer);

            // “Remember Me” 
            if (RememberMe)
                _auth.SetRememberMe(customer, true);
            else
                _auth.SetRememberMe(customer, false);

            _messenger.Send(new LoginSuccessfulMessage(customer));
        }

        private void OnNavigateToRegister()
        {
            _messenger.Send(new GoToRegisterMessage());
        }
    }
}
