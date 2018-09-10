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
        public const string DESCRIPTION = "Triggered when a missions route has been generated or updated.";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionsRouteEvent()
        {
            VARIABLES.Add("update", "True = update, false = calculate");
            VARIABLES.Add("nextsystem", "The next system in the calculated missions route");
            VARIABLES.Add("route", "The remaining '_' delimited missions route");
            VARIABLES.Add("nextdistance", "The distance of the next system in the missions route");
            VARIABLES.Add("routedistance", "The remaining distance of the missions route");
        }

        public bool update { get; private set; }

        public string nextsystem { get; private set; }

        public string route { get; private set; }

        public decimal nextdistance { get; private set; }

        public decimal routedistance { get; private set; }

        public MissionsRouteEvent(DateTime timestamp, bool Update, string NextSystem, string Route, decimal NextDistance, decimal RouteDistance) : base(timestamp, NAME)
        {
            this.update = Update;
            this.nextsystem = NextSystem;
            this.route = Route;
            this.nextdistance = NextDistance;
            this.routedistance = RouteDistance;
        }
    }
}