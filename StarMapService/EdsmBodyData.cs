using EddiDataDefinitions;
using Newtonsoft.Json;
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
                var token = JToken.Parse(clientResponse.Content);
                if (token is JObject response)
                {
                    return ParseStarMapBodies(response);
                }
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
                            if (Body != null)
                            {
                                Bodies.Add(Body);
                            }
                        }
                        catch (Exception ex)
                        {
                            Dictionary<string, object> data = new Dictionary<string, object>();
                            data.Add("body", JsonConvert.SerializeObject(body));
                            data.Add("exception", ex.Message);
                            data.Add("stacktrace", ex.StackTrace);
                            Logging.Error("Error parsing EDSM body result.", data);
                            throw;
                        }
                    }
                }
            }
            return Bodies;
        }

        private static Body ParseStarMapBody(JObject body, string systemName)
        {
            // General items 
            long? bodyId = (long?)body["bodyId"];
            long? EDSMID = (long?)body["id"];
            string bodyname = (string)body["name"];
            BodyType bodyType = BodyType.FromName((string)body["type"]) ?? BodyType.None;
            decimal? distanceLs = (decimal?)body["distanceToArrival"]; // Light Seconds
            decimal? temperatureKelvin = (long?)body["surfaceTemperature"]; // Kelvin

            // Orbital characteristics 
            decimal? orbitalPeriodDays = (decimal?)body["orbitalPeriod"]; // Days
            decimal? semimajoraxisLs = ConstantConverters.au2ls((decimal?)body["semiMajorAxis"]); // Light seconds
            decimal? eccentricity = (decimal?)body["orbitalEccentricity"];
            decimal? orbitalInclinationDegrees = (decimal?)body["orbitalInclination"]; // Degrees
            decimal? periapsisDegrees = (decimal?)body["argOfPeriapsis"]; // Degrees
            decimal? rotationPeriodDays = (decimal?)body["rotationalPeriod"]; // Days
            decimal? axialTiltDegrees = (decimal?)body["axialTilt"]; // Degrees

            List<IDictionary<string, object>> parents = new List<IDictionary<string, object>>();
            if (body["parents"] != null)
            {
                // Parent body types and IDs
                parents = body["parents"].ToObject<List<IDictionary<string, object>>>() ?? new List<IDictionary<string, object>>();
            }

            List<Ring> rings = new List<Ring>();
            if ((JArray)body["rings"] != null || (JArray)body["belts"] != null)
            {
                var ringsData = body["rings"] ?? body["belts"];
                if (ringsData != null)
                {
                    foreach (JObject ring in ringsData)
                    {
                        rings.Add(new Ring(
                            (string)ring["name"],
                            RingComposition.FromName((string)ring["type"]),
                            (decimal)ring["mass"],
                            (decimal)ring["innerRadius"],
                            (decimal)ring["outerRadius"]
                        ));
                    }
                }
            }

            if ((string)body["type"] == "Star")
            {
                // Star-specific items 
                string stellarclass = ((string)body["subType"]).Split(' ')[0]; // Splits "B (Blue-White) Star" to "B" 
                int? stellarsubclass = null;
                string endOfStellarClass = stellarclass.ToCharArray().ElementAt(stellarclass.Length - 1).ToString();
                if (int.TryParse(endOfStellarClass, out int subclass))
                {
                    // If our stellarclass ends in a number, we need to separate the class from the subclass
                    stellarsubclass = subclass;
                    stellarclass = stellarclass.Replace(endOfStellarClass, "");
                }

                long? ageMegaYears = (long?)body["age"]; // Age in megayears
                string luminosityclass = (string)body["luminosity"];
                decimal? absolutemagnitude = (decimal?)body["absoluteMagnitude"];
                decimal? stellarMass = (decimal?)body["solarMasses"];
                decimal? solarradius = (decimal?)body["solarRadius"];
                decimal radiusKm = (decimal)(solarradius != null ? solarradius * Constants.solarRadiusMeters / 1000 : null);

                Body Body = new Body(bodyname, bodyId, parents, distanceLs, stellarclass, stellarsubclass, stellarMass, radiusKm, absolutemagnitude, ageMegaYears, temperatureKelvin, luminosityclass, semimajoraxisLs, eccentricity, orbitalInclinationDegrees, periapsisDegrees, orbitalPeriodDays, rotationPeriodDays, axialTiltDegrees, rings, true, false, systemName, null);
                Body.EDSMID = EDSMID;
                DateTime updatedAt = DateTime.SpecifyKind(DateTime.Parse((string)body["updateTime"]), DateTimeKind.Utc);
                Body.updatedat = updatedAt == null ? null : (long?)(updatedAt.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds;

                return Body;
            }

            if ((string)body["type"] == "Planet")
            {
                // Planet and moon specific items 
                PlanetClass planetClass = PlanetClass.FromName((string)body["subType"]) ?? PlanetClass.None;
                bool? tidallylocked = (bool?)body["rotationalPeriodTidallyLocked"] ?? false;
                bool? landable = (bool?)body["isLandable"];
                decimal? gravity = (decimal?)body["gravity"]; // G's
                decimal? earthmass = (decimal?)body["earthMasses"];
                decimal? radiusKm = (decimal?)body["radius"]; // Kilometers
                TerraformState terraformState = TerraformState.FromName((string)body["terraformingState"]) ?? TerraformState.NotTerraformable;

                Volcanism volcanism = null;
                if ((string)body["volcanismType"] != null)
                {
                    volcanism = Volcanism.FromName((string)body["volcanismType"]);
                }

                List<AtmosphereComposition> atmosphereCompositions = new List<AtmosphereComposition>();
                if (body["atmosphereComposition"] is JObject)
                {
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
                    }
                }
                decimal? pressureAtm = (decimal?)body["surfacePressure"];
                AtmosphereClass atmosphereClass = null;
                if (((string)body["subType"]).Contains("gas giant") &&
                    (string)body["atmosphereType"] == "No atmosphere")
                {
                    // EDSM classifies any body with an empty string atmosphere property as "No atmosphere". 
                    // However, gas giants also receive an empty string. Fix it, since gas giants have atmospheres. 
                    atmosphereClass = AtmosphereClass.FromEDName("GasGiant");
                }
                else
                {
                    atmosphereClass = AtmosphereClass.FromName((string)body["atmosphereType"]);
                }

                List<SolidComposition> solidCompositions = new List<SolidComposition>();
                if (body["solidComposition"] is JObject)
                {
                    var compositions = body["solidComposition"].ToObject<Dictionary<string, decimal?>>();

                    foreach (KeyValuePair<string, decimal?> compositionKV in compositions)
                    {
                        string composition = compositionKV.Key;
                        decimal? share = compositionKV.Value;
                        if (composition != null && share != null)
                        {
                            solidCompositions.Add(new SolidComposition(composition, (decimal)share));
                        }
                    }
                    if (solidCompositions.Count > 0)
                    {
                        solidCompositions = solidCompositions.OrderByDescending(x => x.percent).ToList();
                    }
                }

                List<MaterialPresence> materials = new List<MaterialPresence>();
                if (body["materials"] is JObject)
                {
                    var materialsData = body["materials"].ToObject<Dictionary<string, decimal?>>();
                    foreach (KeyValuePair<string, decimal?> materialKV in materialsData)
                    {
                        Material material = Material.FromName(materialKV.Key);
                        decimal? amount = materialKV.Value;
                        if (material != null && amount != null)
                        {
                            materials.Add(new MaterialPresence(material, (decimal)amount));
                        }
                    }
                    if (materials.Count > 0)
                    {
                        materials = materials.OrderByDescending(o => o.percentage).ToList();
                    }
                }
                ReserveLevel reserveLevel = ReserveLevel.FromName((string)body["reserveLevel"]) ?? ReserveLevel.None;

                DateTime updatedAt = DateTime.SpecifyKind(DateTime.Parse((string)body["updateTime"]), DateTimeKind.Utc);
                Body Body = new Body(bodyname, bodyId, parents, distanceLs, tidallylocked, terraformState, planetClass, atmosphereClass, atmosphereCompositions, volcanism, earthmass, radiusKm, (decimal)gravity, temperatureKelvin, pressureAtm, landable, materials, solidCompositions, semimajoraxisLs, eccentricity, orbitalInclinationDegrees, periapsisDegrees, orbitalPeriodDays, rotationPeriodDays, axialTiltDegrees, rings, reserveLevel, true, true, systemName, null)
                {
                    EDSMID = EDSMID,
                    updatedat = updatedAt == null ? null : (long?)(updatedAt.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds
                };

                return Body;
            }


            return null;
        }
    }
}
