using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        public Faction GetFactionByName (string factionName, string presenceSystemName = null)
        {
            if ( string.IsNullOrEmpty( factionName ) ) { return null; }

            Faction faction = null;
            var searchFilters = new Dictionary<string, object> { { "minor_faction_presences", new { value = new[] { factionName } } } };

            // If a systemName is provided, we can filter factions that share a name according to whether they have a presence in a known system
            if ( !string.IsNullOrEmpty( presenceSystemName ) )
            {
                searchFilters.Add( "name", new { value = new[] { presenceSystemName } } );
            }

            var systemsQueryResults = new List<JToken>();
            try
            {
                var maxResultsPerPage = 500;
                int? count = null;
                int page = 0;
                do
                {
                    var systemQueryResult = Query(QueryGroup.systems, searchFilters, maxResultsPerPage, page);
                    if ( systemQueryResult != null )
                    {
                        count = count ?? systemQueryResult[ "count" ]?.ToObject<int?>();
                        systemsQueryResults.AddRange( systemQueryResult[ "results" ]?.ToObject<List<JToken>>() ?? new List<JToken>() );
                        page++;
                    }

                } while ( systemsQueryResults.Count < count );
            }
            catch ( Exception e )
            {
                Logging.Error( "Error retrieving faction data from Spansh", e );
            }

            if ( systemsQueryResults.Any() )
            {
                faction = GetFactionBaseData( ref faction, factionName, presenceSystemName, systemsQueryResults );

                // Set our faction presence data
                if ( faction != null )
                {
                    faction.presences = GetFactionPresenceData(faction.name, systemsQueryResults);
                }
            }

            return faction;
        }

        private Faction GetFactionBaseData(ref Faction faction, string factionName, string presenceSystemName, List<JToken> systemsQueryResults )
        {
            var controllingMinorFactionData =  systemsQueryResults.FirstOrDefault( s => s?[ "controlling_minor_faction" ]?.ToString()
                .Equals( factionName, StringComparison.InvariantCultureIgnoreCase ) ?? false );

            // Supplement with station data when the faction is not a system controlling faction in any star system
            if ( controllingMinorFactionData is null )
            {
                var searchFilters = new Dictionary<string, object> { { "controlling_minor_faction", new { value = new[] { factionName } } } };

                // If a systemName is provided, we can filter factions that share a name according to whether they have a presence in a known system
                if ( !string.IsNullOrEmpty( presenceSystemName ) )
                {
                    searchFilters.Add( "system_name", new { value = new[] { presenceSystemName } } );
                }

                var stationsQueryResult = Query( QueryGroup.stations, searchFilters, 1 )?[ "results" ]?.FirstOrDefault();
                controllingMinorFactionData = stationsQueryResult;
            }

            if ( controllingMinorFactionData is JToken data )
            {
                faction = new Faction
                {
                    name = data[ "controlling_minor_faction" ]?.ToString(),
                    Allegiance = Superpower.FromName( data[ "allegiance" ]?.ToString() ) ?? Superpower.None,
                    Government = Government.FromName( data[ "government" ]?.ToString() ) ?? Government.None,
                    updatedAt = JsonParsing.getDateTime( "updated_at", data )
                };
            }

            return faction;
        }

        private static List<FactionPresence> GetFactionPresenceData ( string factionName, List<JToken> systemsQueryResults )
        {
            var factionPresences = new List<FactionPresence>();
            foreach ( var systemData in systemsQueryResults )
            {
                var presenceData = systemData[ "minor_faction_presences" ]?.Where( p => p[ "name" ]?.ToString().Equals( factionName ) ?? false ).FirstOrDefault();
                var factionPresence = new FactionPresence
                {
                    systemName = systemData[ "name" ]?.ToString(),
                    systemAddress = systemData[ "id64" ]?.ToObject<ulong>() ?? 0,
                    influence = presenceData?[ "influence" ]?.ToObject<decimal?>() * 100,
                    FactionState = FactionState.FromName( presenceData?[ "state" ]?.ToString() ) ?? FactionState.None,
                    updatedAt = JsonParsing.getDateTime( "updated_at", systemData )
                };
                factionPresences.Add( factionPresence );
            }

            return factionPresences;
        }
    }
}
