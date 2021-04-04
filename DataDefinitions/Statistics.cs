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
        [PublicAPI]
        public long? wealth { get; set; }

        [PublicAPI]
        public long? spentonships { get; set; }

        [PublicAPI]
        public long? spentonoutfitting { get; set; }

        [PublicAPI]
        public long? spentonrepairs { get; set; }

        [PublicAPI]
        public long? spentonfuel { get; set; }

        [PublicAPI]
        public long? spentonammoconsumables { get; set; }

        [PublicAPI]
        public long? insuranceclaims { get; set; }

        [PublicAPI]
        public long? spentoninsurance { get; set; }

        [PublicAPI]
        public long? ownedshipcount { get; set; }
    }

    public class CombatStats
    {
        [PublicAPI]
        public long? bountiesclaimed { get; set; }

        [PublicAPI]
        public decimal? bountyhuntingprofit { get; set; }

        [PublicAPI]
        public long? combatbonds { get; set; }

        [PublicAPI]
        public long? combatbondprofits { get; set; }

        [PublicAPI]
        public long? assassinations { get; set; }

        [PublicAPI]
        public long? assassinationprofits { get; set; }

        [PublicAPI]
        public long? highestsinglereward { get; set; }

        [PublicAPI]
        public long? skimmerskilled { get; set; }
    }

    public class CrimeStats
    {
        [PublicAPI]
        public int? notoriety { get; set; }

        [PublicAPI]
        public long? fines { get; set; }

        [PublicAPI]
        public long? totalfines { get; set; }

        [PublicAPI]
        public long? bountiesreceived { get; set; }

        [PublicAPI]
        public long? totalbounties { get; set; }

        [PublicAPI]
        public long? highestbounty { get; set; }
    }

    public class SmugglingStats
    {
        [PublicAPI]
        public long? blackmarketstradedwith { get; set; }

        [PublicAPI]
        public long? blackmarketprofits { get; set; }

        [PublicAPI]
        public long? resourcessmuggled { get; set; }

        [PublicAPI]
        public decimal? averageprofit { get; set; }

        [PublicAPI]
        public long? highestsingletransaction { get; set; }
    }

    public class ThargoidEncounterStats
    {
        [PublicAPI]
        public long? wakesscanned { get; set; }

        [PublicAPI]
        public long? imprints { get; set; }

        [PublicAPI]
        public long? totalencounters { get; set; }

        [PublicAPI]
        public string lastsystem { get; set; }

        [PublicAPI]
        public DateTime? lasttimestamp { get; set; }

        [PublicAPI]
        public string lastshipmodel { get; set; }

        [PublicAPI]
        public long? scoutsdestroyed { get; set; }
    }

    public class TradingStats
    {
        [PublicAPI]
        public long? marketstradedwith { get; set; }

        [PublicAPI]
        public long? marketprofits { get; set; }

        [PublicAPI]
        public long? resourcestraded { get; set; }

        [PublicAPI]
        public decimal? averageprofit { get; set; }

        [PublicAPI]
        public long? highestsingletransaction { get; set; }
    }

    public class MiningStats
    {
        [PublicAPI]
        public long? profits { get; set; }

        [PublicAPI]
        public long? quantitymined { get; set; }

        [PublicAPI]
        public long? materialscollected { get; set; }
    }

    public class ExplorationStats
    {
        [PublicAPI]
        public long? systemsvisited { get; set; }

        [PublicAPI]
        public long? profits { get; set; }

        [PublicAPI]
        public long? planetsscannedlevel2 { get; set; }

        [PublicAPI]
        public long? planetsscannedlevel3 { get; set; }

        [PublicAPI]
        public long? highestpayout { get; set; }

        [PublicAPI]
        public decimal? totalhyperspacedistance { get; set; }

        [PublicAPI]
        public long? totalhyperspacejumps { get; set; }

        [PublicAPI]
        public decimal? greatestdistancefromstart { get; set; }

        [PublicAPI]
        public long? timeplayedseconds { get; set; }
    }

    public class PassengerStats
    {
        [PublicAPI]
        public long? accepted { get; set; }

        [PublicAPI]
        public long? disgruntled { get; set; }

        [PublicAPI]
        public long? bulk { get; set; }

        [PublicAPI]
        public long? vip { get; set; }

        [PublicAPI]
        public long? delivered { get; set; }

        [PublicAPI]
        public long? ejected { get; set; }
    }

    public class SearchAndRescueStats
    {
        [PublicAPI]
        public long? traded { get; set; }

        [PublicAPI]
        public long? profit { get; set; }

        [PublicAPI]
        public long? count { get; set; }
    }

    public class CraftingStats
    {
        [PublicAPI]
        public long? countofusedengineers { get; set; }

        [PublicAPI]
        public long? recipesgenerated { get; set; }

        [PublicAPI]
        public long? recipesgeneratedrank1 { get; set; }

        [PublicAPI]
        public long? recipesgeneratedrank2 { get; set; }

        [PublicAPI]
        public long? recipesgeneratedrank3 { get; set; }

        [PublicAPI]
        public long? recipesgeneratedrank4 { get; set; }

        [PublicAPI]
        public long? recipesgeneratedrank5 { get; set; }
    }

    public class NpcCrewStats
    {
        [PublicAPI]
        public long? totalwages { get; set; }

        [PublicAPI]
        public long? hired { get; set; }

        [PublicAPI]
        public long? fired { get; set; }

        [PublicAPI]
        public long? died { get; set; }
    }

    public class MulticrewStats
    {
        [PublicAPI]
        public long? timetotalseconds { get; set; }

        [PublicAPI]
        public long? gunnertimetotalseconds { get; set; }

        [PublicAPI]
        public long? fightertimetotalseconds { get; set; }

        [PublicAPI]
        public long? multicrewcreditstotal { get; set; }

        [PublicAPI]
        public long? multicrewfinestotal { get; set; }
    }

    public class MaterialTraderStats
    {
        [PublicAPI]
        public long? tradescompleted { get; set; }

        [PublicAPI]
        public long? materialstraded { get; set; }

        [PublicAPI]
        public long? encodedmaterialstraded { get; set; }

        [PublicAPI]
        public long? rawmaterialstraded { get; set; }

        [PublicAPI]
        public long? grade1materialstraded { get; set; }

        [PublicAPI]
        public long? grade2materialstraded { get; set; }

        [PublicAPI]
        public long? grade3materialstraded { get; set; }

        [PublicAPI]
        public long? grade4materialstraded { get; set; }

        [PublicAPI]
        public long? grade5materialstraded { get; set; }
    }

    public class CQCstats
    {
        [PublicAPI]
        public long? creditsearned { get; set; }

        [PublicAPI]
        public long? timeplayedseconds { get; set; }

        [PublicAPI]
        public long? kills { get; set; }

        [PublicAPI]
        public decimal? killdeathratio { get; set; }

        [PublicAPI]
        public decimal? winlossratio { get; set; }
    }
}
