using CommanderMonitor;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiCommanderMonitor
{
    [UsedImplicitly]
    public class CommanderMonitor : IEddiMonitor
    {
        public string MonitorName () => "Commander monitor";

        public string LocalizedMonitorName () => EddiCommanderMonitor.Properties.Resources.MonitorName;

        public string MonitorDescription () => EddiCommanderMonitor.Properties.Resources.MonitorDescription;

        public bool IsRequired () => true;

        public bool NeedsStart () => false;

        public void Start ()
        {
            Reload();
        }

        public void Stop ()
        { }

        public void Reload ()
        {
            var configuration = ConfigService.Instance.eddiConfiguration;
            setSquadronSystem( configuration.SquadronSystemAddress );
        }

        public IDictionary<string, Tuple<Type, object>> GetVariables ()
        {
            return null;
        }

        public UserControl ConfigurationTabItem () => new ConfigurationWindow();

        public void PreHandle ( Event @event )
        {
            if ( @event is CarrierJumpedEvent carrierJumpedEvent )
            {
                handleCarrierJumpedEvent( carrierJumpedEvent );
            }
            else if ( @event is JumpedEvent jumpedEvent )
            {
                handleJumpedEvent( jumpedEvent );
            }
            else if ( @event is LocationEvent locationEvent )
            {
                handleLocationEvent( locationEvent );
            }
            else if ( @event is SquadronRankEvent squadronRankEvent )
            {
                handleSquadronRankEvent( squadronRankEvent );
            }
            else if ( @event is SquadronStartupEvent squadronStartupEvent )
            {
                handleSquadronStartupEvent( squadronStartupEvent );
            }
            else if ( @event is SquadronStatusEvent squadronStatusEvent )
            {
                handleSquadronStatusEvent( squadronStatusEvent );
            }
        }

        private void handleCarrierJumpedEvent ( CarrierJumpedEvent @event )
        {
            if ( ( @event.docked || @event.onFoot ) && @event.factions.Any() && EDDI.Instance.CurrentStarSystem != null )
            {
                // Check if current system is inhabited by or HQ for squadron faction
                var squadronFaction = @event.factions.FirstOrDefault(f =>
                {
                    var squadronhomesystem = f.presences
                        .FirstOrDefault(p => p.systemAddress == EDDI.Instance.CurrentStarSystem.systemAddress)?.squadronhomesystem;
                    return squadronhomesystem != null && ((bool)squadronhomesystem || f.squadronfaction);
                });
                if ( squadronFaction != null )
                {
                    updateSquadronData( squadronFaction, EDDI.Instance.CurrentStarSystem.systemAddress );
                }
            }
        }

        private void handleJumpedEvent ( JumpedEvent @event )
        {
            if ( @event.factions.Any() && EDDI.Instance.CurrentStarSystem != null )
            {
                // Check if current system is inhabited by or HQ for squadron faction
                var squadronFaction = @event.factions.FirstOrDefault(f =>
                {
                    var squadronhomesystem = f.presences
                        .FirstOrDefault(p => p.systemAddress == EDDI.Instance.CurrentStarSystem.systemAddress)?.squadronhomesystem;
                    return squadronhomesystem != null && ((bool)squadronhomesystem || f.squadronfaction);
                });
                if ( squadronFaction != null )
                {
                    updateSquadronData( squadronFaction, EDDI.Instance.CurrentStarSystem.systemAddress );
                }
            }
        }

        private void handleLocationEvent ( LocationEvent @event )
        {
            if ( @event.factions.Any() && EDDI.Instance.CurrentStarSystem != null )
            {
                // Check if current system is inhabited by or HQ for squadron faction
                var squadronFaction = @event.factions.FirstOrDefault(f =>
                {
                    var squadronhomesystem = f.presences
                        .FirstOrDefault(p => p.systemAddress == EDDI.Instance.CurrentStarSystem.systemAddress)?.squadronhomesystem;
                    return squadronhomesystem != null && ((bool)squadronhomesystem || f.squadronfaction);
                });
                if ( squadronFaction != null )
                {
                    updateSquadronData( squadronFaction, EDDI.Instance.CurrentStarSystem.systemAddress );
                }
            }
        }

        private bool handleSquadronRankEvent ( SquadronRankEvent @event )
        {
            SquadronRank rank = SquadronRank.FromRank(@event.newrank + 1);

            // Update the configuration file
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
            configuration.SquadronName = @event.name;
            configuration.SquadronRank = rank;
            ConfigService.Instance.eddiConfiguration = configuration;

            // Update the squadron UI data
            Application.Current?.Dispatcher?.InvokeAsync( () =>
            {
                if ( Application.Current?.MainWindow != null )
                {
                    ( (ConfigurationWindow)ConfigurationTabItem() ).eddiSquadronNameText.Text = @event.name;
                    ( (ConfigurationWindow)ConfigurationTabItem() ).squadronRankDropDown.SelectedItem = rank.localizedName;
                }
            } );

            // Update the commander object, if it exists
            if ( EDDI.Instance.Cmdr != null )
            {
                EDDI.Instance.Cmdr.squadronname = @event.name;
                EDDI.Instance.Cmdr.squadronrank = rank;
            }
            return true;
        }

        private void handleSquadronStartupEvent ( SquadronStartupEvent @event )
        {
            SquadronRank rank = SquadronRank.FromRank(@event.rank + 1);

            // Update the configuration file
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;
            configuration.SquadronName = @event.name;
            configuration.SquadronRank = rank;
            ConfigService.Instance.eddiConfiguration = configuration;

            // Update the squadron UI data
            Application.Current?.Dispatcher?.InvokeAsync( () =>
            {
                if ( Application.Current?.MainWindow != null )
                {
                    ( (ConfigurationWindow)ConfigurationTabItem() ).eddiSquadronNameText.Text = @event.name;
                    ( (ConfigurationWindow)ConfigurationTabItem() ).squadronRankDropDown.SelectedItem = rank.localizedName;
                }
            } );

            // Update the commander object, if it exists
            if ( EDDI.Instance.Cmdr != null )
            {
                EDDI.Instance.Cmdr.squadronname = @event.name;
                EDDI.Instance.Cmdr.squadronrank = rank;
            }
        }

        private void handleSquadronStatusEvent ( SquadronStatusEvent @event )
        {
            EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;

            switch ( @event.status )
            {
                case "created":
                    {
                        SquadronRank rank = SquadronRank.FromRank(1);

                        // Update the configuration file
                        configuration.SquadronName = @event.name;
                        configuration.SquadronRank = rank;

                        // Update the squadron UI data
                        Application.Current?.Dispatcher?.InvokeAsync( () =>
                        {
                            if ( Application.Current?.MainWindow != null )
                            {
                                ( (ConfigurationWindow)ConfigurationTabItem() ).eddiSquadronNameText.Text = @event.name;
                                ( (ConfigurationWindow)ConfigurationTabItem() ).squadronRankDropDown.SelectedItem = rank.localizedName;
                                configuration = ( (ConfigurationWindow)ConfigurationTabItem() ).resetSquadronRank( configuration );
                            }
                        } );

                        // Update the commander object, if it exists
                        if ( EDDI.Instance.Cmdr != null )
                        {
                            EDDI.Instance.Cmdr.squadronname = @event.name;
                            EDDI.Instance.Cmdr.squadronrank = rank;
                        }
                        break;
                    }
                case "joined":
                    {
                        // Update the configuration file
                        configuration.SquadronName = @event.name;

                        // Update the squadron UI data
                        Application.Current?.Dispatcher?.InvokeAsync( () =>
                        {
                            if ( Application.Current?.MainWindow != null )
                            {
                                ( (ConfigurationWindow)ConfigurationTabItem() ).eddiSquadronNameText.Text = @event.name;
                            }
                        } );

                        // Update the commander object, if it exists
                        if ( EDDI.Instance.Cmdr != null )
                        {
                            EDDI.Instance.Cmdr.squadronname = @event.name;
                        }
                        break;
                    }
                case "disbanded":
                case "kicked":
                case "left":
                    {
                        // Update the configuration file
                        configuration.SquadronName = null;
                        configuration.SquadronID = null;

                        // Update the squadron UI data
                        Application.Current?.Dispatcher?.InvokeAsync( () =>
                        {
                            if ( Application.Current?.MainWindow != null )
                            {
                                ( (ConfigurationWindow)ConfigurationTabItem() ).eddiSquadronNameText.Text = string.Empty;
                                ( (ConfigurationWindow)ConfigurationTabItem() ).eddiSquadronIDText.Text = string.Empty;
                                configuration = ( (ConfigurationWindow)ConfigurationTabItem() ).resetSquadronRank( configuration );
                            }
                        } );

                        // Update the commander object, if it exists
                        if ( EDDI.Instance.Cmdr != null )
                        {
                            EDDI.Instance.Cmdr.squadronname = null;
                        }
                        break;
                    }
            }
            ConfigService.Instance.eddiConfiguration = configuration;
        }

        public void setSquadronSystem ( ulong? newSystemAddress )
        {
            StarSystem newSystem = null;
            if ( newSystemAddress != null )
            {
                newSystem = EDDI.Instance.DataProvider.GetOrFetchStarSystem( (ulong)newSystemAddress );
            }

            //Ignore null & empty systems
            if ( newSystem?.bodies.Count > 0 )
            {
                if ( newSystem.systemAddress != EDDI.Instance.SquadronStarSystem?.systemAddress )
                {
                    EDDI.Instance.SquadronStarSystem = newSystem;
                    Logging.Debug( $"Squadron star system set to: {newSystemAddress} ({EDDI.Instance.SquadronStarSystem.systemname})" );

                    var eddiConfiguration = ConfigService.Instance.eddiConfiguration;
                    eddiConfiguration.SquadronSystem = newSystem.systemname;
                    eddiConfiguration.SquadronSystemAddress = newSystem.systemAddress;
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                }
            }
            else
            {
                EDDI.Instance.SquadronStarSystem = null;
            }
        }

        public void updateSquadronData ( Faction faction, ulong systemAddress )
        {
            if ( faction != null )
            {
                EDDIConfiguration configuration = ConfigService.Instance.eddiConfiguration;

                //Update the squadron faction, if changed
                if ( configuration.SquadronFaction == null || configuration.SquadronFaction != faction.name )
                {
                    configuration.SquadronFaction = faction.name;

                    Application.Current?.Dispatcher?.InvokeAsync( () =>
                    {
                        if ( Application.Current?.MainWindow != null )
                        {
                            ( (ConfigurationWindow)ConfigurationTabItem() ).squadronFactionDropDown.SelectedItem = faction.name;
                        }
                    } );

                    if ( EDDI.Instance.Cmdr != null )
                    {
                        EDDI.Instance.Cmdr.squadronfaction = faction.name;
                    }
                }

                // Update system, allegiance, & power when in squadron home system
                if ( ( faction.presences.FirstOrDefault( p => p.systemAddress == systemAddress )?.squadronhomesystem ?? false ) )
                {
                    // Update the squadron system data, if changed
                    string system = EDDI.Instance.CurrentStarSystem?.systemname;
                    if ( configuration.SquadronSystem == null || configuration.SquadronSystem != system )
                    {
                        configuration.SquadronSystem = system;
                        configuration.SquadronSystemAddress = EDDI.Instance.CurrentStarSystem?.systemAddress;

                        Application.Current?.Dispatcher?.InvokeAsync( () =>
                        {
                            if ( Application.Current?.MainWindow != null )
                            {
                                ( (ConfigurationWindow)ConfigurationTabItem() ).squadronSystemDropDown.Text = system;
                                ( (ConfigurationWindow)ConfigurationTabItem() ).ConfigureSquadronFactionOptions();
                            }
                        } );

                        setSquadronSystem( configuration.SquadronSystemAddress );
                    }

                    //Update the squadron allegiance, if changed
                    Superpower allegiance = EDDI.Instance.CurrentStarSystem?.Faction?.Allegiance ?? Superpower.None;

                    //Prioritize UI entry if squadron system allegiance not specified
                    if ( allegiance != Superpower.None )
                    {
                        if ( configuration.SquadronAllegiance == Superpower.None || configuration.SquadronAllegiance != allegiance )
                        {
                            configuration.SquadronAllegiance = allegiance;
                            if ( EDDI.Instance.Cmdr != null )
                            {
                                EDDI.Instance.Cmdr.squadronallegiance = allegiance;
                            }
                        }
                    }

                    // Update the squadron power, if changed
                    Power power = Power.FromName(EDDI.Instance.CurrentStarSystem?.power) ?? Power.None;

                    //Prioritize UI entry if squadron system power not specified
                    if ( power != Power.None )
                    {
                        if ( configuration.SquadronPower == Power.None && configuration.SquadronPower != power )
                        {
                            configuration.SquadronPower = power;

                            Application.Current?.Dispatcher?.InvokeAsync( () =>
                            {
                                if ( Application.Current?.MainWindow != null )
                                {
                                    ( (ConfigurationWindow)ConfigurationTabItem() ).squadronPowerDropDown.SelectedItem = power.localizedName;
                                    ( (ConfigurationWindow)ConfigurationTabItem() ).ConfigureSquadronPowerOptions( configuration );
                                }
                            } );
                        }
                    }
                }
                ConfigService.Instance.eddiConfiguration = configuration;
            }
        }

        public void PostHandle ( Event @event )
        { }

        public void HandleStatus ( Status status )
        { }

        public void HandleProfile ( JObject profile )
        { }
    }
}
