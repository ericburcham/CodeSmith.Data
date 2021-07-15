using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;

using CodeSmith.Data.Collections;

namespace CodeSmith.Data.Caching
{
    /// <summary>
    ///     A class to manage cached items via a provider.
    /// </summary>
    /// <example>
    ///     The following example gets an item to the default cache provider.
    ///     <code><![CDATA[
    /// CacheManager.Set("key", "some cached data");
    /// var data = CacheManager.Get<string>("key");
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    ///     The following example uses CacheManager to expire a cache group.
    ///     <code><![CDATA[
    /// var db = new TrackerDataContext { Log = Console.Out };
    /// // get a CacheSettings instance using the default profile with a group of 'Role'.
    /// var cache = CacheManager.GetProfile().WithGroup("Role");
    /// 
    /// // queries that can be cached
    /// var roles = db.Role
    ///     .Where(r => r.Name == "Test Role")
    ///     .FromCache(cache);
    /// var role = db.Role
    ///     .ByName("Duck Roll")
    ///     .FromCacheFirstOrDefault(cache);
    /// 
    /// // after you make some update, expire group using InvalidateGroup
    /// CacheManager.GetProvider().InvalidateGroup("Role");
    /// ]]>
    /// </code>
    /// </example>
    public class CacheManager
    {
        private static readonly ConcurrentDictionary<string, ICacheProvider> _providers;

        private static readonly ConcurrentDictionary<string, CacheSettings> _profiles;

        private static CacheSettings _defaultProfile;

        static CacheManager()
        {
            _providers = new ConcurrentDictionary<string, ICacheProvider>(StringComparer.OrdinalIgnoreCase);
            _profiles = new ConcurrentDictionary<string, CacheSettings>(StringComparer.OrdinalIgnoreCase);

            RegisterProvider<MemoryCacheProvider>(true);
            RegisterProvider<StaticCacheProvider>();
            _defaultProfile = new CacheSettings();

            Initialize();
        }

        public static string DefaultGroup { get; set; }

        public static ICacheProvider DefaultProvider { get; private set; }

        public static void Clear()
        {
            foreach (var provider in _providers.Values)
            {
                provider.Clear();
            }
        }

        public static void Clear<T>()
        {
            ICacheProvider provider;
            if (_providers.TryGetValue(typeof(T).Name, out provider))
            {
                provider.Clear();
            }
        }

        public static bool Exists(string key)
        {
            return DefaultProvider.Exists(key);
        }

        public static bool Exists(string key, string group)
        {
            return DefaultProvider.Exists(key, group);
        }

        public static object Get(string key)
        {
            return DefaultProvider.Get(key);
        }

        public static T Get<T>(string key)
        {
            return DefaultProvider.Get<T>(key);
        }

        public static T Get<T>(string key, string groupName)
        {
            return DefaultProvider.Get<T>(key, groupName);
        }

        public static T GetOrSet<T>(string key, T data)
        {
            return DefaultProvider.GetOrSet(key, data);
        }

        public static T GetOrSet<T>(string key, string group, T data)
        {
            return DefaultProvider.GetOrSet(key, group, data);
        }

        public static T GetOrSet<T>(string key, T data, CacheSettings settings)
        {
            return DefaultProvider.GetOrSet(key, data, settings);
        }

        public static T GetOrSet<T>(string key, Func<string, T> valueFactory)
        {
            return DefaultProvider.GetOrSet(key, valueFactory);
        }

        public static T GetOrSet<T>(string key, string group, Func<string, T> valueFactory)
        {
            return DefaultProvider.GetOrSet(key, group, valueFactory);
        }

        public static T GetOrSet<T>(string key, Func<string, T> valueFactory, CacheSettings settings)
        {
            return DefaultProvider.GetOrSet(key, valueFactory, settings);
        }

        public static CacheSettings GetProfile()
        {
            return GetProfile(null);
        }

        public static CacheSettings GetProfile(string profileName)
        {
            if (string.IsNullOrEmpty(profileName))
            {
                return _defaultProfile.Clone();
            }

            CacheSettings cacheSettings;
            _profiles.TryGetValue(profileName, out cacheSettings);

            if (cacheSettings == null)
            {
                cacheSettings = _defaultProfile;
            }

            return cacheSettings == null
                ? new CacheSettings()
                : cacheSettings.Clone();
        }

        public static ICacheProvider GetProvider()
        {
            return GetProvider(null);
        }

        public static ICacheProvider GetProvider(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                return DefaultProvider;
            }

            ICacheProvider provider;
            if (!_providers.TryGetValue(providerName, out provider))
            {
                throw new ArgumentException(string.Format("Unable to locate cache provider '{0}'.", providerName), "providerName");
            }

            return provider;
        }

        public static ICacheProvider GetProvider<T>()
            where T : ICacheProvider
        {
            return GetProvider(typeof(T).Name);
        }

        public static void InvalidateGroup()
        {
            DefaultProvider.InvalidateGroup(DefaultGroup);
        }

        public static void InvalidateGroup(string groupName)
        {
            DefaultProvider.InvalidateGroup(groupName);
        }

        public static void InvalidateGroup<T>(string groupName)
            where T : ICacheProvider
        {
            ICacheProvider provider;
            if (_providers.TryGetValue(typeof(T).Name, out provider))
            {
                provider.InvalidateGroup(groupName);
            }
        }

        public static void InvalidateGroups(IEnumerable<string> groupNames)
        {
            foreach (var groupName in groupNames)
            {
                DefaultProvider.InvalidateGroup(groupName);
            }
        }

        public static void InvalidateGroups(params string[] groupNames)
        {
            InvalidateGroups(groupNames.AsEnumerable());
        }

        public static void InvalidateGroups<T>(IEnumerable<string> groupNames)
        {
            ICacheProvider provider;
            if (_providers.TryGetValue(typeof(T).Name, out provider))
            {
                foreach (var groupName in groupNames)
                {
                    provider.InvalidateGroup(groupName);
                }
            }
        }

        public static void InvalidateGroups<T>(params string[] groupNames)
        {
            InvalidateGroups<T>(groupNames.AsEnumerable());
        }

        public static ICacheProvider RegisterProvider<T>(string providerName, bool defaultProvider)
            where T : ICacheProvider, new()
        {
            return RegisterProvider(providerName, defaultProvider, k => new T());
        }

        public static ICacheProvider RegisterProvider(
            string providerName,
            bool defaultProvider,
            Func<string, ICacheProvider> createFactory)
        {
            var provider = _providers.GetOrAdd(providerName, createFactory);
            if (defaultProvider)
            {
                DefaultProvider = provider;
            }

            return provider;
        }

        public static ICacheProvider RegisterProvider<T>()
            where T : ICacheProvider, new()
        {
            return RegisterProvider<T>(false);
        }

        public static ICacheProvider RegisterProvider<T>(bool defaultProvider)
            where T : ICacheProvider, new()
        {
            return RegisterProvider<T>(typeof(T).Name, defaultProvider);
        }

        public static bool Remove(string key)
        {
            return DefaultProvider.Remove(key);
        }

        public static bool Remove(string key, string groupName)
        {
            return DefaultProvider.Remove(key, groupName);
        }

        public static void Set<T>(string key, T data)
        {
            DefaultProvider.Set(key, data);
        }

        public static void Set<T>(string key, T data, int duration)
        {
            DefaultProvider.Set(key, data, CacheSettings.FromDuration(duration));
        }

        public static void Set<T>(string key, T data, string profile)
        {
            var settings = GetProfile(profile);
            GetProvider(settings.Provider).Set(key, data, settings);
        }

        public static void Set<T>(string key, T data, CacheSettings settings)
        {
            GetProvider(settings.Provider).Set(key, data, settings);
        }

        private static void Initialize()
        {
            var cacheSection = ConfigurationManager.GetSection("cacheManager") as CacheManagerSection;
            if (cacheSection == null)
            {
                return;
            }

            // load providers
            if (cacheSection.Providers.Count > 0)
            {
                var cacheProviders = new CacheProviderCollection();
                ProvidersHelper.InstantiateProviders(cacheSection.Providers, cacheProviders, typeof(ICacheProvider));
                foreach (ICacheProvider provider in cacheProviders)
                {
                    _providers.TryAdd(provider.Name, provider);
                }

                ICacheProvider cacheProvider;
                if (!string.IsNullOrEmpty(cacheSection.DefaultProvider))
                {
                    if (_providers.TryGetValue(cacheSection.DefaultProvider, out cacheProvider))
                    {
                        DefaultProvider = cacheProvider;
                    }
                }
            }

            // load profiles
            if (cacheSection.Profiles.Count > 0)
            {
                foreach (ProfileElement profile in cacheSection.Profiles)
                {
                    _profiles.TryAdd(profile.Name, profile.ToCacheSettings());
                }

                CacheSettings cacheSettings;
                if (!string.IsNullOrEmpty(cacheSection.DefaultProfile))
                {
                    if (_profiles.TryGetValue(cacheSection.DefaultProfile, out cacheSettings))
                    {
                        _defaultProfile = cacheSettings;
                    }
                }
            }

            if (!string.IsNullOrEmpty(cacheSection.DefaultGroup))
            {
                DefaultGroup = cacheSection.DefaultGroup;
            }
        }
    }
}
