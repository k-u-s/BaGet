using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore.Query;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace BaGet.Database.RavenDb
{
    public class AsyncRavenQueryProvider : IAsyncQueryProvider
    {
        private readonly IQueryProvider _provider;

        public AsyncRavenQueryProvider(IQueryProvider provider)
        {
            _provider = provider;
        }

        public IQueryable CreateQuery(Expression expression)
            => _provider.CreateQuery(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => _provider.CreateQuery<TElement>(expression);

        public object Execute(Expression expression)
            => _provider.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            => _provider.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression,
            CancellationToken cancellationToken = new CancellationToken())
            => _provider.Execute<TResult>(expression);
    }
}
