using System.Windows;
using System.Windows.Controls;
using LuxuryCarRental.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LuxuryCarRental.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            if (Application.Current is not App app) return;
            DataContext = app.Services.GetRequiredService<LoginViewModel>();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }
    }
}
