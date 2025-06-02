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
        // 1) The list of CartItems to display
        public ObservableCollection<CartItem> Items { get; } = new();

        // 2) The total cost (decimal)
        private decimal _total;
        public decimal Total
        {
            get => _total;
            private set => SetProperty(ref _total, value);
        }

        // 3) Dependencies
        private readonly ICartService _cart;
        private readonly IUnitOfWork _uow;
        private readonly IMessenger _messenger;
        private readonly UserSessionService _session;

        // 4) Commands
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

            // Whenever this user’s cart is updated elsewhere, we refresh
            _messenger.Register<CartUpdatedMessage>(this);

            // Don’t call Refresh() unconditionally here—Refresh() now guards for null session.
            // If you want an initial load, you can do:
            Refresh();
        }

        /// <summary>
        /// Reloads the cart items and total for the current customer.
        /// If no customer is logged in, do nothing (no exception).
        /// </summary>
        private void Refresh()
        {
            // If nobody is logged in yet, just clear and return:
            if (_session.CurrentCustomer == null)
            {
                Items.Clear();
                Total = 0m;
                return;
            }

            Items.Clear();

            var current = _session.CurrentCustomer!; // we know it’s not null here

            // 1) Load all CartItems for this user
            var cartItems = _cart.GetCartItems(current.Id);
            foreach (var item in cartItems)
                Items.Add(item);

            // 2) Calculate total via the CartService
            Total = _cart.GetCartTotal(current.Id).Amount;
        }

        /// <summary>
        /// Removes one CartItem (and sets its vehicle back to Available).
        /// Then notifies other screens that the cart changed.
        /// </summary>
        private void Remove(CartItem? item)
        {
            if (item == null) return;

            var current = _session.CurrentCustomer;
            if (current == null)
            {
                // If they somehow clicked “Remove” but aren’t logged in, send them to login
                _messenger.Send(new GoToLoginMessage());
                return;
            }

            // 1) Ask the CartService to remove it (which also sets vehicle.Status = Available & saves)
            _cart.RemoveFromCart(current.Id, item.Id);

            // 2) Refresh local list & total
            Refresh();

            // 3) Broadcast change: other screens will re‐load
            _messenger.Send(new CartUpdatedMessage(current.Id));
        }

        /// <summary>
        /// Clears the entire basket for this user (returns all vehicles to Available),
        /// then notifies everyone that the cart changed.
        /// </summary>
        private void Clear()
        {
            var current = _session.CurrentCustomer;
            if (current == null)
            {
                _messenger.Send(new GoToLoginMessage());
                return;
            }

            // 1) Clear the cart (sets each vehicle.Status = Available in DB)
            _cart.ClearCart(current.Id);

            // 2) Refresh local view
            Refresh();

            // 3) Broadcast change
            _messenger.Send(new CartUpdatedMessage(current.Id));
        }

        private void OnProceedToCheckout()
        {
            var current = _session.CurrentCustomer;
            if (current == null)
            {
                // Redirect them to Login instead of Checkout
                _messenger.Send(new GoToLoginMessage());
                return;
            }

            // Otherwise, it’s safe to go to Checkout:
            _messenger.Send(new GoToCheckoutMessage());
        }

        // IMessageHandler for CartUpdatedMessage
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
