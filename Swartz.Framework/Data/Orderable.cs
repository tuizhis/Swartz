using System;
using System.Linq;
using System.Linq.Expressions;

namespace Swartz.Data
{
    public class Orderable<T>
    {
        public Orderable(IQueryable<T> enumerable)
        {
            Queryable = enumerable;
        }

        public IQueryable<T> Queryable { get; private set; }

        public Orderable<T> Asc<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Queryable = Queryable
                .OrderBy(keySelector);
            return this;
        }

        public Orderable<T> Asc<TKey1, TKey2>(Expression<Func<T, TKey1>> keySelector1,
            Expression<Func<T, TKey2>> keySelector2)
        {
            Queryable = Queryable
                .OrderBy(keySelector1)
                .ThenBy(keySelector2);
            return this;
        }

        public Orderable<T> Asc<TKey1, TKey2, TKey3>(Expression<Func<T, TKey1>> keySelector1,
            Expression<Func<T, TKey2>> keySelector2,
            Expression<Func<T, TKey3>> keySelector3)
        {
            Queryable = Queryable
                .OrderBy(keySelector1)
                .ThenBy(keySelector2)
                .ThenBy(keySelector3);
            return this;
        }

        public Orderable<T> Desc<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Queryable = Queryable
                .OrderByDescending(keySelector);
            return this;
        }

        public Orderable<T> Desc<TKey1, TKey2>(Expression<Func<T, TKey1>> keySelector1,
            Expression<Func<T, TKey2>> keySelector2)
        {
            Queryable = Queryable
                .OrderByDescending(keySelector1)
                .ThenByDescending(keySelector2);
            return this;
        }

        public Orderable<T> Desc<TKey1, TKey2, TKey3>(Expression<Func<T, TKey1>> keySelector1,
            Expression<Func<T, TKey2>> keySelector2,
            Expression<Func<T, TKey3>> keySelector3)
        {
            Queryable = Queryable
                .OrderByDescending(keySelector1)
                .ThenByDescending(keySelector2)
                .ThenByDescending(keySelector3);
            return this;
        }
    }
}