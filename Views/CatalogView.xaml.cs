using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LuxuryCarRental.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LuxuryCarRental.Views
{
    public partial class CatalogView : UserControl
    {
        // 1) Parameterless ctor for XAML instantiation
        public CatalogView()
        {
            InitializeComponent();

            // wire up its VM from the DI container
            if (Application.Current is not App app)
                return;

            DataContext = app.Services.GetRequiredService<CatalogViewModel>();
        }

        // 2) (Optional) an overload if you want to support DI directly
        public CatalogView(CatalogViewModel vm) : this()
        {
            DataContext = vm;
        }
    }
}
