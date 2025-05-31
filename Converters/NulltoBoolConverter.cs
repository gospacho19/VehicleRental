using System;
using System.Globalization;
using System.Windows.Data;


namespace LuxuryCarRental.Converters
{
    /// <summary>
    /// Returns true if the bound value is null, false otherwise.
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        // value will be the source (e.g. SelectedSavedCard)
        // If it is null, return true (enables manual entry fields).
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null;
        }

        // We do not need two‐way conversion for this scenario.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
