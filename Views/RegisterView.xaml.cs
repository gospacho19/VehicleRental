// LuxuryCarRental/Views/RegisterView.xaml.cs
using System.Windows;
using System.Windows.Controls;
using LuxuryCarRental.ViewModels;

namespace LuxuryCarRental.Views
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                vm.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}
