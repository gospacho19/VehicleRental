// ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Services.Interfaces;
using LuxuryCarRental.Repositories.Interfaces;
using LuxuryCarRental.Services.Implementations;

namespace LuxuryCarRental.ViewModels
{
    public class MainViewModel : ObservableObject,
                                 IRecipient<GoToVehicleDetailMessage>,
                                 IRecipient<GoToCategoryViewMessage>
    {
        // 1) Screen VMs (injected via DI)
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

        // *** 2) A single instance of VehicleDetailViewModel, also injected ***
        public VehicleDetailViewModel VehicleVM { get; }

        // 3) The “Current” VM shown in the content area:
        private object _currentViewModel = null!;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        // 4) Navigation commands (bound to your top‐bar buttons)
        public IRelayCommand ShowCatalogCmd { get; }
        public IRelayCommand ShowCategoryCmd { get; }
        public IRelayCommand ShowCartCmd { get; }
        public IRelayCommand ShowCheckoutCmd { get; }
        public IRelayCommand ShowDealsCmd { get; }
        public IRelayCommand ShowConfirmationCmd { get; }
        public IRelayCommand ShowProfileCmd { get; }
        public IRelayCommand ShowPaymentInfoCmd { get; }

        // 5) Store shared services so we can pass them into VehicleVM if needed
        private readonly IUnitOfWork _uow;
        private readonly ICartService _cartService;
        private readonly IMessenger _messenger;
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
            VehicleDetailViewModel vehicleVm,  // <-- inject the single instance here
            IMessenger messenger,
            IAuthService auth,
            IUnitOfWork uow,
            ICartService cartService,
            UserSessionService session)
        {
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

            VehicleVM = vehicleVm;   // <-- store the injected VehicleDetailViewModel

            _uow = uow;
            _cartService = cartService;
            _messenger = messenger;
            _session = session;

            // 6) Subscribe to the usual navigation messages
            messenger.Register<LoginSuccessfulMessage>(this, (r, msg) =>
            {
                CartVM.RefreshCommand.Execute(null);
                CheckoutVM.RefreshCommand.Execute(null);
                CatalogVM.RefreshCommand.Execute(null);
                CurrentViewModel = CatalogVM;
            });

            messenger.Register<GoToRegisterMessage>(this, (r, msg) =>
            {
                CurrentViewModel = RegisterVM;
            });

            messenger.Register<RegistrationSuccessfulMessage>(this, (r, msg) =>
            {
                LoginVM.Username = msg.Value;
                CurrentViewModel = LoginVM;
            });

            messenger.Register<GoToLoginMessage>(this, (_, __) =>
            {
                CurrentViewModel = LoginVM;
            });

            messenger.Register<GoToCheckoutMessage>(this, (_, __) =>
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

            messenger.Register<GoToConfirmationMessage>(this, (r, msg) =>
            {
                ConfirmVM.Initialize(msg.Total, msg.Items, msg.PaymentCard);
                CurrentViewModel = ConfirmVM;
            });

            messenger.Register<GoToProfileMessage>(this, (_, __) =>
            {
                CurrentViewModel = ProfileVM;
            });

            messenger.Register<GoToPaymentInfoMessage>(this, (_, __) =>
            {
                CurrentViewModel = PaymentInfoVM;
            });

            messenger.Register<GoToDealsMessage>(this, (r, msg) =>
            {
                if (_session.CurrentCustomer != null)
                {
                    DealsVM.RefreshCommand.Execute(null);
                    CurrentViewModel = DealsVM;
                }
                else
                {
                    CurrentViewModel = LoginVM;
                }
            });

            messenger.Register<GoToCartMessage>(this, (r, msg) => {
                // Refresh the CartViewModel if needed:
                CartVM.RefreshCommand.Execute(null);
                // Switch the content
                CurrentViewModel = CartVM;
            });


            // 7) Now register for “GoToVehicleDetail” and “GoToCategoryView”
            messenger.Register<GoToVehicleDetailMessage>(this);
            messenger.Register<GoToCategoryViewMessage>(this);

            // 8) Build the navigation commands (for your button row)
            ShowCatalogCmd = new RelayCommand(() =>
            {
                CatalogVM.RefreshCommand.Execute(null);
                CurrentViewModel = CatalogVM;
            });

            ShowCategoryCmd = new RelayCommand(() =>
            {
                CategoryVM.RefreshCommand.Execute(null);
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
                    DealsVM.RefreshCommand.Execute(null);
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

            // Show either Catalog (if “remember me”) or Login at startup
            var rememberedCustomer = auth.GetRememberedUser();
            if (rememberedCustomer != null)
            {
                session.SetCurrentCustomer(rememberedCustomer);

                CartVM.RefreshCommand.Execute(null);
                CheckoutVM.RefreshCommand.Execute(null);
                CatalogVM.RefreshCommand.Execute(null);

                CurrentViewModel = CatalogVM;
            }
            else
            {
                CurrentViewModel = LoginVM;
            }
        }

        // 9) When someone sends GoToVehicleDetailMessage, reuse the injected VehicleVM
        public void Receive(GoToVehicleDetailMessage message)
        {
            // Tell VehicleVM to load the requested vehicle ID
            VehicleVM.Load(message.VehicleId);

            // Then show that VM
            CurrentViewModel = VehicleVM;
        }

        // 10) When someone sends GoToCategoryViewMessage, switch back to CategoryVM
        public void Receive(GoToCategoryViewMessage message)
        {
            CategoryVM.RefreshCommand.Execute(null);
            CurrentViewModel = CategoryVM;
        }
    }
}
