using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EddiDataProviderService
{
    public abstract class SlidingExpirationCache<TKey, TValue> : IDisposable
    {
        private class CacheItem
        {
            public TValue Value;
            public DateTime LastAccess;
        }

        private readonly ConcurrentDictionary<TKey, CacheItem> cache = new ConcurrentDictionary<TKey, CacheItem>();
        private readonly TimeSpan slidingExpiration;
        private readonly Timer cleanupTimer;

        protected SlidingExpirationCache ( int expirationSeconds )
        {
            slidingExpiration = TimeSpan.FromSeconds( expirationSeconds );
            cleanupTimer = new Timer( Cleanup, null, TimeSpan.FromMinutes( 1 ), TimeSpan.FromMinutes( 1 ) );
        }

        public void AddOrUpdate ( TKey key, TValue value )
        {
            cache.AddOrUpdate(
                key,
                _ => new CacheItem { Value = value, LastAccess = DateTime.UtcNow },
                ( _, existing ) => new CacheItem { Value = value, LastAccess = DateTime.UtcNow }
            );
        }

        public bool TryGet ( TKey key, out TValue value )
        {
            value = default;
            if ( cache.TryGetValue( key, out var item ) )
            {
                if ( DateTime.UtcNow - item.LastAccess < slidingExpiration )
                {
                    item.LastAccess = DateTime.UtcNow;
                    value = item.Value;
                    return true;
                }
                else
                {
                    cache.TryRemove( key, out _ );
                }
            }
            return false;
        }

        public void AddOrUpdate ( IEnumerable<KeyValuePair<TKey, TValue>> items )
        {
            foreach ( var kvp in items )
            {
                AddOrUpdate( kvp.Key, kvp.Value );
            }
        }

        private void Cleanup ( object state )
        {
            var now = DateTime.UtcNow;
            foreach ( var key in cache.Keys )
            {
                if ( cache.TryGetValue( key, out var item ) && now - item.LastAccess >= slidingExpiration )
                {
                    cache.TryRemove( key, out _ );
                }
            }
        }

        public void Dispose ()
        {
            cleanupTimer?.Dispose();
        }
    }
}
