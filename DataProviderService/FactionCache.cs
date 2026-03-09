using EddiDataDefinitions;
using System.Collections.Generic;
using System.Linq;

namespace EddiDataProviderService
{
    public class FactionCache : SlidingExpirationCache<string, Faction>
    {
        public FactionCache ( int expirationSeconds ) : base( expirationSeconds ) { }

        /// <summary>
        /// Add or update a faction in the cache. Retain faction presence information
        /// </summary>
        /// <param name="faction"></param>
        public void AddOrUpdate ( Faction faction )
        {
            if ( faction == null ) { return; }
            if ( TryGet( faction.name, out var existing ) )
            {
                faction = PreservePresenceData( faction, existing );
            }
            base.AddOrUpdate( faction.name, faction );
        }

        public void AddOrUpdate ( IEnumerable<Faction> factions )
        {
            if ( factions is null ) { return; }
            foreach ( var faction in factions )
            {
                AddOrUpdate(faction);
            }
        }

        public new bool TryGet ( string factionName, out Faction result )
        {
            return base.TryGet( factionName, out result );
        }

        private static Faction PreservePresenceData ( Faction faction, Faction oldFaction )
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
                        if ( presence.ActiveStates.Count == 0 && oldPresence.ActiveStates.Count > 0 )
                        {
                            presence.ActiveStates = oldPresence.ActiveStates;
                        }
                        if ( presence.PendingStates.Count == 0 && oldPresence.PendingStates.Count > 0 )
                        {
                            presence.PendingStates = oldPresence.PendingStates;
                        }
                        if ( presence.RecoveringStates.Count == 0 && oldPresence.RecoveringStates.Count > 0 )
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
    }
}
