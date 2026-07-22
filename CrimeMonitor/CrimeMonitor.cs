using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiCrimeMonitor
{
    /**
     * Monitor claims, fines, and bounties for the current ship
     */
    public class CrimeMonitor : IEddiMonitor
    {
        // Combat bond award journals carry base values; the Transactions panel shows boosted voucher values.
        // Ship bond samples vary, so this remains a fallback estimate until the missing condition is identified.
        private const decimal HiddenShipCombatBondVoucherMultiplier = 2.85M;
        private const decimal HiddenOnFootCombatBondVoucherMultiplier = 4.2486M;

        // Observable collection for us to handle changes
        public ObservableCollection<FactionRecord> criminalrecord { get; }
        private long claims => criminalrecord.Sum(r => r.claims);
        private long fines => criminalrecord.Sum(r => r.fines);
        private long bounties => criminalrecord.Sum(r => r.bounties);
        private Dictionary<string, string> homeSystems;
        private DateTime updateDat;
        private string crimeAuthorityFaction;
        private Power pledgedPower = Power.None;

        private Power currentSystemPower = Power.None;
        public readonly List<Target> shipTargets = [ ];

        internal static readonly object recordLock = new();
        public event EventHandler RecordUpdatedEvent;

        internal decimal? PowerplayBountyBonus { get; private set; }
        internal decimal? PowerplayCrimeReduction { get; private set; }
        internal int PledgedPowerRank { get; private set; }

        public string MonitorName()
        {
            return "Crime monitor";
        }

        public string LocalizedMonitorName()
        {
            return Properties.CrimeMonitor.crime_monitor_name;
        }

        public string MonitorDescription()
        {
            return Properties.CrimeMonitor.crime_monitor_desc;
        }

        public bool IsRequired()
        {
            return false;
        }

        public CrimeMonitor()
        {
            criminalrecord = [ ];
            homeSystems = new Dictionary<string, string>();
            BindingOperations.CollectionRegistering += Record_CollectionRegistering;
            initializeCrimeMonitor();
        }

        private void initializeCrimeMonitor(CrimeMonitorConfiguration configuration = null)
        {
            readRecord(configuration);
            Logging.Info($"Initialized {MonitorName()}");
        }

        private void Record_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(criminalrecord, recordLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(criminalrecord, recordLock); });
            }
        }

        public bool NeedsStart()
        {
            // We don't actively do anything, just listen to events
            return false;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Reload()
        {
            readRecord();
            Logging.Info($"Reloaded {MonitorName()}");
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public Task HandleProfileAsync(JObject profile)
        {
            return Task.CompletedTask;
        }

        public Task HandleStatusAsync ( Status status )
        {
            return Task.CompletedTask;
        }
        
        public Task PostHandleAsync ( Event @event )
        {
            if ( @event is ShipSwappedEvent )
            {
                postHandleShipSwappedEventAsync().SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
            }
            else if ( @event is ShipTargetedEvent targetedEvent )
            {
                postHandleShipTargetedEventAsync( targetedEvent ).SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
            }
            
            return Task.CompletedTask;
        }

        private async Task postHandleShipSwappedEventAsync()
        {
            // Update stations in minor faction records asynchronously
            List<FactionRecord> records;
            lock ( recordLock )
            {
                records = criminalrecord.ToList();
            }

            foreach ( var record in records )
            {
                var Allegiance = Superpower.FromNameOrEdName(record.faction);
                if ( Allegiance is null )
                {
                    var factionStation = await GetFactionStationAsync( record.system ).ConfigureAwait(false);
                    lock ( recordLock )
                    {
                        record.station = factionStation;
                    }
                }
            }
        }

        internal async Task postHandleShipTargetedEventAsync ( ShipTargetedEvent @event )
        {
            // System targets list may be 're-built' for the current system from Log Load
            if ( @event.targetlocked )
            {
                var target = new Target();
                if ( @event.scanstage >= 1 )
                {
                    lock ( recordLock )
                    {
                        target = shipTargets.FirstOrDefault( t => t.name == @event.name );
                        if ( target == null )
                        {
                            target = new Target( @event.name, @event.CombatRank, @event.ship );
                            shipTargets.Add( target );
                        }
                    }
                }
                if ( @event.scanstage >= 3 && target.LegalStatus == null )
                {
                    target.faction = @event.faction;
                    target.Power = @event.Power;
                    target.LegalStatus = @event.LegalStatus;
                    target.bounty = @event.bounty;
                    target.Allegiance = (await EDDI.Instance.DataProvider.FetchFactionByNameAsync( @event.faction ).ConfigureAwait(false))?.Allegiance;
                }
            }
        }

        public async Task PreHandleAsync(Event @event)
        {
            // Handle the events that we care about
            if (@event is LocationEvent locationEvent)
            {
                handleLocationEvent(locationEvent);
            }
            else if (@event is DockedEvent dockedEvent)
            {
                handleDockedEvent(dockedEvent);
            }
            else if (@event is JumpedEvent jumpedEvent)
            {
                handleJumpedEvent(jumpedEvent);
            }
            else if (@event is PowerplayEvent powerplayEvent)
            {
                handlePowerplayEvent(powerplayEvent);
            }
            else if (@event is PowerRankEvent powerRankEvent)
            {
                handlePowerRankEvent(powerRankEvent);
            }
            else if (@event is PowerMeritsEvent powerMeritsEvent)
            {
                handlePowerMeritsEvent(powerMeritsEvent);
            }
            else if (@event is PowerJoinedEvent powerJoinedEvent)
            {
                handlePowerJoinedEvent(powerJoinedEvent);
            }
            else if (@event is PowerLeftEvent powerLeftEvent)
            {
                handlePowerLeftEvent(powerLeftEvent);
            }
            else if (@event is BondAwardedEvent awardedEvent)
            {
                await handleBondAwardedEventAsync(awardedEvent).ConfigureAwait( false );
            }
            else if (@event is BondRedeemedEvent redeemedEvent)
            {
                handleBondRedeemedEvent(redeemedEvent);
            }
            else if (@event is BountyAwardedEvent bountyAwardedEvent)
            {
                await handleBountyAwardedEventAsync(bountyAwardedEvent).ConfigureAwait( false );
            }
            else if (@event is BountyIncurredEvent incurredEvent)
            {
                await handleBountyIncurredEventAsync(incurredEvent).ConfigureAwait( false );
            }
            else if (@event is BountyPaidEvent paidEvent)
            {
                handleBountyPaidEvent(paidEvent);
            }
            else if (@event is BountyRedeemedEvent bountyRedeemedEvent)
            {
                handleBountyRedeemedEvent(bountyRedeemedEvent);
            }
            else if (@event is FineIncurredEvent fineIncurredEvent)
            {
                await handleFineIncurredEventAsync(fineIncurredEvent).ConfigureAwait( false );
            }
            else if (@event is FinePaidEvent finePaidEvent)
            {
                handleFinePaidEvent(finePaidEvent);
            }
            else if (@event is MissionAbandonedEvent abandonedEvent)
            {
                await handleMissionAbandonedEventAsync(abandonedEvent).ConfigureAwait( false );
            }
            else if (@event is MissionFailedEvent failedEvent)
            {
                await handleMissionFailedEventAsync(failedEvent).ConfigureAwait( false );
            }
            else if (@event is RespawnedEvent respawnEvent)
            {
                handleRespawnedEvent(respawnEvent);
            }
        }

        private void handleDockedEvent(DockedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                _handleDockedEvent(@event);
            }
        }

        internal void _handleDockedEvent(DockedEvent @event)
        {
            crimeAuthorityFaction = @event.faction;
            UpdateCrimeValueModifiers();
        }

        private void handleLocationEvent(LocationEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                _handleLocationEvent(@event);
                writeRecord();
            }
        }

        internal void _handleLocationEvent(LocationEvent @event)
        {
            currentSystemPower = @event.Power ?? Power.None;
            UpdateCrimeValueModifiers();
        }

        private void handleJumpedEvent(JumpedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                _handleJumpedEvent(@event);
                writeRecord();
            }
        }

        internal void _handleJumpedEvent()
        {
            lock ( recordLock )
            {
                shipTargets.Clear();
            }
        }

        internal void _handleJumpedEvent(JumpedEvent @event)
        {
            currentSystemPower = @event.Power ?? Power.None;
            _handleJumpedEvent();
            UpdateCrimeValueModifiers();
        }

        private void handlePowerplayEvent(PowerplayEvent @event)
        {
            pledgedPower = @event.Power ?? Power.None;
            PledgedPowerRank = @event.rank;
            UpdateCrimeValueModifiers();
            RaiseOnUIThread(RecordUpdatedEvent, criminalrecord);
        }

        private void handlePowerRankEvent(PowerRankEvent @event)
        {
            pledgedPower = @event.Power ?? Power.None;
            PledgedPowerRank = @event.rank;
            UpdateCrimeValueModifiers();
            RaiseOnUIThread(RecordUpdatedEvent, criminalrecord);
        }

        private void handlePowerMeritsEvent(PowerMeritsEvent @event)
        {
            pledgedPower = @event.Power ?? Power.None;
            UpdateCrimeValueModifiers();
            RaiseOnUIThread(RecordUpdatedEvent, criminalrecord);
        }

        private void handlePowerJoinedEvent(PowerJoinedEvent @event)
        {
            pledgedPower = @event.Power ?? Power.None;
            PledgedPowerRank = 0;
            UpdateCrimeValueModifiers();
            RaiseOnUIThread(RecordUpdatedEvent, criminalrecord);
        }

        private void handlePowerLeftEvent(PowerLeftEvent _)
        {
            pledgedPower = Power.None;
            PledgedPowerRank = 0;
            UpdateCrimeValueModifiers();
            RaiseOnUIThread(RecordUpdatedEvent, criminalrecord);
        }

        private void LoadPowerplayContext()
        {
            var commanderConfiguration = ConfigService.Instance.commanderConfiguration;
            pledgedPower = commanderConfiguration.Power ?? Power.None;
            PledgedPowerRank = commanderConfiguration.powerRank;

            var currentSystem = EDDI.Instance.GameState.CurrentStarSystem;
            currentSystemPower = currentSystem?.Power ?? Power.None;
        }

        private void UpdateCrimeValueModifiers()
        {
            UpdateFinalClaimValues();
            UpdatePowerplayBountyReduction();
        }

        private void UpdateFinalClaimValues()
        {
            var bonus = GetActivePowerplayBountyVoucherBonus();
            lock ( recordLock )
            {
                foreach ( var record in criminalrecord )
                {
                    var finalBountyClaims = record.basebountyclaims > 0
                        ? EstimateRecordBountyClaims( record, bonus )
                        : (long?)null;
                    record.UpdateFinalClaimValues(
                        EstimateRecordClaims( record, bonus ),
                        finalBountyClaims );
                }
            }

            PowerplayBountyBonus = bonus;
        }

        private void UpdatePowerplayBountyReduction()
        {
            if ( pledgedPower is null || pledgedPower == Power.None ||
                 currentSystemPower is null || currentSystemPower == Power.None ||
                 !string.Equals( pledgedPower.edname, currentSystemPower.edname, StringComparison.OrdinalIgnoreCase ) ||
                 !PowerplayBountyReduction.TryGetReduction( pledgedPower, PledgedPowerRank, out var reduction ) )
            {
                PowerplayCrimeReduction = null;
                return;
            }

            PowerplayCrimeReduction = reduction;
        }

        private async Task handleBondAwardedEventAsync(BondAwardedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                await _handleBondAwardedEventAsync(@event).ConfigureAwait(false);
                writeRecord();
            }
        }

        internal async Task _handleBondAwardedEventAsync(BondAwardedEvent @event)
        {
            var currentSystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;

            // Get the victim faction data
            var faction = await EDDI.Instance.DataProvider.FetchFactionByNameAsync( @event.victimfaction ).ConfigureAwait(false);

            var report = new FactionReport(@event.timestamp, false, Crime.None, currentSystem, @event.reward)
            {
                claimtype = FactionReport.BondClaimType,
                claimvehicle = EDDI.Instance.GameState.Vehicle,
                station = EDDI.Instance.GameState.CurrentStation?.name,
                body = EDDI.Instance.GameState.CurrentStellarBody?.bodyname,
                victim = @event.victimfaction,
                victimAllegiance = (faction?.Allegiance ?? Superpower.None).invariantName
            };

            var record = GetRecordWithFaction(@event.awardingfaction) 
                ?? await AddRecordAsync(@event.awardingfaction).ConfigureAwait(false);
            record.factionReports.Add(report);
            record.baseclaims += @event.reward;
            UpdateCrimeValueModifiers();
        }

        private void handleBondRedeemedEvent(BondRedeemedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                if (_handleBondRedeemedEvent(@event))
                {
                    writeRecord();
                }
            }
        }

        internal bool _handleBondRedeemedEvent(BondRedeemedEvent @event)
        {
            return RedeemClaimRewards(
                @event.rewards,
                @event.brokerpercentage,
                r => !r.bounty && r.crimeDef == Crime.None && r.claimtype == FactionReport.BondClaimType,
                r => r.bondsAmount,
                true );
        }

        private async Task handleBountyAwardedEventAsync(BountyAwardedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                await _handleBountyAwardedEventAsync(@event).ConfigureAwait(false);
                writeRecord();
            }
        }

        internal async Task _handleBountyAwardedEventAsync(BountyAwardedEvent @event)
        {
            var currentSystem = EDDI.Instance.GameState.CurrentStarSystem;

            // Get the victim faction data
            var faction = await EDDI.Instance.DataProvider.FetchFactionByNameAsync( @event.faction ).ConfigureAwait(false);

            foreach (var reward in @event.rewards.ToList())
            {
                var amount = reward.amount;
                var report = new FactionReport(@event.timestamp, true, Crime.None, currentSystem?.systemname, amount)
                {
                    claimtype = FactionReport.BountyClaimType,
                    station = EDDI.Instance.GameState.CurrentStation?.name,
                    body = EDDI.Instance.GameState.CurrentStellarBody?.bodyname,
                    victim = @event.faction,
                    victimAllegiance = (faction?.Allegiance ?? Superpower.None).invariantName
                };

                var record = GetRecordWithFaction(reward.faction) 
                    ?? await AddRecordAsync(reward.faction).ConfigureAwait(false);
                record.factionReports.Add(report);
                record.baseclaims += amount;
            }
            UpdateCrimeValueModifiers();
        }

        private void handleBountyRedeemedEvent(BountyRedeemedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                if (_handleBountyRedeemedEvent(@event))
                {
                    writeRecord();
                }
            }
        }

        internal bool _handleBountyRedeemedEvent(BountyRedeemedEvent @event)
        {
            return RedeemClaimRewards(
                @event.rewards,
                @event.brokerpercentage,
                r => r.bounty && r.crimeDef == Crime.None && r.claimtype == FactionReport.BountyClaimType,
                r => r.bountiesAmount );
        }

        private bool RedeemClaimRewards (
            IEnumerable<Reward> rewards,
            decimal? brokerpercentage,
            Func<FactionReport, bool> reportSelector,
            Func<FactionRecord, long> recordAmountSelector,
            bool restrictFallbackToSystemFactions = false )
        {
            var update = false;
            foreach ( var reward in rewards.ToList() )
            {
                var amount = AmountBeforeRedeemBrokerFee( reward.amount, brokerpercentage );
                var record = FindClaimRedemptionRecord( reward.faction, amount, recordAmountSelector, restrictFallbackToSystemFactions );
                if ( record != null )
                {
                    update |= RemoveClaimReports( record, amount, reportSelector );
                }
            }
            return update;
        }

        private FactionRecord FindClaimRedemptionRecord (
            string faction,
            long amount,
            Func<FactionRecord, long> recordAmountSelector,
            bool restrictFallbackToSystemFactions )
        {
            if ( !string.IsNullOrEmpty( faction ) )
            {
                return GetRecordWithFaction( faction );
            }

            var systemFactions = restrictFallbackToSystemFactions
                ? EDDI.Instance.GameState.CurrentStarSystem?.factions?.Select( f => f.name ).ToList()
                : null;

            lock ( recordLock )
            {
                return criminalrecord
                    .Where( r => !restrictFallbackToSystemFactions || (systemFactions?.Contains( r.faction ) ?? false) )
                    .FirstOrDefault( r => recordAmountSelector( r ) == amount );
            }
        }

        private bool RemoveClaimReports ( FactionRecord record, long amount, Func<FactionReport, bool> reportSelector )
        {
            var reports = record.factionReports.Where( reportSelector ).ToList();
            if ( reports.Count == 0 )
            {
                return false;
            }

            var removedAmount = reports.Sum( ReportBaseAmount );
            if ( removedAmount < amount )
            {
                var report = record.factionReports.FirstOrDefault( r => r.crimeDef == Crime.Claim );
                if ( report != null )
                {
                    var discrepancyAmount = Math.Min( amount - removedAmount, ReportBaseAmount( report ) );
                    report.amount -= discrepancyAmount;
                    removedAmount += discrepancyAmount;
                    if ( report.amount == 0 ) { reports.Add( report ); }
                }
            }

            record.factionReports = record.factionReports.Except( reports ).ToList();
            record.baseclaims -= Math.Min( removedAmount, record.baseclaims );
            RemoveRecordIfEmpty( record );
            UpdateCrimeValueModifiers();
            return true;
        }

        private async Task handleBountyIncurredEventAsync(BountyIncurredEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                await _handleBountyIncurredEventAsync(@event).ConfigureAwait(false);
                writeRecord();
            }
        }

        internal async Task _handleBountyIncurredEventAsync(BountyIncurredEvent @event)
        {
            crimeAuthorityFaction = @event.faction;
            var crime = Crime.FromEDName(@event.crimetype);
            var currentSystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;

            // Get victim allegiance from the 'Ship targeted' data
            Target target;
            lock ( recordLock )
            {
                target = shipTargets.FirstOrDefault(t => t.name == @event.victim);
            }

            // Create a bounty report and add it to our record
            var report = new FactionReport(@event.timestamp, true, crime, currentSystem, @event.bounty)
            {
                station = EDDI.Instance.GameState.CurrentStation?.name,
                body = EDDI.Instance.GameState.CurrentStellarBody?.bodyname,
                victim = @event.victim,
                victimAllegiance = (target?.Allegiance ?? Superpower.None).invariantName
            };
            var record = GetRecordWithFaction(@event.faction)
                                   ?? await AddRecordAsync(@event.faction).ConfigureAwait(false);
            await AddReportToRecordAsync(record, report).ConfigureAwait(false);
        }

        private void handleBountyPaidEvent(BountyPaidEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                if (_handleBountyPaidEvent(@event))
                {
                    writeRecord();
                }
            }
        }

        internal bool _handleBountyPaidEvent(BountyPaidEvent @event)
        {
            var update = false;
            var amount = AmountBeforePaymentBrokerFee( @event.amount, @event.brokerpercentage );
            var records = GetBountyPaymentRecords( @event.faction, @event.allbounties, amount );
            foreach ( var record in records )
            {
                update |= RemoveCrimeReports(
                    record,
                    amount,
                    r => r.bounty && r.crimeDef != Crime.None && r.crimeDef != Crime.Bounty && r.crimeDef != Crime.Claim,
                    r => r.bounty && r.crimeDef == Crime.Bounty,
                    true );
            }

            return update;
        }

        private List<FactionRecord> GetBountyPaymentRecords ( string faction, bool allbounties, long amount )
        {
            var records = SnapshotRecords();

            if ( !string.IsNullOrEmpty( faction ) )
            {
                var exactRecord = records.FirstOrDefault( r => string.Equals( r.faction, faction, StringComparison.InvariantCultureIgnoreCase ) );
                if ( exactRecord?.bounties > 0 )
                {
                    return [ exactRecord ];
                }

                var superpower = Superpower.FromNameOrEdName( faction );
                if ( superpower != null )
                {
                    var matches = records.Where( r => r.Allegiance == superpower && r.bounties == amount ).ToList();
                    if ( matches.Count == 1 )
                    {
                        return matches;
                    }
                }
            }

            if ( allbounties )
            {
                var exactMatches = records.Where( r => r.bounties == amount ).ToList();
                if ( exactMatches.Count == 1 )
                {
                    return exactMatches;
                }

                var bountyRecords = records.Where( r => r.bounties > 0 ).ToList();
                if ( bountyRecords.Count > 0 && bountyRecords.Sum( r => r.bounties ) <= amount )
                {
                    return bountyRecords;
                }
            }

            return [ ];
        }

        private async Task handleFineIncurredEventAsync(FineIncurredEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                await _handleFineIncurredEventAsync(@event).ConfigureAwait(false);
                writeRecord();
            }
        }

        internal async Task _handleFineIncurredEventAsync(FineIncurredEvent @event)
        {
            crimeAuthorityFaction = @event.faction;
            var crime = Crime.FromEDName(@event.crimetype);
            var currentSystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
            var report = new FactionReport(@event.timestamp, false, crime, currentSystem, @event.fine)
            {
                station = EDDI.Instance.GameState.CurrentStation?.name,
                body = EDDI.Instance.GameState.CurrentStellarBody?.bodyname,
                victim = @event.victim
            };
            var record = GetRecordWithFaction(@event.faction) ?? await AddRecordAsync(@event.faction).ConfigureAwait(false);
            await AddReportToRecordAsync(record, report).ConfigureAwait(false);
        }

        private void handleFinePaidEvent(FinePaidEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                if (_handleFinePaidEvent(@event))
                {
                    writeRecord();
                }
            }
        }

        internal bool _handleFinePaidEvent(FinePaidEvent @event)
        {
            var update = false;
            var amount = AmountBeforePaymentBrokerFee( @event.amount, @event.brokerpercentage );
            var records = GetFinePaymentRecords( @event.faction, @event.allfines, amount );
            foreach ( var record in records )
            {
                update |= RemoveCrimeReports(
                    record,
                    amount,
                    r => !r.bounty && r.crimeDef != Crime.None && r.crimeDef != Crime.Fine && r.crimeDef != Crime.Claim,
                    r => !r.bounty && r.crimeDef == Crime.Fine,
                    false );
            }

            // Older journals can report a bounty payment as PayFines. Only apply that fallback when
            // there is no fine match and the bounty match is unambiguous.
            if ( !update )
            {
                var legacyBountyRecord = GetLegacyFinePaidBountyFallbackRecord( @event.faction, amount );
                if ( legacyBountyRecord != null )
                {
                    update = RemoveCrimeReports(
                        legacyBountyRecord,
                        amount,
                        r => r.bounty && r.crimeDef != Crime.None && r.crimeDef != Crime.Bounty && r.crimeDef != Crime.Claim,
                        r => r.bounty && r.crimeDef == Crime.Bounty,
                        true );
                }
            }

            return update;
        }

        private List<FactionRecord> GetFinePaymentRecords ( string faction, bool allfines, long amount )
        {
            var records = SnapshotRecords();

            if ( !string.IsNullOrEmpty( faction ) )
            {
                var exactRecord = records.FirstOrDefault( r => string.Equals( r.faction, faction, StringComparison.InvariantCultureIgnoreCase ) );
                return exactRecord?.fines > 0 ? [ exactRecord ] : [ ];
            }

            var authorityRecord = GetRecordWithFaction( crimeAuthorityFaction );
            if ( authorityRecord?.fines > 0 && authorityRecord.fines <= amount )
            {
                return [ authorityRecord ];
            }

            if ( allfines )
            {
                var exactMatches = records.Where( r => r.fines == amount ).ToList();
                if ( exactMatches.Count == 1 )
                {
                    return exactMatches;
                }

                var fineRecords = records.Where( r => r.fines > 0 ).ToList();
                if ( fineRecords.Count > 0 && fineRecords.Sum( r => r.fines ) <= amount )
                {
                    return fineRecords;
                }
            }

            return [ ];
        }

        private FactionRecord GetLegacyFinePaidBountyFallbackRecord ( string faction, long amount )
        {
            var records = SnapshotRecords();
            var candidates = new List<FactionRecord>();

            if ( !string.IsNullOrEmpty( faction ) )
            {
                var exactRecord = records.FirstOrDefault( r =>
                    string.Equals( r.faction, faction, StringComparison.InvariantCultureIgnoreCase ) &&
                    r.bounties == amount );
                if ( exactRecord != null )
                {
                    candidates.Add( exactRecord );
                }

                var superpower = Superpower.FromNameOrEdName( faction );
                if ( superpower != null )
                {
                    candidates.AddRange( records.Where( r => r.Allegiance == superpower && r.bounties == amount ) );
                }
            }
            else
            {
                candidates.AddRange( records.Where( r => r.bounties == amount ) );
            }

            var distinctCandidates = candidates.Distinct().ToList();
            return distinctCandidates.Count == 1 ? distinctCandidates.Single() : null;
        }

        private List<FactionRecord> SnapshotRecords()
        {
            lock ( recordLock )
            {
                return criminalrecord.ToList();
            }
        }

        private static long AmountBeforePaymentBrokerFee(long amount, decimal? brokerpercentage)
        {
            var multiplier = 1 + ((brokerpercentage ?? 0) / 100);
            return multiplier <= 0
                ? amount
                : Convert.ToInt64(Math.Round(amount / multiplier, MidpointRounding.AwayFromZero));
        }

        private static long AmountBeforeRedeemBrokerFee(long amount, decimal? brokerpercentage)
        {
            var multiplier = (100 - (brokerpercentage ?? 0)) / 100;
            return multiplier <= 0
                ? amount
                : Convert.ToInt64(Math.Ceiling(amount / multiplier));
        }

        private decimal? GetActivePowerplayBountyVoucherBonus()
        {
            return pledgedPower is null || pledgedPower == Power.None ||
                   currentSystemPower is null || currentSystemPower == Power.None ||
                   !string.Equals( pledgedPower.edname, currentSystemPower.edname, StringComparison.OrdinalIgnoreCase ) ||
                   !PowerplayBountyVoucherBonus.TryGetBonus( pledgedPower, PledgedPowerRank, out var bonus )
                ? null
                : bonus;
        }

        private static long EstimateRecordClaims ( FactionRecord record, decimal? bountyVoucherBonus )
        {
            if ( record is null )
            {
                return 0;
            }

            var claimReports = record.factionReports
                .Where( r => r.crimeDef == Crime.None || r.crimeDef == Crime.Claim )
                .ToList();
            if ( claimReports.Count == 0 )
            {
                return record.baseclaims;
            }

            var discrepancyClaims = claimReports
                .Where( r => r.crimeDef == Crime.Claim )
                .Sum( ReportBaseAmount );

            return EstimateRecordBondClaims( record ) +
                   EstimateRecordBountyClaims( record, bountyVoucherBonus ) +
                   discrepancyClaims;
        }

        private static long EstimateRecordBondClaims ( FactionRecord record )
        {
            return record?.bondsAwarded.Sum( EstimateBondVoucherValue ) ?? 0;
        }

        private static long EstimateRecordBountyClaims ( FactionRecord record, decimal? bountyVoucherBonus )
        {
            return record?.bountiesAwarded.Sum( r => EstimateBountyVoucherValue( ReportBaseAmount( r ), bountyVoucherBonus ) ) ?? 0;
        }

        private static long EstimateBondVoucherValue ( FactionReport report )
        {
            var multiplier = IsOnFootBondReport( report )
                ? HiddenOnFootCombatBondVoucherMultiplier
                : HiddenShipCombatBondVoucherMultiplier;

            return Convert.ToInt64( Math.Round( ReportBaseAmount( report ) * multiplier, MidpointRounding.AwayFromZero ) );
        }

        private static bool IsOnFootBondReport ( FactionReport report )
        {
            return string.Equals( report?.claimvehicle, Constants.VEHICLE_LEGS, StringComparison.OrdinalIgnoreCase );
        }

        private static long EstimateBountyVoucherValue ( long amount, decimal? bountyVoucherBonus )
        {
            return bountyVoucherBonus is > 0
                ? PowerplayBountyVoucherBonus.ApplyBonus( amount, bountyVoucherBonus.Value )
                : amount;
        }

        private static long ReportBaseAmount ( FactionReport report )
        {
            return report?.amount ?? 0;
        }

        private bool RemoveCrimeReports (
            FactionRecord record,
            long amount,
            Func<FactionReport, bool> reportSelector,
            Func<FactionReport, bool> discrepancySelector,
            bool bounty )
        {
            var reports = record.factionReports.Where( reportSelector ).ToList();
            var removedAmount = reports.Sum( r => r.amount );
            if ( removedAmount < amount )
            {
                var report = record.factionReports.FirstOrDefault( discrepancySelector );
                if ( report != null )
                {
                    var discrepancyAmount = Math.Min( amount - removedAmount, report.amount );
                    report.amount -= discrepancyAmount;
                    removedAmount += discrepancyAmount;
                    if ( report.amount == 0 ) { reports.Add( report ); }
                }
            }

            if ( removedAmount == 0 )
            {
                return false;
            }

            record.factionReports = record.factionReports.Except( reports ).ToList();
            if ( bounty )
            {
                record.bounties -= Math.Min( removedAmount, record.bounties );
            }
            else
            {
                record.fines -= Math.Min( removedAmount, record.fines );
            }

            RemoveRecordIfEmpty( record );
            return true;
        }

        private async Task handleMissionAbandonedEventAsync(MissionAbandonedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                if (await _handleMissionAbandonedEventAsync(@event).ConfigureAwait(false))
                {
                    writeRecord();
                }
            }
        }

        private async Task<bool> _handleMissionAbandonedEventAsync(MissionAbandonedEvent @event)
        {
            if (@event.fine > 0)
            {
                return await handleMissionFineAsync(@event.timestamp, @event.missionid, @event.fine).ConfigureAwait(false);
            }
            return false;
        }

        private async Task handleMissionFailedEventAsync(MissionFailedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                if (await _handleMissionFailedEventAsync(@event).ConfigureAwait(false))
                {
                    writeRecord();
                }
            }
        }

        private async Task<bool> _handleMissionFailedEventAsync(MissionFailedEvent @event)
        {
            var update = false;
            if (@event.fine > 0)
            {
                update = await handleMissionFineAsync(@event.timestamp, @event.missionid, @event.fine).ConfigureAwait(false);
            }
            return update;
        }

        private void handleRespawnedEvent(RespawnedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                _handleRespawnedEvent(@event);
                writeRecord();
            }
        }

        private void _handleRespawnedEvent(RespawnedEvent @event)
        {
            long RemoveCriminalRecords(string faction = null)
            {
                long removed = 0;
                // Update the criminal record fines and bounties for each faction, as appropriate.
                lock (recordLock)
                {
                    foreach (var record in criminalrecord.ToList())
                    {
                        if ((!string.IsNullOrEmpty(faction) && faction == record.faction) || string.IsNullOrEmpty(faction))
                        {
                            var crimeReports = record.factionReports
                                .Where(r => r.crimeDef != Crime.None && r.crimeDef != Crime.Claim)
                                .ToList();
                            // Remove all pending fines and bounties (from a named faction, if a faction name is given)
                            var forFaction = !string.IsNullOrEmpty(faction) ? $"for faction {record.faction} " : "";
                            Logging.Debug($"Paid {@event.price} credits to resolve fines and bounties {forFaction} (expected {crimeReports.Sum(r => r.amount)}).");
                            removed += crimeReports.Sum(r => r.amount);
                            record.factionReports = record.factionReports.Except(crimeReports).ToList();
                            RemoveRecordIfEmpty(record);
                        }
                    }
                }
                return removed;
            }

            long RemoveCriminalRecordsByAmount(long amount)
            {
                if ( amount <= 0 )
                {
                    return 0;
                }

                lock (recordLock)
                {
                    var matches = criminalrecord
                        .Select(record => new
                        {
                            record,
                            crimeReports = record.factionReports
                                .Where(r => r.crimeDef != Crime.None && r.crimeDef != Crime.Claim)
                                .ToList()
                        })
                        .Select(match => new
                        {
                            match.record,
                            match.crimeReports,
                            total = match.crimeReports.Sum(r => r.amount)
                        })
                        .Where(match => match.total == amount)
                        .ToList();

                    if (matches.Count != 1)
                    {
                        return 0;
                    }

                    var match = matches[0];
                    Logging.Debug($"Paid {@event.price} credits to resolve fines and bounties by matching the recover cost to {match.record.faction}.");
                    match.record.factionReports = match.record.factionReports.Except(match.crimeReports).ToList();
                    RemoveRecordIfEmpty(match.record);
                    return match.total;
                }
            }

            void RemoveClaimsRecords()
            {
                // Update the criminal record pending claims for each faction, as appropriate.
                lock (recordLock)
                {
                    foreach (var record in criminalrecord.ToList())
                    {
                        // Remove all pending claims from faction
                        var claimReports = record.factionReports
                            .Where(r => r.crimeDef == Crime.None || r.crimeDef == Crime.Claim)
                            .ToList();
                        Logging.Debug($"Removed vouchers for {claimReports.Sum(r => r.amount)} unclaimed credits from {record.faction}.");
                        record.factionReports = record.factionReports.Except(claimReports).ToList();
                        RemoveRecordIfEmpty(record);
                    }
                }
            }

            switch (@event.trigger)
            {
                case "rebuy": // Repurchase a destroyed ship. All fines and bounties must be paid. Claims are lost.
                {
                    RemoveCriminalRecords();
                    RemoveClaimsRecords();
                    break;
                }
                case "recover":  // Recover from an on-foot critical injury. All fines and bounties for the local authority faction (only) must be paid. Claims are lost.
                {
                    var removed = RemoveCriminalRecords(crimeAuthorityFaction);
                    if (removed == 0)
                    {
                        RemoveCriminalRecordsByAmount(@event.price);
                    }
                    RemoveClaimsRecords();
                    break;
                }
                case "rejoin": // Rejoin your ship. Fines and bounties remain unpaid. Claims are lost.
                {
                    RemoveClaimsRecords();
                    break;
                }
                case "handin": // Hand-in to authorities. Fines and bounties for the station authority faction (only) must be paid.
                               // Claims are preserved. Fines and bounties pertaining to other factions are preserved.
                {
                    RemoveCriminalRecords(EDDI.Instance.GameState.CurrentStation?.Faction?.name);
                    break;
                }
            }
        }

        [PublicAPI( "A list of active fine, bounty, and claim records by faction." )]
        public static RuntimeVariableDefinition CriminalRecordVariable => new( "criminalrecord", typeof(List<FactionRecord>) );

        [PublicAPI( "The total value of outstanding bounty claims." )]
        public static RuntimeVariableDefinition ClaimsVariable => new( "claims", typeof(long) );

        [PublicAPI( "The total value of outstanding fines." )]
        public static RuntimeVariableDefinition FinesVariable => new( "fines", typeof(long) );

        [PublicAPI( "The total value of outstanding bounties." )]
        public static RuntimeVariableDefinition BountiesVariable => new( "bounties", typeof(long) );

        [PublicAPI( "The Powerplay bounty claim bonus currently available, if any." )]
        public static RuntimeVariableDefinition PowerplayBountyBonusVariable => new( "powerplaybountybonus", typeof(decimal?) );

        [PublicAPI( "The Powerplay crime reduction currently available, if any." )]
        public static RuntimeVariableDefinition PowerplayCrimeReductionVariable => new( "powerplaycrimereduction", typeof(decimal?) );

        [PublicAPI( "A list of recently targeted ships." )]
        public static RuntimeVariableDefinition ShipTargetsVariable => new( "shiptargets", typeof(List<Target>) );

        public IReadOnlyList<RuntimeVariableDeclaration> GetVariableDeclarations () =>
            RuntimeVariableDefinitionExtensions.DiscoverDeclarations( typeof(CrimeMonitor) );

        public IReadOnlyList<RuntimeVariableValue> GetVariableValues ()
        {
            UpdateCrimeValueModifiers();
            lock ( recordLock )
            {
                return
                [
                    new( CriminalRecordVariable.Name, CriminalRecordVariable.Type, criminalrecord.ToList() ),
                    new( ClaimsVariable.Name, ClaimsVariable.Type, claims ),
                    new( FinesVariable.Name, FinesVariable.Type, fines ),
                    new( BountiesVariable.Name, BountiesVariable.Type, bounties ),
                    new( PowerplayBountyBonusVariable.Name, PowerplayBountyBonusVariable.Type, PowerplayBountyBonus ),
                    new( PowerplayCrimeReductionVariable.Name, PowerplayCrimeReductionVariable.Type, PowerplayCrimeReduction ),
                    new( ShipTargetsVariable.Name, ShipTargetsVariable.Type, shipTargets.ToList() )
                ];
            }
        }

        public void writeRecord()
        {
            lock (recordLock)
            {
                UpdateCrimeValueModifiers();

                // Write criminal configuration with current criminal record
                var configuration = new CrimeMonitorConfiguration()
                {
                    criminalrecord = criminalrecord,
                    homeSystems = homeSystems,
                    updatedat = updateDat
                };
                ConfigService.Instance.crimeMonitorConfiguration = configuration;
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(RecordUpdatedEvent, criminalrecord);
        }

        public void readRecord(CrimeMonitorConfiguration configuration = null)
        {
            lock (recordLock)
            {
                // Obtain current criminal record from configuration
                configuration ??= ConfigService.Instance.crimeMonitorConfiguration;
                homeSystems = configuration.homeSystems;
                updateDat = configuration.updatedat;
                LoadPowerplayContext();

                // Build a new criminal record
                var records = configuration.criminalrecord.OrderBy(c => c.faction).ToList();
                criminalrecord.Clear();
                foreach (var record in records)
                {
                    criminalrecord.Add(record);
                }
                UpdateCrimeValueModifiers();
            }
        }

        private async Task<FactionRecord> AddRecordAsync(string faction)
        {
            if (faction == null) { return null; }

            var record = new FactionRecord(faction);
            var Allegiance = Superpower.FromNameOrEdName(faction);
            if (Allegiance == null)
            {
                await GetFactionDataAsync(record).ConfigureAwait(false);
            }
            else
            {
                record.Allegiance = Allegiance;
            }

            lock (recordLock)
            {
                criminalrecord.Add(record);
            }
            return record;
        }

        private void RemoveRecordIfEmpty(FactionRecord record)
        {
            // Check if claims or crimes are pending
            if (record?.factionReports is not null && record.factionReports.Count > 0) { return; }
            _RemoveRecord(record);
        }

        public void _RemoveRecord(FactionRecord record)
        {
            var faction = record.faction;
            lock (recordLock)
            {
                for (var i = 0; i < criminalrecord.Count; i++)
                {
                    if ( string.Equals( criminalrecord[ i ].faction, faction,
                            StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        criminalrecord.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private async Task AddReportToRecordAsync(FactionRecord record, FactionReport report)
        {
            if (record is null || report is null) { return; }

            var total = record.fines + record.bounties + report.amount;
            var powerRecord = GetRecordWithFaction(record.allegiance);
            if ( ReferenceEquals( powerRecord, record ) )
            {
                _AddReportToRecord(record, report);
                return;
            }

            // Minor faction crimes are converted to an interstellar power record, owned by the faction's aligned
            // superpower, when total fines & bounties incurred exceed 10,000 credits
            if (powerRecord == null && total <= 10000) 
            {
                // Add new report to the minor faction record
                _AddReportToRecord(record, report);
            }
            else
            {
                // We've exceeded the threshold for an interstellar bounty
                if (powerRecord == null) 
                {
                    // Add a new interstellar bounty. 
                    // Transfer existing fines and bounties incurred to the interstellar power record
                    // Collect all minor faction fines and bounties incurred
                    powerRecord = await AddRecordAsync(record.allegiance).ConfigureAwait(false);
                    var reports = record.factionReports
                        .Where(r => r.crimeDef != Crime.None && r.crimeDef != Crime.Claim).ToList();
                    powerRecord.factionReports.AddRange(reports);
                    powerRecord.fines += record.fines;
                    powerRecord.bounties += record.bounties;
                    powerRecord.interstellarBountyFactions.Add(record.faction);
                    record.factionReports = record.factionReports.Except(reports).ToList();
                    record.fines = 0;
                    record.bounties = 0;

                    // Add new report to the interstellar power record and remove minor faction record if no pending claims
                    _AddReportToRecord(powerRecord, report);
                    RemoveRecordIfEmpty(record);
                }
                else if (powerRecord.interstellarBountyFactions.Contains(record.faction))
                {
                    // An interstellar power record is already active, update it
                    _AddReportToRecord(powerRecord, report);
                }
                else if (report.bounty)
                {
                    // A superpower bounty is already active. Any bounty from an aligned faction
                    // contributes to that superpower bounty, even if this faction was not yet tracked.
                    var reports = record.factionReports
                        .Where(r => r.crimeDef != Crime.None && r.crimeDef != Crime.Claim)
                        .ToList();
                    powerRecord.factionReports.AddRange(reports);
                    powerRecord.fines += record.fines;
                    powerRecord.bounties += record.bounties;
                    if (!powerRecord.interstellarBountyFactions.Contains(record.faction))
                    {
                        powerRecord.interstellarBountyFactions.Add(record.faction);
                    }
                    record.factionReports = record.factionReports.Except(reports).ToList();
                    record.fines = 0;
                    record.bounties = 0;

                    _AddReportToRecord(powerRecord, report);
                    RemoveRecordIfEmpty(record);
                }
                else
                {
                    _AddReportToRecord(record, report);
                }
            }
        }

        private static void _AddReportToRecord(FactionRecord record, FactionReport report)
        {
            record.factionReports.Add(report);
            if (report.bounty)
            {
                record.bounties += report.amount;
            }
            else if ( !report.bounty &&
                      !IsOnFootCrimeReport( report ) &&
                      record.bountiesIncurred.Any( r => !IsOnFootCrimeReport( r ) ) )
            {
                // This fine is converted to a bounty because we already have a bounty from this faction.
                record.bounties += report.amount;
                report.bounty = true;
            }
            else
            {
                record.fines += report.amount;
            }

            // When a bounty is incurred, we convert any pending fines to bounties
            if ( report.bounty && report.crimeDef != Crime.None && !IsOnFootCrimeReport( report ) )
            {
                var fineReports = record.factionReports
                    .Where( r => r.crimeDef != Crime.None &&
                                 r.crimeDef != Crime.Claim &&
                                 !r.bounty &&
                                 !IsOnFootCrimeReport( r ) )
                    .ToList();
                if (fineReports.Count > 0 )
                {
                    foreach (var fineReport in fineReports) { fineReport.bounty = true; }
                    record.fines -= Math.Min(record.fines, fineReports.Sum(r => r.amount));
                    record.bounties += fineReports.Sum(r => r.amount);
                }
            }
        }

        private static bool IsOnFootCrimeReport ( FactionReport report )
        {
            return report?.crimeDef?.edname?.StartsWith( "onFoot_", StringComparison.OrdinalIgnoreCase ) ?? false;
        }

        private async Task<bool> handleMissionFineAsync(DateTime timestamp, ulong missionid, long fine)
        {
            var update = false;
            var mission = ConfigService.Instance.missionMonitorConfiguration
                ?.missions
                ?.FirstOrDefault(m => m.missionid == missionid);
            if (mission != null)
            {
                update = await _handleMissionFineAsync(timestamp, mission, fine).ConfigureAwait(false);
            }
            return update;
        }

        internal async Task<bool> _handleMissionFineAsync(DateTime timestamp, Mission mission, long fine)
        {
            var update = false;

            if (mission?.faction != null)
            {
                var currentSystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;

                var report = new FactionReport(timestamp, false, Crime.MissionFine, currentSystem, fine)
                {
                    station = EDDI.Instance.GameState.CurrentStation?.name,
                    body = EDDI.Instance.GameState.CurrentStellarBody?.bodyname,
                };

                var record = GetRecordWithFaction(mission.faction) ?? 
                             await AddRecordAsync(mission.faction).ConfigureAwait(false);
                await AddReportToRecordAsync(record, report).ConfigureAwait(false);
                update = true;
            }
            return update;
        }

        private FactionRecord GetRecordWithFaction(string faction)
        {
            if (faction == null) { return null; }
            lock (recordLock)
            {
                return criminalrecord.FirstOrDefault(c =>
                    string.Equals(c.faction, faction, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public async Task GetFactionDataAsync(FactionRecord record, string homeSystem = null)
        {
            if (record == null || string.IsNullOrEmpty(record.faction) || record.faction == Properties.CrimeMonitor.blank_faction) { return; }

            // Get the faction and set faction record values
            var faction = await EDDI.Instance.DataProvider.FetchFactionByNameAsync( record.faction ).ConfigureAwait(false);
            record.Allegiance = faction?.Allegiance ?? Superpower.None;

            // Check faction with archived home systems
            if (homeSystems.TryGetValue(record.faction, out var factionHomeSystem))
            {
                record.system = factionHomeSystem;
                record.station = await GetFactionStationAsync(factionHomeSystem).ConfigureAwait(false);
                return;
            }

            if (faction != null && faction.presences.Count > 0 )
            {
                var factionSystems = faction.presences
                    .OrderByDescending(p => p.influence)
                    .Select(p => p.systemName).ToList();
                record.factionSystems = factionSystems;

                // If 'home system' is desiginated, check if system is part of faction presence
                if (homeSystem != null && factionSystems.Contains(homeSystem))
                {
                    record.system = homeSystem;
                    record.station = await GetFactionStationAsync(homeSystem).ConfigureAwait(false);
                    if (FindHomeSystem(record.faction, factionSystems) == null && !homeSystems.ContainsKey(record.faction))
                    {
                        // Save home system if not part of faction name and not previously saved
                        homeSystems.Add(record.faction, homeSystem);
                    }
                    return;
                }

                // Find 'home system' by matching faction name with presence and check for qualifying station
                homeSystem = FindHomeSystem(record.faction, factionSystems);
                if (homeSystem != null)
                {
                    var factionStation = await GetFactionStationAsync(homeSystem).ConfigureAwait(false);

                    // Station found meeting game/user requirements
                    if (factionStation != null)
                    {
                        record.system = homeSystem;
                        record.station = factionStation;
                        return;
                    }
                }

                // Check faction presences, by order of influence, for qualifying station
                foreach (var system in factionSystems)
                {
                    var factionStation = await GetFactionStationAsync(system).ConfigureAwait(false);
                    if (factionStation != null)
                    {
                        record.system = system;
                        record.station = factionStation;
                        return;
                    }
                }

                // Settle for highest influence faction presence, with no station found
                record.system = factionSystems.FirstOrDefault();
                record.station = null;
            }
        }

        private static async Task<string> GetFactionStationAsync(string factionSystem)
        {
            if (factionSystem == null) { return null; }
            var factionStarSystem = await EDDI.Instance.DataProvider.GetOrFetchStarSystemAsync(factionSystem, true, false).ConfigureAwait(false);

            if (factionStarSystem != null)
            {
                // Filter stations within the faction system which meet the station type prioritization,
                // max distance from the main star, game version, and landing pad size requirements
                var padSize = EDDI.Instance.GameState.CurrentShip?.Size ?? LandingPadSize.Large;
                var factionStations = !ConfigService.Instance.navigationMonitorConfiguration.prioritizeOrbitalStations && EDDI.Instance.GameState.inHorizons
                    ? factionStarSystem.stations.ToList()
                    : factionStarSystem.orbitalstations;
                factionStations = factionStations
                    .Where(s => s.Model != StationModel.FleetCarrier)
                    .Where(s => s.stationservices.Count > 0)
                    .Where(s => s.distancefromstar <= ConfigService.Instance.navigationMonitorConfiguration.maxSearchDistanceFromStarLs)
                    .Where(s => s.landingPads.LandingPadCheck(padSize))
                    .ToList();

                // Build list to find the faction station nearest to the main star
                var nearestList = new SortedList<decimal, string>();
                foreach (var station in factionStations)
                {
                    if (!nearestList.ContainsKey(station.distancefromstar ?? 0))
                    {
                        nearestList.Add(station.distancefromstar ?? 0, station.name);
                    }
                }

                // Faction station nearest to the main star
                return nearestList.Values.FirstOrDefault();
            }
            return null;
        }

        private static string FindHomeSystem(string faction, List<string> factionSystems)
        {
            // Look for system which is part of faction name
            foreach (var system in factionSystems)
            {
                var pattern = @"\b" + Regex.Escape(system) + @"\b";
                if (Regex.IsMatch(faction, pattern)) { return system; }
            }
            return null;
        }

        static void RaiseOnUIThread(EventHandler handler, object sender)
        {
            if (handler != null)
            {
                var uiSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
                if (uiSyncContext == null)
                {
                    handler(sender, EventArgs.Empty);
                }
                else
                {
                    uiSyncContext.Send(delegate { handler(sender, EventArgs.Empty); }, null);
                }
            }
        }
    }
}
