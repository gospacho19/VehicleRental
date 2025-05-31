// LuxuryCarRental/ViewModels/ConfirmationViewModel.cs
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Handlers.Interfaces;
using LuxuryCarRental.Messaging;
using LuxuryCarRental.Models;

namespace LuxuryCarRental.ViewModels
{
    public partial class ConfirmationViewModel : ObservableObject
    {
        private readonly ICheckoutHandler _checkout;
        private readonly IMessenger _messenger;

        // старый вариант — три отдельных свойства,
        // чтобы он работал с вашим XAML
        public Money Total { get; private set; } = default!;
        public IEnumerable<CartItem> Items { get; private set; } = default!;
        public Card PaymentCard { get; private set; } = default!;

        public IRelayCommand ConfirmCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ConfirmationViewModel(
            ICheckoutHandler checkout,
            IMessenger messenger)
        {
            _checkout = checkout;
            _messenger = messenger;

            ConfirmCommand = new RelayCommand(OnConfirm);
            CancelCommand = new RelayCommand(() =>
                               _messenger.Send(new GoToCatalogMessage()));
        }

        /// <summary>Вызывается MainViewModel-ом после создания VM.</summary>
        public void Initialize(Money total,
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
            // здесь можно передать period и paymentToken,
            // если они нужны вашему CheckoutHandler’у
            _checkout.Checkout(customerId: 1,
                               period: new DateRange(Items.First().StartDate,
                                                            Items.First().EndDate),
                               paymentToken: "auth:" + PaymentCard.Id);

            _messenger.Send(new GoToDealsMessage());
        }
    }
}
