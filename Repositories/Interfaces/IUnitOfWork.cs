using LuxuryCarRental.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Vehicle> Vehicles { get; }
        IRepository<Car> Cars { get; }
        IRepository<LuxuryCar> LuxuryCars { get; }
        IRepository<Customer> Customers { get; }
        IRepository<Rental> Rentals { get; }
        IRepository<Basket> Baskets { get; }
        IRepository<CartItem> CartItems { get; }
        void Commit();
    }
}