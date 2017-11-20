using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ExplorationDataSoldEvent : Event
    {
        public const string NAME = "Exploration data sold";
        public const string DESCRIPTION = "Triggered when you sell exploration data";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-23T18:57:55Z\", \"event\":\"SellExplorationData\", \"Systems\":[ \"Gamma Tucanae\", \"Rho Capricorni\", \"Dain\", \"Col 285 Sector BR-S b18-0\", \"LP 571-80\", \"Kawilocidi\", \"Irulachan\", \"Alrai Sector MC-M a7-0\", \"Col 285 Sector FX-Q b19-5\", \"Col 285 Sector EX-Q b19-7\", \"Alrai Sector FB-O a6-3\" ], \"Discovered\":[ \"Irulachan\" ], \"BaseValue\":63573, \"Bonus\":1445 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ExplorationDataSoldEvent()
        {
            VARIABLES.Add("systems", "The systems for which the exploration data was sold");
            VARIABLES.Add("firsts", "The bodies which wre first discovereds");
            VARIABLES.Add("reward", "The reward for selling the exploration data");
            VARIABLES.Add("bonus", "The bonus for first discovereds");
        }

        public List<string> systems { get; private set; }

        public List<string> firsts { get; private set; }

        public decimal reward { get; private set; }

        public decimal bonus { get; private set; }

        public ExplorationDataSoldEvent(DateTime timestamp, List<string> systems, List<string> firsts, decimal reward, decimal bonus) : base(timestamp, NAME)
        {
            this.systems = systems;
            this.firsts = firsts;
            this.reward = reward;
            this.bonus = bonus;
        }
    }
}
