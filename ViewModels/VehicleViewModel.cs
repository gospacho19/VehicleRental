using LuxuryCarRental.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.ViewModels
{
    public class VehicleViewModel
    {
        private readonly Vehicle _model;

        public VehicleViewModel(Vehicle model)
        {
            _model = model;
        }

        public int Id => _model.Id;
        public string Name => _model.Name;
        public Money DailyRate => _model.DailyRate;
        public string ImagePath => _model.ImagePath;
        // add any other props you need, e.g. Type, Status, etc.
    }
}

