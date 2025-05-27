using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LuxuryCarRental.Models;
using LuxuryCarRental.Repositories.Interfaces;
using LuxuryCarRental.Handlers.Interfaces;

namespace LuxuryCarRental.ViewModels
{
    public class DealsViewModel : ObservableObject
    {
        private readonly IRentalHandler _rentalHandler;
        public ObservableCollection<Rental> Rentals { get; } = new();
        public IRelayCommand RefreshCommand { get; }

        public DealsViewModel(IRentalHandler rentalHandler)
        {
            _rentalHandler = rentalHandler;
            RefreshCommand = new RelayCommand(Refresh);
            Refresh();
        }

        private void Refresh()
        {
            Rentals.Clear();
            foreach (var r in _rentalHandler.GetAllDeals())
                Rentals.Add(r);
        }
    }

}
