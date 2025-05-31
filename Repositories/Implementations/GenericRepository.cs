using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using LuxuryCarRental.Data;
using LuxuryCarRental.Repositories.Interfaces;

namespace LuxuryCarRental.Repositories.Implementations
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _ctx;
        private readonly DbSet<T> _db;

        public GenericRepository(AppDbContext ctx)
        {
            _ctx = ctx;
            _db = ctx.Set<T>();
        }
        
        public IEnumerable<T> GetAll() => _db.ToList();
        public T? GetById(int id) => _db.Find(id);
        public void Add(T entity) => _db.Add(entity);
        public void Update(T entity) => _db.Update(entity);
        public void Remove(T entity) => _db.Remove(entity);
    }
}