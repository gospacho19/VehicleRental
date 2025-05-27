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

        public MainViewModel(
            CatalogViewModel catalog,
            CategoryViewModel category,
            CartViewModel cart,
            CheckoutViewModel checkout,
            ConfirmationViewModel confirm,
            DealsViewModel deals,
            IMessenger messenger)
        {
            // 4) Assign injected VMs:
            CatalogVM = catalog;
            CategoryVM = category;
            CartVM = cart;
            CheckoutVM = checkout;
            ConfirmVM = confirm;
            DealsVM = deals;


            // Cart → Checkout
            messenger.Register<GoToCheckoutMessage>(this, (_, __) =>
                CurrentViewModel = CheckoutVM);

            // Checkout → Confirmation
            messenger.Register<GoToConfirmationMessage>(this, (r, msg) =>
            {
                ConfirmVM.Initialize(msg.Total, msg.Items, msg.PaymentCard);
                CurrentViewModel = ConfirmVM;
            });

            // 6) Build your commands:
            ShowCatalogCmd = new RelayCommand(() => CurrentViewModel = CatalogVM);
            ShowCategoryCmd = new RelayCommand(() => CurrentViewModel = CategoryVM);
            ShowCartCmd = new RelayCommand(() => CurrentViewModel = CartVM);
            ShowCheckoutCmd = new RelayCommand(() => CurrentViewModel = CheckoutVM);
            ShowDealsCmd = new RelayCommand(() => CurrentViewModel = DealsVM);
            ShowConfirmationCmd = new RelayCommand(() => CurrentViewModel = ConfirmVM);

            // 7) Start on catalog:
            CurrentViewModel = CatalogVM;
        }
    }
}
