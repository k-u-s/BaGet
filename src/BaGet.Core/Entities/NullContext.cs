using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Core
{
    public class NullContext : IContext
    {
        public bool SupportsLimitInSubqueries { get; }
        public IQueryable<Package> PackagesQueryable { get; }
        public IQueryable<Package> PackagesIncludedQueryable { get; }
        public bool IsUniqueConstraintViolationException(Exception exception)
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RunMigrationsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RunCreateDatabaseAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(Package package)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(Package package)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountPackagesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<Package>> GetBatchAsync(int batch, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync(IQueryable<Package> query, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Package> FirstOrDefaultAsync(IQueryable<Package> query, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
