using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace LuxuryCarRental.Models
{
    public abstract class Vehicle
    {
        public int Id { get; set; }
        public virtual string Name { get; set; } = "";
        public required Money DailyRate { get; set; }
        // … any other shared props …
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
        public VehicleType VehicleType { get; set; }

        public string ImagePath { get; set; } = "";

        public bool CurrentlyAvailable { get; set; } = false;


        private ImageSource? _cachedImage;

        public ImageSource? ImageSource
        {
            get
            {
                if (_cachedImage != null)
                    return _cachedImage;

                if (string.IsNullOrWhiteSpace(ImagePath))
                    return null;

                try
                {
                    Uri uri;
                    if (ImagePath.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                    {
                        uri = new Uri(ImagePath, UriKind.Absolute);
                    }
                    else
                    {
                        var fullPath = Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            ImagePath.Replace('/', Path.DirectorySeparatorChar));
                        if (!File.Exists(fullPath))
                            return null;

                        uri = new Uri(fullPath, UriKind.Absolute);
                    }

                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = uri;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze(); // Freeze so it’s thread‐safe
                    _cachedImage = bmp;
                }
                catch
                {
                    _cachedImage = null;
                }

                return _cachedImage;
            }
        }

        /// <summary>
        /// Returns a display‐string combining the actual DB Status plus
        /// the computed flag CurrentlyAvailable.
        /// </summary>
        public string DisplayStatus
        {
            get
            {
                // If the car is physically “Rented” in the DB, say “Rented”:
                if (Status == VehicleStatus.Rented)
                    return "Rented";

                // If it's marked Available in the DB but our
                // availability check says “not available right now,” say “Unavailable”:
                if (!CurrentlyAvailable)
                    return "Unavailable";

                // Otherwise, it's truly free:
                return "Available";
            }
        }


    }
    // hi
    public enum VehicleType
    {
        Car,
        Motorcycle,
        Yacht,
        LuxuryCar
    }
}

