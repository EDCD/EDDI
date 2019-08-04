using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public long? wealth { get; set; }
        public long? spentonships { get; set; }
        public long? spentonoutfitting { get; set; }
        public long? spentonrepairs { get; set; }
        public long? spentonfuel { get; set; }
        public long? spentonammoconsumables { get; set; }
        public long? insuranceclaims { get; set; }
        public long? spentoninsurance { get; set; }
        public long? ownedshipcount { get; set; }
    }

    public class CombatStats
    {
        public long? bountiesclaimed { get; set; }
        public decimal? bountyhuntingprofit { get; set; }
        public long? combatbonds { get; set; }
        public long? combatbondprofits { get; set; }
        public long? assassinations { get; set; }
        public long? assassinationprofits { get; set; }
        public long? highestsinglereward { get; set; }
        public long? skimmerskilled { get; set; }
    }

    public class CrimeStats
    {
        public int? notoriety { get; set; }
        public long? fines { get; set; }
        public long? totalfines { get; set; }
        public long? bountiesreceived { get; set; }
        public long? totalbounties { get; set; }
        public long? highestbounty { get; set; }
    }

    public class SmugglingStats
    {
        public long? blackmarketstradedwith { get; set; }
        public long? blackmarketprofits { get; set; }
        public long? resourcessmuggled { get; set; }
        public decimal? averageprofit { get; set; }
        public long? highestsingletransaction { get; set; }
    }

    public class ThargoidEncounterStats
    {
        public long? wakesscanned { get; set; }
        public long? imprints { get; set; }
        public long? totalencounters { get; set; }
        public string lastsystem { get; set; }
        public DateTime? lasttimestamp { get; set; }
        public string lastshipmodel { get; set; }
        public long? scoutsdestroyed { get; set; }
    }

    public class TradingStats
    {
        public long? marketstradedwith { get; set; }
        public long? marketprofits { get; set; }
        public long? resourcestraded { get; set; }
        public decimal? averageprofit { get; set; }
        public long? highestsingletransaction { get; set; }
    }

    public class MiningStats
    {
        public long? profits { get; set; }
        public long? quantitymined { get; set; }
        public long? materialscollected { get; set; }
    }

    public class ExplorationStats
    {
        public long? systemsvisited { get; set; }
        public long? profits { get; set; }
        public long? planetsscannedlevel2 { get; set; }
        public long? planetsscannedlevel3 { get; set; }
        public long? highestpayout { get; set; }
        public decimal? totalhyperspacedistance { get; set; }
        public long? totalhyperspacejumps { get; set; }
        public decimal? greatestdistancefromstart { get; set; }
        public long? timeplayedseconds { get; set; }
    }

    public class PassengerStats
    {
        public long? accepted { get; set; }
        public long? disgruntled { get; set; }
        public long? bulk { get; set; }
        public long? vip { get; set; }
        public long? delivered { get; set; }
        public long? ejected { get; set; }
    }

    public class SearchAndRescueStats
    {
        public long? traded { get; set; }
        public long? profit { get; set; }
        public long? count { get; set; }
    }

    public class CraftingStats
    {
        public long? countofusedengineers { get; set; }
        public long? recipesgenerated { get; set; }
        public long? recipesgeneratedrank1 { get; set; }
        public long? recipesgeneratedrank2 { get; set; }
        public long? recipesgeneratedrank3 { get; set; }
        public long? recipesgeneratedrank4 { get; set; }
        public long? recipesgeneratedrank5 { get; set; }
    }

    public class NpcCrewStats
    {
        public long? totalwages { get; set; }
        public long? hired { get; set; }
        public long? fired { get; set; }
        public long? died { get; set; }
    }

    public class MulticrewStats
    {
        public long? timetotalseconds { get; set; }
        public long? gunnertimetotalseconds { get; set; }
        public long? fightertimetotalseconds { get; set; }
        public long? multicrewcreditstotal { get; set; }
        public long? multicrewfinestotal { get; set; }
    }

    public class MaterialTraderStats
    {
        public long? tradescompleted { get; set; }
        public long? materialstraded { get; set; }
        public long? encodedmaterialstraded { get; set; }
        public long? rawmaterialstraded { get; set; }
        public long? grade1materialstraded { get; set; }
        public long? grade2materialstraded { get; set; }
        public long? grade3materialstraded { get; set; }
        public long? grade4materialstraded { get; set; }
        public long? grade5materialstraded { get; set; }
    }

    public class CQCstats
    {
        public long? creditsearned { get; set; }
        public long? timeplayedseconds { get; set; }
        public long? kills { get; set; }
        public decimal? killdeathratio { get; set; }
        public decimal? winlossratio { get; set; }
    }
}
