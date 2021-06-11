using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BaGet.Database.RavenDb
{
    public class AsyncRavenQuery<T> : IQueryable<T>
    {
        private readonly IQueryable<T> _query;
        private readonly AsyncRavenQueryProvider _queryProvider;

        public Type ElementType => _query.ElementType;
        public Expression Expression => _query.Expression;
        public IQueryProvider Provider  => _queryProvider;

        public AsyncRavenQuery(IQueryable<T> query)
        {
            _query = query;
            _queryProvider = new AsyncRavenQueryProvider(query.Provider);
        }

        public IEnumerator<T> GetEnumerator()
            => _query.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
