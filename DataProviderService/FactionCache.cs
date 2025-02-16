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
        /// Add or update a faction in the cache. Retain faction presence information
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
                faction = PreservePresenceData(faction, oldFaction);
            }

            factionCache.Add( faction.name, faction, cacheItemPolicy );
        }

        private static Faction PreservePresenceData(Faction faction, Faction oldFaction)
        {
            foreach ( var oldPresence in oldFaction.presences )
            {
                foreach ( var presence in faction.presences )
                {
                    if ( presence.systemAddress == oldPresence.systemAddress )
                    {
                        if ( presence.influence is null && oldPresence.influence != null )
                        {
                            presence.influence = oldPresence.influence;
                        }
                        if ( presence.Happiness is null && oldPresence.Happiness != null )
                        {
                            presence.Happiness = oldPresence.Happiness;
                        }
                        if ( !presence.ActiveStates.Any() && oldPresence.ActiveStates.Any() )
                        {
                            presence.ActiveStates = oldPresence.ActiveStates;
                        }
                        if ( !presence.PendingStates.Any() && oldPresence.PendingStates.Any() )
                        {
                            presence.PendingStates = oldPresence.PendingStates;
                        }
                        if ( !presence.RecoveringStates.Any() && oldPresence.RecoveringStates.Any() )
                        {
                            presence.RecoveringStates = oldPresence.RecoveringStates;
                        }
                        if ( !presence.squadronhomesystem && oldPresence.squadronhomesystem )
                        {
                            presence.squadronhomesystem = oldPresence.squadronhomesystem;
                        }
                        if ( !presence.squadronhappiestsystem && oldPresence.squadronhappiestsystem )
                        {
                            presence.squadronhappiestsystem = oldPresence.squadronhappiestsystem;
                        }
                    }
                }
            }

            return faction;
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
