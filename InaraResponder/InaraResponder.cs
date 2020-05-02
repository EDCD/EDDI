﻿using Eddi;
using EddiCargoMonitor;
using EddiDataDefinitions;
using EddiEvents;
using EddiInaraService;
using EddiMissionMonitor;
using EddiShipMonitor;
using EddiSpeechService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using Utilities;

namespace EddiInaraResponder
{
    // Documentation: https://inara.cz/inara-api-docs/

    public class InaraResponder : EDDIResponder
    {
        private IInaraService inaraService = new InaraService();

        public string ResponderName()
        {
            return Properties.InaraResources.ResourceManager.GetString("name", CultureInfo.InvariantCulture);
        }

        public string LocalizedResponderName()
        {
            return Properties.InaraResources.name;
        }

        public string ResponderDescription()
        {
            return Properties.InaraResources.desc;
        }

        public bool Start()
        {
            Reload();

            // Subscribe to events from the Inara configuration that require our attention
            InaraService.invalidAPIkey += (s, e) => 
            {
                // Alert the user that there is a problem with the Inara API key
                Logging.Info("API key is invalid: Please open the Inara Responder and update the API key.");
                ShipMonitor shipMonitor = (ShipMonitor)EDDI.Instance.ObtainMonitor(EddiShipMonitor.Properties.ShipMonitor.ResourceManager.GetString("name", CultureInfo.InvariantCulture));
                SpeechService.Instance.Say(shipMonitor.GetCurrentShip(), Properties.InaraResources.invalidKeyErr);
            };

            Logging.Info($"Initialized {ResponderName()}");
            return true;
        }

        public void Stop()
        {
            inaraService.Stop();
        }

        public void Reload()
        {
            Stop();
            inaraService.Start(EDDI.Instance.EddiIsBeta());
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void Handle(Event theEvent)
        {
            if (theEvent is null)
            {
                return;
            }

            if (EDDI.Instance.inCQC)
            {
                // We don't do anything whilst in CQC
                return;
            }

            if (EDDI.Instance.inCrew)
            {
                // We don't do anything whilst in multicrew
                return;
            }

            if (EDDI.Instance.gameIsBeta)
            {
                // We don't send data whilst in beta
                return;
            }

            if (inaraService?.lastSync > theEvent.timestamp)
            {
                return;
            }

            try
            {
                Logging.Debug("Handling event " + JsonConvert.SerializeObject(theEvent));

                // These events will start or restart our instance of InaraService
                if (theEvent is CommanderLoadingEvent commanderLoadingEvent)
                {
                    handleCommanderLoadingEvent(commanderLoadingEvent);
                }
                else if (theEvent is CommanderStartedEvent commanderStartedEvent)
                {
                    handleCommanderStartedEvent(commanderStartedEvent);
                }
                else if (inaraService != null)
                {
                    if (theEvent is CommanderContinuedEvent commanderContinuedEvent)
                    {
                        handleCommanderContinuedEvent(commanderContinuedEvent);
                    }
                    else if (theEvent is CommanderProgressEvent commanderProgressEvent)
                    {
                        handleCommanderProgressEvent(commanderProgressEvent);
                    }
                    else if (theEvent is CommanderRatingsEvent commanderRatingsEvent)
                    {
                        handleCommanderRatingsEvent(commanderRatingsEvent);
                    }
                    else if (theEvent is EngineerProgressedEvent engineerProgressedEvent)
                    {
                        handleEngineerProgressedEvent(engineerProgressedEvent);
                    }
                    else if (theEvent is StatisticsEvent statisticsEvent)
                    {
                        handleStatisticsEvent(statisticsEvent);
                    }
                    else if (theEvent is PowerplayEvent powerplayEvent)
                    {
                        handlePowerplayEvent(powerplayEvent);
                    }
                    else if (theEvent is PowerLeftEvent powerLeftEvent)
                    {
                        handlePowerLeftEvent(powerLeftEvent);
                    }
                    else if (theEvent is PowerJoinedEvent powerJoinedEvent)
                    {
                        handlePowerJoinedEvent(powerJoinedEvent);
                    }
                    else if (theEvent is CommanderReputationEvent commanderReputationEvent)
                    {
                        handleCommanderReputationEvent(commanderReputationEvent);
                    }
                    else if (theEvent is JumpedEvent jumpedEvent)
                    {
                        handleJumpedEvent(jumpedEvent);
                    }
                    else if (theEvent is LocationEvent locationEvent)
                    {
                        handleLocationEvent(locationEvent);
                    }
                    else if (theEvent is CargoEvent cargoEvent)
                    {
                        handleCargoEvent(cargoEvent);
                    }
                    else if (theEvent is CommodityCollectedEvent commodityCollectedEvent)
                    {
                        handleCommodityCollectedEvent(commodityCollectedEvent);
                    }
                    else if (theEvent is CommodityEjectedEvent commodityEjectedEvent)
                    {
                        handleCommodityEjectedEvent(commodityEjectedEvent);
                    }
                    else if (theEvent is CommodityPurchasedEvent commodityPurchasedEvent)
                    {
                        handleCommodityPurchasedEvent(commodityPurchasedEvent);
                    }
                    else if (theEvent is CommodityRefinedEvent commodityRefinedEvent)
                    {
                        handleCommodityRefinedEvent(commodityRefinedEvent);
                    }
                    else if (theEvent is CommoditySoldEvent commoditySoldEvent)
                    {
                        handleCommoditySoldEvent(commoditySoldEvent);
                    }
                    else if (theEvent is CargoDepotEvent cargoDepotEvent)
                    {
                        handleCargoDepotEvent(cargoDepotEvent);
                    }
                    else if (theEvent is DiedEvent diedEvent)
                    {
                        handleDiedEvent(diedEvent);
                    }
                    else if (theEvent is EngineerContributedEvent engineerContributedEvent)
                    {
                        handleEngineerContributedEvent(engineerContributedEvent);
                    }
                    else if (theEvent is SearchAndRescueEvent searchAndRescueEvent)
                    {
                        handleSearchAndRescueEvent(searchAndRescueEvent);
                    }
                    else if (theEvent is MaterialInventoryEvent materialInventoryEvent)
                    {
                        handleMaterialInventoryEvent(materialInventoryEvent);
                    }
                    else if (theEvent is MaterialCollectedEvent materialCollectedEvent)
                    {
                        handleMaterialCollectedEvent(materialCollectedEvent);
                    }
                    else if (theEvent is MaterialDiscardedEvent materialDiscardedEvent)
                    {
                        handleMaterialDiscardedEvent(materialDiscardedEvent);
                    }
                    else if (theEvent is MaterialDonatedEvent materialDonatedEvent)
                    {
                        handleMaterialDonatedEvent(materialDonatedEvent);
                    }
                    else if (theEvent is MaterialTradedEvent materialTradedEvent)
                    {
                        handleMaterialTradedEvent(materialTradedEvent);
                    }
                    else if (theEvent is SynthesisedEvent synthesisedEvent)
                    {
                        handleSynthesisedEvent(synthesisedEvent);
                    }
                    else if (theEvent is ModificationCraftedEvent modificationCraftedEvent)
                    {
                        handleModificationCraftedEvent(modificationCraftedEvent);
                    }
                    else if (theEvent is TechnologyBrokerEvent technologyBrokerEvent)
                    {
                        handleTechnologyBrokerEvent(technologyBrokerEvent);
                    }
                    else if (theEvent is StoredModulesEvent storedModulesEvent)
                    {
                        handleStoredModulesEvent(storedModulesEvent);
                    }
                    else if (theEvent is ShipPurchasedEvent shipPurchasedEvent)
                    {
                        handleShipPurchasedEvent(shipPurchasedEvent);
                    }
                    else if (theEvent is ShipDeliveredEvent shipDeliveredEvent)
                    {
                        handleShipDeliveredEvent(shipDeliveredEvent);
                    }
                    else if (theEvent is ShipSoldEvent shipSoldEvent)
                    {
                        handleShipSoldEvent(shipSoldEvent);
                    }
                    else if (theEvent is ShipSoldOnRebuyEvent shipSoldOnRebuyEvent)
                    {
                        handleShipSoldOnRebuyEvent(shipSoldOnRebuyEvent);
                    }
                    else if (theEvent is ShipSwappedEvent shipSwappedEvent)
                    {
                        handleShipSwappedEvent(shipSwappedEvent);
                    }
                    else if (theEvent is ShipLoadoutEvent shipLoadoutEvent)
                    {
                        handleShipLoadoutEvent(shipLoadoutEvent);
                    }
                    else if (theEvent is ShipRenamedEvent shipRenamedEvent)
                    {
                        handleShipRenamedEvent(shipRenamedEvent);
                    }
                    else if (theEvent is ShipTransferInitiatedEvent shipTransferInitiatedEvent)
                    {
                        handleShipTransferInitiatedEvent(shipTransferInitiatedEvent);
                    }
                    else if (theEvent is DockedEvent dockedEvent)
                    {
                        handleDockedEvent(dockedEvent);
                    }
                    else if (theEvent is MissionAcceptedEvent missionAcceptedEvent)
                    {
                        handleMissionAcceptedEvent(missionAcceptedEvent);
                    }
                    else if (theEvent is MissionAbandonedEvent missionAbandonedEvent)
                    {
                        handleMissionAbandonedEvent(missionAbandonedEvent);
                    }
                    else if (theEvent is MissionCompletedEvent missionCompletedEvent)
                    {
                        handleMissionCompletedEvent(missionCompletedEvent);
                    }
                    else if (theEvent is MissionFailedEvent missionFailedEvent)
                    {
                        handleMissionFailedEvent(missionFailedEvent);
                    }
                    else if (theEvent is ShipInterdictedEvent shipInterdictedEvent)
                    {
                        handleShipInterdictedEvent(shipInterdictedEvent);
                    }
                    else if (theEvent is ShipInterdictionEvent shipInterdictionEvent)
                    {
                        handleShipInterdictionEvent(shipInterdictionEvent);
                    }
                    else if (theEvent is KilledEvent killedEvent)
                    {
                        handleKilledEvent(killedEvent);
                    }
                    else if (theEvent is CommunityGoalEvent communityGoalEvent)
                    {
                        handleCommunityGoalEvent(communityGoalEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, object> data = new Dictionary<string, object>
                {
                    { "exception", ex.Message },
                    { "stacktrace", ex.StackTrace },
                    { "event", JsonConvert.SerializeObject(theEvent) }
                };
                Logging.Error("Failed to handle event " + theEvent.type, data);
            }
        }

        private void handleCommunityGoalEvent(CommunityGoalEvent @event)
        {
            for (int i = 0; i < @event.cgid?.Count; i++)
            {
                Dictionary<string, object> cgEventData = new Dictionary<string, object>()
                {
                    { "communitygoalGameID", @event.cgid[i] },
                    { "communitygoalName", @event.name[i] },
                    { "starsystemName", @event.system[i] },
                    { "stationName", @event.station[i] },
                    { "goalExpiry", @event.expiryDateTime[i].ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "tierReached", int.Parse(@event.tier[i].Replace("Tier ", "")) },
                    { "isCompleted", @event.iscomplete[i] },
                    { "contributorsNum", @event.contributors[i] },
                    { "contributionsTotal", @event.total[i] }
                };
                if (@event.topranksize != null)
                {
                    cgEventData.Add("topRankSize", @event.topranksize);
                }
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommunityGoal", cgEventData, EDDI.Instance.gameIsBeta));

                if (@event.contribution[i] > 0)
                {
                    inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderCommunityGoalProgress", new Dictionary<string, object>()
                    {
                        { "communitygoalGameID", @event.cgid[i] },
                        { "contribution", @event.contribution[i] },
                        { "percentileBand", @event.percentileband[i] },
                        { "percentileBandReward", @event.tierreward[i] },
                        { "isTopRank", @event.toprank[i] }
                    }, EDDI.Instance.gameIsBeta));
                }
            }
        }

        private void handleKilledEvent(KilledEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderCombatKill", new Dictionary<string, object>()
            {
                { "starsystemName", EDDI.Instance.CurrentStarSystem.systemname },
                { "opponentName", @event.victim }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleShipInterdictionEvent(ShipInterdictionEvent @event)
        {
            // If the player successfully performed an interdiction
            // opponentName: Name of the target (commander or NPC). If there is no 'Inderticted' property in the journal event, use just 'Power' or 'Faction' property instead. 
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderCombatInterdiction", new Dictionary<string, object>()
            {
                { "starsystemName", EDDI.Instance.CurrentStarSystem.systemname },
                { "opponentName", @event.interdictee ?? @event.faction ?? @event.power }, // Ordered from more precise to less precise
                { "isPlayer", @event.iscommander },
                { "isSuccess", @event.succeeded }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleShipInterdictedEvent(ShipInterdictedEvent @event)
        {
            // If the player was interdicted
            if (@event.succeeded)
            {
                // The player did not escape
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderCombatInterdicted", new Dictionary<string, object>()
                {
                    { "starsystemName", EDDI.Instance.CurrentStarSystem.systemname },
                    { "opponentName", @event.interdictor },
                    { "isPlayer", @event.iscommander },
                    { "isSubmit", @event.submitted }
                }, EDDI.Instance.gameIsBeta));
            }
            else
            {
                // The player escaped
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderCombatInterdictionEscape", new Dictionary<string, object>()
                {
                    { "starsystemName", EDDI.Instance.CurrentStarSystem.systemname },
                    { "opponentName", @event.interdictor },
                    { "isPlayer", @event.iscommander }
                }, EDDI.Instance.gameIsBeta));
            }
        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderMissionFailed", new Dictionary<string, object>()
            {
                { "missionGameID", @event.missionid }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderMissionAbandoned", new Dictionary<string, object>()
            {
                { "missionGameID", @event.missionid }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            IDictionary<string, object> rawMissionObj = Deserializtion.DeserializeData(@event.raw);
            string commodity = JsonParsing.getString(rawMissionObj, "Commodity");
            int? commodityCount = JsonParsing.getOptionalInt(rawMissionObj, "Count");
            int? killCount = JsonParsing.getOptionalInt(rawMissionObj, "KillCount");
            int? passengerCount = JsonParsing.getOptionalInt(rawMissionObj, "PassengerCount");
            Dictionary<string, object> eventData = new Dictionary<string, object>()
            {
                { "missionName", @event.name },
                { "missionGameID", @event.missionid },
                { "missionExpiry", @event.expiry },
                { "influenceGain", @event.influence },
                { "reputationGain", @event.reputation }
            };
            if (EDDI.Instance.CurrentStarSystem != null)
            {
                eventData.Add("starsystemNameOrigin", EDDI.Instance.CurrentStarSystem?.systemname);
            }
            if (EDDI.Instance.CurrentStation != null)
            {
                eventData.Add("stationNameOrigin", EDDI.Instance.CurrentStation?.name);
            }
            if (!string.IsNullOrEmpty(@event.faction))
            {
                eventData.Add("minorfactionNameOrigin", @event.faction);
            }
            if (!string.IsNullOrEmpty(@event.destinationsystem))
            {
                eventData.Add("starsystemNameTarget", @event.destinationsystem);
            }
            if (!string.IsNullOrEmpty(@event.destinationstation))
            {
                eventData.Add("stationNameTarget", @event.destinationstation);
            }
            if (!string.IsNullOrEmpty(commodity))
            {
                eventData.Add("commodityName", commodity);
                eventData.Add("commodityCount", commodityCount);
            }
            if (passengerCount > 0)
            {
                eventData.Add("passengerCount", passengerCount);
                eventData.Add("passengerType", @event.passengertype);
                eventData.Add("passengerIsVIP", @event.passengervips);
                eventData.Add("passengerIsWanted", @event.passengerwanted);
            }
            if (killCount > 0)
            {
                eventData.Add("killCount", killCount);
            }
            if (!string.IsNullOrEmpty(@event.target))
            {
                eventData.Add("targetName", @event.target);
            }
            if (!string.IsNullOrEmpty(@event.targettype))
            {
                eventData.Add("targetType", @event.targettype);
            }
            if (!string.IsNullOrEmpty(@event.targetfaction))
            {
                eventData.Add("minorfactionNameTarget", @event.targetfaction);
            }
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderMission", eventData, EDDI.Instance.gameIsBeta));
        }

        private void handleDockedEvent(DockedEvent @event)
        {
            // Don't add this at the session start after Location, per guidance from Inara. 
            if (@event.station != firstDockedLocation)
            {
                Ship currentShip = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship Monitor")).GetCurrentShip();
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderTravelDock", new Dictionary<string, object>()
                {
                    { "starsystemName", @event.system },
                    { "stationName", @event.station },
                    { "marketID", @event.marketId },
                    { "shipType", currentShip.EDName },
                    { "shipGameID", currentShip.LocalId }
                }, EDDI.Instance.gameIsBeta));
            }
            firstDockedLocation = null;
        }

        private void handleShipTransferInitiatedEvent(ShipTransferInitiatedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderShipTransfer", new Dictionary<string, object>()
            {
                { "shipType", @event.shipDefinition?.EDName ?? @event.ship },
                { "shipGameID", @event.shipid },
                { "starsystemName", EDDI.Instance.CurrentStarSystem?.systemname },
                { "stationName", EDDI.Instance.CurrentStation?.name },
                { "marketID", EDDI.Instance.CurrentStation?.marketId },
                { "transferTime", @event.time }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleShipRenamedEvent(ShipRenamedEvent @event)
        {
            Ship currentShip = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship Monitor")).GetShip(@event.shipid);
            Dictionary<string, object> currentShipData = new Dictionary<string, object>()
            {
                { "shipType", currentShip.EDName },
                { "shipGameID", currentShip.LocalId },
                { "shipName", currentShip.name },
                { "shipIdent", currentShip.ident },
                { "shipRole", currentShip.Role.invariantName },
                { "isHot", currentShip.hot },
                { "isCurrentShip", true }
            };
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderShip", currentShipData, EDDI.Instance.gameIsBeta));
        }

        private void handleShipLoadoutEvent(ShipLoadoutEvent @event)
        {
            Ship currentShip = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship Monitor")).GetShip(@event.shipid);
            Dictionary<string, object> currentShipData = new Dictionary<string, object>()
            {
                { "shipType", currentShip.EDName },
                { "shipGameID", currentShip.LocalId },
                { "shipName", currentShip.name },
                { "shipIdent", currentShip.ident },
                { "shipRole", currentShip.Role.invariantName },
                { "isHot", currentShip.hot },
                { "isCurrentShip", true },
                { "shipHullValue", @event.hullvalue },
                { "shipModulesValue", @event.modulesvalue },
                { "shipRebuyCost", @event.rebuy }
            };
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderShip", currentShipData, EDDI.Instance.gameIsBeta));

            List<Dictionary<string, object>> modulesData = new List<Dictionary<string, object>>();
            foreach (Hardpoint hardpoint in @event.hardpoints)
            {
                if (hardpoint != null)
                {
                    Dictionary<string, object> moduleData = GetModuleData(hardpoint.name, hardpoint.module);
                    modulesData.Add(moduleData);
                }
            }
            foreach (Compartment compartment in @event.compartments)
            {
                if (compartment != null)
                {
                    Dictionary<string, object> moduleData = GetModuleData(compartment.name, compartment.module);
                    modulesData.Add(moduleData);
                }
            }
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderShipLoadout", new Dictionary<string, object>()
            {
                { "shipType", @event.shipDefinition?.EDName ?? @event.ship },
                { "shipGameID", @event.shipid },
                { "shipLoadout", modulesData }
            }, EDDI.Instance.gameIsBeta));
        }

        private static Dictionary<string, object> GetModuleData(string slotName, Module module)
        {
            Dictionary<string, object> moduleData;
            if (module is null)
            {
                moduleData = new Dictionary<string, object>()
                {
                    { "slotName", slotName }
                };
            }
            else
            {
                moduleData = new Dictionary<string, object>()
                {
                    {"slotName", slotName},
                    {"itemName", module.edname},
                    {"itemValue", module.value},
                    {"itemHealth", module.health / 100}, // From 0 - 1
                    {"isOn", module.enabled},
                    {"isHot", module.hot},
                    {"itemPriority", module.priority},
                    {"itemAmmoClip", module.clipcapacity},
                    {"itemAmmoHopper", module.hoppercapacity}
                };
                if (module.modified)
                {
                    List<Dictionary<string, object>> modifiers = new List<Dictionary<string, object>>();
                    foreach (EngineeringModifier modifier in module.modifiers)
                    {
                        if (modifier.currentValue != null)
                        {
                            modifiers.Add(new Dictionary<string, object>()
                            {
                                { "name", modifier.EDName },
                                { "value", modifier.currentValue },
                                { "originalValue", modifier.originalValue },
                                { "lessIsGood", modifier.lessIsGood }
                            });
                        }
                        else if (!string.IsNullOrEmpty(modifier.valueStr))
                        {
                            modifiers.Add(new Dictionary<string, object>()
                            {
                                { "name", modifier.EDName },
                                { "valueStr", modifier.valueStr }
                            });
                        }
                    }

                    Dictionary<string, object> engineering = new Dictionary<string, object>()
                    {
                        {"blueprintName", module.modificationEDName},
                        {"blueprintLevel", module.engineerlevel},
                        {"blueprintQuality", module.engineerquality},
                        {"experimentalEffect", module.engineerExperimentalEffectEDName},
                        {"modifiers", modifiers}
                    };
                    moduleData.Add("engineering", engineering);
                }
            }
            return moduleData;
        }

        private void handleShipSwappedEvent(ShipSwappedEvent @event)
        {
            if (!string.IsNullOrEmpty(@event.storedship))
            {
                Ship storedShip = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship Monitor")).GetShip(@event.storedshipid);
                Dictionary<string, object> storedShipData = new Dictionary<string, object>()
                {
                    { "shipType", storedShip.EDName },
                    { "shipGameID", storedShip.LocalId },
                    { "shipName", storedShip.name },
                    { "shipIdent", storedShip.ident },
                    { "isHot", storedShip.hot },
                    { "shipRole", storedShip.Role.invariantName },
                    { "isCurrentShip", true },
                    { "starsystemName", EDDI.Instance.CurrentStarSystem?.systemname },
                    { "stationName", EDDI.Instance.CurrentStation?.name },
                    { "marketID", EDDI.Instance.CurrentStation?.marketId }
                };
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderShip", storedShipData, EDDI.Instance.gameIsBeta));
            }
            else if (!string.IsNullOrEmpty(@event.soldship))
            {
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderShip", new Dictionary<string, object>()
                {
                    { "shipType", @event.soldShipDefinition?.EDName ?? @event.soldship },
                    { "shipGameID", @event.storedshipid }
                }, EDDI.Instance.gameIsBeta));
            }
            Ship currentShip = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship Monitor")).GetShip(@event.shipid);
            Dictionary<string, object> currentShipData = new Dictionary<string, object>()
            {
                { "shipType", currentShip.EDName },
                { "shipGameID", currentShip.LocalId },
                { "shipName", currentShip.name },
                { "shipIdent", currentShip.ident },
                { "shipRole", currentShip.Role.invariantName },
                { "isHot", currentShip.hot },
                { "isCurrentShip", true }
            };
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderShip", currentShipData, EDDI.Instance.gameIsBeta));
        }

        private void handleShipSoldOnRebuyEvent(ShipSoldOnRebuyEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderShip", new Dictionary<string, object>()
            {
                { "shipType", @event.shipDefinition?.EDName ?? @event.ship },
                { "shipGameID", @event.shipid }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleShipSoldEvent(ShipSoldEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderShip", new Dictionary<string, object>()
            {
                { "shipType", @event.shipDefinition?.EDName ?? @event.ship },
                { "shipGameID", @event.shipid }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleShipDeliveredEvent(ShipDeliveredEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderShip", new Dictionary<string, object>()
            {
                { "shipType", @event.shipDefinition?.EDName ?? @event.ship },
                { "shipGameID", @event.shipid }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleShipPurchasedEvent(ShipPurchasedEvent @event)
        {
            // The new ship is handled by responding to `ShipDeliveredEvent`. 
            // In this event, we simply remove the old ship data. 
            if (!string.IsNullOrEmpty(@event.storedship))
            {
                Ship storedShip = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship Monitor")).GetShip(@event.storedshipid);
                Dictionary<string, object> storedShipData = new Dictionary<string, object>()
                {
                    { "shipType", storedShip.EDName },
                    { "shipGameID", storedShip.LocalId },
                    { "shipName", storedShip.name },
                    { "shipIdent", storedShip.ident },
                    { "isHot", storedShip.hot },
                    { "shipRole", storedShip.Role.invariantName },
                    { "isCurrentShip", true },
                    { "starsystemName", EDDI.Instance.CurrentStarSystem?.systemname },
                    { "stationName", EDDI.Instance.CurrentStation?.name },
                    { "marketID", EDDI.Instance.CurrentStation?.marketId }
                };
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderShip", storedShipData, EDDI.Instance.gameIsBeta));
            }
            else if (!string.IsNullOrEmpty(@event.soldship))
            {
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderShip", new Dictionary<string, object>()
                {
                    { "shipType", @event.soldShipDefinition?.EDName },
                    { "shipGameID", @event.soldshipid }
                }, EDDI.Instance.gameIsBeta));
            }
        }

        private void handleStoredModulesEvent(StoredModulesEvent @event)
        {
            List<Dictionary<string, object>> eventData = new List<Dictionary<string, object>>();
            foreach (StoredModule storedModule in @event.storedmodules)
            {
                Dictionary<string, object> moduleData = new Dictionary<string, object>()
                {
                    { "itemName", storedModule?.module?.edname },
                    { "itemValue", storedModule?.module?.value },
                    { "isHot", storedModule?.module?.hot },
                    { "starsystemName", storedModule?.system },
                    { "stationName", storedModule?.station },
                    { "marketID", storedModule?.marketid }
                };
                if (storedModule?.module != null && (bool)storedModule.module?.modified)
                {
                    Dictionary<string, object> engineering = new Dictionary<string, object>()
                    {
                        { "blueprintName", storedModule.module.modificationEDName },
                        { "blueprintLevel", storedModule.module.engineerlevel },
                        { "blueprintQuality", storedModule.module.engineerquality }
                    };
                    if (!string.IsNullOrEmpty(storedModule.module.engineerExperimentalEffectEDName))
                    {
                        engineering.Add("experimentalEffect", storedModule.module.engineerExperimentalEffectEDName);
                    }
                    moduleData.Add("engineering", engineering);
                }
                eventData.Add(moduleData);
            }
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderStorageModules", eventData, EDDI.Instance.gameIsBeta));
        }

        private void handleMaterialInventoryEvent(MaterialInventoryEvent @event)
        {
            List<Dictionary<string, object>> eventData = new List<Dictionary<string, object>>();
            foreach (MaterialAmount materialAmount in @event.inventory)
            {
                eventData.Add(new Dictionary<string, object>()
                {
                    { "itemName", materialAmount?.edname },
                    { "itemCount", materialAmount?.amount }
                });
            }
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderInventoryMaterials", eventData, EDDI.Instance.gameIsBeta));
        }

        private void handleModificationCraftedEvent(ModificationCraftedEvent @event)
        {
            if (@event.materials?.Count > 0)
            {
                foreach (MaterialAmount materialAmount in @event.materials)
                {
                    inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryMaterialsItem", new Dictionary<string, object>()
                    {
                        { "itemName", materialAmount?.edname },
                        { "itemCount", materialAmount?.amount }
                    }, EDDI.Instance.gameIsBeta));
                }
            }
            if (@event.commodities?.Count > 0)
            {
                foreach (CommodityAmount commodityAmount in @event.commodities)
                {
                    inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryCargoItem", new Dictionary<string, object>()
                    {
                        { "itemName", commodityAmount?.commodityDefinition?.edname },
                        { "itemCount", commodityAmount?.amount }
                    }, EDDI.Instance.gameIsBeta));
                }
            }
        }

        private void handleSynthesisedEvent(SynthesisedEvent @event)
        {
            if (@event.materials?.Count > 0)
            {
                foreach (MaterialAmount materialAmount in @event.materials)
                {
                    inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryMaterialsItem", new Dictionary<string, object>()
                    {
                        { "itemName", materialAmount?.edname },
                        { "itemCount", materialAmount?.amount }
                    }, EDDI.Instance.gameIsBeta));
                }
            }
        }

        private void handleMaterialDonatedEvent(MaterialDonatedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryMaterialsItem", new Dictionary<string, object>()
            {
                { "itemName", @event.edname },
                { "itemCount", @event.amount }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleMaterialDiscardedEvent(MaterialDiscardedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryMaterialsItem", new Dictionary<string, object>()
            {
                { "itemName", @event.edname },
                { "itemCount", @event.amount }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleMaterialTradedEvent(MaterialTradedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryMaterialsItem", new Dictionary<string, object>()
            {
                { "itemName", @event.paid_edname },
                { "itemCount", @event.paid_quantity }
            }, EDDI.Instance.gameIsBeta));
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderInventoryMaterialsItem", new Dictionary<string, object>()
            {
                { "itemName", @event.received_edname },
                { "itemCount", @event.received_quantity }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleTechnologyBrokerEvent(TechnologyBrokerEvent @event)
        {
            if (@event.materials?.Count > 0)
            {
                foreach (MaterialAmount materialAmount in @event.materials)
                {
                    inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryMaterialsItem", new Dictionary<string, object>()
                    {
                        { "itemName", materialAmount?.edname },
                        { "itemCount", materialAmount?.amount }
                    }, EDDI.Instance.gameIsBeta));
                }
            }
            if (@event.commodities?.Count > 0)
            {
                foreach (CommodityAmount commodityAmount in @event.commodities)
                {
                    inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryCargoItem", new Dictionary<string, object>()
                    {
                        { "itemName", commodityAmount?.commodityDefinition?.edname },
                        { "itemCount", commodityAmount?.amount }
                    }, EDDI.Instance.gameIsBeta));
                }
            }
        }

        private void handleMaterialCollectedEvent(MaterialCollectedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderInventoryMaterialsItem", new Dictionary<string, object>()
            {
                { "itemName", @event.edname },
                { "itemCount", @event.amount }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleCargoEvent(CargoEvent @event)
        {
            List<Dictionary<string, object>> eventData = new List<Dictionary<string, object>>();
            foreach (CargoInfo cargoInfo in @event.inventory)
            {
                eventData.Add(new Dictionary<string, object>()
                {
                    { "itemName", cargoInfo.name },
                    { "itemCount", cargoInfo.count }
                });
            }
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderInventoryCargo", eventData, EDDI.Instance.gameIsBeta));
        }

        private void handleDiedEvent(DiedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderInventoryCargo", new List<Dictionary<string, object>>(), EDDI.Instance.gameIsBeta));
            Dictionary<string, object> diedEventData = new Dictionary<string, object>()
            {
                { "starsystemName", EDDI.Instance.CurrentStarSystem.systemname }
            };
            if (@event.commanders?.Count > 1)
            {
                diedEventData.Add("wingOpponentNames", @event.commanders);
            }
            else if (@event.commanders?.Count == 1)
            {
                diedEventData.Add("opponentName", @event.commanders[0]);
            }
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderCombatDeath", diedEventData, EDDI.Instance.gameIsBeta));
        }

        private void handleSearchAndRescueEvent(SearchAndRescueEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryCargoItem", new Dictionary<string, object>()
            {
                { "itemName", @event.commodity?.invariantName },
                { "itemCount", @event.amount }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleEngineerContributedEvent(EngineerContributedEvent @event)
        {
            if (@event.contributiontype == "Commodity")
            {
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryCargoItem", new Dictionary<string, object>()
                {
                    { "itemName", @event.commodityAmount?.commodityDefinition?.edname },
                    { "itemCount", @event.amount }
                }, EDDI.Instance.gameIsBeta));
            }
            else if (@event.contributiontype == "Material")
            {
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryMaterialsItem", new Dictionary<string, object>()
                {
                    { "itemName", @event.materialAmount?.edname },
                    { "itemCount", @event.amount }
                }, EDDI.Instance.gameIsBeta));
            }
        }

        private void handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryCargoItem", new Dictionary<string, object>()
            {
                { "itemName", @event.commodityDefinition?.edname },
                { "itemCount", @event.amount },
                { "isStolen", @event.stolen }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryCargoItem", new Dictionary<string, object>()
            {
                { "itemName", @event.commodityDefinition?.edname },
                { "itemCount", @event.amount },
                { "missionGameID", @event.missionid }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleCargoDepotEvent(CargoDepotEvent @event)
        {
            if (@event.updatetype == "Collect")
            {
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderInventoryCargoItem", new Dictionary<string, object>()
                {
                    { "itemName", @event.commodityDefinition?.edname },
                    { "itemCount", @event.amount },
                    { "isStolen", false },
                    { "missionGameID", @event.missionid }
                }, EDDI.Instance.gameIsBeta));
            }
            else if (@event.updatetype == "Deliver")
            {
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "delCommanderInventoryCargoItem", new Dictionary<string, object>()
                {
                    { "itemName", @event.commodityDefinition?.edname },
                    { "itemCount", @event.amount },
                    { "isStolen", false },
                    { "missionGameID", @event.missionid }
                }, EDDI.Instance.gameIsBeta));
            }
        }

        private void handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderInventoryCargoItem", new Dictionary<string, object>()
            {
                { "itemName", @event.commodityDefinition?.edname },
                { "itemCount", 1 },
                { "isStolen", false }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderInventoryCargoItem", new Dictionary<string, object>()
            {
                { "itemName", @event.commodityDefinition?.edname },
                { "itemCount", @event.amount },
                { "isStolen", false }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderInventoryCargoItem", new Dictionary<string, object>()
            {
                { "itemName", @event.commodityDefinition?.edname },
                { "itemCount", 1 },
                { "isStolen", @event.stolen },
                { "missionGameID", @event.missionid }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleLocationEvent(LocationEvent @event)
        {
            List<Dictionary<string, object>> minorFactionRepData = minorFactionReputations(@event.factions);
            if (minorFactionRepData.Count > 0)
            {
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderReputationMinorFaction", minorFactionRepData, EDDI.Instance.gameIsBeta));
            }
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderTravelLocation", new Dictionary<string, object>()
            {
                { "starsystemName", @event.systemname },
                { "stationName", @event.station },
                { "marketID", @event.marketId }
            }, EDDI.Instance.gameIsBeta));
            if (@event.docked)
            {
                // Set our docked lcoation for reference by the `Docked` event.
                firstDockedLocation = @event.station;
            }
        }
        string firstDockedLocation;

        private void handleJumpedEvent(JumpedEvent @event)
        {
            List<Dictionary<string, object>> minorFactionRepData = minorFactionReputations(@event.factions);
            if (minorFactionRepData.Count > 0)
            {
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderReputationMinorFaction", minorFactionRepData, EDDI.Instance.gameIsBeta));
            }
            Ship currentShip = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship Monitor")).GetCurrentShip();
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderTravelFSDJump", new Dictionary<string, object>()
            {
                { "starsystemName", @event.system },
                { "jumpDistance", @event.distance },
                { "shipType", currentShip.EDName },
                { "shipGameID", currentShip.LocalId }
            }, EDDI.Instance.gameIsBeta));
        }

        private static List<Dictionary<string, object>> minorFactionReputations(List<Faction> factions)
        {
            // Reputation progress in a range: [-1..1], which corresponds to a reputation range from -100% (hostile) to 100% (allied).
            List<Dictionary<string, object>> eventData = new List<Dictionary<string, object>>();
            foreach (Faction faction in factions)
            {
                if (faction != null && faction.myreputation > -10M && faction.myreputation < 5M) // faction.myreputation is out of 100.
                {
                    // Skip posting updates for factions where the commander has a near neutral reputation modifier (-10% to 5%).
                    continue;
                }
                eventData.Add(new Dictionary<string, object>()
                {
                    { "minorfactionName", faction?.name },
                    { "minorfactionReputation", (faction?.myreputation ?? 0) / 100 }
                });
            }
            return eventData;
        }

        private void handleCommanderReputationEvent(CommanderReputationEvent @event)
        {
            // Reputation progress in a range: [-1..1], which corresponds to a reputation range from -100% (hostile) to 100% (allied).
            List<Dictionary<string, object>> eventData = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "majorfactionName", "empire" },
                    { "majorfactionReputation", @event.empire / 100 }
                },
                new Dictionary<string, object>
                {
                    { "majorfactionName", "federation" },
                    { "majorfactionReputation", @event.federation / 100 }
                },
                new Dictionary<string, object>
                {
                    { "majorfactionName", "independent" },
                    { "majorfactionReputation", @event.independent / 100 }
                },
                new Dictionary<string, object>
                {
                    { "majorfactionName", "alliance" },
                    { "majorfactionReputation", @event.alliance / 100 }
                }
            };
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderReputationMajorFaction", eventData, EDDI.Instance.gameIsBeta));
        }

        private void handlePowerJoinedEvent(PowerJoinedEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankPower", new Dictionary<string, object>()
            {
                { "powerName", @event.Power?.invariantName },
                { "rankValue", 1 }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handlePowerLeftEvent(PowerLeftEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankPower", new Dictionary<string, object>()
            {
                { "powerName", @event.Power?.invariantName },
                { "rankValue", 0 }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handlePowerplayEvent(PowerplayEvent @event)
        {
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankPower", new Dictionary<string, object>()
            {
                { "powerName", @event.Power?.invariantName },
                { "rankValue", @event.rank }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleCommanderProgressEvent(CommanderProgressEvent @event)
        {
            // Pilots federation/Navy rank name as are in the journals (["combat", "trade", "explore", "cqc", "federation", "empire"]) 
            // Rank progress (range: [0..1], which corresponds to 0% - 100%) (In the journal, these are given out of 100)
            List<Dictionary<string, object>> eventData = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "rankName", "combat" },
                    { "rankProgress", @event.combat / 100 }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "trade" },
                    { "rankProgress", @event.trade / 100 }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "explore" },
                    { "rankProgress", @event.exploration / 100 }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "empire" },
                    { "rankProgress", @event.empire / 100 }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "federation" },
                    { "rankProgress", @event.federation / 100 }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "cqc" },
                    { "rankProgress", @event.cqc / 100 }
                }
            };
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankPilot", eventData, EDDI.Instance.gameIsBeta));
        }

        private void handleCommanderRatingsEvent(CommanderRatingsEvent @event)
        {
            // Pilots federation/Navy rank name as are in the journals (["combat", "trade", "explore", "cqc", "federation", "empire"]) 
            // Rank value (range [0..8] for Pilots federation ranks, range [0..14] for Navy ranks)
            List<Dictionary<string, object>> eventData = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "rankName", "combat" },
                    { "rankValue", @event.combat?.rank }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "trade" },
                    { "rankValue", @event.trade?.rank }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "explore" },
                    { "rankValue", @event.exploration?.rank }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "empire" },
                    { "rankValue", @event.empire?.rank }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "federation" },
                    { "rankValue", @event.federation?.rank }
                },
                new Dictionary<string, object>()
                {
                    { "rankName", "cqc" },
                    { "rankValue", @event.cqc?.rank }
                }
            };
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankPilot", eventData, EDDI.Instance.gameIsBeta));
        }

        private void handleEngineerProgressedEvent(EngineerProgressedEvent @event)
        {
            // Send engineer rank progress to Inara
            IDictionary<string, object> data = Deserializtion.DeserializeData(@event.raw);
            data.TryGetValue("Engineers", out object val);
            if (val != null)
            {
                // This is a startup entry, containing data about all known engineers
                List<Dictionary<string, object>> eventData = new List<Dictionary<string, object>>();
                List<object> engineers = (List<object>)val;
                foreach (IDictionary<string, object> engineerData in engineers)
                {
                    Dictionary<string, object> engineer = parseEngineerInara(engineerData);

                    eventData.Add(engineer);
                }
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankEngineer", eventData, EDDI.Instance.gameIsBeta));
            }
            else
            {
                // This is a progress entry, containing data about a single engineer
                Dictionary<string, object> eventData = parseEngineerInara(data);
                inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderRankEngineer", eventData, EDDI.Instance.gameIsBeta));
            }
        }

        private static Dictionary<string, object> parseEngineerInara(IDictionary<string, object> engineerData)
        {
            Dictionary<string, object> engineer = new Dictionary<string, object>()
            {
                { "engineerName", JsonParsing.getString(engineerData, "Engineer") },
                { "rankStage", JsonParsing.getString(engineerData, "Progress") }
            };
            int? rank = JsonParsing.getOptionalInt(engineerData, "Rank");
            if (!(rank is null))
            {
                engineer.Add("rankValue", rank);
            }
            return engineer;
        }

        private void handleStatisticsEvent(StatisticsEvent @event)
        {
            // Send the commanders game statistics to Inara
            // Prepare and send the raw event, less the event name and timestamp. Please note that the statistics 
            // are always overridden as a whole, so any partial updates will cause erasing of the rest.
            IDictionary<string, object> data = Deserializtion.DeserializeData(@event.raw);
            data.Remove("timestamp");
            data.Remove("event");
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderGameStatistics", (Dictionary<string, object>)data, EDDI.Instance.gameIsBeta));
        }

        private void handleCommanderContinuedEvent(CommanderContinuedEvent @event)
        {
            // Sets current credits and loans. A record is added to the credits log (if the value differs).
            // Warning: Do NOT set credits/assets unless you are absolutely sure they are correct. 
            // The journals currently doesn't contain crew wage cuts, so credit gains are very probably off 
            // for most of the players. Also, please, do not send each minor credits change, as it will 
            // spam player's credits log with unusable data and they won't be most likely very happy about it. 
            // It may be good to set credits just on the session start, session end and on the big changes 
            // or in hourly intervals.
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderCredits", new Dictionary<string, object>()
            {
                { "commanderCredits", @event.credits },
                { "commanderLoan", @event.loan }
            }, EDDI.Instance.gameIsBeta));
        }

        private void handleCommanderStartedEvent(CommanderStartedEvent @event)
        {
            InaraConfiguration inaraConfiguration = InaraConfiguration.FromFile();
            if (inaraConfiguration.commanderName != @event.name || inaraConfiguration.commanderFrontierID != @event.frontierID)
            {
                inaraConfiguration.commanderName = @event.name;
                inaraConfiguration.commanderFrontierID = @event.frontierID;
                inaraConfiguration.ToFile();
                Reload();
            }
        }

        private void handleCommanderLoadingEvent(CommanderLoadingEvent @event)
        {
            InaraConfiguration inaraConfiguration = InaraConfiguration.FromFile();
            if (inaraConfiguration.commanderName != @event.name || inaraConfiguration.commanderFrontierID != @event.frontierID)
            {
                inaraConfiguration.commanderName = @event.name;
                inaraConfiguration.commanderFrontierID = @event.frontierID;
                inaraConfiguration.ToFile();
                Reload();
            }
        }

        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            // Adds star system permit for the commander. You do not need to handle permits granted for the 
            // Pilots Federation or Navy rank promotion, but you should handle any other ways (like mission 
            // rewards).
            if (@event.permitsawarded.Count > 0)
            {
                foreach (string systemName in @event.permitsawarded)
                {
                    if (string.IsNullOrEmpty(systemName)) { continue; }
                    inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderPermit", new Dictionary<string, object>() { { "starsystemName", systemName } }, EDDI.Instance.gameIsBeta));
                }
            }
            if (@event.materialsrewards?.Count > 0)
            {
                foreach (MaterialAmount materialAmount in @event.materialsrewards)
                {
                    inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderInventoryMaterialsItem", new Dictionary<string, object>()
                    {
                        { "itemName", materialAmount?.edname },
                        { "itemCount", materialAmount?.amount },
                        { "isStolen", false },
                        { "missionGameID", @event.missionid }
                    }, EDDI.Instance.gameIsBeta));
                }
            }
            if (@event.commodityrewards?.Count > 0)
            {
                foreach (CommodityAmount commodityAmount in @event.commodityrewards)
                {
                    inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "addCommanderInventoryCargoItem", new Dictionary<string, object>()
                    {
                        { "itemName", commodityAmount?.commodityDefinition?.edname },
                        { "itemCount", commodityAmount?.amount },
                        { "isStolen", false },
                        { "missionGameID", @event.missionid }
                    }, EDDI.Instance.gameIsBeta));
                }
            }

            IDictionary<string, object> missionCompletedObj = Deserializtion.DeserializeData(@event.raw);
            Dictionary<string, object> eventData = new Dictionary<string, object>() { { "missionGameID", @event.missionid } };
            if (@event.donation > 0)
            {
                eventData.Add("donationCredits", @event.donation);
            }
            if (@event.reward > 0)
            {
                eventData.Add("rewardCredits", @event.reward);
            }
            if (@event.permitsawarded?.Count > 0)
            {
                eventData.Add("rewardPermits", @event.permitsawarded);
            }
            if (@event.commodityrewards?.Count > 0)
            {
                List<Dictionary<string, object>> rewardCommodities = new List<Dictionary<string, object>>();
                missionCompletedObj.TryGetValue("CommodityReward", out object commodityRewardVal);
                if (commodityRewardVal != null)
                {
                    foreach (Dictionary<string, object> commodityRewardData in (List<object>)commodityRewardVal)
                    {
                        string commodityName = JsonParsing.getString(commodityRewardData, "Name");
                        int commodityCount = JsonParsing.getInt(commodityRewardData, "Count");
                        rewardCommodities.Add(new Dictionary<string, object>()
                        {
                            { "itemName", commodityName },
                            { "itemCount", commodityCount }
                        });
                    }
                    eventData.Add("rewardCommodities", rewardCommodities);
                }
            }
            if (@event.materialsrewards?.Count > 0)
            {
                List<Dictionary<string, object>> rewardMaterials = new List<Dictionary<string, object>>();
                missionCompletedObj.TryGetValue("MaterialsReward", out object materialsRewardVal);
                if (materialsRewardVal != null)
                {
                    foreach (Dictionary<string, object> materialRewardData in (List<object>)materialsRewardVal)
                    {
                        string materialName = JsonParsing.getString(materialRewardData, "Name");
                        int materialCount = JsonParsing.getInt(materialRewardData, "Count");
                        rewardMaterials.Add(new Dictionary<string, object>()
                        {
                            { "itemName", materialName },
                            { "itemCount", materialCount }
                        });
                    }
                }
                eventData.Add("rewardMaterials", rewardMaterials);
            }
            missionCompletedObj.TryGetValue("FactionEffects", out object factionEffectsVal);
            if (factionEffectsVal is List<object> factionEffects)
            {
                List<Dictionary<string, object>> minorfactionEffects = new List<Dictionary<string, object>>();
                foreach (Dictionary<string, object> factionEffect in factionEffects)
                {
                    string factionName = JsonParsing.getString(factionEffect, "Faction");
                    Dictionary<string, object> minorfactionEffect = new Dictionary<string, object>() { { "minorfactionName", factionName } };
                    factionEffect.TryGetValue("Effect", out object effectsVal);
                    if (effectsVal is List<object> effects)
                    {
                        foreach (Dictionary<string, object> effect in effects)
                        {
                            effect.TryGetValue("Influence", out object influenceVal);
                            if (influenceVal is Dictionary<string, object> influenceData)
                            {
                                string influence = JsonParsing.getString(influenceData, "Influence");
                                minorfactionEffect.Add("influenceGain", influence);
                            }
                            string reputation = JsonParsing.getString(effect, "Reputation");
                            minorfactionEffect.Add("reputationGain", reputation);
                        }
                    }
                    minorfactionEffects.Add(minorfactionEffect);
                }
                eventData.Add("minorfactionEffects", minorfactionEffects);
            }
            inaraService.EnqueueAPIEvent(new InaraAPIEvent(@event.timestamp, "setCommanderMissionCompleted", eventData, EDDI.Instance.gameIsBeta));
        }
    }
}
