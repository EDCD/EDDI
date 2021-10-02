using System.Collections.Generic;
using EddiDataDefinitions;

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