﻿using Eddi;
using EddiBgsService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiMissionMonitor;
using Newtonsoft.Json;
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
    public class CrimeMonitor : EDDIMonitor
    {
        // Observable collection for us to handle changes
        public ObservableCollection<FactionRecord> criminalrecord { get; private set; }
        public long claims;
        public long fines;
        public long bounties;
        public int? maxStationDistanceFromStarLs;
        public bool prioritizeOrbitalStations;
        public string targetSystem;
        public Dictionary<string, string> homeSystems;
        private DateTime updateDat;

        public List<Target> shipTargets = new List<Target>();

        private static readonly object recordLock = new object();
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

        public void EnableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(criminalrecord, recordLock); });
        }

        public void DisableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(criminalrecord); });
        }

        public void HandleProfile(JObject profile)
        {
        }

        public void PostHandle(Event @event)
        {
        }

        public void PreHandle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is LocationEvent)
            {
                handleLocationEvent((LocationEvent)@event);
            }
            else if (@event is JumpedEvent)
            {
                handleJumpedEvent((JumpedEvent)@event);
            }
            else if (@event is ShipTargetedEvent)
            {
                handleShipTargetedEvent((ShipTargetedEvent)@event);
            }
            else if (@event is BondAwardedEvent)
            {
                handleBondAwardedEvent((BondAwardedEvent)@event);
            }
            else if (@event is BondRedeemedEvent)
            {
                handleBondRedeemedEvent((BondRedeemedEvent)@event);
            }
            else if (@event is BountyAwardedEvent)
            {
                handleBountyAwardedEvent((BountyAwardedEvent)@event);
            }
            else if (@event is BountyIncurredEvent)
            {
                handleBountyIncurredEvent((BountyIncurredEvent)@event);
            }
            else if (@event is BountyPaidEvent)
            {
                handleBountyPaidEvent((BountyPaidEvent)@event);
            }
            else if (@event is BountyRedeemedEvent)
            {
                handleBountyRedeemedEvent((BountyRedeemedEvent)@event);
            }
            else if (@event is FineIncurredEvent)
            {
                handleFineIncurredEvent((FineIncurredEvent)@event);
            }
            else if (@event is FinePaidEvent)
            {
                handleFinePaidEvent((FinePaidEvent)@event);
            }
            else if (@event is MissionAbandonedEvent)
            {
                handleMissionAbandonedEvent((MissionAbandonedEvent)@event);
            }
            else if (@event is MissionFailedEvent)
            {
                handleMissionFailedEvent((MissionFailedEvent)@event);
            }
            else if (@event is DiedEvent)
            {
                handleDiedEvent((DiedEvent)@event);
            }
        }

        private void handleLocationEvent(LocationEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                targetSystem = @event.systemname;
                updateDat = @event.timestamp;
                writeRecord();
            }
        }

        private void handleJumpedEvent(JumpedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleJumpedEvent(@event);
                writeRecord();
            }
        }

        private void _handleJumpedEvent(JumpedEvent @event)
        {
            shipTargets.Clear();
            targetSystem = @event.system;
        }

        private void handleShipTargetedEvent(ShipTargetedEvent @event)
        {
            // System targets list may be 're-built' for the current system from Log Load
            string currentSystem = EDDI.Instance?.CurrentStarSystem?.systemname;
            if (targetSystem == null) { targetSystem = currentSystem; }
            if (@event.targetlocked && currentSystem == targetSystem)
            {
                Target target = new Target();
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
                    Faction faction = bgsService.GetFactionByName(@event.faction);
                    target.Power = @event.Power ?? Power.None;

                    // Prioritize power allegiance (when present) over faction
                    target.Allegiance = @event.Power != Power.None
                        ? @event.Power?.Allegiance
                        : faction?.Allegiance;

                    target.LegalStatus = @event.LegalStatus;
                    target.bounty = @event.bounty;
                }
            }
        }

        private void handleBondAwardedEvent(BondAwardedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleBondAwardedEvent(@event);
                writeRecord();
            }
        }

        private void _handleBondAwardedEvent(BondAwardedEvent @event)
        {
            int shipId = EDDI.Instance?.CurrentShip?.LocalId ?? 0;
            string currentSystem = EDDI.Instance?.CurrentStarSystem?.systemname;

            // Get the victim faction data
            Faction faction = bgsService.GetFactionByName(@event.victimfaction);

            FactionReport report = new FactionReport(@event.timestamp, false, shipId, Crime.None, currentSystem, @event.reward)
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
            if (@event.timestamp > updateDat)
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
                record = criminalrecord
                    .Where(r => systemFactions.Contains(r.faction))
                    .FirstOrDefault(r => r.bondsAmount == amount);
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
                if (reports?.Any() ?? false)
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

                RemoveRecord(record);
                update = true;
            }
            return update;
        }

        private void handleBountyAwardedEvent(BountyAwardedEvent @event)
        {
            if (@event.timestamp > updateDat)
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
                int shipId = EDDI.Instance?.CurrentShip?.LocalId ?? 0;
                long amount = Convert.ToInt64(reward.amount * bonus);
                FactionReport report = new FactionReport(@event.timestamp, true, shipId, Crime.None, currentSystem?.systemname, amount)
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
            if (@event.timestamp > updateDat)
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
                    record = criminalrecord.FirstOrDefault(r => r.bountiesAmount == amount);
                }
                else
                {
                    record = GetRecordWithFaction(reward.faction);
                }

                if (record != null)
                {
                    // Get all bounty claims, excluding the discrepancy report
                    List<FactionReport> reports = record.factionReports
                        .Where(r => r.bounty && r.crimeDef == Crime.None).ToList();
                    if (reports?.Any() ?? false)
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

                    RemoveRecord(record);
                    update = true;
                }
            }
            return update;
        }

        private void handleBountyIncurredEvent(BountyIncurredEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleBountyIncurredEvent(@event);
                writeRecord();
            }

        }

        private void _handleBountyIncurredEvent(BountyIncurredEvent @event)
        {
            int shipId = EDDI.Instance?.CurrentShip?.LocalId ?? 0;
            Crime crime = Crime.FromEDName(@event.crimetype);
            string currentSystem = EDDI.Instance?.CurrentStarSystem?.systemname;

            // Get victim allegiance from the 'Ship targeted' data
            Target target = shipTargets.FirstOrDefault(t => t.name == @event.victim);

            FactionReport report = new FactionReport(@event.timestamp, true, shipId, crime, currentSystem, @event.bounty)
            {
                station = EDDI.Instance?.CurrentStation?.name,
                body = EDDI.Instance?.CurrentStellarBody?.bodyname,
                victim = @event.victim,
                victimAllegiance = (target?.Allegiance ?? Superpower.None).invariantName
            };

            FactionRecord record = GetRecordWithFaction(@event.faction) 
                ?? AddRecord(@event.faction);
            AddCrimeToRecord(record, report);
        }

        private void handleBountyPaidEvent(BountyPaidEvent @event)
        {
            if (@event.timestamp > updateDat)
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
            bool update = false;

            foreach (FactionRecord record in criminalrecord.ToList())
            {
                // If paid at 'Legal Facilities', bounties are grouped by superpower
                bool match = @event.brokerpercentage == null ? record.faction == @event.faction : record.Allegiance.invariantName == @event.faction;
                if (@event.allbounties || match)
                {
                    // Get all bounties incurred, excluding the discrepancy report
                    // Note that all bounties are assigned to the ship, not the commander
                    List<FactionReport> reports = record.factionReports
                        .Where(r => r.bounty && r.crimeDef != Crime.None && r.crimeDef != Crime.Bounty && r.shipId == @event.shipid)
                        .ToList();
                    long total = reports?.Sum(r => r.amount) ?? 0;

                    // Check for discrepancy in logged bounties incurred
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
                    // Remove associated bounties
                    record.factionReports = record.factionReports.Except(reports).ToList();

                    // Adjust the total bounties incurred amount
                    record.bounties -= Math.Min(@event.amount, record.bounties);

                    RemoveRecord(record);
                    update = true;
                }
            }
            return update;
        }

        private void handleFineIncurredEvent(FineIncurredEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleFineIncurredEvent(@event);
                writeRecord();
            }
        }

        private void _handleFineIncurredEvent(FineIncurredEvent @event)
        {
            int shipId = EDDI.Instance?.CurrentShip?.LocalId ?? 0;
            Crime crime = Crime.FromEDName(@event.crimetype);
            string currentSystem = EDDI.Instance?.CurrentStarSystem?.systemname;
            FactionReport report = new FactionReport(@event.timestamp, false, shipId, crime, currentSystem, @event.fine)
            {
                station = EDDI.Instance?.CurrentStation?.name,
                body = EDDI.Instance?.CurrentStellarBody?.bodyname,
                victim = @event.victim
            };

            FactionRecord record = GetRecordWithFaction(@event.faction) ?? AddRecord(@event.faction);
            AddCrimeToRecord(record, report);
        }

        private void handleFinePaidEvent(FinePaidEvent @event)
        {
            if (@event.timestamp > updateDat)
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
            bool update = false;

            foreach (FactionRecord record in criminalrecord.ToList())
            {
                // If paid at 'Legal Facilities', fines are grouped by superpower
                bool match = @event.brokerpercentage == null ? record.faction == @event.faction : record.Allegiance.invariantName == @event.faction;
                if (@event.allfines || match)
                {
                    // Get all fines incurred, excluding the discrepancy report
                    // Note that all fines are assigned to the ship, not the commander
                    List<FactionReport> reports = record.factionReports
                        .Where(r => !r.bounty && r.crimeDef != Crime.None && r.crimeDef != Crime.Fine && r.shipId == @event.shipid)
                        .ToList();
                    long total = reports?.Sum(r => r.amount) ?? 0;

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
                    // Remove associated fines
                    record.factionReports = record.factionReports.Except(reports).ToList();

                    // Adjust the total fines incurred amount
                    record.fines -= Math.Min(@event.amount, record.fines);

                    RemoveRecord(record);
                    update = true;
                }
            }
            return update;
        }

        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            if (@event.timestamp > updateDat)
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
            bool update = false;

            if (@event.fine > 0 && @event.missionid != null)
            {
                update = handleMissionFine(@event.timestamp, @event.missionid ?? 0, @event.fine);
            }
            return update;
        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            if (@event.timestamp > updateDat)
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
            bool update = false;

            if (@event.fine > 0 && @event.missionid != null)
            {
                update = handleMissionFine(@event.timestamp, @event.missionid ?? 0, @event.fine);
            }
            return update;
        }

        private void handleDiedEvent(DiedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleDiedEvent(@event);
                writeRecord();
            }
        }

        private void _handleDiedEvent(DiedEvent _)
        {
            List<FactionReport> reports = new List<FactionReport>();

            // Remove all pending claims from criminal record
            foreach (FactionRecord record in criminalrecord.ToList())
            {
                reports = record.factionReports
                    .Where(r => r.crimeDef == Crime.None || r.crimeDef == Crime.Claim)
                    .ToList();
                if (reports != null)
                {
                    // Remove all pending claims from faction
                    record.factionReports = record.factionReports.Except(reports).ToList();
                }
                record.claims = 0;
            }
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["criminalrecord"] = new List<FactionRecord>(criminalrecord),
                ["claims"] = claims,
                ["fines"] = fines,
                ["bounties"] = bounties,
                ["orbitalpriority"] = prioritizeOrbitalStations,
                ["shiptargets"] = new List<Target>(shipTargets)
            };
            return variables;
        }

        public void writeRecord()
        {
            lock (recordLock)
            {
                // Write criminal configuration with current criminal record
                claims = criminalrecord.Sum(r => r.claims);
                fines = criminalrecord.Sum(r => r.fines);
                bounties = criminalrecord.Sum(r => r.bounties);
                CrimeMonitorConfiguration configuration = new CrimeMonitorConfiguration()
                {
                    criminalrecord = criminalrecord,
                    claims = claims,
                    fines = fines,
                    bounties = bounties,
                    maxStationDistanceFromStarLs = maxStationDistanceFromStarLs,
                    prioritizeOrbitalStations = prioritizeOrbitalStations,
                    targetSystem = targetSystem,
                    homeSystems = homeSystems,
                    updatedat = updateDat
                };
                configuration.ToFile();
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(RecordUpdatedEvent, criminalrecord);
        }

        public void readRecord(CrimeMonitorConfiguration configuration = null)
        {
            lock (recordLock)
            {
                // Obtain current criminal record from configuration
                configuration = configuration ?? CrimeMonitorConfiguration.FromFile();
                claims = configuration.claims;
                fines = configuration.fines;
                bounties = configuration.bounties;
                maxStationDistanceFromStarLs = configuration.maxStationDistanceFromStarLs ?? Constants.maxStationDistanceDefault;
                prioritizeOrbitalStations = configuration.prioritizeOrbitalStations;
                targetSystem = configuration.targetSystem;
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
            Superpower Allegiance = Superpower.FromNameOrEdName(faction);
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

        private void RemoveRecord(FactionRecord record)
        {
            // Check if claims or crimes are pending
            if ((record.factionReports?.Any() ?? false)) { return; }
            {
                _RemoveRecord(record);
            }
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

        private void AddCrimeToRecord(FactionRecord record, FactionReport report)
        {
            if (record is null || report is null) { return; }

            var total = record.fines + record.bounties + report.amount;
            var powerRecord = GetRecordWithFaction(record.allegiance);

            if (powerRecord == null && total <= 2000000) 
            {
                // Add new report to the minor faction record
                _AddCrimeToRecord(record, report); 
            }
            else
            {
                // Minor faction crimes are converted to an interstellar power record, owned by the faction's aligned
                // superpower, when total fines & bounties incurred exceed 2 million credits
                if (powerRecord == null) 
                {
                    // Add a new interstellar bounty. 
                    // Transfer existing fines and bounties incurred to the interstellar power record
                    // Collect all minor faction fines and bounties incurred
                    powerRecord = AddRecord(record.allegiance);
                    List<FactionReport> reports = record.factionReports
                        .Where(r => r.crimeDef != Crime.None && r.crimeDef != Crime.Claim).ToList();
                    powerRecord.factionReports.AddRange(reports);
                    powerRecord.fines += record.fines;
                    powerRecord.bounties += record.bounties;
                    powerRecord.interstellarBountyFactions.Add(record.faction);
                    record.factionReports = record.factionReports.Except(reports).ToList();
                    record.fines = 0;
                    record.bounties = 0;

                    // Add new report to the interstellar power record and remove minor faction record if no pending claims
                    _AddCrimeToRecord(powerRecord, report);
                    RemoveRecord(record);
                }
                else if (powerRecord.interstellarBountyFactions.Contains(record.faction))
                {
                    // An interstellar power record is already active, update it
                    _AddCrimeToRecord(powerRecord, report);
                }
            }
        }

        private void _AddCrimeToRecord(FactionRecord record, FactionReport report)
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
        }

        private bool handleMissionFine(DateTime timestamp, long missionid, long fine)
        {
            bool update = false;
            MissionMonitor missionMonitor = (MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor");
            Mission mission = missionMonitor?.GetMissionWithMissionId(missionid);
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
                int shipId = EDDI.Instance?.CurrentShip?.LocalId ?? 0;
                Crime crime = Crime.FromEDName("missionFine");
                string currentSystem = EDDI.Instance?.CurrentStarSystem?.systemname;

                FactionReport report = new FactionReport(timestamp, false, shipId, crime, currentSystem, fine)
                {
                    station = EDDI.Instance?.CurrentStation?.name,
                    body = EDDI.Instance?.CurrentStellarBody?.bodyname,
                };

                FactionRecord record = GetRecordWithFaction(mission.faction);
                if (record == null)
                {
                    record = AddRecord(mission.faction);
                }
                AddCrimeToRecord(record, report);
                update = true;
            }
            return update;
        }

        public FactionRecord GetRecordWithFaction(string faction)
        {
            if (faction == null)
            {
                return null;
            }
            return criminalrecord.FirstOrDefault(c => 
                string.Equals(c.faction, faction, StringComparison.InvariantCultureIgnoreCase));
        }

        public void GetFactionData(FactionRecord record, string homeSystem = null)
        {
            if (record == null || record.faction == null || record.faction == Properties.CrimeMonitor.blank_faction) { return; }

            // Get the faction from Elite BGS and set faction record values
            Faction faction = bgsService.GetFactionByName(record.faction);
            record.Allegiance = faction.Allegiance ?? Superpower.None;

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
            StarSystem factionStarSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(factionSystem);

            if (factionStarSystem != null)
            {
                // Filter stations within the faction system which meet the station type prioritization,
                // max distance from the main star, game version, and landing pad size requirements
                LandingPadSize padSize = EDDI.Instance?.CurrentShip?.Size ?? LandingPadSize.Large;
                List<Station> factionStations = !prioritizeOrbitalStations && (EDDI.Instance?.inHorizons ?? false) ? factionStarSystem.stations : factionStarSystem.orbitalstations
                    .Where(s => s.stationservices.Count > 0).ToList();
                factionStations = factionStations.Where(s => s.distancefromstar <= maxStationDistanceFromStarLs).ToList();
                factionStations = factionStations.Where(s => s.LandingPadCheck(padSize)).ToList();

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
                foreach (FactionRecord record in criminalrecord.ToList())
                {
                    Superpower Allegiance = Superpower.FromNameOrEdName(record.faction);
                    if (Allegiance == null)
                    {
                        record.station = GetFactionStation(record.system);
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
