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
    public class AsyncRavenQueryProvider<T> : IQueryable<T>, IAsyncQueryProvider
    {
        private readonly IQueryable<T> _query;

        public Type ElementType => _query.ElementType;
        public Expression Expression => _query.Expression;
        public IQueryProvider Provider  => _query.Provider;

        public AsyncRavenQueryProvider(IQueryable<T> query)
        {
            _query = query;
        }

        public IEnumerator<T> GetEnumerator()
            => _query.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IQueryable CreateQuery(Expression expression)
            => Provider.CreateQuery(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => Provider.CreateQuery<TElement>(expression);

        public object Execute(Expression expression)
            => Provider.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            => Provider.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression,
            CancellationToken cancellationToken = new CancellationToken())
            => Provider.Execute<TResult>(expression);
    }
}
