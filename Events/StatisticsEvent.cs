using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class StatisticsEvent : Event
    {
        public const string NAME = "Statistics";
        public const string DESCRIPTION = "Statistics provided at the beginning of a game session";
        public const string SAMPLE = "{ \"timestamp\":\"2019-04-12T04:32:39Z\", \"event\":\"Statistics\", \"Bank_Account\":{ \"Current_Wealth\":5159157167, \"Spent_On_Ships\":1120070427, \"Spent_On_Outfitting\":4789623432, \"Spent_On_Repairs\":35846267, \"Spent_On_Fuel\":607469, \"Spent_On_Ammo_Consumables\":2333849, \"Insurance_Claims\":65, \"Spent_On_Insurance\":113693872, \"Owned_Ship_Count\":30 }, \"Combat\":{ \"Bounties_Claimed\":4115, \"Bounty_Hunting_Profit\":174831710.5, \"Combat_Bonds\":1630, \"Combat_Bond_Profits\":49624687, \"Assassinations\":148, \"Assassination_Profits\":160772353, \"Highest_Single_Reward\":763828, \"Skimmers_Killed\":299 }, \"Crime\":{ \"Notoriety\":0, \"Fines\":402, \"Total_Fines\":2133850, \"Bounties_Received\":716, \"Total_Bounties\":1363285, \"Highest_Bounty\":10615 }, \"Smuggling\":{ \"Black_Markets_Traded_With\":42, \"Black_Markets_Profits\":16115721, \"Resources_Smuggled\":4362, \"Average_Profit\":124928.06976744, \"Highest_Single_Transaction\":1449990 }, \"Trading\":{ \"Markets_Traded_With\":209, \"Market_Profits\":72979036, \"Resources_Traded\":32394, \"Average_Profit\":57965.874503574, \"Highest_Single_Transaction\":7373696 }, \"Mining\":{ \"Mining_Profits\":14267147, \"Quantity_Mined\":1456, \"Materials_Collected\":45683 }, \"Exploration\":{ \"Systems_Visited\":13401, \"Exploration_Profits\":436075034, \"Planets_Scanned_To_Level_2\":4762, \"Planets_Scanned_To_Level_3\":6585, \"Efficient_Scans\":0, \"Highest_Payout\":5639116, \"Total_Hyperspace_Distance\":480443, \"Total_Hyperspace_Jumps\":18670, \"Greatest_Distance_From_Start\":22470.803927811, \"Time_Played\":8271840 }, \"Passengers\":{ \"Passengers_Missions_Accepted\":501, \"Passengers_Missions_Disgruntled\":35, \"Passengers_Missions_Bulk\":3418, \"Passengers_Missions_VIP\":914, \"Passengers_Missions_Delivered\":4332, \"Passengers_Missions_Ejected\":68 }, \"Search_And_Rescue\":{ \"SearchRescue_Traded\":75, \"SearchRescue_Profit\":1138348, \"SearchRescue_Count\":27 }, \"TG_ENCOUNTERS\":{ \"TG_ENCOUNTER_WAKES\":1, \"TG_ENCOUNTER_IMPRINT\":6, \"TG_ENCOUNTER_TOTAL\":9, \"TG_ENCOUNTER_TOTAL_LAST_SYSTEM\":\"Aries Dark Region TE-P b6-4\", \"TG_ENCOUNTER_TOTAL_LAST_TIMESTAMP\":\"3304-12-02 03:21\", \"TG_ENCOUNTER_TOTAL_LAST_SHIP\":\"Python\", \"TG_SCOUT_COUNT\":12 }, \"Crafting\":{ \"Count_Of_Used_Engineers\":20, \"Recipes_Generated\":4502, \"Recipes_Generated_Rank_1\":749, \"Recipes_Generated_Rank_2\":209, \"Recipes_Generated_Rank_3\":595, \"Recipes_Generated_Rank_4\":600, \"Recipes_Generated_Rank_5\":2349 }, \"Crew\":{ \"NpcCrew_TotalWages\":238928453, \"NpcCrew_Hired\":2, \"NpcCrew_Fired\":1 }, \"Multicrew\":{ \"Multicrew_Time_Total\":2601, \"Multicrew_Gunner_Time_Total\":0, \"Multicrew_Fighter_Time_Total\":0, \"Multicrew_Credits_Total\":0, \"Multicrew_Fines_Total\":0 }, \"Material_Trader_Stats\":{ \"Trades_Completed\":157, \"Materials_Traded\":7527, \"Encoded_Materials_Traded\":1473, \"Raw_Materials_Traded\":2054, \"Grade_1_Materials_Traded\":2412, \"Grade_2_Materials_Traded\":1781, \"Grade_3_Materials_Traded\":1793, \"Grade_4_Materials_Traded\":1278, \"Grade_5_Materials_Traded\":263 }, \"CQC\":{ \"CQC_Credits_Earned\":58630, \"CQC_Time_Played\":27450, \"CQC_KD\":1.1729323308271, \"CQC_Kills\":156, \"CQC_WL\":0.24444444444444 } }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static StatisticsEvent()
        {
            VARIABLES.Add("bankaccount.wealth", "The commander's current total wealth in credits (including all assets)");
            VARIABLES.Add("bankaccount.spentonships", "The credits spent on ships");
            VARIABLES.Add("bankaccount.spentonoutfitting", "The credits spent on outfitting");
            VARIABLES.Add("bankaccount.spentonrepairs", "The credits spent on repairs");
            VARIABLES.Add("bankaccount.spentonfuel", "The credits spent on fuel");
            VARIABLES.Add("bankaccount.spentonammoconsumables", "The credits spent on ammo and consumables");
            VARIABLES.Add("bankaccount.spentoninsurance", "The credits spent on insurance");
            VARIABLES.Add("bankaccount.insuranceclaims", "The number of insurance claims filed");
            VARIABLES.Add("bankaccount.ownedshipcount", "The number of ships owned");
            VARIABLES.Add("combat.bountiesclaimed", "The number of bounties claimed");
            VARIABLES.Add("combat.bountyhuntingprofit", "The total credits earned from bounty claims");
            VARIABLES.Add("combat.combatbonds", "The number of combat bonds claimed");
            VARIABLES.Add("combat.combatbondprofits", "The total credits earned from combat bond claims");
            VARIABLES.Add("combat.assassinations", "The number of assassinations performed");
            VARIABLES.Add("combat.assassinationprofits", "The total credits earned from assassinations");
            VARIABLES.Add("combat.highestsinglereward", "The largest credit reward collected from combat");
            VARIABLES.Add("combat.skimmerskilled", "The number of skimmers destroyed");
            VARIABLES.Add("crafting.countofusedengineers", "The total number of engineers used");
            VARIABLES.Add("crafting.recipesgenerated", "The total number of recipes generated");
            VARIABLES.Add("crafting.recipesgeneratedrank1", "The total number of grade 1 recipes generated");
            VARIABLES.Add("crafting.recipesgeneratedrank2", "The total number of grade 2 recipes generated");
            VARIABLES.Add("crafting.recipesgeneratedrank3", "The total number of grade 3 recipes generated");
            VARIABLES.Add("crafting.recipesgeneratedrank4", "The total number of grade 4 recipes generated");
            VARIABLES.Add("crafting.recipesgeneratedrank5", "The total number of grade 5 recipes generated");
            VARIABLES.Add("crime.notoriety", "The current criminal notoriety");
            VARIABLES.Add("crime.fines", "The number of fines received");
            VARIABLES.Add("crime.totalfines", "The total credits accumulated in fines");
            VARIABLES.Add("crime.bountiesreceived", "The number of bounties received");
            VARIABLES.Add("crime.totalbounties", "The total credits accumulated in bounties");
            VARIABLES.Add("crime.highestbounty", "The largest credit bounty received");
            VARIABLES.Add("exploration.systemsvisited", "The number of systems visited");
            VARIABLES.Add("exploration.profits", "The total number of credits earned from exploration");
            VARIABLES.Add("exploration.planetsscannedlevel2", "The number of planets and moons scanned to level 2");
            VARIABLES.Add("exploration.planetsscannedlevel3", "The number of planets and moons scanned to level 3");
            VARIABLES.Add("exploration.highestpayout", "The largest credit reward from exploration");
            VARIABLES.Add("exploration.totalhyperspacedistance", "The total distance traveled in light years");
            VARIABLES.Add("exploration.totalhyperspacejumps", "The total number of hyperspace jumps performed");
            VARIABLES.Add("exploration.greatestdistancefromstart", "The largest distance traveled in light years from starting");
            VARIABLES.Add("exploration.timeplayedseconds", "The total time played, in seconds");
            VARIABLES.Add("materialtrader.tradescompleted", "The number of trades performed at a material trader");
            VARIABLES.Add("materialtrader.materialstraded", "The number of materials traded at a material trader");
            VARIABLES.Add("materialtrader.encodedmaterialstraded", "The number of encoded materials traded at a material trader");
            VARIABLES.Add("materialtrader.rawmaterialstraded", "The number of raw materials traded at a material trader");
            VARIABLES.Add("materialtrader.grade1materialstraded", "The number of grade 1 materials traded at a material trader");
            VARIABLES.Add("materialtrader.grade2materialstraded", "The number of grade 2 materials traded at a material trader");
            VARIABLES.Add("materialtrader.grade3materialstraded", "The number of grade 3 materials traded at a material trader");
            VARIABLES.Add("materialtrader.grade4materialstraded", "The number of grade 4 materials traded at a material trader");
            VARIABLES.Add("materialtrader.grade5materialstraded", "The number of grade 5 materials traded at a material trader");
            VARIABLES.Add("mining.profits", "The total number of credits earned from mining");
            VARIABLES.Add("mining.quantitymined", "The number of commodities refined from mining");
            VARIABLES.Add("mining.materialscollected", "The number of materials collected while mining");
            VARIABLES.Add("multicrew.timetotalseconds", "The total time spent in multicrew, in seconds");
            VARIABLES.Add("multicrew.gunnertimetotalseconds", "The total time spent in multicrew in a gunner role, in seconds");
            VARIABLES.Add("multicrew.fightertimetotalseconds", "The total time spent in multicrew in a fighter role, in seconds");
            VARIABLES.Add("multicrew.multicrewcreditstotal", "The total credits rewarded in multicrew");
            VARIABLES.Add("multicrew.multicrewfinestotal", "The total credits accumulated in fines received in multicrew");
            VARIABLES.Add("npccrew.totalwages", "The total credits paid to npc crew");
            VARIABLES.Add("npccrew.hired", "The number of npc crew hired");
            VARIABLES.Add("npccrew.fired", "The number of npc crew fired");
            VARIABLES.Add("npccrew.died", "The number of npc crew which have died");
            VARIABLES.Add("passengers.accepted", "The total number of passengers accepted for transport");
            VARIABLES.Add("passengers.disgruntled", "The total number of disgruntled passengers");
            VARIABLES.Add("passengers.vip", "The total number of VIP passengers transported");
            VARIABLES.Add("passengers.delivered", "The total number of passengers delivered");
            VARIABLES.Add("passengers.ejected", "The total number of passengers ejected");
            VARIABLES.Add("searchandrescue.traded", "The total number of search and rescue items traded");
            VARIABLES.Add("searchandrescue.profit", "The total number of credits earned from search and rescue");
            VARIABLES.Add("searchandrescue.count", "The number of search and rescue transactions");
            VARIABLES.Add("smuggling.blackmarketstradedwith", "The number of black markets traded with");
            VARIABLES.Add("smuggling.blackmarketprofits", "The total credits earned from trading with black markets");
            VARIABLES.Add("smuggling.resourcessmuggled", "The number of resources smuggled");
            VARIABLES.Add("smuggling.averageprofit", "The average credits earned from black market transactions");
            VARIABLES.Add("smuggling.highestsingletransaction", "The largest credit reward from smuggling");
            VARIABLES.Add("thargoidencounters.wakesscanned", "The number of Thargoid wakes scanned");
            VARIABLES.Add("thargoidencounters.imprints", "The number of Thargoid imprints achieved");
            VARIABLES.Add("thargoidencounters.totalencounters", "The total number of Thargoid enounters");
            VARIABLES.Add("thargoidencounters.lastsystem", "The last system where a Thargoid was enountered");
            VARIABLES.Add("thargoidencounters.lastshipmodel", "The last ship piloted during a Thargoid enounter");
            VARIABLES.Add("trading.marketstradedwith", "The number of legal markets traded with");
            VARIABLES.Add("trading.marketprofits", "The total credits earned from trading with legal markets");
            VARIABLES.Add("trading.resourcestraded", "The number of resources traded");
            VARIABLES.Add("trading.averageprofit", "The average credits earned from legal market transactions");
            VARIABLES.Add("trading.highestsingletransaction", "The largest credit reward from trading");
            VARIABLES.Add("cqc.creditsearned", "The total credits earned from CQC combat");
            VARIABLES.Add("cqc.timeplayedseconds", "The total time spent in CQC combat, in seconds");
            VARIABLES.Add("cqc.killdeathratio", "The ratio of kills to deaths in CQC combat");
            VARIABLES.Add("cqc.kills", "The total number of kills earned in CQC combat");
            VARIABLES.Add("cqc.winlossratio", "The ratio of wins to losses in CQC combat");
        }

        public BankAccountStats bankaccount => statistics.bankaccount;
        public CombatStats combat => statistics.combat;
        public CrimeStats crime => statistics.crime;
        public SmugglingStats smuggling => statistics.smuggling;
        public ThargoidEncounterStats thargoidencounters => statistics.thargoidencounters;
        public TradingStats trading => statistics.trading;
        public MiningStats mining => statistics.mining;
        public ExplorationStats exploration => statistics.exploration;
        public PassengerStats passengers => statistics.passengers;
        public SearchAndRescueStats searchandrescue => statistics.searchandrescue;
        public CraftingStats crafting => statistics.crafting;
        public NpcCrewStats npccrew => statistics.npccrew;
        public MulticrewStats multicrew => statistics.multicrew;
        public MaterialTraderStats materialTrader => statistics.materialtrader;
        public CQCstats cqc => statistics.cqc;

        // Not intended to be user facing
        private Statistics statistics { get; set; }

        public StatisticsEvent(DateTime timestamp, Statistics statistics) : base(timestamp, NAME)
        {
            this.statistics = statistics;
        }
    }
}
