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
        public const string SAMPLE = "{ \"timestamp\":\"2016-10-05T10:28:04Z\", \"event\":\"Scan\", \"BodyName\":\"Dagutii ABC 1 b\", \"DistanceFromArrivalLS\":644.074463, \"TidalLock\":true, \"TerraformState\":\"\", \"PlanetClass\":\"Icy body\", \"Atmosphere\":\"\", \"Volcanism\":\"carbon dioxide geysers volcanism\", \"MassEM\":0.001305, \"Radius\":964000.375000, \"SurfaceGravity\":0.559799, \"SurfaceTemperature\":89.839241, \"SurfacePressure\":0.000000, \"Landable\":true, \"Materials\":{ \"sulphur\":26.8, \"carbon\":22.5, \"phosphorus\":14.4, \"iron\":12.1, \"nickel\":9.2, \"chromium\":5.4, \"selenium\":4.2, \"vanadium\":3.0, \"niobium\":0.8, \"molybdenum\":0.8, \"ruthenium\":0.7 }, \"SemiMajorAxis\":739982912.000000, \"Eccentricity\":0.000102, \"OrbitalInclination\":-0.614765, \"Periapsis\":233.420425, \"OrbitalPeriod\":242733.156250, \"RotationPeriod\":242735.265625 }";

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
            VARIABLES.Add("distancefromarrival", "The distance in LS from the main star");
            VARIABLES.Add("orbitalperiod", "The number of seconds taken for a full orbit of the main star");
            VARIABLES.Add("rotationperiod", "The number of seconds taken for a full rotation");
            VARIABLES.Add("semimajoraxis", "The semi major axis of the body");
            VARIABLES.Add("eccentricity", "The orbital eccentricity of the body");
            VARIABLES.Add("orbitalinclination", "The orbital inclination of the body");
            VARIABLES.Add("periapsis", "The periapsis of the body");
            VARIABLES.Add("rings", "A list of the body's rings");
            VARIABLES.Add("materials", "A list of materials present on the body that has been scanned");
            VARIABLES.Add("terraformstate", "Whether the body can be, is in the process of, or has been terraformed");
        }

        public string name { get; private set; }

        public string bodyclass { get; private set; }

        public decimal gravity{ get; private set; }

        public decimal? temperature { get; private set; }

        public decimal? pressure { get; private set; }

        public bool? tidallylocked { get; private set; }

        public bool? landable { get; private set; }

        public string atmosphere { get; private set; }

        public string volcanism{ get; private set; }

        public decimal distancefromarrival { get; private set; }

        public decimal orbitalperiod { get; private set; }

        public decimal rotationperiod { get; private set; }

        public decimal? semimajoraxis { get; private set; }

        public decimal? eccentricity { get; private set; }

        public decimal? orbitalinclination { get; private set; }

        public decimal? periapsis { get; private set; }

        public List<Ring> rings { get; private set; }

        public List<MaterialPresence> materials { get; private set; }

        public string terraformstate { get; private set; }

        public BodyScannedEvent(DateTime timestamp, string name, string bodyclass, decimal gravity, decimal? temperature, decimal? pressure, bool? tidallylocked, bool? landable, string atmosphere, string volcanism, decimal distancefromarrival, decimal orbitalperiod, decimal rotationperiod, decimal? semimajoraxis, decimal? eccentricity, decimal? orbitalinclination, decimal? periapsis, List<Ring> rings, List<MaterialPresence> materials, string terraformState) : base(timestamp, NAME)
        {
            this.name = name;
            this.distancefromarrival = distancefromarrival;
            this.bodyclass = bodyclass;
            this.gravity = gravity;
            this.temperature = temperature;
            this.pressure = pressure;
            this.tidallylocked = tidallylocked;
            this.landable = landable;
            this.atmosphere = atmosphere;
            this.volcanism = volcanism;
            this.orbitalperiod = orbitalperiod;
            this.rotationperiod = rotationperiod;
            this.semimajoraxis = semimajoraxis;
            this.eccentricity = eccentricity;
            this.orbitalinclination = orbitalinclination;
            this.periapsis = periapsis;
            this.rings = rings;
            this.materials = materials;
            this.terraformstate = terraformState;
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
