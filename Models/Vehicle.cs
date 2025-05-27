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


        public ImageSource? ImageSource
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ImagePath))
                    return null;

                // normalize & build full path...
                var fullPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    ImagePath.Replace('/', Path.DirectorySeparatorChar)
                );

                if (!File.Exists(fullPath))
                    return null;

                return new BitmapImage(new Uri(fullPath, UriKind.Absolute));
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

