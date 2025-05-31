using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LuxuryCarRental.Models;
using LuxuryCarRental.Repositories.Interfaces;
using LuxuryCarRental.Services.Interfaces;
using LuxuryCarRental.Services.Implementations;
using LuxuryCarRental.Handlers.Interfaces;
using LuxuryCarRental.Handlers.Implementations;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Messaging;

namespace LuxuryCarRental.ViewModels
{
    public class CartViewModel : ObservableObject
    {
        public ObservableCollection<CartItem> Items { get; } = new();
        public decimal Total { get; private set; }

        private readonly ICartService _cart;
        private readonly ICheckoutHandler _checkoutHandler;

        private readonly IMessenger _messenger;
        public CartViewModel(
            ICartService cart,
            ICheckoutHandler checkoutHandler,
            IMessenger messenger)
        {
            _cart = cart;
            _checkoutHandler = checkoutHandler;
            _messenger = messenger;

            RefreshCommand = new RelayCommand(Refresh);
            RemoveCommand = new RelayCommand<CartItem?>(Remove);
            ClearCommand = new RelayCommand(Clear);

            NavigateToCheckoutCommand = new RelayCommand(() =>
            {
                // Let MainViewModel know to show CheckoutView
                _messenger.Send(new GoToCheckoutMessage());
            });


            Refresh();
        }

        public IRelayCommand NavigateToCheckoutCommand { get; }

        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand<CartItem?> RemoveCommand { get; }
        public IRelayCommand ClearCommand { get; }

        private void Refresh()
        {
            Items.Clear();
            foreach (var item in _cart.GetCartItems(1))
                Items.Add(item);

            Total = _cart.GetCartTotal(1).Amount;
            OnPropertyChanged(nameof(Total));
        }

        private void Remove(CartItem? item)
        {
            if (item == null) return;
            _cart.RemoveFromCart(1, item.Id);
            Refresh();
        }

        private void Clear()
        {
            _cart.ClearCart(1);
            Refresh();
        }

    }

}