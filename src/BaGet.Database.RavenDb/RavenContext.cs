using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Core;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace BaGet.Database.RavenDb
{
    public class RavenContext : IContext
    {
        private readonly IAsyncDocumentSession _session;

        public bool SupportsLimitInSubqueries => false;

        public IQueryable<Package> PackagesQueryable => _session.Query<Package>();
        public IQueryable<Package> PackagesIncludedQueryable => PackagesQueryable;

        public RavenContext(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public bool IsUniqueConstraintViolationException(Exception exception)
            => false;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _session.SaveChangesAsync(cancellationToken);
            return 0; // TODO: check if it is used anywhere
        }

        public Task RunMigrationsAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task RunCreateDatabaseAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public async Task AddAsync(Package package)
        {
            await _session.StoreAsync(package, $"{package.Identifier}/{package.NormalizedVersionString}");
            package.Id = _session.Advanced.GetDocumentId(package);
        }

        public Task RemoveAsync(Package package)
        {
            _session.Delete(package);
            return Task.CompletedTask;
        }

        public Task<int> CountPackagesAsync(CancellationToken cancellationToken)
            => PackagesQueryable.CountAsync(cancellationToken);

        public Task<List<Package>> GetBatchAsync(int batch, CancellationToken cancellationToken)
            => PackagesQueryable
                .OrderBy(p => p.Key)
                .Skip(batch * DownloadsImporter.BatchSize)
                .Take(DownloadsImporter.BatchSize)
                .ToListAsync(cancellationToken);

        public Task<bool> AnyAsync(IQueryable<Package> query, CancellationToken cancellationToken)
            => query.AnyAsync(cancellationToken);

        public Task<Package> FirstOrDefaultAsync(IQueryable<Package> query, CancellationToken cancellationToken)
            => query.FirstOrDefaultAsync(cancellationToken);

        public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken)
            => query.ToListAsync(cancellationToken);
    }
}
