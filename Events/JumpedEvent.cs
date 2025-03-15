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
            @"{ ""timestamp"":""2024-10-28T02:55:58Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""Nik"", ""SystemAddress"":7230812328674, ""StarPos"":[90.84375,-20.71875,94.87500], ""SystemAllegiance"":""Empire"", ""SystemEconomy"":""$economy_Terraforming;"", ""SystemEconomy_Localised"":""Terraforming"", ""SystemSecondEconomy"":""$economy_Industrial;"", ""SystemSecondEconomy_Localised"":""Industrial"", ""SystemGovernment"":""$government_Dictatorship;"", ""SystemGovernment_Localised"":""Dictatorship"", ""SystemSecurity"":""$SYSTEM_SECURITY_medium;"", ""SystemSecurity_Localised"":""Medium Security"", ""Population"":17835682, ""Body"":""Nik A"", ""BodyID"":1, ""BodyType"":""Star"", ""ControllingPower"":""Nakato Kaine"", ""Powers"":[ ""Aisling Duval"", ""Felicia Winters"", ""Nakato Kaine"" ], ""PowerplayState"":""Exploited"", ""JumpDist"":64.049, ""FuelUsed"":1.856707, ""FuelLevel"":21.687792, ""BoostUsed"":4, ""Factions"":[ { ""Name"":""Pisamazin Movement"", ""FactionState"":""None"", ""Government"":""Dictatorship"", ""Influence"":0.063936, ""Allegiance"":""Empire"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Nik Future"", ""FactionState"":""None"", ""Government"":""Democracy"", ""Influence"":0.117882, ""Allegiance"":""Federation"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Bajoji Imperial Society"", ""FactionState"":""None"", ""Government"":""Patronage"", ""Influence"":0.175824, ""Allegiance"":""Empire"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""League of Nik"", ""FactionState"":""None"", ""Government"":""Dictatorship"", ""Influence"":0.037962, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Nik Legal & Co"", ""FactionState"":""None"", ""Government"":""Corporate"", ""Influence"":0.077922, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Nik Jet Drug Empire"", ""FactionState"":""None"", ""Government"":""Anarchy"", ""Influence"":0.020979, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Sitakapan Expeditionary Forces"", ""FactionState"":""None"", ""Government"":""Dictatorship"", ""Influence"":0.505495, ""Allegiance"":""Empire"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 } ], ""SystemFaction"":{ ""Name"":""Sitakapan Expeditionary Forces"" } }"
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