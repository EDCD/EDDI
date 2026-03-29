using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ExplorationDataSoldEvent (
        DateTime timestamp,
        List<string> systems,
        decimal reward,
        decimal bonus,
        decimal total )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Exploration data sold";
        public const string DESCRIPTION = "Triggered when you sell exploration data";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-23T18:57:55Z\", \"event\":\"SellExplorationData\", \"Systems\":[ \"Gamma Tucanae\", \"Rho Capricorni\", \"Dain\", \"Col 285 Sector BR-S b18-0\", \"LP 571-80\", \"Kawilocidi\", \"Irulachan\", \"Alrai Sector MC-M a7-0\", \"Col 285 Sector FX-Q b19-5\", \"Col 285 Sector EX-Q b19-7\", \"Alrai Sector FB-O a6-3\" ], \"Discovered\":[ \"Irulachan\" ], \"BaseValue\":63573, \"Bonus\":1445, \"TotalEarnings\":65018 }";

        [PublicAPI("The systems for which the exploration data was sold")]
        public List<string> systems { get; private set; } = systems;

        [PublicAPI("The reward for selling the exploration data")]
        public decimal reward { get; private set; } = reward;

        [PublicAPI("The bonus for first discoveries")]
        public decimal bonus { get; private set; } = bonus;

        [PublicAPI("The total credits received (after any wages paid to crew and including for example the 200% bonus if rank 5 with Li Yong Rui)")]
        public decimal total { get; private set; } = total;

        public static bool Handle ( DateTime timestamp, string edType, string line, IDictionary<string, object> data,
            ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            if ( edType == "MultiSellExplorationData" )
            {
                var systems = new List<string>();
                data.TryGetValue( "Discovered", out var val );
                var discovered = (List<object>)val;
                if ( discovered != null )
                {
                    foreach ( var discoveredSystem in discovered.Cast<IDictionary<string, object>>() )
                    {
                        var system = JsonParsing.getString( discoveredSystem, "SystemName" );
                        if ( !string.IsNullOrEmpty( system ) )
                        {
                            systems.Add( system );
                        }
                    }
                }

                var reward = JsonParsing.getDecimal( data, "BaseValue" );
                var bonus = JsonParsing.getDecimal( data, "Bonus" );
                var total = JsonParsing.getDecimal( data, "TotalEarnings" );
                events.Add( new ExplorationDataSoldEvent( timestamp, systems, reward, bonus, total ) { raw = line, fromLoad = false } );
                return true;
            }

            if ( edType == "SellExplorationData" )
            {
                data.TryGetValue( "Systems", out var val );
                var systems = ((List<object>)val)?.Cast<string>().ToList();
                //data.TryGetValue( "Discovered", out val );
                //var firsts = ((List<object>)val)?.Cast<string>().ToList();
                var reward = JsonParsing.getDecimal( data, "BaseValue" );
                var bonus = JsonParsing.getDecimal( data, "Bonus" );
                var total = JsonParsing.getDecimal( data, "TotalEarnings" );
                events.Add( new ExplorationDataSoldEvent( timestamp, systems, reward, bonus, total ) { raw = line, fromLoad = false } );
                return true;
            }
            
            return false;
        }
    }
}
