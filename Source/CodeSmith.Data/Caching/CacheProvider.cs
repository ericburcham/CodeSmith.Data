using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Provider;
using System.Text.RegularExpressions;

using CodeSmith.Data.Linq;

namespace CodeSmith.Data.Caching
{
    public abstract class CacheProvider : ProviderBase, ICacheProvider
    {
        public const string GroupVersionPrefix = "g_";

        public const string GroupKeyPrefix = "p_";

        private static readonly Random _random = new Random();

        public abstract void Clear();

        public bool Exists(string key)
        {
            return Exists(key, CacheManager.DefaultGroup);
        }

        public virtual bool Exists(string key, string group)
        {
            return Get(key, group) != null;
        }

        public virtual object Get(string key)
        {
            return Get(key, CacheManager.DefaultGroup);
        }

        public abstract object Get(string key, string group);

        public virtual T Get<T>(string key)
        {
            return Get<T>(key, CacheManager.DefaultGroup);
        }

        public virtual T Get<T>(string key, string group)
        {
            var data = Get(key, group);

            return Convert<T>(data);
        }

        public string GetGroupKey(string key)
        {
            return GetGroupKey(key, CacheManager.DefaultGroup);
        }

        public virtual string GetGroupKey(string key, string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                return key;
            }

            var groupVersion = GetGroupVersion(group);
            var cleanName = Regex.Replace(group, @"\W+", "");

            return string.Format("{0}{1}_{2}_{3}", GroupKeyPrefix, cleanName, groupVersion, key);
        }

        public int GetGroupVersion()
        {
            return GetGroupVersion(CacheManager.DefaultGroup);
        }

        public virtual int GetGroupVersion(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                return 0;
            }

            var key = GetGroupVersionKey(group);

            var version = Get<int>(key, null);
            if (version != 0)
            {
                return version;
            }

            version = _random.Next(1, 10000);
            Set(key, version, new CacheSettings().WithGroup(null));

            return version;
        }

        public T GetOrSet<T>(string key, T data)
        {
            return GetOrSet(key, CacheManager.DefaultGroup, data);
        }

        public T GetOrSet<T>(string key, string group, T data)
        {
            return GetOrSet(key, data, CacheManager.GetProfile().WithGroup(group));
        }

        public virtual T GetOrSet<T>(string key, T data, CacheSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            var d = Get(key, settings.Group);
            if (d != null)
            {
                return Convert<T>(d);
            }

            Set(key, data, settings);

            return data;
        }

        public T GetOrSet<T>(string key, Func<string, T> valueFactory)
        {
            return GetOrSet(key, CacheManager.DefaultGroup, valueFactory);
        }

        public T GetOrSet<T>(string key, string group, Func<string, T> valueFactory)
        {
            return GetOrSet(key, valueFactory, CacheManager.GetProfile().WithGroup(group));
        }

        public virtual T GetOrSet<T>(string key, Func<string, T> valueFactory, CacheSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            var d = Get(key, settings.Group);
            if (d != null)
            {
                return Convert<T>(d);
            }

            var data = valueFactory.Invoke(key);
            Set(key, data, settings);

            return data;
        }

        public void InvalidateGroup()
        {
            InvalidateGroup(CacheManager.DefaultGroup);
        }

        public virtual void InvalidateGroup(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                return;
            }

            var key = GetGroupVersionKey(group);
            var value = Get<int>(key, null);

            if (value == 0)
            {
                return;
            }

            value++;
            Set(key, value, new CacheSettings().WithGroup(null));
        }

        public virtual bool Remove(string key)
        {
            return Remove(key, CacheManager.DefaultGroup);
        }

        public abstract bool Remove(string key, string group);

        public virtual void Set<T>(string key, T data)
        {
            Set(key, data, CacheManager.GetProfile());
        }

        public void Set<T>(string key, T data, int duration)
        {
            Set(key, data, new CacheSettings(duration));
        }

        public void Set<T>(string key, T data, string profile)
        {
            Set(key, data, CacheManager.GetProfile(profile));
        }

        public abstract void Set<T>(string key, T data, CacheSettings settings);

        private static T Convert<T>(object data)
        {
            if (data == null)
            {
                return default;
            }

            var dataType = data.GetType();
            var valueType = typeof(T);

            if (valueType == dataType || valueType.IsAssignableFrom(dataType))
            {
                return (T)data;
            }

            var converter = TypeDescriptor.GetConverter(valueType);
            if (converter.CanConvertFrom(dataType))
            {
                return (T)converter.ConvertFrom(data);
            }

            if (dataType != typeof(byte[]))
            {
                return default;
            }

            if (valueType.IsSubclassOf(typeof(IEnumerable<T>)))
            {
                try
                {
                    var converted = (T)((byte[])data).ToCollection<T>();

                    return converted;
                }
                catch
                {
                }
            }
            else
            {
                try
                {
                    var converted = ((byte[])data).ToObject<T>();

                    return converted;
                }
                catch
                {
                }
            }

            return default;
        }

        private static string GetGroupVersionKey(string group)
        {
            var cleanName = Regex.Replace(group, @"\W+", "");

            return GroupVersionPrefix + cleanName;
        }
    }
}
