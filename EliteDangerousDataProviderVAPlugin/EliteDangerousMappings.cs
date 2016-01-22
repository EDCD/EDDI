using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousDataProviderVAPlugin
{
    // Mappings from one ID to another for the disparate systems
    // Handles EDDB, Coriolis.io and Elite: Dangerous itself
    public class EliteDangerousMappings
    {
        public static Dictionary<long, string> EDDBToCoriolis = new Dictionary<long, string>
        {
        };
        public static Dictionary<string, long> CoriolisToEDDB = EDDBToCoriolis.ToDictionary(g => g.Value, g => g.Key);

        public static Dictionary<long, long> EDDBToED = new Dictionary<long, long>
        {
        };
        public static Dictionary<long, long> EDToEDDB = EDDBToED.ToDictionary(g => g.Value, g => g.Key);
    }
}
