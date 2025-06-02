using System.Windows.Controls;
using LuxuryCarRental.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LuxuryCarRental.Views
{
    public partial class PaymentInfoView : UserControl
    {
        public PaymentInfoView()
        {
            InitializeComponent();

            if (App.Current is not App app) return;
            DataContext = app.Services.GetRequiredService<PaymentInfoViewModel>();
        }
    }
}
