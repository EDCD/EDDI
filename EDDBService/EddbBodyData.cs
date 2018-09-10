using EddiDataDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiEddbService
{
    partial class EddbService
    {
        /// <summary> At least one body name is required. </summary>
        public static List<Body> Bodies(string[] bodyNames)
        {
            List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>();
            foreach (string body in bodyNames)
            {
                queryList.Add(new KeyValuePair<string, object>(BodyQuery.bodyName, body));
            }
            return GetBodies(queryList);
        }

        /// <summary> At least one body EDDBID is required. </summary>
        public static List<Body> Bodies(long[] eddbBodyIds)
        {
            List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>();
            foreach (long eddbId in eddbBodyIds)
            {
                queryList.Add(new KeyValuePair<string, object>(BodyQuery.eddbId, eddbId));
            }
            return GetBodies(queryList);
        }

        /// <summary> Exactly one system name is required. </summary>
        public static List<Body> Bodies(string systemName)
        {
            return GetBodies(new KeyValuePair<string, object>(BodyQuery.systemName, systemName));
        }

        /// <summary> Exactly one body name is required. </summary>
        public static Body Body(string bodyName)
        {
            return GetBody(new KeyValuePair<string, object>(BodyQuery.bodyName, bodyName));
        }

        /// <summary> Exactly one body EDDBID is required. </summary>
        public static Body Body(long eddbId)
        {
            return GetBody(new KeyValuePair<string, object>(BodyQuery.eddbId, eddbId));
        }

        private static Body GetBody(KeyValuePair<string, object> query)
        {
            if (query.Value != null)
            {
                List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>();
                queryList.Add(query);

                List<object> responses = GetData(Endpoint.bodies, queryList);

                if (responses != null)
                {
                    return ParseEddbBody(responses[0]);
                }
            }
            return null;
        }

        private static List<Body> GetBodies(KeyValuePair<string, object> query)
        {
            if (query.Value != null)
            {
                List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>();
                queryList.Add(query);

                List<object> responses = GetData(Endpoint.bodies, queryList);

                if (responses != null)
                {
                    List<Body> bodies = new List<Body>();
                    foreach (object response in responses)
                    {
                        bodies.Add(ParseEddbBody(response));
                    }
                    return bodies;
                }
            }
            return null;
        }

        private static List<Body> GetBodies(List<KeyValuePair<string, object>> queryList)
        {
            if (queryList != null)
            {
                List<object> responses = GetData(Endpoint.bodies, queryList);

                if (responses != null)
                {
                    List<Body> bodies = new List<Body>();
                    foreach (object response in responses)
                    {
                        bodies.Add(ParseEddbBody(response));
                    }
                    return bodies;
                }
            }
            return null;
        }

        private static Body ParseEddbBody(object response)
        {
            JObject bodyJson = ((JObject)response);

            if ((string)bodyJson["group_name"] == "belt")
            {
                // Not interested in asteroid belts
                return null;
            }

            Body Body = new Body();

            // General items
            Body.EDDBID = (long)bodyJson["id"];
            Body.updatedat = (long)(Dates.fromDateTimeStringToSeconds((string)bodyJson["updated_at"]));
            Body.name = (string)bodyJson["name"];
            Body.Type = BodyType.FromEDName((string)bodyJson["group_name"]);

            // System name is not given directly, but we have an EDDB ID linking to the system 
            // that we will be able to use for lookups once our system lookup is complete.
            // We will also need to use this lookup to find the reserve level associated with this system
            Body.systemEDDBID = (long)bodyJson["system_id"];
            /*
            Body.systemname = systemName;
            Body.reserves = reserves
            */

            // Orbital data
            Body.distance = (long?)bodyJson["distance_to_arrival"];
            Body.temperature = (decimal?)bodyJson["surface_temperature"];
            Body.tidallylocked = (bool?)bodyJson["is_rotational_period_tidally_locked"];
            Body.rotationalperiod = (decimal?)(double?)bodyJson["rotational_period"];
            Body.tilt = (decimal?)(double?)bodyJson["axis_tilt"];
            Body.semimajoraxis = ConstantConverters.au2km((decimal?)(double?)bodyJson["semi_major_axis"]);
            Body.orbitalperiod = (decimal?)(double?)bodyJson["orbital_period"];
            Body.periapsis = (decimal?)(double?)bodyJson["arg_of_periapsis"];
            Body.eccentricity = (decimal?)(double?)bodyJson["orbital_eccentricity"];
            Body.inclination = (decimal?)(double?)bodyJson["orbital_inclination"];

            if (Body.type == "Star")
            {
                // Star-specific items
                Body.stellarclass = ((string)bodyJson["spectral_class"]).ToUpperInvariant();
                Body.luminosityclass = ((string)bodyJson["luminosity_class"]).ToUpperInvariant();
                Body.solarmass = (decimal?)(double?)bodyJson["solar_masses"];
                Body.solarradius = (decimal?)(double?)bodyJson["solar_radius"];
                Body.age = (long?)bodyJson["age"];
                Body.mainstar = (bool?)bodyJson["is_main_star"];
                Body.landable = false;
                Body.setStellarExtras();
            }

            if (Body.type == "Planet")
            {
                // Planet-specific items
                Body.planetClass = PlanetClass.FromEDName((string)bodyJson["type_name"]);
                Body.landable = (bool?)bodyJson["is_landable"];
                Body.earthmass = (decimal?)(double?)bodyJson["earth_masses"];
                Body.gravity = (decimal?)(double?)bodyJson["gravity"];
                Body.radius = (decimal?)bodyJson["radius"];
                Body.pressure = (decimal?)(double?)bodyJson["surface_pressure"] ?? 0;
                Body.terraformState = TerraformState.FromName((string)bodyJson["terraforming_state_name"]);
                Body.volcanism = Volcanism.FromName((string)bodyJson["volcanism_type_name"]);
                Body.atmosphereclass = AtmosphereClass.FromEDName((string)bodyJson["atmosphere_type_name"]);
                if (bodyJson["atmosphere_composition"] != null)
                {
                    List<AtmosphereComposition> atmosphereCompositions = new List<AtmosphereComposition>();
                    foreach (JObject atmoJson in bodyJson["atmosphere_composition"])
                    {
                        string composition = (string)atmoJson["atmosphere_component_name"];
                        decimal? share = (decimal?)atmoJson["share"];
                        if (composition != null && share != null)
                        {
                            atmosphereCompositions.Add(new AtmosphereComposition(composition, (decimal)share));
                        }
                    }
                    if (atmosphereCompositions.Count > 0)
                    {
                        atmosphereCompositions = atmosphereCompositions.OrderByDescending(x => x.percent).ToList();
                        Body.atmosphereCompositions = atmosphereCompositions;
                    }
                }
                if (bodyJson["solid_composition"] != null)
                {
                    List<BodySolidComposition> bodyCompositions = new List<BodySolidComposition>();
                    foreach (JObject bodyCompJson in bodyJson["solid_composition"])
                    {
                        string composition = (string)bodyCompJson["solid_component_name"];
                        decimal? share = (decimal?)bodyCompJson["share"];
                        if (composition != null && share != null)
                        {
                            bodyCompositions.Add(new BodySolidComposition(composition, (decimal)share));
                        }
                    }
                    if (bodyCompositions.Count > 0)
                    {
                        bodyCompositions = bodyCompositions.OrderByDescending(x => x.percent).ToList();
                        Body.solidComposition = bodyCompositions;
                    }
                }
                if (bodyJson["materials"] != null)
                {
                    List<MaterialPresence> Materials = new List<MaterialPresence>();
                    foreach (JObject materialJson in bodyJson["materials"])
                    {
                        Material material = Material.FromEDName((string)materialJson["material_name"]);
                        decimal? amount = (decimal?)(double?)materialJson["share"];
                        if (material != null && amount != null)
                        {
                            Materials.Add(new MaterialPresence(material, (decimal)amount));
                        }
                    }
                    if (Materials.Count > 0)
                    {
                        Body.materials = Materials.OrderByDescending(o => o.percentage).ToList();
                    }
                }

            }

            // Rings may be an object or may be a singular
            List<Ring> rings = new List<Ring>();
            if (bodyJson["rings"] != null)
            {
                foreach (JObject ringJson in bodyJson["rings"])
                {
                    string name = (string)ringJson["name"];
                    string composition = (string)ringJson["ring_type_name"];
                    decimal mass = (decimal)ringJson["ring_mass"];
                    decimal innerRadius = (decimal)ringJson["ring_inner_radius"];
                    decimal outerRadius = (decimal)ringJson["ring_outer_radius"];
                    Ring ring = new Ring(name, composition, mass, innerRadius, outerRadius);
                    rings.Add(ring);
                }
            }
            else if (bodyJson["ring_type_name"] != null)
            {
                string name = (string)bodyJson["name"];
                string composition = (string)bodyJson["ring_type_name"];
                decimal mass = (decimal)bodyJson["ring_mass"];
                decimal innerRadius = (decimal)bodyJson["ring_inner_radius"];
                decimal outerRadius = (decimal)bodyJson["ring_outer_radius"];
                Ring ring = new Ring(name, composition, mass, innerRadius, outerRadius);
                rings.Add(ring);
            }
            if (rings.Count > 0)
            {
                Body.rings = rings.OrderBy(o => o.innerradius).ToList();
            }

            return Body;
        }
    }
}
