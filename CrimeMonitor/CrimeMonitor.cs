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
        // Observable collection for us to handle changes
        public ObservableCollection<FactionRecord> criminalrecord { get; }
        private long claims => criminalrecord.Sum(r => r.claims);
        private long fines => criminalrecord.Sum(r => r.fines);
        private long bounties => criminalrecord.Sum(r => r.bounties);
        private Dictionary<string, string> homeSystems;
        private DateTime updateDat;
        private string crimeAuthorityFaction;
        public readonly List<Target> shipTargets = new();

        internal static readonly object recordLock = new();
        public event EventHandler RecordUpdatedEvent;

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
            criminalrecord = new ObservableCollection<FactionRecord>();
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
            else if (@event is JumpedEvent jumpedEvent)
            {
                handleJumpedEvent(jumpedEvent);
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

        private void handleLocationEvent(LocationEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                writeRecord();
            }
        }

        private void handleJumpedEvent(JumpedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                _handleJumpedEvent();
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
            var currentSystem = EDDI.Instance.CurrentStarSystem?.systemname;

            // Get the victim faction data
            var faction = await EDDI.Instance.DataProvider.FetchFactionByNameAsync( @event.victimfaction ).ConfigureAwait(false);

            var report = new FactionReport(@event.timestamp, false, Crime.None, currentSystem, @event.reward)
            {
                station = EDDI.Instance.CurrentStation?.name,
                body = EDDI.Instance.CurrentStellarBody?.bodyname,
                victim = @event.victimfaction,
                victimAllegiance = (faction?.Allegiance ?? Superpower.None).invariantName
            };

            var record = GetRecordWithFaction(@event.awardingfaction) 
                ?? await AddRecordAsync(@event.awardingfaction).ConfigureAwait(false);
            record.factionReports.Add(report);
            record.claims += @event.reward;
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
            bool update = false;

            FactionRecord record;

            // Calculate amount, broker fees
            decimal percentage = (100 - (@event.brokerpercentage ?? 0)) / 100;
            long amount = Convert.ToInt64(Math.Ceiling(@event.rewards[0].amount / percentage));

            // Handle journal event from Interstellar Factors transaction (FDEV bug)
            if (string.IsNullOrEmpty(@event.rewards[0].faction))
            {
                List<string> systemFactions = EDDI.Instance.CurrentStarSystem?.factions.Select(f => f.name).ToList();

                // Get record which matches a system faction and the bond claims amount
                lock (recordLock)
                {
                    record = criminalrecord
                        .Where(r => systemFactions?.Contains(r.faction) ?? false)
                        .FirstOrDefault(r => r.bondsAmount == amount);
                }
            }
            else
            {
                record = GetRecordWithFaction(@event.rewards[0].faction);
            }

            if (record != null)
            {
                // Get all bond claims, excluding the discrepancy report
                List<FactionReport> reports = record.factionReports
                    .Where(r => !r.bounty && r.crimeDef == Crime.None).ToList();
                if (reports.Any())
                {
                    long total = reports.Sum(r => r.amount);

                    // Check for discrepancy in logged bond claims
                    if (total < amount)
                    {
                        // Adjust the discrepancy report & remove when zeroed out
                        FactionReport report = record.factionReports
                            .FirstOrDefault(r => r.crimeDef == Crime.Claim);
                        if (report != null)
                        {
                            report.amount -= Math.Min(amount - total, report.amount);
                            if (report.amount == 0) { reports.Add(report); }
                        }
                    }
                    // Remove associated bonds claims
                    record.factionReports = record.factionReports.Except(reports).ToList();
                }
                // Adjust the total claims
                record.claims -= Math.Min(amount, record.claims);

                RemoveRecordIfEmpty(record);
                update = true;
            }
            return update;
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

        internal async Task _handleBountyAwardedEventAsync(BountyAwardedEvent @event, bool test = false)
        {
            // 20% bonus for Arissa Lavigny-Duval 'controlled' and 'exploited' systems
            var currentSystem = EDDI.Instance.CurrentStarSystem;

            // Default to 1.0 for unit testing
            var bonus = !test && currentSystem?.Power == Power.ALavignyDuval ? 1.2 : 1.0;

            // Get the victim faction data
            var faction = await EDDI.Instance.DataProvider.FetchFactionByNameAsync( @event.faction ).ConfigureAwait(false);

            foreach (var reward in @event.rewards.ToList())
            {
                var amount = Convert.ToInt64(reward.amount * bonus);
                var report = new FactionReport(@event.timestamp, true, Crime.None, currentSystem?.systemname, amount)
                {
                    station = EDDI.Instance.CurrentStation?.name,
                    body = EDDI.Instance.CurrentStellarBody?.bodyname,
                    victim = @event.faction,
                    victimAllegiance = (faction?.Allegiance ?? Superpower.None).invariantName
                };

                var record = GetRecordWithFaction(reward.faction) 
                    ?? await AddRecordAsync(reward.faction).ConfigureAwait(false);
                record.factionReports.Add(report);
                record.claims += amount;
            }
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
            bool update = false;

            foreach (Reward reward in @event.rewards.ToList())
            {
                FactionRecord record;

                // Calculate amount, before broker fees
                decimal percentage = (100 - (@event.brokerpercentage ?? 0)) / 100;
                long amount = Convert.ToInt64(Math.Ceiling(reward.amount / percentage));

                // Handle journal event from Interstellar Factors transaction (FDEV bug)
                if (string.IsNullOrEmpty(reward.faction))
                {
                    lock (recordLock)
                    {
                        record = criminalrecord.FirstOrDefault(r => r.bountiesAmount == amount);
                    }
                }
                else
                {
                    record = GetRecordWithFaction(reward.faction);
                }

                if (record != null)
                {
                    // Get all bounty claims, excluding the discrepancy report
                    var reports = record.factionReports
                        .Where(r => r.bounty && r.crimeDef == Crime.None).ToList();
                    if (reports.Any())
                    {
                        long total = reports.Sum(r => r.amount);

                        // Check for discrepancy in logged bounty claims
                        if (total < amount)
                        {
                            // Adjust the discrepancy report & remove when zeroed out
                            FactionReport report = record.factionReports
                                .FirstOrDefault(r => r.crimeDef == Crime.Claim);
                            if (report != null)
                            {
                                report.amount -= Math.Min(amount - total, report.amount);
                                if (report.amount == 0) { reports.Add(report); }
                            }
                        }
                        // Remove associated bounty claims
                        record.factionReports = record.factionReports.Except(reports).ToList();
                    }
                    // Adjust the total claims
                    record.claims -= Math.Min(amount, record.claims);

                    RemoveRecordIfEmpty(record);
                    update = true;
                }
            }
            return update;
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
            var currentSystem = EDDI.Instance.CurrentStarSystem?.systemname;

            // Get victim allegiance from the 'Ship targeted' data
            Target target;
            lock ( recordLock )
            {
                target = shipTargets.FirstOrDefault(t => t.name == @event.victim);
            }

            // Create a bounty report and add it to our record
            var report = new FactionReport(@event.timestamp, true, crime, currentSystem, @event.bounty)
            {
                station = EDDI.Instance.CurrentStation?.name,
                body = EDDI.Instance.CurrentStellarBody?.bodyname,
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
            void PayBounty(FactionRecord record)
            {
                // Get all bounties incurred, excluding the discrepancy report
                List<FactionReport> reports = record.factionReports
                    .Where(r => r.crimeDef != Crime.None && r.crimeDef != Crime.Bounty)
                    .ToList();

                // Check for discrepancy in logged bounties incurred
                long total = reports.Sum(r => r.amount);
                if (total < @event.amount)
                {
                    // Adjust the discrepancy report & remove when zeroed out
                    FactionReport report = record.factionReports
                        .FirstOrDefault(r => r.crimeDef == Crime.Bounty);
                    if (report != null)
                    {
                        report.amount -= Math.Min(@event.amount - total, report.amount);
                        if (report.amount == 0) { reports.Add(report); }
                    }
                }
                // Remove associated records
                record.factionReports = record.factionReports.Except(reports).ToList();

                // Adjust the total bounties incurred amount
                record.bounties -= Math.Min(@event.amount, record.bounties);

                RemoveRecordIfEmpty(record);
            }

            bool update = false;
            lock (recordLock)
            {
                foreach (FactionRecord record in criminalrecord.ToList()
                             // Filter out records from factions within the current star system
                             .Where(r => !(EDDI.Instance.CurrentStarSystem?.factions?.Select(f => f.name) ?? new List<string>()).Contains(r.faction)))
                {
                    if (@event.allbounties || record.faction == @event.faction)
                    {
                        PayBounty(record);
                        update = true;
                        if (record.faction == @event.faction) { break; }
                    }
                }
            }
            if (!update)
            {
                // The bounty may have been converted to a Superpower bounty. See if we can find a record w/ a matching bounty.
                var superpower = Superpower.FromNameOrEdName(@event.faction);
                if (superpower != null)
                {
                    lock (recordLock)
                    {
                        var record = criminalrecord.ToList().SingleOrDefault(r => r.Allegiance == superpower && r.bounties == @event.amount);
                        if (record != null)
                        {
                            PayBounty(record);
                            update = true;
                        }
                    }
                }
            }

            return update;
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
            Crime crime = Crime.FromEDName(@event.crimetype);
            string currentSystem = EDDI.Instance.CurrentStarSystem?.systemname;
            FactionReport report = new FactionReport(@event.timestamp, false, crime, currentSystem, @event.fine)
            {
                station = EDDI.Instance.CurrentStation?.name,
                body = EDDI.Instance.CurrentStellarBody?.bodyname,
                victim = @event.victim
            };

            FactionRecord record = GetRecordWithFaction(@event.faction) ?? await AddRecordAsync(@event.faction).ConfigureAwait(false);
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
            // This event may trigger for both bounties paid and fines paid (FDev bug)
            bool update = false;
            lock (recordLock)
            {
                foreach (FactionRecord record in criminalrecord.ToList())
                {
                    if (@event.allfines || record.faction == @event.faction)
                    {
                        // Get all fines incurred, excluding the discrepancy report
                        List<FactionReport> reports = record.factionReports
                            .Where(r => r.crimeDef != Crime.None && r.crimeDef != Crime.Fine)
                            .ToList();
                        long total = reports.Sum(r => r.amount);

                        // Check for discrepancy in logged fines incurred
                        if (total < @event.amount)
                        {
                            // Adjust the discrepancy report & remove when zeroed out
                            FactionReport report = record.factionReports
                                .FirstOrDefault(r => r.crimeDef == Crime.Fine);
                            if (report != null)
                            {
                                report.amount -= Math.Min(@event.amount - total, report.amount);
                                if (report.amount == 0) { reports.Add(report); }
                            }
                        }
                        // Remove associated records
                        record.factionReports = record.factionReports.Except(reports).ToList();

                        // Adjust the total fines incurred amount
                        record.fines -= Math.Min(@event.amount, record.fines);

                        RemoveRecordIfEmpty(record);
                        update = true;
                        if (record.faction == @event.faction) { break; }
                    }
                }
                return update;
            }
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
            void RemoveCriminalRecords(string faction = null)
            {
                // Update the criminal record fines and bounties for each faction, as appropriate.
                lock (recordLock)
                {
                    foreach (FactionRecord record in criminalrecord.ToList())
                    {
                        if ((!string.IsNullOrEmpty(faction) && faction == record.faction) || string.IsNullOrEmpty(faction))
                        {
                            var crimeReports = record.factionReports
                                .Where(r => r.crimeDef != Crime.None && r.crimeDef != Crime.Claim)
                                .ToList();
                            // Remove all pending fines and bounties (from a named faction, if a faction name is given)
                            string forFaction = !string.IsNullOrEmpty(faction) ? $"for faction {record.faction} " : "";
                            Logging.Debug($"Paid {@event.price} credits to resolve fines and bounties {forFaction} (expected {crimeReports.Sum(r => r.amount)}).");
                            record.factionReports = record.factionReports.Except(crimeReports).ToList();
                            RemoveRecordIfEmpty(record);
                        }
                    }
                }
            }

            void RemoveClaimsRecords()
            {
                // Update the criminal record pending claims for each faction, as appropriate.
                lock (recordLock)
                {
                    foreach (FactionRecord record in criminalrecord.ToList())
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
                    RemoveCriminalRecords(crimeAuthorityFaction);
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
                    RemoveCriminalRecords(EDDI.Instance.CurrentStation?.Faction?.name);
                    break;
                }
            }
        }

        public IDictionary<string, Tuple<Type, object>> GetVariables()
        {
            lock ( recordLock )
            {
                return new Dictionary<string, Tuple<Type, object>>
                {
                    [ "criminalrecord" ] = new( typeof(List<FactionRecord>), criminalrecord.ToList() ),
                    [ "claims" ] = new( typeof(long), claims ),
                    [ "fines" ] = new( typeof(long), fines ),
                    [ "bounties" ] = new( typeof(long), bounties ),
                    [ "shiptargets" ] = new( typeof(List<Target>), shipTargets.ToList() )
                };
            }
        }

        public void writeRecord()
        {
            lock (recordLock)
            {
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
                configuration = configuration ?? ConfigService.Instance.crimeMonitorConfiguration;
                homeSystems = configuration.homeSystems;
                updateDat = configuration.updatedat;

                // Build a new criminal record
                List<FactionRecord> records = configuration.criminalrecord.OrderBy(c => c.faction).ToList();
                criminalrecord.Clear();
                foreach (FactionRecord record in records)
                {
                    criminalrecord.Add(record);
                }
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
            if (record.factionReports?.Any() ?? false) { return; }
            _RemoveRecord(record);
        }

        public void _RemoveRecord(FactionRecord record)
        {
            string faction = record.faction.ToLowerInvariant();
            lock (recordLock)
            {
                for (int i = 0; i < criminalrecord.Count; i++)
                {
                    if (criminalrecord[i].faction.ToLowerInvariant() == faction)
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
            }
        }

        private void _AddReportToRecord(FactionRecord record, FactionReport report)
        {
            record.factionReports.Add(report);
            if (report.bounty)
            {
                record.bounties += report.amount;
            }
            else if ( !report.bounty && record.bounties > 0 )
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
            if (report.bounty && report.crimeDef != Crime.None)
            {
                var fineReports = record.factionReports
                    .Where(r => r.crimeDef != Crime.None && r.crimeDef != Crime.Claim && !r.bounty)
                    .ToList();
                if (fineReports.Any())
                {
                    foreach (var fineReport in fineReports) { fineReport.bounty = true; }
                    record.fines -= Math.Min(record.fines, fineReports.Sum(r => r.amount));
                    record.bounties += fineReports.Sum(r => r.amount);
                }
            }
        }

        private async Task<bool> handleMissionFineAsync(DateTime timestamp, ulong missionid, long fine)
        {
            bool update = false;
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
            bool update = false;

            if (mission?.faction != null)
            {
                string currentSystem = EDDI.Instance.CurrentStarSystem?.systemname;

                FactionReport report = new FactionReport(timestamp, false, Crime.MissionFine, currentSystem, fine)
                {
                    station = EDDI.Instance.CurrentStation?.name,
                    body = EDDI.Instance.CurrentStellarBody?.bodyname,
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

            if (faction?.presences.Any() ?? false)
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

        private async Task<string> GetFactionStationAsync(string factionSystem)
        {
            if (factionSystem == null) { return null; }
            var factionStarSystem = await EDDI.Instance.DataProvider.GetOrFetchStarSystemAsync(factionSystem, true, false).ConfigureAwait(false);

            if (factionStarSystem != null)
            {
                // Filter stations within the faction system which meet the station type prioritization,
                // max distance from the main star, game version, and landing pad size requirements
                var padSize = EDDI.Instance.CurrentShip?.Size ?? LandingPadSize.Large;
                var factionStations = !ConfigService.Instance.navigationMonitorConfiguration.prioritizeOrbitalStations && EDDI.Instance.inHorizons
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

        private string FindHomeSystem(string faction, List<string> factionSystems)
        {
            // Look for system which is part of faction name
            foreach (string system in factionSystems)
            {
                string pattern = @"\b" + Regex.Escape(system) + @"\b";
                if (Regex.IsMatch(faction, pattern)) { return system; }
            }
            return null;
        }

        static void RaiseOnUIThread(EventHandler handler, object sender)
        {
            if (handler != null)
            {
                SynchronizationContext uiSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
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
