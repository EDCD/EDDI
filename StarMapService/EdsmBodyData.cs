using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiStarMapService
{
    public partial class StarMapService
    {
        public static List<Body> GetStarMapBodies(string system, long? edsmId = null)
        {
            if (system == null) { return null; }
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-system-v1/bodies", Method.POST);
            request.AddParameter("systemName", system);
            request.AddParameter("systemId", edsmId);
            var clientResponse = client.Execute<Dictionary<string, object>>(request);
            if (clientResponse.IsSuccessful)
            {
                JObject response = JObject.Parse(clientResponse.Content);
                return ParseStarMapBodies(response);
            }
            else
            {
                Logging.Debug("EDSM responded with " + clientResponse.ErrorMessage, clientResponse.ErrorException);
            }
            return null;
        }

        private static List<Body> ParseStarMapBodies(JObject response)
        {
            List<Body> Bodies = new List<Body>();
            if (response != null)
            {
                string system = (string)response["name"];
                JArray bodies = (JArray)response["bodies"];

                if (bodies != null)
                {
                    foreach (JObject body in bodies)
                    {
                        try
                        {
                            Body Body = ParseStarMapBody(body, system);
                            Bodies.Add(Body);
                        }
                        catch (Exception ex)
                        {
                            Logging.Error("Error parsing EDSM body result " + body.ToString(), ex);
                            throw;
                        }
                    }
                }
            }
            return Bodies;
        }

        private static Body ParseStarMapBody(JObject body, string system)
        {
            Body Body = new Body
            {
                // General items 
                EDSMID = (long?)body["id"],
                name = (string)body["name"],
                systemname = system,
                Type = BodyType.FromName((string)body["type"]) ?? BodyType.None,
                distance = (decimal?)body["distanceToArrival"], // Light Seconds
                temperature = (long?)body["surfaceTemperature"], // Kelvin

                // Orbital characteristics 
                orbitalperiod = (decimal?)body["orbitalPeriod"], // Days
                semimajoraxis = ConstantConverters.au2ls((decimal?)body["semiMajorAxis"]), // Light seconds
                eccentricity = (decimal?)body["orbitalEccentricity"],
                inclination = (decimal?)body["orbitalInclination"], // Degrees
                periapsis = (decimal?)body["argOfPeriapsis"], // Degrees
                rotationalperiod = (decimal?)body["rotationalPeriod"], // Days
                tidallylocked = (bool?)body["rotationalPeriodTidallyLocked"] ?? false,
                tilt = (decimal?)body["axialTilt"] // Degrees
            };

            if ((string)body["type"] == "Belt")
            {
                // Not interested in asteroid belts, 
                // no need to add additional information at this time.
            }

            if ((string)body["type"] == "Star")
            {
                // Star-specific items 
                Body.stellarclass = ((string)body["subType"]).Split(' ')[0]; // Splits "B (Blue-White) Star" to "B" 
                Body.mainstar = (bool?)body["isMainStar"];
                Body.age = (long?)body["age"]; // Age in megayears
                Body.luminosityclass = (string)body["luminosity"];
                Body.absoluteMagnitude = (decimal?)body["absoluteMagnitude"];
                Body.solarmass = (decimal?)body["solarMasses"];
                Body.solarradius = (decimal?)body["solarRadius"];
                Body.landable = false;
                Body.setStellarExtras();
            }

            if ((string)body["type"] == "Planet")
            {
                // Planet-specific items 
                Body.planetClass = PlanetClass.FromName((string)body["subType"]) ?? PlanetClass.None;
                Body.landable = (bool?)body["isLandable"];
                Body.gravity = (decimal?)body["gravity"]; // G's
                Body.earthmass = (decimal?)body["earthMasses"];
                Body.radius = (decimal?)body["radius"]; // Kilometers
                Body.terraformState = TerraformState.FromName((string)body["terraformingState"]) ?? TerraformState.NotTerraformable;
                if ((string)body["volcanismType"] != null)
                {
                    Body.volcanism = Volcanism.FromName((string)body["volcanismType"]);
                }

                if (body["atmosphereComposition"] is JObject)
                {
                    List<AtmosphereComposition> atmosphereCompositions = new List<AtmosphereComposition>();
                    var compositions = body["atmosphereComposition"].ToObject<Dictionary<string, decimal?>>();

                    foreach (KeyValuePair<string, decimal?> compositionKV in compositions)
                    {
                        string compositionName = compositionKV.Key;
                        decimal? share = compositionKV.Value;
                        if (compositionName != null && share != null)
                        {
                            atmosphereCompositions.Add(new AtmosphereComposition(compositionName, (decimal)share));
                        }
                    }
                    if (atmosphereCompositions.Count > 0)
                    {
                        atmosphereCompositions = atmosphereCompositions.OrderByDescending(x => x.percent).ToList();
                        Body.atmospherecompositions = atmosphereCompositions;
                    }
                }
                Body.pressure = (decimal?)body["surfacePressure"];
                if (((string)body["subType"]).Contains("gas giant") && 
                    (string)body["atmosphereType"] == "No atmosphere")
                {
                    // EDSM classifies any body with an empty string atmosphere property as "No atmosphere". 
                    // However, gas giants also receive an empty string. Fix it, since gas giants have atmospheres. 
                    Body.atmosphereclass = AtmosphereClass.FromEDName("GasGiant");
                }
                else
                {
                    Body.atmosphereclass = AtmosphereClass.FromName((string)body["atmosphereType"]);
                }

                if (body["solidComposition"] is JObject)
                {
                    List<SolidComposition> bodyCompositions = new List<SolidComposition>();
                    var compositions = body["solidComposition"].ToObject<Dictionary<string, decimal?>>();

                    foreach (KeyValuePair<string, decimal?> compositionKV in compositions)
                    {
                        string composition = compositionKV.Key;
                        decimal? share = compositionKV.Value;
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

                if (body["materials"] is JObject)
                {
                    List<MaterialPresence> Materials = new List<MaterialPresence>();
                    var materials = body["materials"].ToObject<Dictionary<string, decimal?>>();
                    foreach (KeyValuePair<string, decimal?> materialKV in materials)
                    {
                        Material material = Material.FromName(materialKV.Key);
                        decimal? amount = materialKV.Value;
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

            if ((JArray)body["rings"] != null || (JArray)body["belts"] != null)
            {
                var rings = body["rings"] ?? body["belts"];
                if (rings != null)
                {
                    List<Ring> Rings = new List<Ring>();
                    foreach (JObject ring in rings)
                    {
                        Rings.Add(new Ring(
                            (string)ring["name"],
                            RingComposition.FromName((string)ring["type"]),
                            (decimal)ring["mass"],
                            (decimal)ring["innerRadius"],
                            (decimal)ring["outerRadius"]
                        ));
                    }
                    Body.rings = Rings;
                }
            }
            Body.reserveLevel = ReserveLevel.FromName((string)body["reserveLevel"]) ?? ReserveLevel.None;

            DateTime updatedAt = DateTime.SpecifyKind(DateTime.Parse((string)body["updateTime"]), DateTimeKind.Utc);
            Body.updatedat = updatedAt == null ? null : (long?)(updatedAt.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds;

            return Body;
        }
    }
}
