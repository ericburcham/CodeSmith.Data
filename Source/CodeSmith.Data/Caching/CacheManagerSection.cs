using System.Configuration;

namespace CodeSmith.Data.Caching
{
    /// <summary>
    ///     A class for CacheManager configuration settings section.
    /// </summary>
    public class CacheManagerSection : ConfigurationSection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CacheManagerSection" /> class.
        /// </summary>
        public CacheManagerSection()
        {
            DefaultProvider = "MemoryCacheProvider";
        }

        /// <summary>
        ///     Gets or sets the default group name.
        /// </summary>
        /// <value>The default group name.</value>
        [ConfigurationProperty("defaultGroup")]
        public string DefaultGroup
        {
            get => base["defaultGroup"] as string;
            set => base["defaultGroup"] = value;
        }

        /// <summary>
        ///     Gets or sets the default profile name.
        /// </summary>
        /// <value>The default profile name.</value>
        [ConfigurationProperty("defaultProfile")]
        public string DefaultProfile
        {
            get => base["defaultProfile"] as string;
            set => base["defaultProfile"] = value;
        }

        /// <summary>
        ///     Gets or sets the default provider name.
        /// </summary>
        /// <value>The default provider name.</value>
        [ConfigurationProperty("defaultProvider", DefaultValue = "MemoryCacheProvider")]
        [StringValidator(MinLength = 1)]
        public string DefaultProvider
        {
            get => base["defaultProvider"] as string;
            set => base["defaultProvider"] = value;
        }

        /// <summary>
        ///     Gets the cache profiles.
        /// </summary>
        /// <value>The cache profiles.</value>
        [ConfigurationProperty("profiles")]
        public ProfileElementCollection Profiles => this["profiles"] as ProfileElementCollection;

        /// <summary>
        ///     Gets the cache providers.
        /// </summary>
        /// <value>The cache providers.</value>
        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers => this["providers"] as ProviderSettingsCollection;
    }
}
