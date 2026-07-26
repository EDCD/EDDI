using EddiDataDefinitions;
using EddiSpanshService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataProviderService
{
    internal class FactionDataProvider
    {
        private readonly StarSystemSqLiteRepository starSystemRepository;
        private readonly FactionCache factionCache;
        private readonly CancellationTokenSource cts;

        internal FactionDataProvider (
            StarSystemSqLiteRepository starSystemRepository,
            FactionCache factionCache,
            CancellationTokenSource cts )
        {
            this.starSystemRepository = starSystemRepository;
            this.factionCache = factionCache;
            this.cts = cts;
        }

        internal async Task<Faction> FetchFactionByNameAsync (
            string factionName,
            SpanshService spanshService,
            CancellationToken cancellationToken,
            string presenceSystemName = null )
        {
            if ( string.IsNullOrEmpty( factionName ) )
            { return null; }

            if ( factionCache.TryGet( factionName, out var faction ) )
            {
                await HydrateFactionDataAsync( [ faction ] ).ConfigureAwait( false );
                return faction;
            }

            faction = await spanshService.GetFactionByNameAsync( factionName, cancellationToken, presenceSystemName ).ConfigureAwait( false );
            if ( faction != null )
            {
                await HydrateFactionDataAsync( [ faction ] ).ConfigureAwait( false );
                factionCache.AddOrUpdate( faction );
            }

            return faction;
        }

        internal async Task HydrateFactionDataAsync ( IEnumerable<StarSystem> starSystems )
        {
            await HydrateFactionDataAsync( starSystems?
                    .SelectMany( s => s?.factions ?? [ ] ) )
                .ConfigureAwait( false );
        }

        internal async Task HydrateFactionDataAsync ( IEnumerable<Faction> factions )
        {
            var factionList = factions?
                .Where( f => !string.IsNullOrWhiteSpace( f?.name ) )
                .ToList() ?? [ ];
            if ( factionList.Count == 0 )
            {
                return;
            }

            var factionData = await starSystemRepository
                .GetFactionDataAsync( factionList.Select( f => f.name ), cts.Token )
                .ConfigureAwait( false );
            if ( factionData.Count == 0 )
            {
                return;
            }

            foreach ( var faction in factionList )
            {
                if ( factionData.TryGetValue( faction.name, out var databaseFaction ) &&
                     databaseFaction.myreputation != null )
                {
                    faction.myreputation = databaseFaction.myreputation;
                    if ( databaseFaction.reputationUpdatedAt != null )
                    {
                        faction.updatedAt = databaseFaction.reputationUpdatedAt.Value;
                    }
                }
            }
        }

        internal void ApplyFactionCache ( StarSystem starSystem )
        {
            if ( starSystem?.factions is null )
            {
                return;
            }

            foreach ( var faction in starSystem.factions )
            {
                if ( string.IsNullOrEmpty( faction?.name ) )
                {
                    continue;
                }

                if ( factionCache.TryGet( faction.name, out var cachedFaction ) )
                {
                    if ( cachedFaction != null && cachedFaction.updatedAt > faction.updatedAt )
                    {
                        faction.myreputation = cachedFaction.myreputation;
                        faction.presences = cachedFaction.presences;
                        faction.updatedAt = cachedFaction.updatedAt;
                    }
                }
            }
        }

        internal void CacheFactionData ( IEnumerable<Faction> factions )
        {
            factionCache.AddOrUpdate( factions );
        }

        internal void NormalizeFactionReputationUpdatedAt ( StarSystem starSystem )
        {
            if ( starSystem?.factions is null )
            {
                return;
            }

            foreach ( var faction in starSystem.factions )
            {
                if ( faction?.myreputation != null && faction.updatedAt <= DateTime.MinValue )
                {
                    faction.updatedAt = ResolveReputationUpdatedAt( starSystem );
                }
            }
        }

        private static DateTime ResolveReputationUpdatedAt ( StarSystem starSystem )
        {
            var updatedAt = Dates.fromTimestamp( starSystem.updatedat );
            if ( updatedAt > DateTime.MinValue )
            {
                return NormalizeDateTime( updatedAt.Value );
            }

            if ( starSystem.lastvisit > DateTime.MinValue )
            {
                return NormalizeDateTime( starSystem.lastvisit.Value );
            }

            if ( starSystem.lastupdated > DateTime.MinValue )
            {
                return NormalizeDateTime( starSystem.lastupdated );
            }

            return DateTime.UtcNow;
        }

        private static DateTime NormalizeDateTime ( DateTime dateTime )
        {
            return dateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind( dateTime, DateTimeKind.Utc )
                : dateTime.ToUniversalTime();
        }
    }
}
