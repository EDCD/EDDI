using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Mission types
    /// </summary>
    public class MissionType : ResourceBasedLocalizedEDName<MissionType>
    {
        static MissionType ()
        {
            resourceManager = Properties.MissionType.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new MissionType( edname );

            // Include tags derived from economies, faction states, and government types
            foreach ( var economy in Economy.AllOfThem )
            {
                _ = new MissionType( economy.edname )
                {
                    fallbackLocalizedName = economy.localizedName, fallbackInvariantName = economy.invariantName
                };
            }

            foreach ( var factionState in FactionState.AllOfThem )
            {
                _ = new MissionType( factionState.edname )
                {
                    fallbackLocalizedName = factionState.localizedName,
                    fallbackInvariantName = factionState.invariantName
                };
            }

            foreach ( var government in Government.AllOfThem )
            {
                _ = new MissionType( government.edname )
                {
                    fallbackLocalizedName = government.localizedName,
                    fallbackInvariantName = government.invariantName
                };
            }
        }

        // PRIMARY mission types (typically listed first in mission name)

        public static readonly MissionType Altruism = new( "Altruism", true, false, true );
        public static readonly MissionType Assassinate = new( "Assassinate", true, false, true );
        public static readonly MissionType Collect = new( "Collect", true, true, true );
        public static readonly MissionType Colonisation = new( "Colonisation", false, false, false );
        public static readonly MissionType CommunityGoal = new( "CommunityGoal", false, false, true );
        public static readonly MissionType Courier = new( "Courier", false, false, true );
        public static readonly MissionType Delivery = new( "Delivery", false, true, true );
        public static readonly MissionType Disable = new( "Disable", true, false, true );
        public static readonly MissionType Hack = new( "Hack", true, false, true );
        public static readonly MissionType LongDistanceExpedition = new( "LongDistanceExpedition", true, false, true );
        public static readonly MissionType Massacre = new( "Massacre", true, false, true );
        public static readonly MissionType Mining = new( "Mining", true, true, true );
        public static readonly MissionType OnFoot = new( "OnFoot", true, false, true );
        public static readonly MissionType Passenger = new( "Passenger", false, false, true );
        public static readonly MissionType Piracy = new( "Piracy", true, false, true );
        public static readonly MissionType Rescue = new( "Rescue", true, true, true ); // Horizons / surface salvage missions
        public static readonly MissionType Salvage = new( "Salvage", true, true, true );
        public static readonly MissionType Scan = new( "Scan", true, false, true ); // Surface scan
        public static readonly MissionType SightSeeing = new( "Sightseeing", true, false, true );
        public static readonly MissionType Smuggle = new( "Smuggle", false, true, true );

        // SECONDARY mission types

        public static readonly MissionType Alert = new( "Alert" );
        public static readonly MissionType Assassination = new( "Assassination" ); // On-Foot missions to assassinate individual persons 
        public static readonly MissionType BlOps = new( "BlOps" ); // Black ops
        public static readonly MissionType Bulk = new( "Bulk" ); // Bulk passenger missions
        public static readonly MissionType Burning = new( "Burning" ); // Burning station
        public static readonly MissionType Chained = new( "Chain" ); // Chained missions
        public static readonly MissionType Conflict = new( "Conflict" ); // Conflict zone massacre missions
        public static readonly MissionType Contact = new( "Contact" ); // On-foot missions assigned by an on-foot contact
        public static readonly MissionType Covert = new( "Covert" ); // On-foot covert missions
        public static readonly MissionType Credits = new( "Credits" ); // Altruism missions delivering credits
        public static readonly MissionType Download = new( "Download" ); // On-foot hack missions
        public static readonly MissionType Evacuation = new( "Evacuation" ); // Evacuation passenger missions
        public static readonly MissionType Founder = new( "Founder" ); // Founder missions (seen on delivery missions)
        public static readonly MissionType GenericPermit1 = new( "GenericPermit1" ); // This is a special type which seems to be awarded / completed immediately.
        public static readonly MissionType Hard = new( "Hard" ); // Hard missions
        public static readonly MissionType Heist = new( "Heist" ); // On-foot heist missions
        public static readonly MissionType Illegal = new( "Illegal" ); // Illegal missions
        public static readonly MissionType Legal = new( "Legal" ); // Legal missions
        public static readonly MissionType Megaship = new( "Megaship" ); // Missions interacting with a megaship
        public static readonly MissionType NCD = new( "NCD" ); // On-foot nonviolent missions
        public static readonly MissionType Offline = new( "Offline" ); // On-foot settlement state
        public static readonly MissionType Onslaught = new( "Onslaught" ); // On-foot massacre of settlement personnel
        public static readonly MissionType Planet = new( "Planet" ); // Horizons rescue and salvage missions
        public static readonly MissionType Planetary = new( "Planetary" ); // Horizons assassinate missions
        public static readonly MissionType POI = new( "POI" ); // On-foot POI heist missions, e.g. 'Seize the Hush from a hidden cache'
        public static readonly MissionType PoliticalPrisoners = new( "PolPrisoner" ); // Political prisoners passenger mission
        public static readonly MissionType Power = new( "Power" ); // On-foot mission sabotage target
        public static readonly MissionType Production = new( "Production" ); // On-foot mission sabotage target
        public static readonly MissionType ProductionHeist = new( "ProductionHeist" ); // On-foot heist missions, e.g. 'Acquire a (chemical) sample from Gough's Works'
        public static readonly MissionType RankEmp = new( "RankEmp" ); // Imperial rank missions
        public static readonly MissionType RankFed = new( "RankFed" ); // Federation rank missions
        public static readonly MissionType Reboot = new( "Reboot" ); // On-foot settlement mission objective
        public static readonly MissionType RebootRestore = new( "RebootRestore" ); // On-foot settlement mission objective
        public static readonly MissionType Sabotage = new( "Sabotage" ); // On-foot settlement sabotage missions
        public static readonly MissionType Scout = new( "Scout" ); // Thargoid war massacre scout missions
        public static readonly MissionType Skimmer = new( "Skimmer" ); // Horizons massacre planetary skimmer missions
        public static readonly MissionType Special = new( "Special" ); // Special missions
        public static readonly MissionType StartZone = new( "StartZone" ); // Start zone missions
        public static readonly MissionType Thargoid = new( "Thargoid" ); // Thargoid war massacre missions
        public static readonly MissionType TheDead = new( "TheDead" ); // Ram-Tah special mission investigating Guardian ruins
        public static readonly MissionType ThargoidWar = new( "TW" ); // Thargoid War
        public static readonly MissionType Upload = new( "Upload" ); // On-foot hack missions
        public static readonly MissionType VIP = new( "VIP" ); // VIP passenger missions
        public static readonly MissionType Welcome = new( "Welcome" ); // Welcome missions
        public static readonly MissionType Wing = new( "Wing" ); // Wing missions

        // Operations update special missions
        public static readonly MissionType BiohazardTakedown = new ( "BiohazardTakedown" ); // Megaship Massacre (Strike): A group of researchers are developing a dangerous weapon, you need to assault the ship, obtain the location and IDs of the researchers and take them out.
        public static readonly MissionType FirestormRescue = new ( "FirestormRescue" ); // Burning Rescue: There are hostages trapped in a burning station, break the siege and evacuate those that remain. You will need to manage the stations heat to buy enough time to complete the rescue while dealing with the hostile forces trying to stop you.
        public static readonly MissionType RapidResponse = new( "RapidResponse" ); // Surface Rescue: A planetary port has come under attack, there are still some trapped personnel inside that need rescuing, but you can't delay, a huge hostile enemy fleet is on the way, if the evacuation isn't complete in time there will be no hope for those still trapped inside.
        public static readonly MissionType ReclamationPoint = new ( "ReclamationPoint" ); // Megaship Massacre (Reclaim): The megaship has been hijacked, your job is to eliminate all hostile forces in and around the megaship so the client can recover it. You'll need your best gear and ship to accomplish your goal.
        public static readonly MissionType TacticalTakedown = new( "TacticalTakedown" ); // Counter Attack: The client's megaship has been compromised by a rival faction, you will need to board the ship and fight your way through the command deck to obtain the flight records and trace the origin of the attack. Then you'll want to bring your firepower to bear on the general responsible for the attack at their home base.
        public static readonly MissionType TerminalProsecution = new( "TerminalProsecution" ); // Pirate Hunt: A criminal element is causing trouble in the system, they've been raiding for weeks now, it's your job to clear them out.

        public bool IncludeInMissionRouting { get; set; }

        public bool ClaimAtOrigin { get; set; }

        public bool ClaimAtCargoDepot { get; set; }

        // dummy used to ensure that the static constructor has run
        public MissionType () : this( "" )
        { }

        private MissionType ( string edname, bool claimAtOrigin = false, bool claimAtCargoDepot = false, bool includeInMissionRouting = false ) : base(
            edname, edname )
        {
            ClaimAtOrigin = claimAtOrigin;
            ClaimAtCargoDepot = claimAtCargoDepot;
            IncludeInMissionRouting = includeInMissionRouting;
        }
    }

    public class MissionTypes
    {
        public static List<MissionType> FromMissionName ( string name )
        {
            var tagsList = new List<MissionType>();
            if ( string.IsNullOrEmpty( name ) ) { return tagsList; }

            var tidiedName = name.ToLowerInvariant()
                .Replace("agriculture", "agri") // to match the `agri` economy definition
                .Replace("altruismcredits", "altruism_credits")
                .Replace("assassinatewing", "assassinate_wing")
                .Replace("assassinationillegal", "assassinate_illegal")
                .Replace("collectwing", "collect_wing")
                .Replace("deliverywing", "delivery_wing")
                .Replace("disablemegaship", "disable_megaship")
                .Replace("disablewing", "disable_wing")
                .Replace("elections", "election") // to match the `election` faction state definition
                .Replace("hackmegaship", "hack_megaship")
                .Replace("massacreillegal", "massacre_illegal")
                .Replace("massacrethargoid", "massacre_thargoid")
                .Replace("massacrewing", "massacre_wing")
                .Replace("miningwing", "mining_wing")
                .Replace("onslaughtillegal", "onslaught_illegal")
                .Replace("passengerbulk", "passenger_bulk")
                .Replace("passengerevacuation", "passenger_evacuation")
                .Replace("passengervip", "passenger_vip")
                .Replace("salvageillegal", "salvage_illegal")
                ;

            var elements = tidiedName.Replace(" ", "").Split('_').ToList();

            // Skip various obscure mission type elements that we don't need or that we're representing some other way
            elements.RemoveAll( t => t is "mission" or "arriving" or "initial" or "leaving" or "plural" or "name" or "bs" or "ds" or "rs" or "mb" );

            // Some elements should not be removed but should be moved to the end of the list.
            // Do that here.
            foreach ( var elementToMove in new[] { "tw" } )
            {
                if ( elements.FirstOrDefault() == elementToMove )
                {
                    elements.Remove( elementToMove );
                    elements.Add( elementToMove );
                }
            }

            // Skip passenger elements (we'll fill these using the `Passengers` event)
            elements.RemoveAll( t => PassengerType
                .AllOfThem
                .Select( s => s.edname )
                .Contains( t, StringComparer.InvariantCultureIgnoreCase ) );

            // Tidy up any government name embedded in the elements
            for ( var index = 0; index < elements.Count; index++ )
            {
                var gov = Government.AllOfThem
                    .FirstOrDefault(e => string.Equals( e.edname,
                        $"$government_{elements[ index ]};", StringComparison.OrdinalIgnoreCase ) );
                if ( gov != null )
                {
                    elements[ index ] = gov.edname;
                }
            }

            // Skip numeric elements
            elements.RemoveAll( t => int.TryParse( t, out _ ) );

            // Replace chained mission types with conventional equivalents
            foreach ( var chainedElement in CHAINED )
            {
                if ( elements.Remove( chainedElement.Key ) )
                {
                    elements = elements.Prepend( chainedElement.Value ).ToList();
                }
            }

            foreach ( var element in elements )
            {
                var typeDef = MissionType.FromEDName(element);
                if ( typeDef != null )
                {
                    tagsList.Add( typeDef );
                }
            }

            return tagsList;
        }

        private static readonly Dictionary<string, string> CHAINED = new()
        {
            {"clearingthepath", "delivery"},
            {"drawthegeneralout", "assassinate"},
            {"findthepiratelord", "assassinate"},
            {"helpfinishtheorder", "delivery"},
            {"helpwithpreventionmeasures", "massacre"},
            {"miningtoorder", "mining"},
            {"piracyfraud", "delivery"},
            {"planetaryincursions", "scan"},
            {"rampantleadership", "assassinate"},
            {"regainfooting", "assassinate"},
            {"rescuefromthetwins", "salvage"},
            {"rescuethewares", "salvage"},
            {"safetravelling", "passengervip"},
            {"salvagejustice", "assassinate"},
            {"securingmyposition", "passengervip"},
            {"seekingasylum", "assassinate"},
            {"thedead", "special"},
            {"wrongtarget", "assassinate"},
        };
    }
}
