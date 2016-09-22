using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousEvents
{
    public class BodyScannedEvent : Event
    {
        public const string NAME = "Body scanned";
        public const string DESCRIPTION = "Triggered when you complete a scan of a planetary body";
        public static BodyScannedEvent SAMPLE = new BodyScannedEvent(DateTime.Now, "We Jenu 1", "High metal content body", 1.048553M, 240.578812M, 0, false, true, new List<MaterialPresence> { new MaterialPresence(MaterialDefinition.Iron, 22.1M), new MaterialPresence(MaterialDefinition.Nickel, 16.7M), new MaterialPresence(MaterialDefinition.Sulphur, 15.6M), new MaterialPresence(MaterialDefinition.Carbon, 13.1M), new MaterialPresence(MaterialDefinition.Chromium, 9.9M), new MaterialPresence(MaterialDefinition.Phosphorus, 8.4M), new MaterialPresence(MaterialDefinition.Zinc, 6.0M), new MaterialPresence(MaterialDefinition.Germanium, 4.6M), new MaterialPresence(MaterialDefinition.Molybdenum, 1.4M), new MaterialPresence(MaterialDefinition.Tin, 1.4M), new MaterialPresence(MaterialDefinition.Technetium, 0.8M) });
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BodyScannedEvent()
        {
            SAMPLE.raw = "{ \"timestamp\":\"2016-09-21T18:36:23Z\", \"event\":\"Scan\", \"BodyName\":\"We Jenu 1\", \"DistanceFromArrivalLS\":319.790344, \"TidalLock\":false, \"TerraformState\":\"\", \"PlanetClass\":\"High metal content body\", \"Atmosphere\":\"\", \"Volcanism\":\"\", \"MassEM\":0.001508, \"Radius\":757189.000000, \"SurfaceGravity\":1.048553, \"SurfaceTemperature\":240.578812, \"SurfacePressure\":0.000000, \"Landable\":true, \"Materials\":{ \"iron\":22.1, \"nickel\":16.7, \"sulphur\":15.6, \"carbon\":13.1, \"chromium\":9.9, \"phosphorus\":8.4, \"zinc\":6.0, \"germanium\":4.6, \"molybdenum\":1.4, \"tin\":1.4, \"technetium\":0.8 }, \"OrbitalPeriod\":18741434.000000, \"RotationPeriod\":179945.156250 }";

            VARIABLES.Add("name", "The name of the body that has been scanned");
            VARIABLES.Add("bodyclass", "The class of the body that has been scanned (High metal content body etc)");
            VARIABLES.Add("gravity", "The surface gravity of the body that has been scanned, relative to Earth's gravity");
            VARIABLES.Add("temperature", "The surface temperature of the body that has been scanned");
            VARIABLES.Add("pressure", "The surface pressure of the body that has been scanned");
            VARIABLES.Add("tidallylocked", "True if the body is tidally locked");
            VARIABLES.Add("landable", "True if the body is landable");
            VARIABLES.Add("materials", "A list of materials present on the body that has been scanned");
        }

        public string name { get; private set; }

        public string bodyclass { get; private set; }

        public decimal gravity{ get; private set; }

        public decimal temperature { get; private set; }

        public decimal pressure { get; private set; }

        public bool tidallylocked { get; private set; }

        public bool landable { get; private set; }

        public List<MaterialPresence> materials { get; private set; }

        public BodyScannedEvent(DateTime timestamp, string name, string bodyclass, decimal gravity, decimal temperature, decimal pressure, bool tidallylocked, bool landable, List<MaterialPresence> materials) : base(timestamp, NAME)
        {
            this.name = name;
            this.bodyclass = bodyclass;
            this.gravity = gravity;
            this.temperature = temperature;
            this.pressure = pressure;
            this.tidallylocked = tidallylocked;
            this.landable = landable;
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
