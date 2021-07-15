using System;

namespace CodeSmith.Data.Caching
{
    public interface ICacheProvider
    {
        string Name { get; }

        void Clear();

        bool Exists(string key);

        bool Exists(string key, string group);

        object Get(string key);

        object Get(string key, string group);

        T Get<T>(string key);

        T Get<T>(string key, string group);

        string GetGroupKey(string key);

        string GetGroupKey(string key, string group);

        int GetGroupVersion();

        int GetGroupVersion(string group);

        T GetOrSet<T>(string key, T data);

        T GetOrSet<T>(string key, string group, T data);

        T GetOrSet<T>(string key, T data, CacheSettings settings);

        T GetOrSet<T>(string key, Func<string, T> valueFactory);

        T GetOrSet<T>(string key, string group, Func<string, T> valueFactory);

        T GetOrSet<T>(string key, Func<string, T> valueFactory, CacheSettings settings);

        void InvalidateGroup();

        void InvalidateGroup(string group);

        bool Remove(string key);

        bool Remove(string key, string group);

        void Set<T>(string key, T data);

        void Set<T>(string key, T data, int duration);

        void Set<T>(string key, T data, string profile);

        void Set<T>(string key, T data, CacheSettings settings);
    }
}
