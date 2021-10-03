using EddiDataDefinitions;
using System.Collections.Generic;

namespace EddiNavigationService
{
    public class ServiceFilter
    {
        public List<string> econ { get; set; }

        public long population { get; set; }

        public List<string> security { get; set; }

        public StationService service { get; set; }

        public int cubeLy { get; set; }
    }
}