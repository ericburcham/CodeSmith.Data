using System;

namespace CodeSmith.Data.Caching
{
    public class AggregateCacheProvider : CacheProvider
    {
        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override object Get(string key, string group)
        {
            throw new NotImplementedException();
        }

        public override bool Remove(string key, string group)
        {
            throw new NotImplementedException();
        }

        public override void Set<T>(string key, T data, CacheSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
