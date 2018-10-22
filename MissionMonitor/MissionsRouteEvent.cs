using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiMissionMonitor
{
    public class MissionsRouteEvent : Event
    {
        public const string NAME = "Missions route";
        public const string DESCRIPTION = "Triggered when a missions route has been generated or updated";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionsRouteEvent()
        {
            VARIABLES.Add("routetype", "Type of route query. 'expiring', 'farthest', 'most', 'nearest', 'route', or 'update'");
            VARIABLES.Add("system", "The destination system");
            VARIABLES.Add("route", "Delimited missions systems list, if applicable");
            VARIABLES.Add("count", "Count of missions, systems, or expiry seconds, depending on route type");
            VARIABLES.Add("distance", "The distance to the destination system");
            VARIABLES.Add("routedistance", "The remaining distance of the missions route, if applicable");
            VARIABLES.Add("missionids", "The mission ID(s) associated with the destination system, if applicable");
        }

        public string routetype { get; private set; }

        public string system { get; private set; }

        public string route { get; private set; }

        public long count { get; private set; }

        public decimal distance { get; private set; }

        public decimal routedistance { get; private set; }

        public List<long> missionids { get; private set; }

        public MissionsRouteEvent(DateTime timestamp, string routetype, string system, string route, long count, decimal distance, decimal routedistance, List<long> missionids) : base(timestamp, NAME)
        {
            this.routetype = routetype;
            this.system = system;
            this.route = route;
            this.count = count;
            this.distance = distance;
            this.routedistance = routedistance;
            this.missionids = missionids;
        }
    }
}