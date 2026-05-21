using EddiDataDefinitions;
using System.Collections.Generic;

namespace EddiDataProviderService
{
    public class StarSystemCache ( int expirationSeconds )
    {
        private readonly SlidingExpirationCache<ulong, StarSystem> starSystemCache = new StarSystemSlidingCache( expirationSeconds );
        private readonly SlidingExpirationCache<string, ulong> starSystemNameCache = new NameToAddressSlidingCache( expirationSeconds );
        private readonly SlidingExpirationCache<ulong, bool> missingStarSystemCache = new MissingStarSystemSlidingCache( expirationSeconds );

        // Store deserialized star systems in short term memory for this amount of time.
        // Storage time is reset whenever the cached value is accessed.

        private class StarSystemSlidingCache ( int expirationSeconds )
            : SlidingExpirationCache<ulong, StarSystem>( expirationSeconds );

        private class NameToAddressSlidingCache ( int expirationSeconds )
            : SlidingExpirationCache<string, ulong>( expirationSeconds );

        private class MissingStarSystemSlidingCache ( int expirationSeconds )
            : SlidingExpirationCache<ulong, bool>( expirationSeconds );

        public void AddOrUpdate ( StarSystem starSystem )
        {
            if ( starSystem is null ) { return; }
            missingStarSystemCache.Remove( starSystem.systemAddress );
            starSystemCache.AddOrUpdate( starSystem.systemAddress, starSystem );
            starSystemNameCache.AddOrUpdate( starSystem.systemname, starSystem.systemAddress );
        }

        public bool IsUnavailable ( ulong systemAddress )
        {
            return missingStarSystemCache.TryGet( systemAddress, out _ );
        }

        public void MarkUnavailable ( ulong systemAddress )
        {
            if ( systemAddress > 0 )
            {
                missingStarSystemCache.AddOrUpdate( systemAddress, true );
            }
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
