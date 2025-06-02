
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
        // Screen VMs 
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

        public VehicleDetailViewModel VehicleVM { get; }

        // The Current VM 
        private object _currentViewModel = null!;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        // Navigation commands 
        public IRelayCommand ShowCatalogCmd { get; }
        public IRelayCommand ShowCategoryCmd { get; }
        public IRelayCommand ShowCartCmd { get; }
        public IRelayCommand ShowCheckoutCmd { get; }
        public IRelayCommand ShowDealsCmd { get; }
        public IRelayCommand ShowConfirmationCmd { get; }
        public IRelayCommand ShowProfileCmd { get; }
        public IRelayCommand ShowPaymentInfoCmd { get; }

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
            VehicleDetailViewModel vehicleVm,  
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
            VehicleVM = vehicleVm;   

            _uow = uow;
            _cartService = cartService;
            _messenger = messenger;
            _session = session;

            // Subscribe to usual navigation messages
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
                if (_session.CurrentCustomer != null)
                {
                    CurrentViewModel = ProfileVM;
                }
                else
                {
                    CurrentViewModel = LoginVM;
                }
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
                CartVM.RefreshCommand.Execute(null);
                CurrentViewModel = CartVM;
            });


            messenger.Register<GoToVehicleDetailMessage>(this);
            messenger.Register<GoToCategoryViewMessage>(this);

            // Build the navigation commands 
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
                if (_session.CurrentCustomer != null)
                {
                    CurrentViewModel = ProfileVM;
                }
                else
                {
                    CurrentViewModel = LoginVM;
                }
            });

            ShowPaymentInfoCmd = new RelayCommand(() =>
            {
                CurrentViewModel = PaymentInfoVM;
            });

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

        public void Receive(GoToVehicleDetailMessage message)
        {
            VehicleVM.Load(message.VehicleId);

            CurrentViewModel = VehicleVM;
        }

        public void Receive(GoToCategoryViewMessage message)
        {
            CategoryVM.RefreshCommand.Execute(null);
            CurrentViewModel = CategoryVM;
        }
    }
}
