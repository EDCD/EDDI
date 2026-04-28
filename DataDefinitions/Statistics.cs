using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    public class Statistics
    {
        [JsonProperty( "Bank_Account" )]
        public BankAccountStats bankaccount { get; set; } = new();

        [JsonProperty( "Combat" )]
        public CombatStats combat { get; set; } = new();

        [JsonProperty( "Crime" )]
        public CrimeStats crime { get; set; } = new();

        [JsonProperty( "Smuggling" )]
        public SmugglingStats smuggling { get; set; } = new();

        [JsonProperty( "Trading" )]
        public TradingStats trading { get; set; } = new();

        [JsonProperty( "Mining" )]
        public MiningStats mining { get; set; } = new();

        [JsonProperty( "Exploration" )]
        public ExplorationStats exploration { get; set; } = new();

        [JsonProperty( "Passengers" )]
        public PassengerStats passengers { get; set; } = new();

        [JsonProperty( "Search_And_Rescue" )]
        public SearchAndRescueStats searchandrescue { get; set; } = new();

        [JsonProperty( "Squadron" )]
        public SquadronStats squadron { get; set; } = new();

        [JsonProperty( "TG_ENCOUNTERS" )]
        public ThargoidEncounterStats thargoidencounters { get; set; } = new();

        [JsonProperty( "Crafting" )]
        public CraftingStats crafting { get; set; } = new();

        [JsonProperty( "Crew" )]
        public NpcCrewStats npccrew { get; set; } = new();

        [JsonProperty( "Multicrew" )]
        public MulticrewStats multicrew { get; set; } = new();

        [JsonProperty( "Material_Trader_Stats" )]
        public MaterialTraderStats materialtrader { get; set; } = new();

        [JsonProperty( "CQC" )]
        public CQCstats cqc { get; set; } = new();

        [ JsonProperty( "FLEETCARRIER" ) ]
        public FleetCarrierStats fleetcarrier { get; set; } = new();

        [ JsonProperty( "Exobiology" ) ] 
        public ExobiologyStats exobiology { get; set; } = new();
    }

    public class BankAccountStats
    {
        [JsonProperty( "Current_Wealth" )]
        [PublicAPI( "The commander's current total wealth in credits (including all assets)" )]
        public long? wealth { get; set; }

        [JsonProperty( "Spent_On_Ships" )]
        [PublicAPI( "The credits spent on ships" )]
        public long? spentonships { get; set; }

        [JsonProperty( "Spent_On_Outfitting" )]
        [PublicAPI( "The credits spent on outfitting" )]
        public long? spentonoutfitting { get; set; }

        [JsonProperty( "Spent_On_Repairs" )]
        [PublicAPI( "The credits spent on repairs" )]
        public long? spentonrepairs { get; set; }

        [JsonProperty( "Spent_On_Fuel" )]
        [PublicAPI( "The credits spent on fuel" )]
        public long? spentonfuel { get; set; }

        [JsonProperty( "Spent_On_Ammo_Consumables" )]
        [PublicAPI( "The credits spent on ammo and consumables" )]
        public long? spentonammoconsumables { get; set; }

        [JsonProperty( "Insurance_Claims" )]
        [PublicAPI( "The number of insurance claims filed" )]
        public long? insuranceclaims { get; set; }

        [JsonProperty( "Spent_On_Insurance" )]
        [PublicAPI( "The credits spent on insurance" )]
        public long? spentoninsurance { get; set; }

        [JsonProperty( "Owned_Ship_Count" )]
        [PublicAPI( "The number of ships owned" )]
        public long? ownedshipcount { get; set; }

        [JsonProperty( "Spent_On_Suits" )]
        [PublicAPI( "The credits spent on (Odyssey) suits" )]
        public long? spentonsuits { get; set; }

        [JsonProperty( "Spent_On_Weapons" )]
        [PublicAPI( "The credits spent on (Odyssey) weapons" )]
        public long? spentonweapons { get; set; }

        [JsonProperty( "Spent_On_Suit_Consumables" )]
        [PublicAPI( "The credits spent on (Odyssey) suit consumables" )]
        public long? spentonsuitconsumables { get; set; }

        [JsonProperty( "Suits_Owned" )]
        [PublicAPI( "The number of (Odyssey) suits owned" )]
        public long? suitsowned { get; set; }

        [JsonProperty( "Weapons_Owned" )]
        [PublicAPI( "The number of (Odyssey) weapons owned" )]
        public long? weaponsowned { get; set; }

        [JsonProperty( "Spent_On_Premium_Stock" )]
        [PublicAPI( "The credits spent on premium stock" )]
        public long? spentonpremiumstock { get; set; }

        [JsonProperty( "Premium_Stock_Bought" )]
        [PublicAPI( "The number of times you have purchased premium stock" )]
        public long? premiumstockbought { get; set; }
    }

    public class CombatStats
    {
        [JsonProperty( "Bounties_Claimed" )]
        [PublicAPI( "The number of bounties claimed" )]
        public long? bountiesclaimed { get; set; }

        [JsonProperty( "Bounty_Hunting_Profit" )]
        [PublicAPI( "The total credits earned from bounty claims" )]
        public decimal? bountyhuntingprofit { get; set; }

        [JsonProperty( "Combat_Bonds" )]
        [PublicAPI( "The number of combat bond vouchers claimed" )]
        public long? combatbonds { get; set; }

        [JsonProperty( "Combat_Bond_Profits" )]
        [PublicAPI( "The total credits earned from combat bond claims" )]
        public long? combatbondprofits { get; set; }

        [JsonProperty( "Assassinations" )]
        [PublicAPI( "The number of assassinations performed" )]
        public long? assassinations { get; set; }

        [JsonProperty( "Assassination_Profits" )]
        [PublicAPI( "The total credits earned from assassinations" )]
        public long? assassinationprofits { get; set; }

        [JsonProperty( "Highest_Single_Reward" )]
        [PublicAPI( "The largest credit reward collected from combat" )]
        public long? highestsinglereward { get; set; }

        [JsonProperty( "Skimmers_Killed" )]
        [PublicAPI( "The number of skimmers destroyed" )]
        public long? skimmerskilled { get; set; }

        [JsonProperty( "OnFoot_Combat_Bonds" )]
        [PublicAPI( "The number of (on foot) combat bond vouchers you have claimed" )]
        public long? onfootcombatbonds { get; set; }

        [JsonProperty( "OnFoot_Combat_Bonds_Profits" )]
        [PublicAPI( "The total credits earned from (on foot) combat bond vouchers you have claimed" )]
        public long? onfootcombatbondprofits { get; set; }

        [JsonProperty( "OnFoot_Vehicles_Destroyed" )]
        [PublicAPI( "The number of vehicles you have destroyed while on foot" )]
        public long? onfootvehiclesdestroyed { get; set; }

        [JsonProperty( "OnFoot_Ships_Destroyed" )]
        [PublicAPI( "The number of ships you have destroyed while on foot" )]
        public long? onfootshipsdestroyed { get; set; }

        [JsonProperty( "Dropships_Taken" )]
        [PublicAPI( "The number of (on foot) dropship flights you have taken" )]
        public long? dropshipstaken { get; set; }

        [JsonProperty( "Dropships_Booked" )]
        [PublicAPI( "The number of (on foot) dropship flights you have booked" )]
        public long? dropshipsbooked { get; set; }

        [JsonProperty( "Dropships_Cancelled" )]
        [PublicAPI( "The number of (on foot) dropship flights you have cancelled" )]
        public long? dropshipscancelled { get; set; }

        [JsonProperty( "ConflictZone_High" )]
        [PublicAPI( "The number of (High) Conflict Zones you've joined" )]
        public long? conflictzonehigh { get; set; }

        [JsonProperty( "ConflictZone_Medium" )]
        [PublicAPI( "The number of (Medium) Conflict Zones you've joined" )]
        public long? conflictzonemedium { get; set; }

        [JsonProperty( "ConflictZone_Low" )]
        [PublicAPI( "The number of (Low) Conflict Zones you've joined" )]
        public long? conflictzonelow { get; set; }

        [JsonProperty( "ConflictZone_Total" )]
        [PublicAPI( "The total number of Conflict Zones you've joined" )]
        public long? conflictzonetotal { get; set; }

        [JsonProperty( "ConflictZone_High_Wins" )]
        [PublicAPI( "The number of (High) Conflict Zones you've won" )]
        public long? conflictzonehighwins { get; set; }

        [JsonProperty( "ConflictZone_Medium_Wins" )]
        [PublicAPI( "The number of (Medium) Conflict Zones you've won" )]
        public long? conflictzonemediumwins { get; set; }

        [JsonProperty( "ConflictZone_Low_Wins" )]
        [PublicAPI( "The number of (Low) Conflict Zones you've won" )]
        public long? conflictzonelowwins { get; set; }

        [JsonProperty( "ConflictZone_Total_Wins" )]
        [PublicAPI( "The total number of Conflict Zones you've won" )]
        public long? conflictzonetotalwins { get; set; }

        [JsonProperty( "Settlement_Defended" )]
        [PublicAPI( "The total number of (on foot) settlements you've defended" )]
        public long? settlementsdefended { get; set; }

        [JsonProperty( "Settlement_Conquered" )]
        [PublicAPI( "The total number of (on foot) settlements you've conquered" )]
        public long? settlementsconquered { get; set; }

        [JsonProperty( "OnFoot_Skimmers_Killed" )]
        [PublicAPI( "The total number of (on foot) skimmers you've killed" )]
        public long? onfootskimmerskilled { get; set; }

        [JsonProperty( "OnFoot_Scavs_Killed" )]
        [PublicAPI( "The total number of (on foot) scavengers you've killed" )]
        public long? onfootscavengerskilled { get; set; }
    }

    public class CraftingStats
    {
        [JsonProperty( "Count_Of_Used_Engineers" )]
        [PublicAPI( "The total number of engineers used" )]
        public long? countofusedengineers { get; set; }

        [JsonProperty( "Recipes_Generated" )]
        [PublicAPI( "The total number of recipes generated" )]
        public long? recipesgenerated { get; set; }

        [JsonProperty( "Recipes_Generated_Rank_1" )]
        [PublicAPI( "The total number of grade 1 recipes generated" )]
        public long? recipesgeneratedrank1 { get; set; }

        [JsonProperty( "Recipes_Generated_Rank_2" )]
        [PublicAPI( "The total number of grade 2 recipes generated" )]
        public long? recipesgeneratedrank2 { get; set; }

        [JsonProperty( "Recipes_Generated_Rank_3" )]
        [PublicAPI( "The total number of grade 3 recipes generated" )]
        public long? recipesgeneratedrank3 { get; set; }

        [JsonProperty( "Recipes_Generated_Rank_4" )]
        [PublicAPI( "The total number of grade 4 recipes generated" )]
        public long? recipesgeneratedrank4 { get; set; }

        [JsonProperty( "Recipes_Generated_Rank_5" )]
        [PublicAPI( "The total number of grade 5 recipes generated" )]
        public long? recipesgeneratedrank5 { get; set; }

        [JsonProperty( "Suit_Mods_Applied" )]
        [PublicAPI( "The total number of (on-foot) suit mods applied" )]
        public long? suitmodsapplied { get; set; }

        [JsonProperty( "Weapon_Mods_Applied" )]
        [PublicAPI( "The total number of (on-foot) weapon mods applied" )]
        public long? weaponmodsapplied { get; set; }

        [JsonProperty( "Suits_Upgraded" )]
        [PublicAPI( "The total number of (on-foot) suits you've upgraded" )]
        public long? suitsupgraded { get; set; }

        [JsonProperty( "Weapons_Upgraded" )]
        [PublicAPI( "The total number of (on-foot) weapons you've upgraded" )]
        public long? weaponsupgraded { get; set; }

        [JsonProperty( "Suits_Upgraded_Full" )]
        [PublicAPI( "The total number of (on-foot) suits you've fully upgraded" )]
        public long? suitsupgradedfull { get; set; }

        [JsonProperty( "Weapons_Upgraded_Full" )]
        [PublicAPI( "The total number of (on-foot) weapons you've fully upgraded" )]
        public long? weaponsupgradedfull { get; set; }

        [JsonProperty( "Suit_Mods_Applied_Full" )]
        [PublicAPI( "The total number of times you've maxed out the (on-foot) suit mods on a suit" )]
        public long? suitmodsappliedfull { get; set; }

        [JsonProperty( "Weapon_Mods_Applied_Full" )]
        [PublicAPI( "The total number of times you've maxed out the (on-foot) weapon mods on a suit" )]
        public long? weaponmodsappliedfull { get; set; }
    }

    public class CrimeStats
    {
        [JsonProperty( "Notoriety" )]
        [PublicAPI( "The current criminal notoriety" )]
        public int? notoriety { get; set; }

        [JsonProperty( "Fines" )]
        [PublicAPI( "The number of fines received" )]
        public long? fines { get; set; }

        [JsonProperty( "Total_Fines" )]
        [PublicAPI( "The total credits accumulated in fines" )]
        public long? totalfines { get; set; }

        [JsonProperty( "Bounties_Received" )]
        [PublicAPI( "The number of bounties received" )]
        public long? bountiesreceived { get; set; }

        [JsonProperty( "Total_Bounties" )]
        [PublicAPI( "The total credits accumulated in bounties" )]
        public long? totalbounties { get; set; }

        [JsonProperty( "Highest_Bounty" )]
        [PublicAPI( "The largest credit bounty received" )]
        public long? highestbounty { get; set; }

        [JsonProperty( "Malware_Uploaded" )]
        [PublicAPI( "The amount of malware you have uploaded in on-foot settlements" )]
        public int? malwareuploaded { get; set; }

        [JsonProperty( "Settlements_State_Shutdown" )]
        [PublicAPI( "The number of on-foot settlements you have placed into a shut down state" )]
        public int? settlementsstateshutdown { get; set; }

        [JsonProperty( "Production_Sabotage" )]
        [PublicAPI( "The number of on-foot production sabatoge missions you've completed" )]
        public int? productionsabotage { get; set; }

        [JsonProperty( "Production_Theft" )]
        [PublicAPI( "The number of on-foot production theft missions you've completed" )]
        public int? productiontheft { get; set; }

        [JsonProperty( "Total_Murders" )]
        [PublicAPI( "The total number of on-foot murders you've committed" )]
        public int? totalmurders { get; set; }

        [JsonProperty( "Citizens_Murdered" )]
        [PublicAPI( "The number of citizens you've murdered while on-foot" )]
        public int? citizensmurdered { get; set; }

        [JsonProperty( "Omnipol_Murdered" )]
        [PublicAPI( "The number of Omnipol agents you've murdered while on-foot" )]
        public int? omnipolmurdered { get; set; }

        [JsonProperty( "Guards_Murdered" )]
        [PublicAPI( "The number of guards you've murdered while on-foot" )]
        public int? guardsmurdered { get; set; }

        [JsonProperty( "Data_Stolen" )]
        [PublicAPI( "The amount of data you've stolen while on-foot" )]
        public int? datastolen { get; set; }

        [JsonProperty( "Goods_Stolen" )]
        [PublicAPI( "The amount of goods you've stolen while on-foot" )]
        public int? goodsstolen { get; set; }

        [JsonProperty( "Sample_Stolen" )]
        [PublicAPI( "The amount of samples you've stolen while on-foot" )]
        public int? samplestolen { get; set; }

        [JsonProperty( "Total_Stolen" )]
        [PublicAPI( "The total amount of items you've stolen while on-foot" )]
        public int? totalstolen { get; set; }

        [JsonProperty( "Turrets_Destroyed" )]
        [PublicAPI( "The number of turrets you've destroyed while on-foot" )]
        public int? turretsdestroyed { get; set; }

        [JsonProperty( "Turrets_Overloaded" )]
        [PublicAPI( "The number of turrets you've overloaded while on-foot" )]
        public int? turretsoverloaded { get; set; }

        [JsonProperty( "Turrets_Total" )]
        [PublicAPI( "The total number of turrets you've disabled while on-foot" )]
        public int? turretstotal { get; set; }

        [JsonProperty( "Value_Stolen_StateChange" )]
        public int? valuestolenstatechange { get; set; }

        [JsonProperty( "Profiles_Cloned" )]
        [PublicAPI( "The number of profiles you've cloned while on-foot" )]
        public int? profilescloned { get; set; }
    }

    public class CQCstats
    {
        [JsonProperty( "CQC_Credits_Earned" )]
        [PublicAPI( "The total credits earned from CQC combat" )]
        public long? creditsearned { get; set; }

        [JsonProperty( "CQC_Time_Played" )]
        [PublicAPI( "The total time spent in CQC combat, in seconds" )]
        public long? timeplayedseconds { get; set; }

        [JsonProperty( "CQC_Kills" )]
        [PublicAPI( "The total number of kills earned in CQC combat" )]
        public long? kills { get; set; }

        [JsonProperty( "CQC_KD" )]
        [PublicAPI( "The ratio of kills to deaths in CQC combat" )]
        public decimal? killdeathratio { get; set; }

        [JsonProperty( "CQC_WL" )]
        [PublicAPI( "The ratio of wins to losses in CQC combat" )]
        public decimal? winlossratio { get; set; }
    }

    public class ExobiologyStats
    {
        [JsonProperty( "Organic_Genus_Encountered" )]
        [PublicAPI( "The number of organic genuses you've encountered" )]
        public int? organicgenusencountered { get; set; }

        [JsonProperty( "Organic_Species_Encountered" )]
        [PublicAPI( "The number of organic species you've encountered" )]
        public int? organicspeciesencountered { get; set; }

        [JsonProperty( "Organic_Variant_Encountered" )]
        [PublicAPI( "The number of organic variants you've encountered" )]
        public int? organicvariantencountered { get; set; }

        [JsonProperty( "Organic_Data_Profits" )]
        [PublicAPI( "The total earnings you have received from exobiology" )]
        public long? organicdataprofits { get; set; }

        [JsonProperty( "Organic_Data" )]
        [PublicAPI( "The amount of organic data you've collected" )]
        public int? organicdata { get; set; }

        [JsonProperty( "First_Logged_Profits" )]
        [PublicAPI( "The total credits you've received from being the first to log the presence of an organic" )]
        public long? firstloggedprofits { get; set; }

        [JsonProperty( "First_Logged" )]
        [PublicAPI( "The number of times where you've been the first to log the presence of an organic" )]
        public int? firstlogged { get; set; }

        [JsonProperty( "Organic_Systems" )]
        [PublicAPI( "The number of star systems with organics that you have sampled" )]
        public int? organicsystems { get; set; }

        [JsonProperty( "Organic_Planets" )]
        [PublicAPI( "The number of planets with organics that you have sampled" )]
        public int? organicplanets { get; set; }

        [JsonProperty( "Organic_Genus" )]
        [PublicAPI( "The number of genuses you have sampled" )]
        public int? organicgenus { get; set; }

        [JsonProperty( "Organic_Species" )]
        [PublicAPI( "The number of species you have sampled" )]
        public int? organicspecies { get; set; }
    }

    public class ExplorationStats
    {
        [JsonProperty( "Systems_Visited" )]
        [PublicAPI( "The number of systems visited" )]
        public long? systemsvisited { get; set; }

        [JsonProperty( "Exploration_Profits" )]
        [PublicAPI( "The total number of credits earned from exploration" )]
        public long? profits { get; set; }

        [JsonProperty( "Planets_Scanned_To_Level_2" )]
        [PublicAPI( "The number of planets and moons scanned to level 2" )]
        public long? planetsscannedlevel2 { get; set; }

        [JsonProperty( "Planets_Scanned_To_Level_3" )]
        [PublicAPI( "The number of planets and moons scanned to level 3" )]
        public long? planetsscannedlevel3 { get; set; }

        [JsonProperty( "Efficient_Scans" )]
        [PublicAPI( "The number of planets and moons you've probed efficiently" )]
        public int? efficientscans { get; set; }

        [JsonProperty( "Highest_Payout" )]
        [PublicAPI( "The largest credit reward from exploration" )]
        public long? highestpayout { get; set; }

        [JsonProperty( "Total_Hyperspace_Distance" )]
        [PublicAPI( "The total distance traveled in light years" )]
        public decimal? totalhyperspacedistance { get; set; }

        [JsonProperty( "Total_Hyperspace_Jumps" )]
        [PublicAPI( "The total number of hyperspace jumps performed" )]
        public long? totalhyperspacejumps { get; set; }

        [JsonProperty( "Greatest_Distance_From_Start" )]
        [PublicAPI( "The largest distance traveled in light years from starting" )]
        public decimal? greatestdistancefromstart { get; set; }

        [JsonProperty( "Time_Played" )]
        [PublicAPI( "The total time played, in seconds" )]
        public long? timeplayedseconds { get; set; }

        [JsonProperty( "OnFoot_Distance_Travelled" )]
        [PublicAPI( "The total distance (in meters) you have traveled on-foot" )]
        public decimal? onfootdistancetraveled { get; set; }

        [JsonProperty( "Shuttle_Journeys" )]
        [PublicAPI( "The total number of times you've taken a shuttle journey while on-foot" )]
        public long? shuttlejourneys { get; set; }

        [JsonProperty( "Shuttle_Distance_Travelled" )]
        [PublicAPI( "The total distance traveled in light years while in shuttle journeys on-foot" )]
        public decimal? shuttledistancetraveled { get; set; }

        [JsonProperty( "Spent_On_Shuttles" )]
        [PublicAPI( "The total credits spent for shuttle journeys while on-foot" )]
        public long? spentonshuttles { get; set; }

        [JsonProperty( "First_Footfalls" )]
        [PublicAPI( "The total number of first footfalls you've recorded" )]
        public long? firstfootfalls { get; set; }

        [JsonProperty( "Planet_Footfalls" )]
        [PublicAPI( "The total number of planet footfalls you've recorded" )]
        public long? planetfootfalls { get; set; }

        [JsonProperty( "Settlements_Visited" )]
        [PublicAPI( "The total number of (on-foot) settlements you've visited" )]
        public long? settlementsvisited { get; set; }
    }

    public class FleetCarrierStats
    {
        [JsonProperty( "FLEETCARRIER_EXPORT_TOTAL" )]
        [PublicAPI( "The number of items exported from your carrier's market" )]
        public int? fleetcarrierexporttotal { get; set; }

        [JsonProperty( "FLEETCARRIER_IMPORT_TOTAL" )]
        [PublicAPI( "The number of items imported into your carrier's market" )]
        public int? fleetcarrierimporttotal { get; set; }

        [JsonProperty( "FLEETCARRIER_TRADEPROFIT_TOTAL" )]
        [PublicAPI( "The profits earned from trades on your carrier's market" )]
        public long? fleetcarriertradeprofittotal { get; set; }

        [JsonProperty( "FLEETCARRIER_TRADESPEND_TOTAL" )]
        [PublicAPI( "The credits spent from trades on your carrier's market" )]
        public long? fleetcarriertradespendtotal { get; set; }

        [JsonProperty( "FLEETCARRIER_STOLENPROFIT_TOTAL" )]
        [PublicAPI( "The profits earned from trades on your carrier's black market" )]
        public long? fleetcarrierstolenprofittotal { get; set; }

        [JsonProperty( "FLEETCARRIER_STOLENSPEND_TOTAL" )]
        [PublicAPI( "The credits spent from trades on your carrier's black market" )]
        public long? fleetcarrierstolenspendtotal { get; set; }

        [JsonProperty( "FLEETCARRIER_DISTANCE_TRAVELLED" )]
        [PublicAPI( "The distance in light years that your carrier has traveled" )]
        public decimal? fleetcarrierdistancetraveled { get; set; }

        [JsonProperty( "FLEETCARRIER_TOTAL_JUMPS" )]
        [PublicAPI( "The number of jumps that your carrier has completed" )]
        public int? fleetcarriertotaljumps { get; set; }

        [JsonProperty( "FLEETCARRIER_SHIPYARD_SOLD" )]
        [PublicAPI( "The number of items sold through your carrier's shipyard" )]
        public int? fleetcarriershipyardsold { get; set; }

        [JsonProperty( "FLEETCARRIER_SHIPYARD_PROFIT" )]
        [PublicAPI( "The total profit from selling items through your carrier's shipyard" )]
        public long? fleetcarriershipyardprofit { get; set; }

        [JsonProperty( "FLEETCARRIER_OUTFITTING_SOLD" )]
        [PublicAPI( "The number of items sold through your carrier's outfitting" )]
        public int? fleetcarriersoutfittingsold { get; set; }

        [JsonProperty( "FLEETCARRIER_OUTFITTING_PROFIT" )]
        [PublicAPI( "The total profit from selling items through your carrier's outfitting" )]
        public long? fleetcarrieroutfittingprofit { get; set; }

        [JsonProperty( "FLEETCARRIER_REARM_TOTAL" )]
        [PublicAPI( "The number of times your carrier has rearmed a commander" )]
        public int? fleetcarrierrearmtotal { get; set; }

        [JsonProperty( "FLEETCARRIER_REFUEL_TOTAL" )]
        [PublicAPI( "The number of times your carrier has refueled a commander" )]
        public int? fleetcarrierrefueltotal { get; set; }

        [JsonProperty( "FLEETCARRIER_REFUEL_PROFIT" )]
        [PublicAPI( "The total profit from commanders refueling at your carrier" )]
        public long? FLEETCARRIERREFUELPROFIT { get; set; }

        [JsonProperty( "FLEETCARRIER_REPAIRS_TOTAL" )]
        [PublicAPI( "The number of times your carrier has repaired a commander's ship" )]
        public int? fleetcarrierrepairstotal { get; set; }

        [JsonProperty( "FLEETCARRIER_VOUCHERS_REDEEMED" )]
        [PublicAPI( "The number of times your carrier has redeemed a commander's bounty vouchers" )]
        public int? fleetcarriervouchersredeemed { get; set; }

        [JsonProperty( "FLEETCARRIER_VOUCHERS_PROFIT" )]
        [PublicAPI( "The total profit from commanders redeeming bounty vouchers at your carrier" )]
        public long? fleetcarriervouchersprofit { get; set; }
    }

    public class MaterialTraderStats
    {
        [JsonProperty( "Trades_Completed" )]
        [PublicAPI( "The number of trades performed at a material trader" )]
        public long? tradescompleted { get; set; }

        [JsonProperty( "Materials_Traded" )]
        [PublicAPI( "The number of materials traded at a material trader" )]
        public long? materialstraded { get; set; }

        [JsonProperty( "Encoded_Materials_Traded" )]
        [PublicAPI( "The number of encoded materials traded at a material trader" )]
        public long? encodedmaterialstraded { get; set; }

        [JsonProperty( "Raw_Materials_Traded" )]
        [PublicAPI( "The number of raw materials traded at a material trader" )]
        public long? rawmaterialstraded { get; set; }

        [JsonProperty( "Grade_1_Materials_Traded" )]
        [PublicAPI( "The number of grade 1 materials traded at a material trader" )]
        public long? grade1materialstraded { get; set; }

        [JsonProperty( "Grade_2_Materials_Traded" )]
        [PublicAPI( "The number of grade 2 materials traded at a material trader" )]
        public long? grade2materialstraded { get; set; }

        [JsonProperty( "Grade_3_Materials_Traded" )]
        [PublicAPI( "The number of grade 3 materials traded at a material trader" )]
        public long? grade3materialstraded { get; set; }

        [JsonProperty( "Grade_4_Materials_Traded" )]
        [PublicAPI( "The number of grade 4 materials traded at a material trader" )]
        public long? grade4materialstraded { get; set; }

        [JsonProperty( "Grade_5_Materials_Traded" )]
        [PublicAPI( "The number of grade 5 materials traded at a material trader" )]
        public long? grade5materialstraded { get; set; }

        [JsonProperty( "Assets_Traded_In" )]
        [PublicAPI( "The number of times you traded assets in" )]
        public long? assetstradedin { get; set; }

        [JsonProperty( "Assets_Traded_Out" )]
        [PublicAPI( "The number of times you traded assets out" )]
        public long? assetstradedout { get; set; }
    }

    public class MiningStats
    {
        [JsonProperty( "Mining_Profits" )]
        [PublicAPI( "The total number of credits earned from mining" )]
        public long? profits { get; set; }

        [JsonProperty( "Quantity_Mined" )]
        [PublicAPI( "The number of commodities refined from mining" )]
        public long? quantitymined { get; set; }

        [JsonProperty( "Materials_Collected" )]
        [PublicAPI( "The number of materials collected while mining" )]
        public long? materialscollected { get; set; }
    }

    public class MulticrewStats
    {
        [JsonProperty( "Multicrew_Time_Total" )]
        [PublicAPI( "The total time spent in multicrew, in seconds" )]
        public long? timetotalseconds { get; set; }

        [JsonProperty( "Multicrew_Gunner_Time_Total" )]
        [PublicAPI( "The total time spent in multicrew in a gunner role, in seconds" )]
        public long? gunnertimetotalseconds { get; set; }

        [JsonProperty( "Multicrew_Fighter_Time_Total" )]
        [PublicAPI( "The total time spent in multicrew in a fighter role, in seconds" )]
        public long? fightertimetotalseconds { get; set; }

        [JsonProperty( "Multicrew_Credits_Total" )]
        [PublicAPI( "The total credits rewarded in multicrew" )]
        public long? multicrewcreditstotal { get; set; }

        [JsonProperty( "Multicrew_Fines_Total" )]
        [PublicAPI( "The total credits accumulated in fines received in multicrew" )]
        public long? multicrewfinestotal { get; set; }
    }

    public class NpcCrewStats
    {
        [JsonProperty( "NpcCrew_TotalWages" )]
        [PublicAPI( "The total credits paid to npc crew" )]
        public long? totalwages { get; set; }

        [JsonProperty( "NpcCrew_Hired" )]
        [PublicAPI( "The number of npc crew hired" )]
        public long? hired { get; set; }

        [JsonProperty( "NpcCrew_Fired" )]
        [PublicAPI( "The number of npc crew fired" )]
        public long? fired { get; set; }

        [JsonProperty( "NpcCrew_Died" )]
        [PublicAPI( "The number of npc crew which have died" )]
        public long? died { get; set; }
    }

    public class PassengerStats
    {
        [JsonProperty( "Passengers_Missions_Accepted" )]
        [PublicAPI( "The total number of passengers accepted for transport" )]
        public long? accepted { get; set; }

        [JsonProperty( "Passengers_Missions_Disgruntled" )]
        [PublicAPI( "The total number of disgruntled passengers" )]
        public long? disgruntled { get; set; }

        [JsonProperty( "Passengers_Missions_Bulk" )]
        [PublicAPI( "The total number of bulk passengers transported" )]
        public long? bulk { get; set; }

        [JsonProperty( "Passengers_Missions_VIP" )]
        [PublicAPI( "The total number of VIP passengers transported" )]
        public long? vip { get; set; }

        [JsonProperty( "Passengers_Missions_Delivered" )]
        [PublicAPI( "The total number of passengers delivered" )]
        public long? delivered { get; set; }

        [JsonProperty( "Passengers_Missions_Ejected" )]
        [PublicAPI( "The total number of passengers ejected" )]
        public long? ejected { get; set; }
    }

    public class SearchAndRescueStats
    {
        [JsonProperty( "SearchRescue_Traded" )]
        [PublicAPI( "The total number of search and rescue items traded" )]
        public long? traded { get; set; }

        [JsonProperty( "SearchRescue_Profit" )]
        [PublicAPI( "The total number of credits earned from search and rescue" )]
        public long? profit { get; set; }

        [JsonProperty( "SearchRescue_Count" )]
        [PublicAPI( "The number of search and rescue transactions" )]
        public long? count { get; set; }

        [JsonProperty( "Salvage_Legal_POI" )]
        [PublicAPI( "The amount of legal salvage you've collected from POI" )]
        public int? salvagelegalpoi { get; set; }

        [JsonProperty( "Salvage_Legal_Settlements" )]
        [PublicAPI( "The amount of legal salvage you've collected from settlements" )]
        public int? salvagelegalsettlements { get; set; }

        [JsonProperty( "Salvage_Illegal_POI" )]
        [PublicAPI( "The amount of illegal salvage you've collected from POI" )]
        public int? salvageillegalpoi { get; set; }

        [JsonProperty( "Salvage_Illegal_Settlements" )]
        [PublicAPI( "The amount of illegal salvage you've collected from settlements" )]
        public int? salvageillegalsettlements { get; set; }

        [JsonProperty( "Maglocks_Opened" )]
        [PublicAPI( "The number of maglocks you've opened" )]
        public int? maglocksopened { get; set; }

        [JsonProperty( "Panels_Opened" )]
        [PublicAPI( "The number of panels you've opened" )]
        public int? panelsopened { get; set; }

        [JsonProperty( "Settlements_State_FireOut" )]
        [PublicAPI( "The number of settlements you've stopped from burning" )]
        public int? settlementsstatefireout { get; set; }

        [JsonProperty( "Settlements_State_Reboot" )]
        [PublicAPI( "The number of settlements you've rebooted" )]
        public int? settlementsstatereboot { get; set; }
    }

    public class SmugglingStats
    {
        [JsonProperty( "Black_Markets_Traded_With" )]
        [PublicAPI( "The number of black markets traded with" )]
        public long? blackmarketstradedwith { get; set; }

        [JsonProperty( "Black_Markets_Profits" )]
        [PublicAPI( "The total credits earned from trading with black markets" )]
        public long? blackmarketprofits { get; set; }

        [JsonProperty( "Resources_Smuggled" )]
        [PublicAPI( "The number of resources smuggled" )]
        public long? resourcessmuggled { get; set; }

        [JsonProperty( "Average_Profit" )]
        [PublicAPI( "The average credits earned from black market transactions" )]
        public decimal? averageprofit { get; set; }

        [JsonProperty( "Highest_Single_Transaction" )]
        [PublicAPI( "The largest credit reward from smuggling" )]
        public long? highestsingletransaction { get; set; }
    }

    public class SquadronStats
    {
        [JsonProperty( "Squadron_Bank_Credits_Deposited" )]
        [PublicAPI( "The total credits you've deposited into the squadron bank" )]
        public long? squadronbankcreditsdeposited { get; set; }

        [JsonProperty( "Squadron_Bank_Credits_Withdrawn" )]
        [PublicAPI( "The total credits you've withdrawn from the squadron bank" )]
        public long? squadronbankcreditswithdrawn { get; set; }

        [JsonProperty( "Squadron_Bank_Commodities_Deposited_Num" )]
        [PublicAPI( "The total number of commodities you've deposited into the squadron bank" )]
        public int? squadronbankcommoditiesdepositednum { get; set; }

        [JsonProperty( "Squadron_Bank_Commodities_Deposited_Value" )]
        [PublicAPI( "The total value of commodities you've deposited into the squadron bank" )]
        public long? squadronbankcommoditiesdepositedvalue { get; set; }

        [JsonProperty( "Squadron_Bank_Commodities_Withdrawn_Num" )]
        [PublicAPI( "The total number of commodities you've withdrawn from the squadron bank" )]
        public int? squadronbankcommoditieswithdrawnnum { get; set; }

        [JsonProperty( "Squadron_Bank_Commodities_Withdrawn_Value" )]
        [PublicAPI( "The total value of commodities you've withdrawn from the squadron bank" )]
        public long? squadronbankcommoditieswithdrawnvalue { get; set; }

        [JsonProperty( "Squadron_Bank_PersonalAssets_Deposited_Num" )]
        [PublicAPI( "The total number of personal assets you've deposited into the squadron bank" )]
        public int? squadronbankpersonalassetsdepositednum { get; set; }

        [JsonProperty( "Squadron_Bank_PersonalAssets_Deposited_Value" )]
        [PublicAPI( "The total value of personal assets you've deposited into the squadron bank" )]
        public long? squadronbankpersonalassetsdepositedvalue { get; set; }

        [JsonProperty( "Squadron_Bank_PersonalAssets_Withdrawn_Num" )]
        [PublicAPI( "The total number of personal assets you've withdrawn from the squadron bank" )]
        public int? squadronbankpersonalassetswithdrawnnum { get; set; }

        [JsonProperty( "Squadron_Bank_PersonalAssets_Withdrawn_Value" )]
        [PublicAPI( "The total value of personal assets you've withdrawn from the squadron bank" )]
        public long? squadronbankpersonalassetswithdrawnvalue { get; set; }

        [JsonProperty( "Squadron_Bank_Ships_Deposited_Num" )]
        [PublicAPI( "The total number of ships you've deposited into the squadron bank" )]
        public int? squadronbankshipsdepositednum { get; set; }

        [JsonProperty( "Squadron_Bank_Ships_Deposited_Value" )]
        [PublicAPI( "The total value of ships you've deposited into the squadron bank" )]
        public long? squadronbankshipsdepositedvalue { get; set; }

        [JsonProperty( "Squadron_Leaderboard_aegis_highestcontribution" )]
        public long? squadronleaderboardaegishighestcontribution { get; set; }

        [JsonProperty( "Squadron_Leaderboard_bgs_highestcontribution" )]
        public long? squadronleaderboardbgshighestcontribution { get; set; }

        [JsonProperty( "Squadron_Leaderboard_bounty_highestcontribution" )]
        public long? squadronleaderboardbountyhighestcontribution { get; set; }

        [JsonProperty( "Squadron_Leaderboard_colonisation_contribution_highestcontribution" )]
        public long? squadronleaderboardcolonisationContributionhighestcontribution { get; set; }

        [JsonProperty( "Squadron_Leaderboard_combat_highestcontribution" )]
        public long? squadronleaderboardcombathighestcontribution { get; set; }

        [JsonProperty( "Squadron_Leaderboard_cqc_highestcontribution" )]
        public long? squadronleaderboardcqchighestcontribution { get; set; }

        [JsonProperty( "Squadron_Leaderboard_exploration_highestcontribution" )]
        public long? squadronleaderboardexplorationhighestcontribution { get; set; }

        [JsonProperty( "Squadron_Leaderboard_mining_highestcontribution" )]
        public long? squadronleaderboardmininghighestcontribution { get; set; }

        [JsonProperty( "Squadron_Leaderboard_powerplay_highestcontribution" )]
        public long? squadronleaderboardpowerplayhighestcontribution { get; set; }

        [JsonProperty( "Squadron_Leaderboard_trade_highestcontribution" )]
        public long? squadronleaderboardtradehighestcontribution { get; set; }

        [JsonProperty( "Squadron_Leaderboard_trade_illicit_highestcontribution" )]
        public long? squadronleaderboardtradeIllicithighestcontribution { get; set; }

        [JsonProperty( "Squadron_Leaderboard_podiums" )]
        public int? squadronleaderboardpodiums { get; set; }
    }

    public class ThargoidEncounterStats
    {
        [JsonProperty( "TG_ENCOUNTER_WAKES" )]
        [PublicAPI( "The number of Thargoid wakes scanned" )]
        public long? wakesscanned { get; set; }

        [JsonProperty( "TG_ENCOUNTER_KILLED" )]
        [PublicAPI( "The number of Thargoids killed" )]
        public long? kills { get; set; }

        [JsonProperty( "TG_ENCOUNTER_IMPRINT" )]
        [PublicAPI( "The number of Thargoid imprints achieved" )]
        public long? imprints { get; set; }

        [JsonProperty( "TG_ENCOUNTER_TOTAL" )]
        [PublicAPI( "The total number of Thargoid enounters" )]
        public long? totalencounters { get; set; }

        [JsonProperty( "TG_ENCOUNTER_TOTAL_LAST_SYSTEM" )]
        [PublicAPI( "The last system where a Thargoid was enountered" )]
        public string lastsystem { get; set; }

        [JsonProperty( "TG_ENCOUNTER_TOTAL_LAST_TIMESTAMP" )]
        public DateTime? lasttimestamp { get; set; }

        [JsonProperty( "TG_ENCOUNTER_TOTAL_LAST_SHIP" )]
        [PublicAPI( "The last ship piloted during a Thargoid enounter" )]
        public string lastshipmodel { get; set; }
    }

    public class TradingStats
    {
        [JsonProperty( "Markets_Traded_With" )]
        [PublicAPI( "The number of legal markets you've traded with" )]
        public long? marketstradedwith { get; set; }

        [JsonProperty( "Market_Profits" )]
        [PublicAPI( "The total credits earned from trading with legal markets" )]
        public long? marketprofits { get; set; }

        [JsonProperty( "Resources_Traded" )]
        [PublicAPI( "The number of resources traded" )]
        public long? resourcestraded { get; set; }

        [JsonProperty( "Average_Profit" )]
        [PublicAPI( "The average credits earned from legal market transactions" )]
        public decimal? averageprofit { get; set; }

        [JsonProperty( "Highest_Single_Transaction" )]
        [PublicAPI( "The largest credit reward from trading" )]
        public long? highestsingletransaction { get; set; }

        [JsonProperty( "Data_Sold" )]
        [PublicAPI( "The number of (on foot) data sold" )]
        public long? datasold { get; set; }

        [JsonProperty( "Goods_Sold" )]
        [PublicAPI( "The number of (on foot) goods sold" )]
        public long? goodssold { get; set; }

        [JsonProperty( "Assets_Sold" )]
        [PublicAPI( "The number of (on foot) assets sold" )]
        public long? assetssold { get; set; }
    }
}
