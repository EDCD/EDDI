using EddiDataDefinitions;
using System.Collections.Generic;

namespace EddiNavigationService
{
    public class ServiceFilter
    {
        public List<Economy> systemEconomies { get; set; }

        public List<Economy> stationEconomies { get; set; }

        public long minPopulation { get; set; }

        public List<SecurityLevel> security { get; set; }

        public List<StationService> services { get; set; }

        public List<StationModel> stationModels { get; set; }

        public int cubeLy { get; set; }
    }
}