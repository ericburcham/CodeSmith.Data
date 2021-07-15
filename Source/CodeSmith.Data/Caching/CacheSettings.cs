using System;
using System.Web.Caching;

namespace CodeSmith.Data.Caching
{
    /// <summary>
    ///     The cache expiration mode.
    /// </summary>
    public enum CacheExpirationMode
    {
        None,

        Duration,

        Sliding,

        Absolute
    }

    public class CacheSettings : ICloneable
    {
        public CacheSettings()
        {
            CacheEmptyResult = true;
            Priority = CacheItemPriority.Normal;
            Mode = CacheExpirationMode.None;
            Group = CacheManager.DefaultGroup;
        }

        public CacheSettings(int duration)
            : this()
        {
            Duration = TimeSpan.FromSeconds(duration);
            Mode = CacheExpirationMode.Duration;
        }

        public CacheSettings(DateTime absoluteExpiration)
            : this()
        {
            AbsoluteExpiration = absoluteExpiration;
            Mode = CacheExpirationMode.Absolute;
        }

        public CacheSettings(TimeSpan slidingExpiration)
            : this()
        {
            Duration = slidingExpiration;
            Mode = CacheExpirationMode.Sliding;
        }

        public DateTime AbsoluteExpiration { get; set; }

        public CacheDependency CacheDependency { get; set; }

        public bool CacheEmptyResult { get; set; }

        public CacheItemRemovedCallback CacheItemRemovedCallback { get; set; }

        public TimeSpan Duration { get; set; }

        public string Group { get; set; }

        public CacheExpirationMode Mode { get; set; }

        public CacheItemPriority Priority { get; set; }

        public string Provider { get; set; }

        public static CacheSettings FromAbsolute(DateTime expiration)
        {
            return new CacheSettings(expiration);
        }

        public static CacheSettings FromDuration(int duration)
        {
            return new CacheSettings(duration);
        }

        public static CacheSettings FromProfile(string profile)
        {
            return CacheManager.GetProfile(profile);
        }

        public CacheSettings Clone()
        {
            var clone = new CacheSettings
            {
                Mode = Mode,
                Duration = Duration,
                AbsoluteExpiration = AbsoluteExpiration,
                Priority = Priority,
                CacheEmptyResult = CacheEmptyResult,
                CacheDependency = CacheDependency,
                CacheItemRemovedCallback = CacheItemRemovedCallback,
                Provider = Provider,
                Group = Group
            };

            if (string.IsNullOrEmpty(Group))
            {
                clone.Group = CacheManager.DefaultGroup;
            }

            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
