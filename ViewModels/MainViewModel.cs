using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;

namespace LuxuryCarRental.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        // 1) Screen VMs, now including confirm:
        public CatalogViewModel CatalogVM { get; }
        public CategoryViewModel CategoryVM { get; }
        public CartViewModel CartVM { get; }
        public CheckoutViewModel CheckoutVM { get; }
        public ConfirmationViewModel ConfirmVM { get; }
        public DealsViewModel DealsVM { get; }
        public ProfileViewModel ProfileVM { get; }
        public PaymentInfoViewModel PaymentInfoVM { get; }


        // 2) The “current” VM shown in your ContentControl:
        private object _currentViewModel = null!;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        // 3) All of your navigation commands:
        public IRelayCommand ShowCatalogCmd { get; }
        public IRelayCommand ShowCategoryCmd { get; }
        public IRelayCommand ShowCartCmd { get; }
        public IRelayCommand ShowCheckoutCmd { get; }
        public IRelayCommand ShowDealsCmd { get; }
        public IRelayCommand ShowConfirmationCmd { get; }
        public IRelayCommand ShowProfileCmd { get; }
        public IRelayCommand ShowPaymentInfoCmd { get; }

        public MainViewModel(
            CatalogViewModel catalog,
            CategoryViewModel category,
            CartViewModel cart,
            CheckoutViewModel checkout,
            ConfirmationViewModel confirm,
            DealsViewModel deals,
            ProfileViewModel profile,
            PaymentInfoViewModel paymentInfo,
            IMessenger messenger)
        {
            // 4) Assign injected VMs:
            CatalogVM = catalog;
            CategoryVM = category;
            CartVM = cart;
            CheckoutVM = checkout;
            ConfirmVM = confirm;
            DealsVM = deals;
            ProfileVM = profile;
            PaymentInfoVM = paymentInfo;


            // Cart → Checkout
            messenger.Register<GoToCheckoutMessage>(this, (_, __) =>
                CurrentViewModel = CheckoutVM);

            // Checkout → Confirmation
            messenger.Register<GoToConfirmationMessage>(this, (r, msg) =>
            {
                ConfirmVM.Initialize(msg.Total, msg.Items, msg.PaymentCard);
                CurrentViewModel = ConfirmVM;
            });

            // Build your one-and-only Catalog command:
            ShowCatalogCmd = new RelayCommand(() =>
            {
                CatalogVM.RefreshCommand.Execute(null);
                CurrentViewModel = CatalogVM;
            });

            // 6) Build your commands:
            ShowCategoryCmd = new RelayCommand(() => CurrentViewModel = CategoryVM);
            ShowCartCmd = new RelayCommand(() =>
            {
                CartVM.RefreshCommand.Execute(null);    
                CurrentViewModel = CartVM;
            });
            ShowCheckoutCmd = new RelayCommand(() =>
            {
                // Before showing Checkout, reload cart items there
                CheckoutVM.LoadCartItems();      // call our new method to refetch current cart
                CurrentViewModel = CheckoutVM;
            });
            ShowDealsCmd = new RelayCommand(() => CurrentViewModel = DealsVM);
            ShowConfirmationCmd = new RelayCommand(() => CurrentViewModel = ConfirmVM);
            ShowProfileCmd = new RelayCommand(() => CurrentViewModel = ProfileVM);
            ShowPaymentInfoCmd = new RelayCommand(() => CurrentViewModel = PaymentInfoVM);


            // At the end of the ctor, *invoke* it so it runs on startup:
            ShowCatalogCmd.Execute(null);

        }
    }
}
