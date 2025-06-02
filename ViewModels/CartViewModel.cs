using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;
using LuxuryCarRental.Repositories.Interfaces;
using LuxuryCarRental.Services.Implementations;
using LuxuryCarRental.Services.Interfaces;

namespace LuxuryCarRental.ViewModels
{
    public class CartViewModel : ObservableObject, IRecipient<CartUpdatedMessage>, IDisposable
    {
        // the list of CartItems 
        public ObservableCollection<CartItem> Items { get; } = new();

        private decimal _total;
        public decimal Total
        {
            get => _total;
            private set => SetProperty(ref _total, value);
        }

        private readonly ICartService _cart;
        private readonly IUnitOfWork _uow;
        private readonly IMessenger _messenger;
        private readonly UserSessionService _session;

        // commands
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand<CartItem?> RemoveCommand { get; }
        public IRelayCommand ClearCommand { get; }
        public IRelayCommand ProceedToCheckoutCommand { get; }

        public CartViewModel(
            ICartService cart,
            IUnitOfWork uow,
            IMessenger messenger,
            UserSessionService session)
        {
            _cart = cart;
            _uow = uow;
            _messenger = messenger;
            _session = session;

            RefreshCommand = new RelayCommand(Refresh);
            RemoveCommand = new RelayCommand<CartItem?>(Remove);
            ClearCommand = new RelayCommand(Clear);
            ProceedToCheckoutCommand = new RelayCommand(OnProceedToCheckout);

            // refresh
            _messenger.Register<CartUpdatedMessage>(this);

            Refresh();
        }

        // Reload the cart items and total for the current customer
        private void Refresh()
        {
            if (_session.CurrentCustomer == null)
            {
                Items.Clear();
                Total = 0m;
                return;
            }

            Items.Clear();

            var current = _session.CurrentCustomer!; 

            var cartItems = _cart.GetCartItems(current.Id);
            foreach (var item in cartItems)
                Items.Add(item);

            Total = _cart.GetCartTotal(current.Id).Amount;
        }

        // remove one CartItem 
        private void Remove(CartItem? item)
        {
            if (item == null) return;

            var current = _session.CurrentCustomer;
            if (current == null)
            {
                _messenger.Send(new GoToLoginMessage());
                return;
            }

            _cart.RemoveFromCart(current.Id, item.Id);

            Refresh();

            _messenger.Send(new CartUpdatedMessage(current.Id));
        }

        // clear the entire basket 
        private void Clear()
        {
            var current = _session.CurrentCustomer;
            if (current == null)
            {
                _messenger.Send(new GoToLoginMessage());
                return;
            }

            _cart.ClearCart(current.Id);

            Refresh();

            _messenger.Send(new CartUpdatedMessage(current.Id));
        }

        private void OnProceedToCheckout()
        {
            var current = _session.CurrentCustomer;
            if (current == null)
            {
                _messenger.Send(new GoToLoginMessage());
                return;
            }

            _messenger.Send(new GoToCheckoutMessage());
        }

        public void Receive(CartUpdatedMessage message)
        {
            var current = _session.CurrentCustomer;
            if (current is not null && message.CustomerId == current.Id)
            {
                Refresh();
            }
        }

        public void Dispose()
        {
            _messenger.UnregisterAll(this);
        }
    }
}
