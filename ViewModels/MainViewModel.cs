// LuxuryCarRental/ViewModels/MainViewModel.cs
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Services.Implementations; // for UserSessionService

namespace LuxuryCarRental.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        // 1) Screen VMs
        public CatalogViewModel CatalogVM { get; }
        public CategoryViewModel CategoryVM { get; }
        public CartViewModel CartVM { get; }
        public CheckoutViewModel CheckoutVM { get; }
        public ConfirmationViewModel ConfirmVM { get; }
        public DealsViewModel DealsVM { get; }
        public ProfileViewModel ProfileVM { get; }
        public PaymentInfoViewModel PaymentInfoVM { get; }
        public LoginViewModel LoginVM { get; }
        public RegisterViewModel RegisterVM { get; }

        // 2) “Current” VM shown in the ContentControl:
        private object _currentViewModel = null!;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        // 3) Navigation commands (all must be initialized in ctor)
        public IRelayCommand ShowCatalogCmd { get; }
        public IRelayCommand ShowCategoryCmd { get; }
        public IRelayCommand ShowCartCmd { get; }
        public IRelayCommand ShowCheckoutCmd { get; }
        public IRelayCommand ShowDealsCmd { get; }
        public IRelayCommand ShowConfirmationCmd { get; }
        public IRelayCommand ShowProfileCmd { get; }
        public IRelayCommand ShowPaymentInfoCmd { get; }

        private readonly UserSessionService _session;

        public MainViewModel(
            CatalogViewModel catalog,
            CategoryViewModel category,
            CartViewModel cart,
            CheckoutViewModel checkout,
            ConfirmationViewModel confirm,
            DealsViewModel deals,
            ProfileViewModel profileVm,
            PaymentInfoViewModel paymentInfoVm,
            LoginViewModel loginVm,
            RegisterViewModel registerVm,
            IMessenger messenger,
            UserSessionService session)   // ← inject session here
        {
            // 4) Assign injected VMs:
            CatalogVM = catalog;
            CategoryVM = category;
            CartVM = cart;
            CheckoutVM = checkout;
            ConfirmVM = confirm;
            DealsVM = deals;
            ProfileVM = profileVm;
            PaymentInfoVM = paymentInfoVm;
            LoginVM = loginVm;
            RegisterVM = registerVm;
            _session = session;  // ← store session

            // 5) Subscribe to messages for navigation:

            // When login succeeds, show the Catalog (and refresh Cart & Checkout Views):
            messenger.Register<LoginSuccessfulMessage>(this, (r, msg) =>
            {
                // A) Refresh cart (so CartVM uses the new CurrentCustomer.Id)
                CartVM.RefreshCommand.Execute(null);

                // B) Refresh checkout (load saved cards + cart items)
                CheckoutVM.RefreshCommand.Execute(null);

                // C) Refresh catalog (mark rented vehicles appropriately)
                CatalogVM.RefreshCommand.Execute(null);

                // D) Show the Catalog screen
                CurrentViewModel = CatalogVM;
            });

            // From LoginView: “Register” button → go to RegisterView
            messenger.Register<GoToRegisterMessage>(this, (r, msg) =>
            {
                CurrentViewModel = RegisterVM;
            });

            // After a successful registration, return to LoginView (prefill username):
            messenger.Register<RegistrationSuccessfulMessage>(this, (r, msg) =>
            {
                LoginVM.Username = msg.Value;
                CurrentViewModel = LoginVM;
            });

            // From RegisterView: “Cancel” → go back to LoginView
            messenger.Register<GoToLoginMessage>(this, (r, msg) =>
            {
                CurrentViewModel = LoginVM;
            });

            // From anywhere: navigate to CheckoutView
            messenger.Register<GoToCheckoutMessage>(this, (_, __) =>
            {
                if (_session.CurrentCustomer != null)
                {
                    // First, tell the Checkout VM to reload its cart items (and saved cards)
                    CheckoutVM.RefreshCommand.Execute(null);
                    // Now show the Checkout screen
                    CurrentViewModel = CheckoutVM;
                }
                else
                {
                    // If no one is logged in, send them to login first
                    CurrentViewModel = LoginVM;
                }
            });

            // From CheckoutViewModel when payment is done → go to ConfirmationView
            messenger.Register<GoToConfirmationMessage>(this, (r, msg) =>
            {
                ConfirmVM.Initialize(msg.Total, msg.Items, msg.PaymentCard);
                CurrentViewModel = ConfirmVM;
            });

            // From anywhere: navigate to ProfileView
            messenger.Register<GoToProfileMessage>(this, (_, __) =>
            {
                CurrentViewModel = ProfileVM;
            });

            // From anywhere: navigate to PaymentInfoView
            messenger.Register<GoToPaymentInfoMessage>(this, (_, __) =>
            {
                CurrentViewModel = PaymentInfoVM;
            });

            // When someone says “GoToDeals”, we show the DealsVM
            messenger.Register<GoToDealsMessage>(this, (r, msg) =>
            {
                if (_session.CurrentCustomer != null)
                {
                    // Optionally refresh deals here if needed:
                    // DealsVM.RefreshCommand.Execute(null);
                    CurrentViewModel = DealsVM;
                }
                else
                {
                    CurrentViewModel = LoginVM;
                }
            });

            // 6) Build the navigation commands – all must be non-nullable:
            ShowCatalogCmd = new RelayCommand(() =>
            {
                CatalogVM.RefreshCommand.Execute(null);
                CurrentViewModel = CatalogVM;
            });
            ShowCategoryCmd = new RelayCommand(() =>
            {
                CurrentViewModel = CategoryVM;
            });
            ShowCartCmd = new RelayCommand(() =>
            {
                if (_session.CurrentCustomer != null)
                {
                    CartVM.RefreshCommand.Execute(null);
                    CurrentViewModel = CartVM;
                }
                else
                {
                    CurrentViewModel = LoginVM;
                }
            });
            ShowCheckoutCmd = new RelayCommand(() =>
            {
                if (_session.CurrentCustomer != null)
                {
                    CheckoutVM.RefreshCommand.Execute(null);
                    CurrentViewModel = CheckoutVM;
                }
                else
                {
                    CurrentViewModel = LoginVM;
                }
            });
            ShowDealsCmd = new RelayCommand(() =>
            {
                if (_session.CurrentCustomer != null)
                {
                    // Optionally: DealsVM.RefreshCommand.Execute(null);
                    CurrentViewModel = DealsVM;
                }
                else
                {
                    CurrentViewModel = LoginVM;
                }
            });
            ShowConfirmationCmd = new RelayCommand(() =>
            {
                CurrentViewModel = ConfirmVM;
            });
            ShowProfileCmd = new RelayCommand(() =>
            {
                CurrentViewModel = ProfileVM;
            });
            ShowPaymentInfoCmd = new RelayCommand(() =>
            {
                CurrentViewModel = PaymentInfoVM;
            });

            // 7) Finally, show the Login screen at startup:
            CurrentViewModel = LoginVM;
        }
    }
}
