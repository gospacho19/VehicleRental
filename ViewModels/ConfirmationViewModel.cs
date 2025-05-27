using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;

namespace LuxuryCarRental.ViewModels
{
    public class ConfirmationViewModel : ObservableObject
    {
        private readonly IMessenger _messenger;

        // 1) A single Total property, typed as Money
        public Money Total { get; private set; } = default!;

        // 2) A single Items property
        public IEnumerable<CartItem> Items { get; private set; } = Enumerable.Empty<CartItem>();

        // 3) The card the user entered
        public Card PaymentCard { get; private set; } = default!;

        // 4) Your Confirm button
        public IRelayCommand ConfirmCommand { get; }

        public ConfirmationViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            ConfirmCommand = new RelayCommand(OnConfirm);
        }

        // 5) One Initialize() that sets all three
        public void Initialize(
            Money total,
            IEnumerable<CartItem> items,
            Card paymentCard)
        {
            Total = total;
            Items = items;
            PaymentCard = paymentCard;

            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(PaymentCard));
        }

        private void OnConfirm()
        {
            _messenger.Send(new GoToThankYouMessage());
        }
    }
}
