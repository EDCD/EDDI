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

        public static MissionType Altruism = new MissionType( "Altruism", true, false, true );
        public static MissionType Assassinate = new MissionType( "Assassinate", true, false, true );
        public static MissionType Collect = new MissionType( "Collect", true, true, true );
        public static MissionType CommunityGoal = new MissionType( "CommunityGoal", false, false, true );
        public static MissionType Courier = new MissionType( "Courier", false, false, true );
        public static MissionType Delivery = new MissionType( "Delivery", false, true, true );
        public static MissionType Disable = new MissionType( "Disable", true, false, true );
        public static MissionType Hack = new MissionType( "Hack", true, false, true );
        public static MissionType LongDistanceExpedition = new MissionType( "LongDistanceExpedition", true, false, true );
        public static MissionType Massacre = new MissionType( "Massacre", true, false, true );
        public static MissionType Mining = new MissionType( "Mining", true, true, true );
        public static MissionType OnFoot = new MissionType( "OnFoot", true, false, true );
        public static MissionType Passenger = new MissionType( "Passenger", false, false, true );
        public static MissionType Piracy = new MissionType( "Piracy", true, false, true );
        public static MissionType Rescue = new MissionType( "Rescue", true, true, true ); // Horizons / surface salvage missions
        public static MissionType Salvage = new MissionType( "Salvage", true, true, true );
        public static MissionType Scan = new MissionType( "Scan", true, false, true ); // Surface scan
        public static MissionType SightSeeing = new MissionType( "Sightseeing", true, false, true );
        public static MissionType Smuggle = new MissionType( "Smuggle", false, true, true );

        // SECONDARY mission types

        public static MissionType Alert = new MissionType( "Alert" );
        public static MissionType Assassination = new MissionType( "Assassination" ); // On-Foot missions to assassinate individual persons 
        public static MissionType BlOps = new MissionType( "BlOps" ); // Black ops
        public static MissionType Bulk = new MissionType( "Bulk" ); // Bulk passenger missions
        public static MissionType Burning = new MissionType( "Burning" ); // Burning station
        public static MissionType Chained = new MissionType( "Chain" ); // Chained missions
        public static MissionType Conflict = new MissionType( "Conflict" ); // Conflict zone massacre missions
        public static MissionType Contact = new MissionType( "Contact" ); // On-foot missions assigned by an on-foot contact
        public static MissionType Covert = new MissionType( "Covert" ); // On-foot covert missions
        public static MissionType Credits = new MissionType( "Credits" ); // Altruism missions delivering credits
        public static MissionType Download = new MissionType( "Download" ); // On-foot hack missions
        public static MissionType Evacuation = new MissionType( "Evacuation" ); // Evacuation passenger missions
        public static MissionType Founder = new MissionType( "Founder" ); // Founder missions (seen on delivery missions)
        public static MissionType GenericPermit1 = new MissionType( "GenericPermit1" ); // This is a special type which seems to be awarded / completed immediately.
        public static MissionType Hard = new MissionType( "Hard" ); // Hard missions
        public static MissionType Heist = new MissionType( "Heist" ); // On-foot heist missions
        public static MissionType Illegal = new MissionType( "Illegal" ); // Illegal missions
        public static MissionType Legal = new MissionType( "Legal" ); // Legal missions
        public static MissionType Megaship = new MissionType( "Megaship" ); // Missions interacting with a megaship
        public static MissionType NCD = new MissionType( "NCD" ); // On-foot nonviolent missions
        public static MissionType Offline = new MissionType( "Offline" ); // On-foot settlement state
        public static MissionType Onslaught = new MissionType( "Onslaught" ); // On-foot massacre of settlement personnel
        public static MissionType Planet = new MissionType( "Planet" ); // Horizons rescue and salvage missions
        public static MissionType Planetary = new MissionType( "Planetary" ); // Horizons assassinate missions
        public static MissionType POI = new MissionType( "POI" ); // On-foot POI heist missions, e.g. 'Seize the Hush from a hidden cache'
        public static MissionType PoliticalPrisoners = new MissionType( "PolPrisoner" ); // Political prisoners passenger mission
        public static MissionType Power = new MissionType( "Power" ); // On-foot mission sabotage target
        public static MissionType Production = new MissionType( "Production" ); // On-foot mission sabotage target
        public static MissionType ProductionHeist = new MissionType( "ProductionHeist" ); // On-foot heist missions, e.g. 'Acquire a (chemical) sample from Gough's Works'
        public static MissionType RankEmp = new MissionType( "RankEmp" ); // Imperial rank missions
        public static MissionType RankFed = new MissionType( "RankFed" ); // Federation rank missions
        public static MissionType Reboot = new MissionType( "Reboot" ); // On-foot settlement mission objective
        public static MissionType RebootRestore = new MissionType( "RebootRestore" ); // On-foot settlement mission objective
        public static MissionType Sabotage = new MissionType( "Sabotage" ); // On-foot settlement sabotage missions
        public static MissionType Scout = new MissionType( "Scout" ); // Thargoid war massacre scout missions
        public static MissionType Skimmer = new MissionType( "Skimmer" ); // Horizons massacre planetary skimmer missions
        public static MissionType Special = new MissionType( "Special" ); // Special missions
        public static MissionType StartZone = new MissionType( "StartZone" ); // Start zone missions
        public static MissionType Thargoid = new MissionType( "Thargoid" ); // Thargoid war massacre missions
        public static MissionType TheDead = new MissionType( "TheDead" ); // Ram-Tah special mission investigating Guardian ruins
        public static MissionType ThargoidWar = new MissionType( "TW" ); // Thargoid War
        public static MissionType Upload = new MissionType( "Upload" ); // On-foot hack missions
        public static MissionType VIP = new MissionType( "VIP" ); // VIP passenger missions
        public static MissionType Welcome = new MissionType( "Welcome" ); // Welcome missions
        public static MissionType Wing = new MissionType( "Wing" ); // Wing missions

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

            var elements = tidiedName.Split('_').ToList();

            // Skip various obscure mission type elements that we don't need or that we're representing some other way
            elements.RemoveAll( t =>
                t == "mission" ||
                t == "arriving" ||
                t == "leaving" ||
                t == "plural" ||
                t == "name" ||
                t == "bs" ||
                t == "ds" ||
                t == "rs" ||
                t == "mb"
            );

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
                    .FirstOrDefault(e => e.edname.ToLowerInvariant() == $"$government_{elements[index]};");
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

        private static readonly Dictionary<string, string> CHAINED = new Dictionary<string, string>()
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
