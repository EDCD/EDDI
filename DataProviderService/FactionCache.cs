using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace EddiDataProviderService
{
    public class FactionCache
    {
        private readonly CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
        private readonly ObjectCache factionCache = new MemoryCache( "FactionCache" );

        // Store deserialized star systems in short term memory for this amount of time.
        // Storage time is reset whenever the cached value is accessed.
        public FactionCache ( int expirationSeconds )
        {
            cacheItemPolicy.SlidingExpiration = TimeSpan.FromSeconds( expirationSeconds );
        }

        /// <summary>
        /// Add or update a faction in the cache retaining any faction presence information
        /// </summary>
        /// <param name="faction"></param>
        public void AddOrUpdate ( Faction faction )
        {
            if ( faction is null ) { return; }

            Faction oldFaction = null;
            if ( factionCache.Contains( faction.name ) )
            {
                oldFaction = factionCache.Get( faction.name ) as Faction;
                factionCache.Remove( faction.name );
            }

            if(oldFaction != null )
            {
                foreach ( var oldPresence in oldFaction.presences.Where(p => !faction.presences.Select(op => op.systemAddress).Contains(p.systemAddress) ) )
                {
                    faction.presences.Add( oldPresence );
                }
            }

            factionCache.Add( faction.name, faction, cacheItemPolicy );
        }

        public void AddOrUpdate ( IEnumerable<Faction> factions )
        {
            if ( factions is null ) { return; }
            foreach ( var faction in factions )
            {
                AddOrUpdate(faction);
            }
        }

        public bool TryGet ( string factionName, out Faction result )
        {
            if ( factionCache.Contains( factionName ) )
            {
                result = factionCache.Get( factionName ) as Faction;
                return true;
            }

            result = null;
            return false;
        }
    }
}
