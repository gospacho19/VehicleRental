
using System;
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
    public class VehicleDetailViewModel : ObservableObject, IRecipient<CartUpdatedMessage>, IDisposable
    {
        private readonly IUnitOfWork _uow;
        private readonly ICartService _cartService;
        private readonly IMessenger _messenger;
        private readonly UserSessionService _session;

        private Vehicle? _vehicle;
        public Vehicle? Vehicle
        {
            get => _vehicle;
            private set => SetProperty(ref _vehicle, value);
        }

        public IRelayCommand AddToCartCommand { get; }
        public IRelayCommand GoBackCommand { get; }

        public VehicleDetailViewModel(
            IUnitOfWork uow,
            ICartService cartService,
            IMessenger messenger,
            UserSessionService session)
        {
            _uow = uow;
            _cartService = cartService;
            _messenger = messenger;
            _session = session;

            AddToCartCommand = new RelayCommand(OnAddToCart, CanAddToCart);
            GoBackCommand = new RelayCommand(OnGoBack);

            _messenger.Register<CartUpdatedMessage>(this);
        }

        public void Load(int vehicleId)
        {
            // Fetch the vehicle 
            Vehicle = _uow.Vehicles.GetById(vehicleId);

            AddToCartCommand.NotifyCanExecuteChanged();
        }

        private bool CanAddToCart()
        {
            if (Vehicle == null)
                return false;

            // Only allow Add to Cart if the vehicle is Available
            return Vehicle.Status == VehicleStatus.Available;
        }

        private void OnAddToCart()
        {
            if (Vehicle == null) return;
            var current = _session.CurrentCustomer;
            if (current == null)
            {
                _messenger.Send(new GoToLoginMessage());
                return;
            }

            var period = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));
            _cartService.AddToCart(current.Id, Vehicle, period, Array.Empty<string>());

            Vehicle.Status = VehicleStatus.Rented;
            _uow.Commit();

            _messenger.Send(new CartUpdatedMessage(current.Id));
            AddToCartCommand.NotifyCanExecuteChanged();
        }

        private void OnGoBack()
        {
            _messenger.Send(new GoToCategoryViewMessage());
        }

        public void Receive(CartUpdatedMessage message)
        {
            AddToCartCommand.NotifyCanExecuteChanged();
        }

        public void Dispose()
        {
            _messenger.UnregisterAll(this);
        }
    }
}
