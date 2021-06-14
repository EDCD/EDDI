using System;
using Utilities;

namespace EddiDataDefinitions
{
    public class Statistics
    {
        public BankAccountStats bankaccount { get; set; } = new BankAccountStats();
        public CombatStats combat { get; set; } = new CombatStats();
        public CrimeStats crime { get; set; } = new CrimeStats();
        public SmugglingStats smuggling { get; set; } = new SmugglingStats();
        public TradingStats trading { get; set; } = new TradingStats();
        public MiningStats mining { get; set; } = new MiningStats();
        public ExplorationStats exploration { get; set; } = new ExplorationStats();
        public PassengerStats passengers { get; set; } = new PassengerStats();
        public SearchAndRescueStats searchandrescue { get; set; } = new SearchAndRescueStats();
        public CraftingStats crafting { get; set; } = new CraftingStats();
        public NpcCrewStats npccrew { get; set; } = new NpcCrewStats();
        public MulticrewStats multicrew { get; set; } = new MulticrewStats();
        public ThargoidEncounterStats thargoidencounters { get; set; } = new ThargoidEncounterStats();
        public MaterialTraderStats materialtrader { get; set; } = new MaterialTraderStats();
        public CQCstats cqc { get; set; } = new CQCstats();
    }

    public class BankAccountStats
    {
        [PublicAPI("The commander's current total wealth in credits (including all assets)")]
        public long? wealth { get; set; }

        [PublicAPI("The credits spent on ships")]
        public long? spentonships { get; set; }

        [PublicAPI("The credits spent on outfitting")]
        public long? spentonoutfitting { get; set; }

        [PublicAPI("The credits spent on repairs")]
        public long? spentonrepairs { get; set; }

        [PublicAPI("The credits spent on fuel")]
        public long? spentonfuel { get; set; }

        [PublicAPI("The credits spent on ammo and consumables")]
        public long? spentonammoconsumables { get; set; }

        [PublicAPI("The credits spent on insurance")]
        public long? insuranceclaims { get; set; }

        [PublicAPI("The number of insurance claims filed")]
        public long? spentoninsurance { get; set; }

        [PublicAPI("The number of ships owned")]
        public long? ownedshipcount { get; set; }
    }

    public class CombatStats
    {
        [PublicAPI("The number of bounties claimed")]
        public long? bountiesclaimed { get; set; }

        [PublicAPI("The total credits earned from bounty claims")]
        public decimal? bountyhuntingprofit { get; set; }

        [PublicAPI("The number of combat bonds claimed")]
        public long? combatbonds { get; set; }

        [PublicAPI("The total credits earned from combat bond claims")]
        public long? combatbondprofits { get; set; }

        [PublicAPI("The number of assassinations performed")]
        public long? assassinations { get; set; }

        [PublicAPI("The total credits earned from assassinations")]
        public long? assassinationprofits { get; set; }

        [PublicAPI("The largest credit reward collected from combat")]
        public long? highestsinglereward { get; set; }

        [PublicAPI("The number of skimmers destroyed")]
        public long? skimmerskilled { get; set; }
    }

    public class CrimeStats
    {
        [PublicAPI("The current criminal notoriety")]
        public int? notoriety { get; set; }

        [PublicAPI("The number of fines received")]
        public long? fines { get; set; }

        [PublicAPI("The total credits accumulated in fines")]
        public long? totalfines { get; set; }

        [PublicAPI("The number of bounties received")]
        public long? bountiesreceived { get; set; }

        [PublicAPI("The total credits accumulated in bounties")]
        public long? totalbounties { get; set; }

        [PublicAPI("The largest credit bounty received")]
        public long? highestbounty { get; set; }
    }

    public class SmugglingStats
    {
        [PublicAPI("The number of black markets traded with")]
        public long? blackmarketstradedwith { get; set; }

        [PublicAPI("The total credits earned from trading with black markets")]
        public long? blackmarketprofits { get; set; }

        [PublicAPI("The number of resources smuggled")]
        public long? resourcessmuggled { get; set; }

        [PublicAPI("The average credits earned from black market transactions")]
        public decimal? averageprofit { get; set; }

        [PublicAPI("The largest credit reward from smuggling")]
        public long? highestsingletransaction { get; set; }
    }

    public class ThargoidEncounterStats
    {
        [PublicAPI("The number of Thargoid wakes scanned")]
        public long? wakesscanned { get; set; }

        [PublicAPI("The number of Thargoid imprints achieved")]
        public long? imprints { get; set; }

        [PublicAPI("The total number of Thargoid enounters")]
        public long? totalencounters { get; set; }

        [PublicAPI("The last system where a Thargoid was enountered")]
        public string lastsystem { get; set; }

        public DateTime? lasttimestamp { get; set; }

        [PublicAPI("The last ship piloted during a Thargoid enounter")]
        public string lastshipmodel { get; set; }

        [PublicAPI("The total number of Thargoid scouts destroyed")]
        public long? scoutsdestroyed { get; set; }
    }

    public class TradingStats
    {
        [PublicAPI("The number of legal markets traded with")]
        public long? marketstradedwith { get; set; }

        [PublicAPI("The total credits earned from trading with legal markets")]
        public long? marketprofits { get; set; }

        [PublicAPI("The number of resources traded")]
        public long? resourcestraded { get; set; }

        [PublicAPI("The average credits earned from legal market transactions")]
        public decimal? averageprofit { get; set; }

        [PublicAPI("The largest credit reward from trading")]
        public long? highestsingletransaction { get; set; }
    }

    public class MiningStats
    {
        [PublicAPI("The total number of credits earned from mining")]
        public long? profits { get; set; }

        [PublicAPI("The number of commodities refined from mining")]
        public long? quantitymined { get; set; }

        [PublicAPI("The number of materials collected while mining")]
        public long? materialscollected { get; set; }
    }

    public class ExplorationStats
    {
        [PublicAPI("The number of systems visited")]
        public long? systemsvisited { get; set; }

        [PublicAPI("The total number of credits earned from exploration")]
        public long? profits { get; set; }

        [PublicAPI("The number of planets and moons scanned to level 2")]
        public long? planetsscannedlevel2 { get; set; }

        [PublicAPI("The number of planets and moons scanned to level 3")]
        public long? planetsscannedlevel3 { get; set; }

        [PublicAPI("The largest credit reward from exploration")]
        public long? highestpayout { get; set; }

        [PublicAPI("The total distance traveled in light years")]
        public decimal? totalhyperspacedistance { get; set; }

        [PublicAPI("The total number of hyperspace jumps performed")]
        public long? totalhyperspacejumps { get; set; }

        [PublicAPI("The largest distance traveled in light years from starting")]
        public decimal? greatestdistancefromstart { get; set; }

        [PublicAPI("The total time played, in seconds")]
        public long? timeplayedseconds { get; set; }
    }

    public class PassengerStats
    {
        [PublicAPI("The total number of passengers accepted for transport")]
        public long? accepted { get; set; }

        [PublicAPI("The total number of disgruntled passengers")]
        public long? disgruntled { get; set; }

        [PublicAPI("The total number of bulk passengers transported")]
        public long? bulk { get; set; }

        [PublicAPI("The total number of VIP passengers transported")]
        public long? vip { get; set; }

        [PublicAPI("The total number of passengers delivered")]
        public long? delivered { get; set; }

        [PublicAPI("The total number of passengers ejected")]
        public long? ejected { get; set; }
    }

    public class SearchAndRescueStats
    {
        [PublicAPI("The total number of search and rescue items traded")]
        public long? traded { get; set; }

        [PublicAPI("The total number of credits earned from search and rescue")]
        public long? profit { get; set; }

        [PublicAPI("The number of search and rescue transactions")]
        public long? count { get; set; }
    }

    public class CraftingStats
    {
        [PublicAPI("The total number of engineers used")]
        public long? countofusedengineers { get; set; }

        [PublicAPI("The total number of recipes generated")]
        public long? recipesgenerated { get; set; }

        [PublicAPI("The total number of grade 1 recipes generated")]
        public long? recipesgeneratedrank1 { get; set; }

        [PublicAPI("The total number of grade 2 recipes generated")]
        public long? recipesgeneratedrank2 { get; set; }

        [PublicAPI("The total number of grade 3 recipes generated")]
        public long? recipesgeneratedrank3 { get; set; }

        [PublicAPI("The total number of grade 4 recipes generated")]
        public long? recipesgeneratedrank4 { get; set; }

        [PublicAPI("The total number of grade 5 recipes generated")]
        public long? recipesgeneratedrank5 { get; set; }
    }

    public class NpcCrewStats
    {
        [PublicAPI("The total credits paid to npc crew")]
        public long? totalwages { get; set; }

        [PublicAPI("The number of npc crew hired")]
        public long? hired { get; set; }

        [PublicAPI("The number of npc crew fired")]
        public long? fired { get; set; }

        [PublicAPI("The number of npc crew which have died")]
        public long? died { get; set; }
    }

    public class MulticrewStats
    {
        [PublicAPI("The total time spent in multicrew, in seconds")]
        public long? timetotalseconds { get; set; }

        [PublicAPI("The total time spent in multicrew in a gunner role, in seconds")]
        public long? gunnertimetotalseconds { get; set; }

        [PublicAPI("The total time spent in multicrew in a fighter role, in seconds")]
        public long? fightertimetotalseconds { get; set; }

        [PublicAPI("The total credits rewarded in multicrew")]
        public long? multicrewcreditstotal { get; set; }

        [PublicAPI("The total credits accumulated in fines received in multicrew")]
        public long? multicrewfinestotal { get; set; }
    }

    public class MaterialTraderStats
    {
        [PublicAPI("The number of trades performed at a material trader")]
        public long? tradescompleted { get; set; }

        [PublicAPI("The number of materials traded at a material trader")]
        public long? materialstraded { get; set; }

        [PublicAPI("The number of encoded materials traded at a material trader")]
        public long? encodedmaterialstraded { get; set; }

        [PublicAPI("The number of raw materials traded at a material trader")]
        public long? rawmaterialstraded { get; set; }

        [PublicAPI("The number of grade 1 materials traded at a material trader")]
        public long? grade1materialstraded { get; set; }

        [PublicAPI("The number of grade 2 materials traded at a material trader")]
        public long? grade2materialstraded { get; set; }

        [PublicAPI("The number of grade 3 materials traded at a material trader")]
        public long? grade3materialstraded { get; set; }

        [PublicAPI("The number of grade 4 materials traded at a material trader")]
        public long? grade4materialstraded { get; set; }

        [PublicAPI("The number of grade 5 materials traded at a material trader")]
        public long? grade5materialstraded { get; set; }
    }

    public class CQCstats
    {
        [PublicAPI("The total credits earned from CQC combat")]
        public long? creditsearned { get; set; }

        [PublicAPI("The total time spent in CQC combat, in seconds")]
        public long? timeplayedseconds { get; set; }

        [PublicAPI("The total number of kills earned in CQC combat")]
        public long? kills { get; set; }

        [PublicAPI("The ratio of kills to deaths in CQC combat")]
        public decimal? killdeathratio { get; set; }

        [PublicAPI("The ratio of wins to losses in CQC combat")]
        public decimal? winlossratio { get; set; }
    }
}
