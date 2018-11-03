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
            List<Body> bodies = new List<Body>();
            foreach (string bodyName in bodyNames)
            {
                Body body = GetBody(new KeyValuePair<string, object>(BodyQuery.bodyName, bodyName)) ?? new Body();
                if (body.EDDBID == null)
                {
                    body.name = bodyName;
                }
                bodies.Add(body);
            }
            return bodies?.OrderBy(x => x.name).ToList();
        }

        /// <summary> At least one body EDDBID is required. </summary>
        public static List<Body> Bodies(long[] eddbBodyIds)
        {
            List<Body> bodies = new List<Body>();
            foreach (long eddbId in eddbBodyIds)
            {
                Body body = GetBody(new KeyValuePair<string, object>(BodyQuery.eddbId, eddbId)) ?? new Body();
                if (body.EDDBID == null)
                {
                    body.EDDBID = eddbId;
                }
                bodies.Add(body);
            }
            return bodies?.OrderBy(x => x.name).ToList();
        }

        /// <summary> Exactly one system name is required. </summary>
        public static List<Body> Bodies(string systemName)
        {
            List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>(BodyQuery.systemName, systemName)
            };
            return GetBodies(queryList) ?? new List<Body>();
        }

        /// <summary> Exactly one body name is required. </summary>
        public static Body Body(string bodyName)
        {
            Body body = GetBody(new KeyValuePair<string, object>(BodyQuery.bodyName, bodyName)) ?? new Body();
            if (body.EDDBID == null)
            {
                body.name = bodyName;
            }
            return body;
        }

        /// <summary> Exactly one body name and system name are required. </summary>
        public static Body Body(string bodyName, string systemName)
        {
            List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(BodyQuery.bodyName, bodyName),
                new KeyValuePair<string, object>(BodyQuery.systemName, systemName)
            };
            Body body = GetBody(queryList) ?? new Body();
            if (body.EDDBID == null)
            {
                body.name = bodyName;
                body.systemname = systemName;
            }
            return body;
        }

        /// <summary> Exactly one body EDDBID is required. </summary>
        public static Body Body(long eddbId)
        {
            Body body = GetBody(new KeyValuePair<string, object>(BodyQuery.eddbId, eddbId)) ?? new Body();
            if (body.EDDBID == null)
            {
                body.EDDBID = eddbId;
            }
            return body;
        }

        private static Body GetBody(KeyValuePair<string, object> query)
        {
            if (query.Value != null)
            {
                List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>
                {
                    query
                };

                List<object> responses = GetData(Endpoint.bodies, queryList);

                if (responses?.Count > 0)
                {
                    return ParseEddbBody(responses[0]);
                }
            }
            return null;
        }

        private static Body GetBody(List<KeyValuePair<string, object>> queryList)
        {
            if (queryList.Count > 0)
            {
                List<object> responses = GetData(Endpoint.bodies, queryList);

                if (responses?.Count > 0)
                {
                    return ParseEddbBody(responses[0]);
                }
            }
            return null;
        }

        private static List<Body> GetBodies(List<KeyValuePair<string, object>> queryList)
        {
            if (queryList.Count > 0)
            {
                List<object> responses = GetData(Endpoint.bodies, queryList);

                if (responses?.Count > 0)
                {
                    return ParseEddbBodiesAsync(responses);
                }
            }
            return null;
        }

        private static List<Body> ParseEddbBodiesAsync(List<object> responses)
        {
            List<Task<Body>> bodyTasks = new List<Task<Body>>();
            foreach (object response in responses)
            {
                bodyTasks.Add(Task.Run(() => ParseEddbBody(response)));
            }
            Task.WhenAll(bodyTasks.ToArray());

            List<Body> bodies = new List<Body>();
            foreach (Task<Body> task in bodyTasks)
            {
                Body body = task.Result;
                if (body != null) { bodies.Add(body); };
            }

            return bodies;
        }

        private static Body ParseEddbBody(object response)
        {
            JObject bodyJson = ((JObject)response);

            Body Body = new Body
            {
                // General items
                EDDBID = (long)bodyJson["id"],
                updatedat = (long)(Dates.fromDateTimeStringToSeconds((string)bodyJson["updated_at"])),
                name = (string)bodyJson["name"],
                Type = BodyType.FromEDName((string)bodyJson["group_name"]),
                systemEDDBID = (long)bodyJson["system_id"],

                // Orbital data
                distance = (decimal?)bodyJson["distance_to_arrival"], // Light seconds
                temperature = (decimal?)bodyJson["surface_temperature"], //Kelvin
                tidallylocked = (bool?)bodyJson["is_rotational_period_tidally_locked"] ?? false, // Days
                rotationalperiod = (decimal?)(double?)bodyJson["rotational_period"], // Days
                tilt = (decimal?)(double?)bodyJson["axis_tilt"], // Degrees
                semimajoraxis = ConstantConverters.au2ls((decimal?)(double?)bodyJson["semi_major_axis"]), // Light Seconds
                orbitalperiod = (decimal?)(double?)bodyJson["orbital_period"], // Days
                periapsis = (decimal?)(double?)bodyJson["arg_of_periapsis"], // Degrees
                eccentricity = (decimal?)(double?)bodyJson["orbital_eccentricity"],
                inclination = (decimal?)(double?)bodyJson["orbital_inclination"] // Degrees
            };

            if ((string)bodyJson["group_name"] == "belt")
            {
                // Not interested in asteroid belts, 
                // no need to add additional information at this time.
            }

            else if ((string)bodyJson["group_name"] == "star")
            {
                // Star-specific items
                Body.stellarclass = ((string)bodyJson["spectral_class"])?.ToUpperInvariant();
                Body.luminosityclass = ((string)bodyJson["luminosity_class"])?.ToUpperInvariant();
                Body.solarmass = (decimal?)(double?)bodyJson["solar_masses"];
                Body.solarradius = (decimal?)(double?)bodyJson["solar_radius"];
                Body.age = (long?)bodyJson["age"]; // MegaYears
                Body.mainstar = (bool?)bodyJson["is_main_star"];
                Body.landable = false;
                Body.setStellarExtras();
            }

            else if ((string)bodyJson["group_name"] == "planet")
            {
                // Planet-specific items
                Body.planetClass = PlanetClass.FromEDName((string)bodyJson["type_name"]);
                Body.landable = (bool?)bodyJson["is_landable"] ?? false;
                Body.earthmass = (decimal?)(double?)bodyJson["earth_masses"];
                Body.gravity = (decimal?)(double?)bodyJson["gravity"]; // G's
                Body.radius = (decimal?)bodyJson["radius"]; // Kilometers
                Body.pressure = (decimal?)(double?)bodyJson["surface_pressure"] ?? 0;
                Body.terraformState = TerraformState.FromName((string)bodyJson["terraforming_state_name"]);
                // Per Themroc @ EDDB, "Major" and "Minor" volcanism descriptors are stripped from EDDB data. 
                Body.volcanism = Volcanism.FromName((string)bodyJson["volcanism_type_name"]);
                Body.atmosphereclass = AtmosphereClass.FromName((string)bodyJson["atmosphere_type_name"]);
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
                        Body.atmospherecompositions = atmosphereCompositions;
                    }
                }
                if (bodyJson["solid_composition"] != null)
                {
                    List<SolidComposition> bodyCompositions = new List<SolidComposition>();
                    foreach (JObject bodyCompJson in bodyJson["solid_composition"])
                    {
                        string composition = (string)bodyCompJson["solid_component_name"];
                        decimal? share = (decimal?)bodyCompJson["share"];
                        if (composition != null && share != null)
                        {
                            bodyCompositions.Add(new SolidComposition(composition, (decimal)share));
                        }
                    }
                    if (bodyCompositions.Count > 0)
                    {
                        bodyCompositions = bodyCompositions.OrderByDescending(x => x.percent).ToList();
                        Body.solidcompositions = bodyCompositions;
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
                    RingComposition composition = RingComposition.FromName((string)ringJson["ring_type_name"]);
                    decimal ringMassMegaTons = (decimal)ringJson["ring_mass"];
                    decimal innerRadiusKm = (decimal)ringJson["ring_inner_radius"];
                    decimal outerRadiusKm = (decimal)ringJson["ring_outer_radius"];
                    Ring ring = new Ring(name, composition, ringMassMegaTons, innerRadiusKm, outerRadiusKm);
                    rings.Add(ring);
                }
            }
            if (bodyJson["ring_type_name"].HasValues)
            {
                string name = (string)bodyJson["name"];
                RingComposition composition = RingComposition.FromName((string)bodyJson["ring_type_name"]);
                decimal ringMassMegaTons = (decimal)bodyJson["ring_mass"];
                decimal innerRadiusKm = (decimal)bodyJson["ring_inner_radius"];
                decimal outerRadiusKm = (decimal)bodyJson["ring_outer_radius"];
                Ring ring = new Ring(name, composition, ringMassMegaTons, innerRadiusKm, outerRadiusKm);
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
