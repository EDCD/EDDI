using EddiDataDefinitions;
using System.Collections.Generic;

namespace EddiDataProviderService
{
    public class StarSystemCache
    {
        private readonly SlidingExpirationCache<ulong, StarSystem> starSystemCache;
        private readonly SlidingExpirationCache<string, ulong> starSystemNameCache;

        // Store deserialized star systems in short term memory for this amount of time.
        // Storage time is reset whenever the cached value is accessed.
        public StarSystemCache ( int expirationSeconds )
        {
            starSystemCache = new StarSystemSlidingCache( expirationSeconds );
            starSystemNameCache = new NameToAddressSlidingCache( expirationSeconds );
        }

        private class StarSystemSlidingCache : SlidingExpirationCache<ulong, StarSystem>
        {
            public StarSystemSlidingCache ( int expirationSeconds ) : base( expirationSeconds ) { }
        }

        private class NameToAddressSlidingCache : SlidingExpirationCache<string, ulong>
        {
            public NameToAddressSlidingCache ( int expirationSeconds ) : base( expirationSeconds ) { }
        }

        public void AddOrUpdate ( StarSystem starSystem )
        {
            if ( starSystem is null ) { return; }
            starSystemCache.AddOrUpdate( starSystem.systemAddress, starSystem );
            starSystemNameCache.AddOrUpdate( starSystem.systemname, starSystem.systemAddress );
        }

        public bool TryGet ( ulong systemAddress, out StarSystem result )
        {
            return starSystemCache.TryGet( systemAddress, out result );
        }

        public bool TryGet ( string systemName, out StarSystem result )
        {
            result = null;
            if ( !string.IsNullOrEmpty( systemName ) && starSystemNameCache.TryGet( systemName, out var systemAddress ) )
            {
                return TryGet( systemAddress, out result );
            }
            return false;
        }

        public List<StarSystem> GetRange ( ulong[] systemAddresses )
        {
            var results = new List<StarSystem>();
            foreach ( var systemAddress in systemAddresses )
            {
                if ( TryGet( systemAddress, out var cachedStarSystem ) )
                {
                    results.Add( cachedStarSystem );
                }
            }
            return results;
        }

        public List<StarSystem> GetRange ( string[] systemNames )
        {
            var results = new List<StarSystem>();
            foreach ( var systemName in systemNames )
            {
                if ( TryGet( systemName, out var cachedStarSystem ) )
                {
                    results.Add( cachedStarSystem );
                }
            }
            return results;
        }
    }
}
