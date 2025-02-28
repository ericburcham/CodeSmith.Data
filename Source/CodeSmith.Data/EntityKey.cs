﻿namespace CodeSmith.Data
{
    /// <summary>Interface for an entity key.</summary>
    public interface IEntityKey
    {
    }

    /// <summary>Interface for an entity key.</summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public interface IEntityKey<TKey> : IEntityKey
    {
        /// <summary>Gets the entity key.</summary>
        TKey Key { get; }
    }

    /// <summary>Interface for a two part entity key.</summary>
    /// <typeparam name="TKey0">The type of the first key.</typeparam>
    /// <typeparam name="TKey1">The type of the second key.</typeparam>
    public interface IEntityKey<TKey0, TKey1> : IEntityKey<TKey0>
    {
        /// <summary>Gets the second key.</summary>
        TKey1 Key1 { get; }
    }

    /// <summary>Interface for a three part entity key.</summary>
    /// <typeparam name="TKey0">The type of the first key.</typeparam>
    /// <typeparam name="TKey1">The type of the second key.</typeparam>
    /// <typeparam name="TKey2">The type of the key2.</typeparam>
    public interface IEntityKey<TKey0, TKey1, TKey2> : IEntityKey<TKey0, TKey1>
    {
        /// <summary>Gets the third key.</summary>
        TKey2 Key2 { get; }
    }

    /// <summary>Interface for a four part entity key.</summary>
    /// <typeparam name="TKey0">The type of the first key.</typeparam>
    /// <typeparam name="TKey1">The type of the second key.</typeparam>
    /// <typeparam name="TKey2">The type of the third key.</typeparam>
    /// <typeparam name="TKey3">The type of the fourth key.</typeparam>
    public interface IEntityKey<TKey0, TKey1, TKey2, TKey3> : IEntityKey<TKey0, TKey1, TKey2>
    {
        /// <summary>Gets the fourth key.</summary>
        TKey3 Key3 { get; }
    }

    /// <summary>Class representing an entity key.</summary>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    public class EntityKey<TKey>
        : IEntityKey<TKey>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityKey&lt;TKey&gt;" /> class.
        /// </summary>
        /// <param name="key">The entity key.</param>
        public EntityKey(TKey key)
        {
            Key = key;
        }

        /// <summary>Gets the entity key.</summary>
        public TKey Key { get; }
    }

    /// <summary>Class representing a two part entity key.</summary>
    /// <typeparam name="TKey0">The type of the first key.</typeparam>
    /// <typeparam name="TKey1">The type of the second key.</typeparam>
    public class EntityKey<TKey0, TKey1>
        : EntityKey<TKey0>, IEntityKey<TKey0, TKey1>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityKey&lt;TKey0, TKey1&gt;" /> class.
        /// </summary>
        /// <param name="key0">The first key value.</param>
        /// <param name="key1">The second key value.</param>
        public EntityKey(TKey0 key0, TKey1 key1)
            : base(key0)
        {
            Key1 = key1;
        }

        /// <summary>Gets the second key.</summary>
        public TKey1 Key1 { get; }
    }

    /// <summary>Class representing a three part entity key.</summary>
    /// <typeparam name="TKey0">The type of the first key.</typeparam>
    /// <typeparam name="TKey1">The type of the second key.</typeparam>
    /// <typeparam name="TKey2">The type of the third key.</typeparam>
    public class EntityKey<TKey0, TKey1, TKey2>
        : EntityKey<TKey0, TKey1>, IEntityKey<TKey0, TKey1, TKey2>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityKey&lt;TKey0, TKey1, TKey2&gt;" /> class.
        /// </summary>
        /// <param name="key0">The first key value.</param>
        /// <param name="key1">The second key value.</param>
        /// <param name="key2">The third key value.</param>
        public EntityKey(TKey0 key0, TKey1 key1, TKey2 key2)
            : base(key0, key1)
        {
            Key2 = key2;
        }

        /// <summary>Gets the third key.</summary>
        public TKey2 Key2 { get; }
    }

    /// <summary>Class representing a four part entity key.</summary>
    /// <typeparam name="TKey0">The type of the first key.</typeparam>
    /// <typeparam name="TKey1">The type of the second key.</typeparam>
    /// <typeparam name="TKey2">The type of the third key.</typeparam>
    /// <typeparam name="TKey3">The type of the fourth key.</typeparam>
    public class EntityKey<TKey0, TKey1, TKey2, TKey3>
        : EntityKey<TKey0, TKey1, TKey2>, IEntityKey<TKey0, TKey1, TKey2, TKey3>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityKey&lt;TKey0, TKey1, TKey2, TKey3&gt;" /> class.
        /// </summary>
        /// <param name="key0">The first key value.</param>
        /// <param name="key1">The second key value.</param>
        /// <param name="key2">The third key value.</param>
        /// <param name="key3">The fourth key value.</param>
        public EntityKey(TKey0 key0, TKey1 key1, TKey2 key2, TKey3 key3)
            : base(key0, key1, key2)
        {
            Key3 = key3;
        }

        /// <summary>Gets the fourth key.</summary>
        public TKey3 Key3 { get; }
    }
}
