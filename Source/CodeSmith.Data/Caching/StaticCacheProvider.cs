using System;
using System.Collections.Generic;
using System.Diagnostics;

using CodeSmith.Data.Collections;

namespace CodeSmith.Data.Caching
{
    public class StaticCacheProvider : CacheProvider
    {
        private static readonly ConcurrentDictionary<string, object> _cache =
            new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public static IDictionary<string, object> Cache => _cache;

        /// <summary>
        ///     Invalidates all cache items.
        /// </summary>
        public override void Clear()
        {
            _cache.Clear();
        }

        public override object Get(string key, string group)
        {
            var groupKey = GetGroupKey(key, group);
            object value;
            var result = _cache.TryGetValue(groupKey, out value);

#if DEBUG
            if (result && !groupKey.StartsWith(GroupVersionPrefix))
            {
                Debug.WriteLine("Cache Hit for key " + groupKey);
            }
#endif
            return value;
        }

        public override bool Remove(string key, string group)
        {
            var groupKey = GetGroupKey(key, group);
            object value;

            var result = _cache.TryRemove(groupKey, out value);

#if DEBUG
            if (result && !groupKey.StartsWith(GroupVersionPrefix))
            {
                Debug.WriteLine("Cache Remove for key " + groupKey);
            }
#endif
            return result;
        }

        public override void Set<T>(string key, T data, CacheSettings settings)
        {
            var groupKey = GetGroupKey(key, settings.Group);
            _cache[groupKey] = data;

#if DEBUG
            if (!groupKey.StartsWith(GroupVersionPrefix))
            {
                Debug.WriteLine("Cache Insert for key " + groupKey);
            }
#endif
        }
    }
}
