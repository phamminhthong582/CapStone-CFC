using BusinessObject.Context;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Repository.Implement
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CustomFlowerChainContext _context;
        private readonly Dictionary<Type, object> _repositories = new();
        private bool disposed = false;

        public UnitOfWork(CustomFlowerChainContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }



        public IRepository<T> Repository<T>() where T : class
        {
            if (!_repositories.ContainsKey(typeof(T)))
            {
                var repository = new Repository<T>(_context);
                _repositories[typeof(T)] = repository;
            }
            return (IRepository<T>)_repositories[typeof(T)];
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
