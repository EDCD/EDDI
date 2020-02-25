using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using EddiDataDefinitions;

namespace EddiDataProviderService
{
    public class StarSystemCache
    {
        private readonly CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
        private readonly ObjectCache starSystemCache = new MemoryCache("StarSystemCache");

        // Store deserialized star systems in short term memory for this amount of time.
        // Storage time is reset whenever the cached value is accessed.
        public StarSystemCache(int expirationSeconds)
        {
            cacheItemPolicy.SlidingExpiration = TimeSpan.FromSeconds(expirationSeconds);
        }

        public void Add(StarSystem starSystem)
        {
            starSystemCache.Add(starSystem.systemname, starSystem, cacheItemPolicy);
        }

        public bool Contains(string systemName)
        {
            return starSystemCache.Contains(systemName);
        }

        public StarSystem Get(string systemName)
        {
            return starSystemCache.Get(systemName) as StarSystem;
        }

        public void Remove(string systemName)
        {
            starSystemCache.Remove(systemName);
        }
    }
}
