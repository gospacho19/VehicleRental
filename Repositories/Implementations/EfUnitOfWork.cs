using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuxuryCarRental.Data;
using LuxuryCarRental.Models;
using LuxuryCarRental.Repositories.Interfaces;

namespace LuxuryCarRental.Repositories.Implementations
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _ctx;

        public IRepository<Vehicle> Vehicles { get; }
        public IRepository<Motorcycle> Motorcycles { get; }
        public IRepository<Yacht> Yachts { get; }
        public IRepository<Car> Cars { get; }
        public IRepository<LuxuryCar> LuxuryCars { get; }
        public IRepository<Customer> Customers { get; }
        public IRepository<Rental> Rentals { get; }
        public IRepository<Basket> Baskets { get; }
        public IRepository<CartItem> CartItems { get; }

        public EfUnitOfWork(AppDbContext ctx)
        {
            _ctx = ctx;
            Vehicles = new GenericRepository<Vehicle>(ctx);
            Motorcycles = new GenericRepository<Motorcycle>(ctx);
            Yachts = new GenericRepository<Yacht>(ctx);
            Cars = new GenericRepository<Car>(ctx);
            LuxuryCars = new GenericRepository<LuxuryCar>(ctx);
            Customers = new GenericRepository<Customer>(ctx);
            Rentals = new GenericRepository<Rental>(ctx);
            Baskets = new GenericRepository<Basket>(ctx);
            CartItems = new GenericRepository<CartItem>(ctx);
        }

        public void Commit() => _ctx.SaveChanges();
        public void Dispose() => _ctx.Dispose();
    }
}