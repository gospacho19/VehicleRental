using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LuxuryCarRental.Data;
using LuxuryCarRental.Models;

namespace LuxuryCarRental.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly AppDbContext _ctx;
        private const int DemoCustomerId = 1;

        [ObservableProperty] private string _fullName = string.Empty;
        [ObservableProperty] private string _driverLicenseNumber = string.Empty;
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _phone = string.Empty;

        public IRelayCommand SaveCommand { get; }

        public ProfileViewModel(AppDbContext ctx)
        {
            _ctx = ctx;
            SaveCommand = new RelayCommand(OnSave);
            LoadCustomer();
        }

        private void LoadCustomer()
        {
            var customer = _ctx.Customers
                               .FirstOrDefault(c => c.Id == DemoCustomerId);
            if (customer != null)
            {
                FullName = customer.FullName;
                DriverLicenseNumber = customer.DriverLicenseNumber;
                Email = customer.Contact.Email;
                Phone = customer.Contact.Phone;
            }
        }

        private void OnSave()
        {
            var existing = _ctx.Customers.Find(DemoCustomerId);
            if (existing == null)
            {
                var c = new Customer
                {
                    Id = DemoCustomerId,
                    FullName = FullName,
                    DriverLicenseNumber = DriverLicenseNumber,
                    Contact = new ContactInfo
                    {
                        Email = Email,
                        Phone = Phone
                    }
                };
                _ctx.Customers.Add(c);
            }
            else
            {
                existing.FullName = FullName;
                existing.DriverLicenseNumber = DriverLicenseNumber;
                // Must reassign the entire Contact owned object:
                existing.Contact = new ContactInfo
                {
                    Email = Email,
                    Phone = Phone
                };
                _ctx.Customers.Update(existing);
            }
            _ctx.SaveChanges();
        }
    }
}
