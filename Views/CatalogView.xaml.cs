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

        // ───────── Carousel_PreviewMouseWheel ─────────
        // Whenever the mouse wheel is used while hovering over a horizontal carousel, 
        // redirect the delta to the outer (vertical) ScrollViewer.
        private void Carousel_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 1) Mark this event as handled by the inner ScrollViewer:
            e.Handled = true;

            // 2) Find the parent that is the outer vertical ScrollViewer:
            //    We know that the CatalogView has one top‐level ScrollViewer, so we climb
            //    the visual/logical tree until we find a ScrollViewer that can scroll vertically.
            var parent = FindParentScrollViewer((DependencyObject)sender);

            if (parent != null)
            {
                // 3) Scroll the parent up or down by the wheel delta.
                //    Mouse wheel delta is positive when scrolling up; negative when scrolling down.
                parent.ScrollToVerticalOffset(parent.VerticalOffset - e.Delta);
            }
        }

        // Utility: climb up until we find the outer ScrollViewer
        private ScrollViewer FindParentScrollViewer(DependencyObject child)
        {
            DependencyObject current = child;
            while (current != null)
            {
                if (current is ScrollViewer sv)
                {
                    // Ensure we are not returning the inner (horizontal) scrollviewer:
                    // the inner ones have VerticalScrollBarVisibility="Disabled".
                    if (sv.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
                        return sv;
                }
                current = LogicalTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
