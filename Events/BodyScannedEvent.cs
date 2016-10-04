using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiEvents
{
    public class BodyScannedEvent : Event
    {
        public const string NAME = "Body scanned";
        public const string DESCRIPTION = "Triggered when you complete a scan of a planetary body";
        public const string SAMPLE = "{\"timestamp\":\"2016-09-23T13:48:46Z\",\"event\":\"Scan\",\"BodyName\":\"HIP 60024 1 a\",\"DistanceFromArrivalLS\":1044.451538,\"TidalLock\":true,\"TerraformState\":\"\",\"PlanetClass\":\"Rocky body\",\"Atmosphere\":\"\",\"Volcanism\":\"\",\"MassEM\":0.000522,\"Radius\":567386.000000,\"SurfaceGravity\":0.646235,\"SurfaceTemperature\":179.129990,\"SurfacePressure\":0.000000,\"Landable\":true,\"Materials\":{\"iron\":19.5,\"sulphur\":19.1,\"carbon\":16.1,\"nickel\":14.8,\"phosphorus\":10.3,\"chromium\":8.8,\"zinc\":5.3,\"arsenic\":2.5,\"cadmium\":1.5,\"antimony\":1.2,\"mercury\":0.9},\"OrbitalPeriod\":245536.875000,\"RotationPeriod\":239640.718750}";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BodyScannedEvent()
        {
            VARIABLES.Add("name", "The name of the body that has been scanned");
            VARIABLES.Add("bodyclass", "The class of the body that has been scanned (High metal content body etc)");
            VARIABLES.Add("gravity", "The surface gravity of the body that has been scanned, relative to Earth's gravity");
            VARIABLES.Add("temperature", "The surface temperature of the body that has been scanned");
            VARIABLES.Add("pressure", "The surface pressure of the body that has been scanned");
            VARIABLES.Add("tidallylocked", "True if the body is tidally locked");
            VARIABLES.Add("landable", "True if the body is landable");
            VARIABLES.Add("atmosphere", "The atmosphere of the body that has been scanned");
            VARIABLES.Add("volcanism", "The volcanism of the body that has been scanned");
            VARIABLES.Add("materials", "A list of materials present on the body that has been scanned");
        }

        public string name { get; private set; }

        public string bodyclass { get; private set; }

        public decimal gravity{ get; private set; }

        public decimal temperature { get; private set; }

        public decimal pressure { get; private set; }

        public bool tidallylocked { get; private set; }

        public bool landable { get; private set; }

        public string atmosphere { get; private set; }

        public string volcanism{ get; private set; }

        public List<MaterialPresence> materials { get; private set; }

        public BodyScannedEvent(DateTime timestamp, string name, string bodyclass, decimal gravity, decimal temperature, decimal pressure, bool tidallylocked, bool landable, string atmosphere, string volcanism, List<MaterialPresence> materials) : base(timestamp, NAME)
        {
            this.name = name;
            this.bodyclass = bodyclass;
            this.gravity = gravity;
            this.temperature = temperature;
            this.pressure = pressure;
            this.tidallylocked = tidallylocked;
            this.landable = landable;
            this.atmosphere = atmosphere;
            this.volcanism = volcanism;
            this.materials = materials;
        }

        private decimal sanitiseCP(decimal cp)
        {
            // Trim decimal places appropriately
            if (cp < .00001M || cp > .9999M)
            {
                return Math.Round(cp * 100, 4);
            }
            else if (cp < .0001M || cp > .999M)
            {
                return Math.Round(cp * 100, 3);
            }
            else if (cp < .001M || cp > .99M)
            {
                return Math.Round(cp * 100, 2);
            }
            else
            {
                return Math.Round(cp * 100);
            }
        }
    }
}
