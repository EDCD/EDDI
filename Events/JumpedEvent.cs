using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class JumpedEvent : Event
    {
        public const string NAME = "Jumped";
        public const string DESCRIPTION = "Triggered when you complete a jump to another system";
        public static readonly string[] SAMPLES =
        {
            @"{ ""timestamp"":""2024-10-28T02:55:58Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""Nik"", ""SystemAddress"":7230812328674, ""StarPos"":[90.84375,-20.71875,94.87500], ""SystemAllegiance"":""Empire"", ""SystemEconomy"":""$economy_Terraforming;"", ""SystemEconomy_Localised"":""Terraforming"", ""SystemSecondEconomy"":""$economy_Industrial;"", ""SystemSecondEconomy_Localised"":""Industrial"", ""SystemGovernment"":""$government_Dictatorship;"", ""SystemGovernment_Localised"":""Dictatorship"", ""SystemSecurity"":""$SYSTEM_SECURITY_medium;"", ""SystemSecurity_Localised"":""Medium Security"", ""Population"":17835682, ""Body"":""Nik A"", ""BodyID"":1, ""BodyType"":""Star"", ""ControllingPower"":""Nakato Kaine"", ""Powers"":[ ""Aisling Duval"", ""Felicia Winters"", ""Nakato Kaine"" ], ""PowerplayState"":""Exploited"", ""JumpDist"":64.049, ""FuelUsed"":1.856707, ""FuelLevel"":21.687792, ""BoostUsed"":4, ""Factions"":[ { ""Name"":""Pisamazin Movement"", ""FactionState"":""None"", ""Government"":""Dictatorship"", ""Influence"":0.063936, ""Allegiance"":""Empire"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Nik Future"", ""FactionState"":""None"", ""Government"":""Democracy"", ""Influence"":0.117882, ""Allegiance"":""Federation"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Bajoji Imperial Society"", ""FactionState"":""None"", ""Government"":""Patronage"", ""Influence"":0.175824, ""Allegiance"":""Empire"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""League of Nik"", ""FactionState"":""None"", ""Government"":""Dictatorship"", ""Influence"":0.037962, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Nik Legal & Co"", ""FactionState"":""None"", ""Government"":""Corporate"", ""Influence"":0.077922, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Nik Jet Drug Empire"", ""FactionState"":""None"", ""Government"":""Anarchy"", ""Influence"":0.020979, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Sitakapan Expeditionary Forces"", ""FactionState"":""None"", ""Government"":""Dictatorship"", ""Influence"":0.505495, ""Allegiance"":""Empire"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 } ], ""SystemFaction"":{ ""Name"":""Sitakapan Expeditionary Forces"" } }",
            @"{ ""timestamp"":""2025-03-10T03:29:32Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""Laksak"", ""SystemAddress"":4305444669811, ""StarPos"":[-21.53125,-6.31250,116.03125], ""SystemAllegiance"":""Federation"", ""SystemEconomy"":""$economy_Agri;"", ""SystemEconomy_Localised"":""Agriculture"", ""SystemSecondEconomy"":""$economy_Refinery;"", ""SystemSecondEconomy_Localised"":""Refinery"", ""SystemGovernment"":""$government_Democracy;"", ""SystemGovernment_Localised"":""Democracy"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""SystemSecurity_Localised"":""High Security"", ""Population"":11126843580, ""Body"":""Laksak A"", ""BodyID"":1, ""BodyType"":""Star"", ""ControllingPower"":""Yuri Grom"", ""Powers"":[ ""Edmund Mahon"", ""Yuri Grom"", ""Zemina Torval"" ], ""PowerplayState"":""Stronghold"", ""JumpDist"":24.393, ""FuelUsed"":0.752512, ""FuelLevel"":22.502544, ""Factions"":[ { ""Name"":""Laksak Liberals"", ""FactionState"":""None"", ""Government"":""Democracy"", ""Influence"":0.050150, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":21.157000 }, { ""Name"":""Union of Kigana Coalition"", ""FactionState"":""Expansion"", ""Government"":""Confederacy"", ""Influence"":0.169509, ""Allegiance"":""Federation"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":19.799999, ""ActiveStates"":[ { ""State"":""Expansion"" } ] }, { ""Name"":""Laksak Ltd"", ""FactionState"":""Expansion"", ""Government"":""Corporate"", ""Influence"":0.098295, ""Allegiance"":""Federation"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":44.118500, ""RecoveringStates"":[ { ""State"":""Drought"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""Expansion"" } ] }, { ""Name"":""Dominion of Laksak"", ""FactionState"":""Bust"", ""Government"":""Dictatorship"", ""Influence"":0.048144, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000, ""RecoveringStates"":[ { ""State"":""Drought"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""Bust"" } ] }, { ""Name"":""Blue Galactic PLC"", ""FactionState"":""None"", ""Government"":""Corporate"", ""Influence"":0.037111, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":11.795000 }, { ""Name"":""Laksak Gold Mafia"", ""FactionState"":""None"", ""Government"":""Anarchy"", ""Influence"":0.010030, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":2.093500 }, { ""Name"":""Intergalactic Nova Republic"", ""FactionState"":""CivilLiberty"", ""Government"":""Democracy"", ""Influence"":0.586760, ""Allegiance"":""Federation"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000, ""PendingStates"":[ { ""State"":""Boom"", ""Trend"":0 } ], ""RecoveringStates"":[ { ""State"":""Expansion"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""CivilLiberty"" } ] } ], ""SystemFaction"":{ ""Name"":""Intergalactic Nova Republic"", ""FactionState"":""CivilLiberty"" } }",
            @"{ ""timestamp"":""2025-03-10T02:59:26Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""Arque"", ""SystemAddress"":113573366131, ""StarPos"":[66.50000,38.06250,61.12500], ""SystemAllegiance"":""Alliance"", ""SystemEconomy"":""$economy_Extraction;"", ""SystemEconomy_Localised"":""Extraction"", ""SystemSecondEconomy"":""$economy_Agri;"", ""SystemSecondEconomy_Localised"":""Agriculture"", ""SystemGovernment"":""$government_Cooperative;"", ""SystemGovernment_Localised"":""Cooperative"", ""SystemSecurity"":""$SYSTEM_SECURITY_low;"", ""SystemSecurity_Localised"":""Low Security"", ""Population"":111849, ""Body"":""Arque"", ""BodyID"":0, ""BodyType"":""Star"", ""ControllingPower"":""Edmund Mahon"", ""Powers"":[ ""Edmund Mahon"", ""Felicia Winters"", ""Nakato Kaine"" ], ""PowerplayState"":""Exploited"", ""JumpDist"":17.200, ""FuelUsed"":0.345181, ""FuelLevel"":30.824978, ""Factions"":[ { ""Name"":""Arque Commodities"", ""FactionState"":""Lockdown"", ""Government"":""Corporate"", ""Influence"":0.045000, ""Allegiance"":""Alliance"", ""Happiness"":""$Faction_HappinessBand3;"", ""Happiness_Localised"":""Content"", ""MyReputation"":7.920000, ""RecoveringStates"":[ { ""State"":""InfrastructureFailure"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""Lockdown"" }, { ""State"":""Bust"" } ] }, { ""Name"":""Uniting Arque"", ""FactionState"":""War"", ""Government"":""Cooperative"", ""Influence"":0.097000, ""Allegiance"":""Alliance"", ""Happiness"":""$Faction_HappinessBand3;"", ""Happiness_Localised"":""Content"", ""MyReputation"":0.000000, ""ActiveStates"":[ { ""State"":""Drought"" }, { ""State"":""War"" } ] }, { ""Name"":""The Dark Wheel"", ""FactionState"":""War"", ""Government"":""Democracy"", ""Influence"":0.193000, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":100.000000, ""RecoveringStates"":[ { ""State"":""PirateAttack"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""CivilLiberty"" }, { ""State"":""Expansion"" }, { ""State"":""War"" } ] }, { ""Name"":""Arque Blue Posse"", ""FactionState"":""None"", ""Government"":""Anarchy"", ""Influence"":0.022000, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Terra-EX Astro Corp"", ""FactionState"":""War"", ""Government"":""Corporate"", ""Influence"":0.172000, ""Allegiance"":""Federation"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":60.115398, ""RecoveringStates"":[ { ""State"":""PirateAttack"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""Expansion"" }, { ""State"":""War"" } ] }, { ""Name"":""Federal Reclamation Co"", ""FactionState"":""War"", ""Government"":""Corporate"", ""Influence"":0.097000, ""Allegiance"":""Federation"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":17.496201, ""PendingStates"":[ { ""State"":""Expansion"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""War"" } ] }, { ""Name"":""Alliance Rapid-reaction Corps"", ""FactionState"":""Expansion"", ""Government"":""Cooperative"", ""Influence"":0.374000, ""Allegiance"":""Alliance"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":87.191704, ""RecoveringStates"":[ { ""State"":""InfrastructureFailure"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""CivilUnrest"" }, { ""State"":""Expansion"" } ] } ], ""SystemFaction"":{ ""Name"":""Alliance Rapid-reaction Corps"", ""FactionState"":""Expansion"" }, ""Conflicts"":[ { ""WarType"":""war"", ""Status"":""active"", ""Faction1"":{ ""Name"":""Uniting Arque"", ""Stake"":""Otero's Prospect"", ""WonDays"":0 }, ""Faction2"":{ ""Name"":""Federal Reclamation Co"", ""Stake"":"""", ""WonDays"":0 } }, { ""WarType"":""war"", ""Status"":""active"", ""Faction1"":{ ""Name"":""The Dark Wheel"", ""Stake"":""Zoungrana Prospecting"", ""WonDays"":2 }, ""Faction2"":{ ""Name"":""Terra-EX Astro Corp"", ""Stake"":""Tudela Arena"", ""WonDays"":3 } } ] }",
            @"{ ""timestamp"":""2025-03-10T02:20:41Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""Wyrd"", ""SystemAddress"":5031654888146, ""StarPos"":[-11.62500,31.53125,-3.93750], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_Extraction;"", ""SystemEconomy_Localised"":""Extraction"", ""SystemSecondEconomy"":""$economy_Agri;"", ""SystemSecondEconomy_Localised"":""Agriculture"", ""SystemGovernment"":""$government_Corporate;"", ""SystemGovernment_Localised"":""Corporate"", ""SystemSecurity"":""$SYSTEM_SECURITY_medium;"", ""SystemSecurity_Localised"":""Medium Security"", ""Population"":27519138, ""Body"":""Wyrd"", ""BodyID"":1, ""BodyType"":""Star"", ""ControllingPower"":""Edmund Mahon"", ""Powers"":[ ""Edmund Mahon"", ""Jerome Archer"" ], ""PowerplayState"":""Fortified"", ""JumpDist"":40.710, ""FuelUsed"":2.853199, ""FuelLevel"":28.646801, ""Factions"":[ { ""Name"":""Wyrd People's Party"", ""FactionState"":""Boom"", ""Government"":""Communism"", ""Influence"":0.060697, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand1;"", ""Happiness_Localised"":""Elated"", ""MyReputation"":0.000000, ""RecoveringStates"":[ { ""State"":""CivilLiberty"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""Boom"" } ] }, { ""Name"":""Alliance of BD+47 2112"", ""FactionState"":""CivilLiberty"", ""Government"":""Confederacy"", ""Influence"":0.257711, ""Allegiance"":""Federation"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":3.986940, ""ActiveStates"":[ { ""State"":""CivilLiberty"" } ] }, { ""Name"":""Traditional Wyrd Justice Party"", ""FactionState"":""CivilWar"", ""Government"":""Dictatorship"", ""Influence"":0.082587, ""Allegiance"":""Alliance"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000, ""PendingStates"":[ { ""State"":""CivilLiberty"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""CivilWar"" } ] }, { ""Name"":""Wyrd Jet Power Industries"", ""FactionState"":""None"", ""Government"":""Corporate"", ""Influence"":0.083582, ""Allegiance"":""Federation"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Wyrd Raiders"", ""FactionState"":""Bust"", ""Government"":""Anarchy"", ""Influence"":0.009950, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":2.000000, ""ActiveStates"":[ { ""State"":""InfrastructureFailure"" }, { ""State"":""Bust"" } ] }, { ""Name"":""Legacy Corp"", ""FactionState"":""CivilWar"", ""Government"":""Democracy"", ""Influence"":0.082587, ""Allegiance"":""Federation"", ""Happiness"":""$Faction_HappinessBand3;"", ""Happiness_Localised"":""Content"", ""MyReputation"":5.600000, ""ActiveStates"":[ { ""State"":""Outbreak"" }, { ""State"":""CivilWar"" } ] }, { ""Name"":""New Pilots Initiative"", ""FactionState"":""Boom"", ""Government"":""Corporate"", ""Influence"":0.422886, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000, ""RecoveringStates"":[ { ""State"":""Expansion"", ""Trend"":0 } ], ""ActiveStates"":[ { ""State"":""Boom"" } ] } ], ""SystemFaction"":{ ""Name"":""New Pilots Initiative"", ""FactionState"":""Boom"" }, ""Conflicts"":[ { ""WarType"":""civilwar"", ""Status"":""active"", ""Faction1"":{ ""Name"":""Traditional Wyrd Justice Party"", ""Stake"":""Shepherd Engineering Hub"", ""WonDays"":0 }, ""Faction2"":{ ""Name"":""Legacy Corp"", ""Stake"":""Shao Industrial Silo"", ""WonDays"":2 } } ] }"
        };

        [PublicAPI("The name of the system to which the commander has jumped")]
        public string system { get; private set; }

        [PublicAPI( "The numeric system address of the star system to which the commander has jumped" )]
        public ulong systemAddress { get; private set; }

        [PublicAPI("The X co-ordinate of the system to which the commander has jumped")]
        public decimal x { get; private set; }

        [PublicAPI("The Y co-ordinate of the system to which the commander has jumped")]
        public decimal y { get; private set; }

        [PublicAPI("The Z co-ordinate of the system to which the commander has jumped")]
        public decimal z { get; private set; }

        [PublicAPI("The name of the star at which you've arrived")]
        public string star { get; private set; }

        [PublicAPI("The distance the commander has jumped, in light years")]
        public decimal distance { get; private set; }

        [PublicAPI("The amount of fuel used in this jump")]
        public decimal fuelused { get; private set; }

        [PublicAPI("The amount of fuel remaining after this jump")]
        public decimal fuelremaining { get; private set; }

        [PublicAPI("The economy of the system to which the commander has jumped")]
        public string economy => (Economy ?? Economy.None).localizedName;

        [PublicAPI("The secondary economy of the system to which the commander has jumped, if any")]
        public string economy2 => (Economy2 ?? Economy.None).localizedName;

        [PublicAPI("The security of the system to which the commander has jumped")]
        public string security => (securityLevel ?? SecurityLevel.None).localizedName;

        [PublicAPI("The population of the system to which the commander has jumped")]
        public long? population { get; private set; }

        [PublicAPI("A list of faction objects describing the factions in the star system")]
        public List<Faction> factions { get; private set; }

        [PublicAPI("A list of conflict objects describing any conflicts between factions in the star system")]
        public List<Conflict> conflicts { get; private set; }

        [PublicAPI("True if the ship is a transport (e.g. taxi or dropship)")]
        public bool? taxi { get; private set; }

        [PublicAPI("True if the ship is belongs to another player")]
        public bool? multicrew { get; private set; }

        // Controlling faction properties

        [PublicAPI("The faction controlling the system to which the commander has jumped")]
        public string faction => controllingfaction?.name;

        [PublicAPI("The state of the faction controlling the system to which the commander has jumped")]
        public string factionstate => (controllingfaction?.presences.FirstOrDefault(p => p.systemAddress == systemAddress )?.FactionState ?? FactionState.None).localizedName;
        
        [PublicAPI("The allegiance of the system to which the commander has jumped")]
        public string allegiance => (controllingfaction?.Allegiance ?? Superpower.None).localizedName;

        [PublicAPI("The government of the system to which the commander has jumped")]
        public string government => (controllingfaction?.Government ?? Government.None).localizedName;

        // Powerplay properties (only when pledged)

        [PublicAPI( "(Only when pledged) The localized powerplay power controlling the star system, if any. If the star system is `Contested`, this will be empty" )]
        public string power => ( Power ?? Power.None ).localizedName;

        [PublicAPI( "(Only when pledged) The localized names of powerplay powers contesting control of the star system, if any" )]
        public List<string> contestingpowers => ContestingPowers?
            .Select( p => p.localizedName )
            .ToList();

        [PublicAPI( "(Only when pledged) The state of powerplay efforts within the star system" )]
        public string powerstate => ( PowerState ?? PowerplayState.None ).localizedName;

        [PublicAPI( "(Only when pledged) The powerplay power controlling the star system, if any, as an object. If the star system is `Contested`, this will be empty" )]
        public Power Power { get; private set; }

        [PublicAPI( "(Only when pledged) Powerplay powers contesting control of the star system, if any, as objects" )]
        public List<Power> ContestingPowers { get; set; }

        [PublicAPI( "(Only when pledged) The state of powerplay efforts within the star system, as an object" )]
        public PowerplayState PowerState { get; private set; }

        // Thargoid War
        [PublicAPI("Thargoid war data, when applicable")]
        public ThargoidWar ThargoidWar { get; private set; }

        // These properties are not intended to be user facing

        public int? boostused { get; private set; }

        public Economy Economy { get; private set; }

        public Economy Economy2 { get; private set; }

        public Faction controllingfaction { get; private set; }

        public SecurityLevel securityLevel { get; private set; }

        public JumpedEvent ( DateTime timestamp, string system, ulong systemAddress, decimal x, decimal y, decimal z,
            string star, decimal distance, decimal fuelused, decimal fuelremaining, int? boostUsed,
            Faction controllingfaction, List<Faction> factions, List<Conflict> conflicts, Economy economy,
            Economy economy2, SecurityLevel security, long? population, Power controllingPower,
            List<Power> powerplayPowers, PowerplayState powerplayState, bool? taxi, bool? multicrew,
            ThargoidWar thargoidWar ) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
            this.x = x;
            this.y = y;
            this.z = z;
            this.star = star;
            this.distance = distance;
            this.fuelused = fuelused;
            this.fuelremaining = fuelremaining;
            this.boostused = boostUsed;
            this.controllingfaction = controllingfaction;
            this.factions = factions;
            this.conflicts = conflicts;
            this.Economy = (economy ?? Economy.None);
            this.Economy2 = (economy2 ?? Economy.None);
            this.securityLevel = (security ?? SecurityLevel.None);
            this.population = population;
            this.Power = controllingPower;
            this.ContestingPowers = powerplayPowers?
                .Where( p => p.edname != Power?.edname )
                .ToList();
            this.PowerState = powerplayState;
            this.taxi = taxi;
            this.multicrew = multicrew;
            this.ThargoidWar = thargoidWar;
        }
    }
}