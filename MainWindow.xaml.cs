using System;
using System.Windows;
using System.Windows.Controls;
using LuxuryCarRental.ViewModels;
using LuxuryCarRental.Views;

namespace LuxuryCarRental
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();

            DataContext = vm ?? throw new ArgumentNullException(nameof(vm));
        }
    }

}
