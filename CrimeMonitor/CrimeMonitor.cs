using EddiBgsService;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

namespace EddiCrimeMonitor
{
    /**
     * Monitor claims, fines, and bounties for the current ship
     */
    public class CrimeMonitor : IEddiMonitor
    {
        // Observable collection for us to handle changes
        public ObservableCollection<FactionRecord> criminalrecord { get; private set; }
        public long claims => criminalrecord.Sum(r => r.claims);
        public long fines => criminalrecord.Sum(r => r.fines);
        public long bounties => criminalrecord.Sum(r => r.bounties);
        public Dictionary<string, string> homeSystems;
        private DateTime updateDat;
        private string crimeAuthorityFaction;
        public readonly List<Target> shipTargets = new List<Target>();

        internal static readonly object recordLock = new object();
        public event EventHandler RecordUpdatedEvent;
        private readonly IBgsService bgsService;

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
            return true;
        }

        public CrimeMonitor()
        {
            bgsService = new BgsService();
            criminalrecord = new ObservableCollection<FactionRecord>();
            homeSystems = new Dictionary<string, string>();
            BindingOperations.CollectionRegistering += Record_CollectionRegistering;
            initializeCrimeMonitor();
        }

        public void initializeCrimeMonitor(CrimeMonitorConfiguration configuration = null)
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

        public void HandleProfile(JObject profile)
        { }

        public void HandleStatus ( Status status )
        { }

        public void PostHandle(Event @event)
        {
            if (@event is ShipSwappedEvent)
            {
                postHandleShipSwappedEvent();
            }
        }

        private void postHandleShipSwappedEvent()
        {
            // Update stations in minor faction records
            UpdateStations();
        }

        public void PreHandle(Event @event)
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
            else if (@event is ShipTargetedEvent targetedEvent)
            {
                handleShipTargetedEvent(targetedEvent);
            }
            else if (@event is BondAwardedEvent awardedEvent)
            {
                handleBondAwardedEvent(awardedEvent);
            }
            else if (@event is BondRedeemedEvent redeemedEvent)
            {
                handleBondRedeemedEvent(redeemedEvent);
            }
            else if (@event is BountyAwardedEvent bountyAwardedEvent)
            {
                handleBountyAwardedEvent(bountyAwardedEvent);
            }
            else if (@event is BountyIncurredEvent incurredEvent)
            {
                handleBountyIncurredEvent(incurredEvent);
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
                handleFineIncurredEvent(fineIncurredEvent);
            }
            else if (@event is FinePaidEvent finePaidEvent)
            {
                handleFinePaidEvent(finePaidEvent);
            }
            else if (@event is MissionAbandonedEvent abandonedEvent)
            {
                handleMissionAbandonedEvent(abandonedEvent);
            }
            else if (@event is MissionFailedEvent failedEvent)
            {
                handleMissionFailedEvent(failedEvent);
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
                _handleJumpedEvent(@event);
                writeRecord();
            }
        }

        private void _handleJumpedEvent(JumpedEvent @event)
        {
            shipTargets.Clear();
        }

        private void handleShipTargetedEvent(ShipTargetedEvent @event)
        {
            // System targets list may be 're-built' for the current system from Log Load
            var currentSystem = EDDI.Instance?.CurrentStarSystem;
            if ( @event.targetlocked )
            {
                var target = new Target();
                if (@event.scanstage >= 1)
                {
                    target = shipTargets.FirstOrDefault(t => t.name == @event.name);
                    if (target == null)
                    {
                        target = new Target(@event.name, @event.CombatRank, @event.ship);
                        shipTargets.Add(target);
                    }
                }
                if (@event.scanstage >= 3 && target.LegalStatus == null)
                {
                    target.faction = @event.faction;
                    target.Power = @event.Power;
                    target.LegalStatus = @event.LegalStatus;
                    target.bounty = @event.bounty;
                    target.Allegiance = currentSystem?.factions.FirstOrDefault(f => f.name == @event.faction)?.Allegiance ?? 
                                        bgsService.GetFactionByName( @event.faction )?.Allegiance;
                }
            }
        }

        private void handleBondAwardedEvent(BondAwardedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                _handleBondAwardedEvent(@event);
                writeRecord();
            }
        }

        private void _handleBondAwardedEvent(BondAwardedEvent @event)
        {
            string currentSystem = EDDI.Instance?.CurrentStarSystem?.systemname;

            // Get the victim faction data
            Faction faction = bgsService.GetFactionByName(@event.victimfaction);

            FactionReport report = new FactionReport(@event.timestamp, false, Crime.None, currentSystem, @event.reward)
            {
                station = EDDI.Instance?.CurrentStation?.name,
                body = EDDI.Instance?.CurrentStellarBody?.bodyname,
                victim = @event.victimfaction,
                victimAllegiance = (faction?.Allegiance ?? Superpower.None).invariantName
            };

            FactionRecord record = GetRecordWithFaction(@event.awardingfaction) 
                ?? AddRecord(@event.awardingfaction);
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

        private bool _handleBondRedeemedEvent(BondRedeemedEvent @event)
        {
            bool update = false;

            FactionRecord record = new FactionRecord();

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

        private void handleBountyAwardedEvent(BountyAwardedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                _handleBountyAwardedEvent(@event);
                writeRecord();
            }
        }

        private void _handleBountyAwardedEvent(BountyAwardedEvent @event, bool test = false)
        {
            // 20% bonus for Arissa Lavigny-Duval 'controlled' and 'exploited' systems
            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;

            // Default to 1.0 for unit testing
            double bonus = (!test && currentSystem?.Power == Power.FromEDName("ALavignyDuval")) ? 1.2 : 1.0;

            // Get the victim faction data
            Faction faction = bgsService.GetFactionByName(@event.faction);

            foreach (Reward reward in @event.rewards.ToList())
            {
                long amount = Convert.ToInt64(reward.amount * bonus);
                FactionReport report = new FactionReport(@event.timestamp, true, Crime.None, currentSystem?.systemname, amount)
                {
                    station = EDDI.Instance?.CurrentStation?.name,
                    body = EDDI.Instance?.CurrentStellarBody?.bodyname,
                    victim = @event.faction,
                    victimAllegiance = (faction.Allegiance ?? Superpower.None).invariantName
                };

                FactionRecord record = GetRecordWithFaction(reward.faction) 
                    ?? AddRecord(reward.faction);
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

        private bool _handleBountyRedeemedEvent(BountyRedeemedEvent @event)
        {
            bool update = false;

            foreach (Reward reward in @event.rewards.ToList())
            {
                FactionRecord record = new FactionRecord();

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

        private void handleBountyIncurredEvent(BountyIncurredEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                _handleBountyIncurredEvent(@event);
                writeRecord();
            }
        }

        private void _handleBountyIncurredEvent(BountyIncurredEvent @event)
        {
            crimeAuthorityFaction = @event.faction;
            var crime = Crime.FromEDName(@event.crimetype);
            var currentSystem = EDDI.Instance?.CurrentStarSystem?.systemname;

            // Get victim allegiance from the 'Ship targeted' data
            var target = shipTargets.FirstOrDefault(t => t.name == @event.victim);

            // Create a bounty report and add it to our record
            var report = new FactionReport(@event.timestamp, true, crime, currentSystem, @event.bounty)
            {
                station = EDDI.Instance?.CurrentStation?.name,
                body = EDDI.Instance?.CurrentStellarBody?.bodyname,
                victim = @event.victim,
                victimAllegiance = (target?.Allegiance ?? Superpower.None).invariantName
            };
            var record = GetRecordWithFaction(@event.faction)
                                   ?? AddRecord(@event.faction);
            AddReportToRecord(record, report);
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

        private bool _handleBountyPaidEvent(BountyPaidEvent @event)
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

        private void handleFineIncurredEvent(FineIncurredEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                _handleFineIncurredEvent(@event);
                writeRecord();
            }
        }

        private void _handleFineIncurredEvent(FineIncurredEvent @event)
        {
            crimeAuthorityFaction = @event.faction;
            Crime crime = Crime.FromEDName(@event.crimetype);
            string currentSystem = EDDI.Instance?.CurrentStarSystem?.systemname;
            FactionReport report = new FactionReport(@event.timestamp, false, crime, currentSystem, @event.fine)
            {
                station = EDDI.Instance?.CurrentStation?.name,
                body = EDDI.Instance?.CurrentStellarBody?.bodyname,
                victim = @event.victim
            };

            FactionRecord record = GetRecordWithFaction(@event.faction) ?? AddRecord(@event.faction);
            AddReportToRecord(record, report);
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

        private bool _handleFinePaidEvent(FinePaidEvent @event)
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

        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                if (_handleMissionAbandonedEvent(@event))
                {
                    writeRecord();
                }
            }
        }

        private bool _handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            if (@event.fine > 0)
            {
                return handleMissionFine(@event.timestamp, @event.missionid, @event.fine);
            }
            return false;
        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            if (@event.timestamp > updateDat || (@event.timestamp == updateDat && !@event.fromLoad))
            {
                updateDat = @event.timestamp;
                if (_handleMissionFailedEvent(@event))
                {
                    writeRecord();
                }
            }
        }

        private bool _handleMissionFailedEvent(MissionFailedEvent @event)
        {
            var update = false;
            if (@event.fine > 0)
            {
                update = handleMissionFine(@event.timestamp, @event.missionid, @event.fine);
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
            lock (recordLock)
            {
                return new Dictionary<string, Tuple<Type, object>>
                {
                    ["criminalrecord"] = new Tuple<Type, object>(typeof(List<FactionRecord>), criminalrecord.ToList()),
                    ["claims"] = new Tuple<Type, object>(typeof(long), claims),
                    ["fines"] = new Tuple<Type, object>(typeof(long), fines),
                    ["bounties"] = new Tuple<Type, object>(typeof(long), bounties ),
                    ["shiptargets"] = new Tuple<Type, object>(typeof(List<Target>), shipTargets.ToList())
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

        private FactionRecord AddRecord(string faction)
        {
            if (faction == null) { return null; }

            FactionRecord record = new FactionRecord(faction);
            var Allegiance = Superpower.FromNameOrEdName(faction);
            if (Allegiance == null)
            {
                GetFactionData(record);
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

        private void AddReportToRecord(FactionRecord record, FactionReport report)
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
                    powerRecord = AddRecord(record.allegiance);
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

        private bool handleMissionFine(DateTime timestamp, long missionid, long fine)
        {
            bool update = false;
            var mission = ConfigService.Instance.missionMonitorConfiguration
                ?.missions
                ?.FirstOrDefault(m => m.missionid == missionid);
            if (mission != null)
            {
                update = _handleMissionFine(timestamp, mission, fine);
            }
            return update;
        }

        public bool _handleMissionFine(DateTime timestamp, Mission mission, long fine)
        {
            bool update = false;

            if (mission?.faction != null)
            {
                string currentSystem = EDDI.Instance?.CurrentStarSystem?.systemname;

                FactionReport report = new FactionReport(timestamp, false, Crime.MissionFine, currentSystem, fine)
                {
                    station = EDDI.Instance?.CurrentStation?.name,
                    body = EDDI.Instance?.CurrentStellarBody?.bodyname,
                };

                var record = GetRecordWithFaction(mission.faction) ?? 
                             AddRecord(mission.faction);
                AddReportToRecord(record, report);
                update = true;
            }
            return update;
        }

        public FactionRecord GetRecordWithFaction(string faction)
        {
            if (faction == null) { return null; }
            lock (recordLock)
            {
                return criminalrecord.FirstOrDefault(c =>
                    string.Equals(c.faction, faction, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public void GetFactionData(FactionRecord record, string homeSystem = null)
        {
            if (record == null || string.IsNullOrEmpty(record.faction) || record.faction == Properties.CrimeMonitor.blank_faction) { return; }

            // Get the faction from Elite BGS and set faction record values
            Faction faction = bgsService.GetFactionByName(record.faction);
            record.Allegiance = faction.Allegiance;

            // Check faction with archived home systems
            if (homeSystems.TryGetValue(record.faction, out string result))
            {
                record.system = result;
                record.station = GetFactionStation(result);
                return;
            }

            if (faction.presences.Any())
            {
                List<string> factionSystems = faction.presences.Select(p => p.systemName).ToList();
                factionSystems = faction.presences
                    .OrderByDescending(p => p.influence)
                    .Select(p => p.systemName).ToList();
                record.factionSystems = factionSystems;

                // If 'home system' is desiginated, check if system is part of faction presence
                if (homeSystem != null && factionSystems.Contains(homeSystem))
                {
                    record.system = homeSystem;
                    record.station = GetFactionStation(homeSystem);
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
                    string factionStation = GetFactionStation(homeSystem);

                    // Station found meeting game/user requirements
                    if (factionStation != null)
                    {
                        record.system = homeSystem;
                        record.station = factionStation;
                        return;
                    }
                }

                // Check faction presences, by order of influence, for qualifying station
                foreach (string system in factionSystems)
                {
                    string factionStation = GetFactionStation(system);
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

        public string GetFactionStation(string factionSystem)
        {
            if (factionSystem == null) { return null; }
            StarSystem factionStarSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(factionSystem, true, true, false, true, false);

            if (factionStarSystem != null)
            {
                // Filter stations within the faction system which meet the station type prioritization,
                // max distance from the main star, game version, and landing pad size requirements
                LandingPadSize padSize = EDDI.Instance?.CurrentShip?.Size ?? LandingPadSize.Large;
                List<Station> factionStations = !ConfigService.Instance.navigationMonitorConfiguration.prioritizeOrbitalStations && (EDDI.Instance?.inHorizons ?? false)
                    ? factionStarSystem.stations
                    : factionStarSystem.orbitalstations;
                factionStations = factionStations
                    .Where(s => s.Model != StationModel.FleetCarrier)
                    .Where(s => s.stationservices.Count > 0)
                    .Where(s => s.distancefromstar <= ConfigService.Instance.navigationMonitorConfiguration.maxSearchDistanceFromStarLs)
                    .Where(s => s.LandingPadCheck(padSize))
                    .ToList();

                // Build list to find the faction station nearest to the main star
                SortedList<decimal, string> nearestList = new SortedList<decimal, string>();
                foreach (Station station in factionStations)
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

        public void UpdateStations()
        {
            Thread stationUpdateThread = new Thread(() =>
            {
                lock (recordLock)
                {
                    foreach (FactionRecord record in criminalrecord.ToList())
                    {
                        var Allegiance = Superpower.FromNameOrEdName(record.faction);
                        if (Allegiance == null)
                        {
                            record.station = GetFactionStation(record.system);
                        }
                    }
                }
                writeRecord();
            })
            {
                IsBackground = true
            };
            stationUpdateThread.Start();
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
