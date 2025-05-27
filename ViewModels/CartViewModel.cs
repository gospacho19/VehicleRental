using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LuxuryCarRental.Models;
using LuxuryCarRental.Repositories.Interfaces;
using LuxuryCarRental.Services.Interfaces;
using LuxuryCarRental.Services.Implementations;
using LuxuryCarRental.Handlers.Interfaces;
using LuxuryCarRental.Handlers.Implementations;

namespace LuxuryCarRental.ViewModels
{
    public class CartViewModel : ObservableObject
    {
        public ObservableCollection<CartItem> Items { get; } = new();
        public decimal Total { get; private set; }

        private readonly ICartService _cart;
        private readonly ICheckoutHandler _checkoutHandler;

        public CartViewModel(
            ICartService cart,
            ICheckoutHandler checkoutHandler)
        {
            _cart = cart;
            _checkoutHandler = checkoutHandler;

            RefreshCommand = new RelayCommand(Refresh);
            RemoveCommand = new RelayCommand<CartItem?>(Remove);
            ClearCommand = new RelayCommand(Clear);
            CheckoutCommand = new RelayCommand(Checkout);

            Refresh();
        }

        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand<CartItem?> RemoveCommand { get; }
        public IRelayCommand ClearCommand { get; }
        public IRelayCommand CheckoutCommand { get; }

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

        private void Checkout()
        {
            // returns new rentals if you need them
            var rentals = _checkoutHandler.Checkout(1, /*paymentToken*/"demo");
            Refresh();
        }
    }

}