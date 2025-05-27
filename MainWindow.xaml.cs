using System;
using System.Windows;
using System.Windows.Controls;
using LuxuryCarRental.ViewModels;
using LuxuryCarRental.Views;

namespace LuxuryCarRental
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _sp;

        // This is the only constructor. DI will call it.
        public MainWindow(IServiceProvider sp, MainViewModel vm)
        {
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));

            InitializeComponent();

            // Bind the shell ViewModel
            DataContext = vm ?? throw new ArgumentNullException(nameof(vm));

            // Wire up tab‐selection to load the correct UserControl
            // NavTabs.SelectionChanged += OnTabChanged;

            // Show first tab by default
            //NavTabs.SelectedIndex = 0;
        }
    }
}
