using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LuxuryCarRental.Data;
using LuxuryCarRental.Models;

namespace LuxuryCarRental.ViewModels
{
    public partial class PaymentInfoViewModel : ObservableObject
    {
        private readonly AppDbContext _ctx;
        private const int DemoCustomerId = 1;

        public ObservableCollection<Card> SavedCards { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditCardCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCardCommand))]
        private Card? _selectedCard;

        public IRelayCommand AddCardCommand { get; }
        public IRelayCommand EditCardCommand { get; }
        public IRelayCommand DeleteCardCommand { get; }

        public PaymentInfoViewModel(AppDbContext ctx)
        {
            _ctx = ctx;

            AddCardCommand = new RelayCommand(OnAddCard);
            EditCardCommand = new RelayCommand(OnEditCard, CanEditOrDelete);
            DeleteCardCommand = new RelayCommand(OnDeleteCard, CanEditOrDelete);

            LoadSavedCards();
        }

        private void LoadSavedCards()
        {
            SavedCards.Clear();
            var cards = _ctx.Cards
                            .Where(c => c.CustomerId == DemoCustomerId)
                            .OrderBy(c => c.ExpiryYear)
                            .ThenBy(c => c.ExpiryMonth)
                            .ToList();
            foreach (var c in cards)
                SavedCards.Add(c);
        }

        private bool CanEditOrDelete() => SelectedCard != null;

        private void OnAddCard()
        {
            // A quick implementation: create a blank Card in DB, then refresh (in a real app, you'd open an "EditCardView")
            var newCard = new Card
            {
                CustomerId = DemoCustomerId,
                CardNumber = "",
                ExpiryMonth = 1,
                ExpiryYear = 2025,
                Cvv = "",
                Nickname = ""
            };

            _ctx.Cards.Add(newCard);
            _ctx.SaveChanges();

            LoadSavedCards();
            SelectedCard = SavedCards.FirstOrDefault(c => c.Id == newCard.Id);
            // In practice, you'd navigate now to an EditCardView to let the user fill in details.
        }

        private void OnEditCard()
        {
            if (SelectedCard == null) return;
            // Open a card‐editing dialog or view (e.g. CardEditViewModel). 
            // For brevity, we assume the user edits and saves there, then sends a message to reload.
        }

        private void OnDeleteCard()
        {
            if (SelectedCard == null) return;
            _ctx.Cards.Remove(SelectedCard);
            _ctx.SaveChanges();
            LoadSavedCards();
        }
    }
}
