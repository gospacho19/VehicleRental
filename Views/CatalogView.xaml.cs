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
        public CatalogView()
        {
            InitializeComponent();

            if (Application.Current is not App app)
                return;

            DataContext = app.Services.GetRequiredService<CatalogViewModel>();
        }

        public CatalogView(CatalogViewModel vm) : this()
        {
            DataContext = vm;
        }

        private void OnCarsScrollLeft(object sender, RoutedEventArgs e)
        {
            CarsScrollViewer.ScrollToHorizontalOffset(
                CarsScrollViewer.HorizontalOffset - 200);
        }
        private void OnCarsScrollRight(object sender, RoutedEventArgs e)
        {
            CarsScrollViewer.ScrollToHorizontalOffset(
                CarsScrollViewer.HorizontalOffset + 200);
        }

        private void OnLuxuryScrollLeft(object sender, RoutedEventArgs e)
        {
            LuxuryScrollViewer.ScrollToHorizontalOffset(
                LuxuryScrollViewer.HorizontalOffset - 200);
        }
        private void OnLuxuryScrollRight(object sender, RoutedEventArgs e)
        {
            LuxuryScrollViewer.ScrollToHorizontalOffset(
                LuxuryScrollViewer.HorizontalOffset + 200);
        }

        private void OnMotorcycleScrollLeft(object sender, RoutedEventArgs e)
        {
            MotorcycleScrollViewer.ScrollToHorizontalOffset(
                MotorcycleScrollViewer.HorizontalOffset - 200);
        }
        private void OnMotorcycleScrollRight(object sender, RoutedEventArgs e)
        {
            MotorcycleScrollViewer.ScrollToHorizontalOffset(
                MotorcycleScrollViewer.HorizontalOffset + 200);
        }

        private void OnYachtScrollLeft(object sender, RoutedEventArgs e)
        {
            YachtScrollViewer.ScrollToHorizontalOffset(
                YachtScrollViewer.HorizontalOffset - 200);
        }
        private void OnYachtScrollRight(object sender, RoutedEventArgs e)
        {
            YachtScrollViewer.ScrollToHorizontalOffset(
                YachtScrollViewer.HorizontalOffset + 200);
        }

        private void Carousel_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            ScrollViewer? parent = FindParentScrollViewer((DependencyObject)sender);
            if (parent != null)
            {
                parent.ScrollToVerticalOffset(parent.VerticalOffset - e.Delta);
            }
        }


        private ScrollViewer? FindParentScrollViewer(DependencyObject child)
        {
            DependencyObject current = child;
            while (current != null)
            {
                if (current is ScrollViewer sv)
                {
                    
                    if (sv.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
                        return sv;
                }
                current = LogicalTreeHelper.GetParent(current);
            }
            return null;
        }

    }
}
