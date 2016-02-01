using EliteDangerousDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousStarMapService
{
    public class StarMapService
    {
        public void sendStarMapSubmission(Commander commander, decimal distanceToSol, decimal distanceToMaia, decimal distanceToRobigo, decimal distanceTo17Draconis)
        {
            StarMapData data = new StarMapData(commander, distanceToSol, distanceToMaia, distanceToRobigo, distanceTo17Draconis);
            StarMapSubmission submission = new StarMapSubmission(data);
        }

    }

    public class StarMapSubmission
    {
        private StarMapData data { get; set; }

        public StarMapSubmission(StarMapData data)
        {
            this.data = data;
        }
    }

    public class Reference
    {
        private string name { get; set; }
        private decimal? distance { get; set; }

        public Reference(string name)
        {
            this.name = name;
        }
        public Reference(string name, decimal distance)
        {
            this.name = name;
            this.distance = distance;
        }
    }

    public class StarMapData
    {
        private string commander { get; set; }
        private string fromSoftware { get; set; }
        private string fromSoftwareVersion { get; set; }
        private Reference p0 { get; set; }
        private List<Reference> refs { get; set; }

        public StarMapData(Commander commander, decimal distanceToSol, decimal distanceToMaia, decimal distanceToRobigo, decimal distanceTo17Draconis)
        {
            this.commander = commander.Name;
            this.fromSoftware = "EDDI";
            this.fromSoftwareVersion = "0.7.2";
            this.p0 = new Reference(commander.StarSystem);
            this.refs = new List<Reference>();
            this.refs.Add(new Reference("Sol", distanceToSol));
            this.refs.Add(new Reference("Maia", distanceToMaia));
            this.refs.Add(new Reference("Robigo", distanceToRobigo));
            this.refs.Add(new Reference("17 Draconis", distanceTo17Draconis));
        }
    }
}
