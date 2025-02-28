﻿using System.Collections.Generic;
using System.Linq;

using CodeSmith.Data.Caching;
using CodeSmith.Data.Future;

namespace CodeSmith.Data.Linq
{
    public static class FutureExtensions
    {
        public static IEnumerable<T> Future<T>(this IQueryable<T> query)
        {
            return query.Future(null);
        }

        public static IFutureValue<int> FutureCount<T>(this IQueryable<T> query)
        {
            var db = query.GetFutureConext();

            return db == null
                ? null
                : db.FutureCount(query, null);
        }

        public static IFutureValue<T> FutureFirstOrDefault<T>(this IQueryable<T> query)
        {
            return query.FutureFirstOrDefault(null);
        }

        internal static IEnumerable<T> Future<T>(this IQueryable<T> query, CacheSettings cacheSettings)
        {
            var db = query.GetFutureConext();

            return db == null
                ? null
                : db.Future(query, cacheSettings);
        }

        internal static IFutureValue<T> FutureFirstOrDefault<T>(this IQueryable<T> query, CacheSettings cacheSettings)
        {
            var db = query.GetFutureConext();

            return db == null
                ? null
                : db.FutureFirstOrDefault(query, cacheSettings);
        }
    }
}
