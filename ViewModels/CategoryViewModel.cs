using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LuxuryCarRental.Models;
using LuxuryCarRental.Repositories.Interfaces;

namespace LuxuryCarRental.ViewModels
{
    public class CategoryViewModel : ObservableObject
    {
        private readonly IUnitOfWork _uow;

        public ObservableCollection<string> Categories { get; } = new();
        public ObservableCollection<Vehicle>    Vehicles       { get; } = new();

        // after — default to "All"
        private string _selectedCategory = "All";

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
            }
        }

        public IRelayCommand RefreshCommand { get; }

        public CategoryViewModel(IUnitOfWork uow)
        {
            _uow = uow;

            // Example categories; adapt to your data
            Categories.Add("All");
            Categories.Add("Luxury");
            Categories.Add("Sport");
            Categories.Add("SUV");
            SelectedCategory = "All";

            RefreshCommand = new RelayCommand(Refresh);
            Refresh();
        }

        private void Refresh()
        {
            Vehicles.Clear();
            foreach (var c in _uow.Vehicles.GetAll())
                Vehicles.Add(c);
        }

    }
}