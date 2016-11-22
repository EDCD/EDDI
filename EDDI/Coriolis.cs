using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using Utilities;

namespace Eddi
{
    public class Coriolis
    {
        public static string ShipUri(Ship ship)
        {
            // Generate a Coriolis import URI to retain as much information as possible
            string uri = "https://coriolis.edcd.io/import?";

            // Take the ship's JSON, gzip it, then turn it in to base64 and attach it to the base uri

            var bytes = Encoding.UTF8.GetBytes(ship.json);
            using (var streamIn = new MemoryStream(bytes))
            using (var streamOut = new MemoryStream())
            {
<<<<<<< HEAD
                using (var compressionStream = new GZipStream(stream, CompressionLevel.Optimal, true))
                using (BinaryWriter writer = new BinaryWriter(compressionStream))
                {
                    string model;
                    if (!shipModels.TryGetValue(ship.model, out model))
                    {
                        // This can happen if the model supplied is invalid, for example if the user is in an SRV at the time
                        return null;
                    }

                    uri += model;
                    uri += "/";

                    byte position = 0;
                    uri += ShipBulkheads(ship.bulkheads.name);
                    modsSize += addModifications(ship.bulkheads, position++, writer);
                    enableds += "1";
                    priorities += "4";
                    uri += CoriolisIDDefinitions.FromEDDBID(ship.powerplant.EDDBID);
                    modsSize += addModifications(ship.powerplant, position++, writer);
                    enableds += "1";
                    priorities += "0";
                    uri += CoriolisIDDefinitions.FromEDDBID(ship.thrusters.EDDBID);
                    modsSize += addModifications(ship.thrusters, position++, writer);
                    enableds += ship.thrusters.enabled ? "1" : "0";
                    priorities += ship.thrusters.priority;
                    uri += CoriolisIDDefinitions.FromEDDBID(ship.frameshiftdrive.EDDBID);
                    modsSize += addModifications(ship.frameshiftdrive, position++, writer);
                    enableds += ship.frameshiftdrive.enabled ? "1" : "0";
                    priorities += ship.frameshiftdrive.priority;
                    uri += CoriolisIDDefinitions.FromEDDBID(ship.lifesupport.EDDBID);
                    modsSize += addModifications(ship.lifesupport, position++, writer);
                    enableds += ship.lifesupport.enabled ? "1" : "0";
                    priorities += ship.lifesupport.priority;
                    uri += CoriolisIDDefinitions.FromEDDBID(ship.powerdistributor.EDDBID);
                    modsSize += addModifications(ship.powerdistributor, position++, writer);
                    enableds += ship.powerdistributor.enabled ? "1" : "0";
                    priorities += ship.powerdistributor.priority;
                    uri += CoriolisIDDefinitions.FromEDDBID(ship.sensors.EDDBID);
                    modsSize += addModifications(ship.sensors, position++, writer);
                    enableds += ship.sensors.enabled ? "1" : "0";
                    priorities += ship.sensors.priority;
                    uri += CoriolisIDDefinitions.FromEDDBID(ship.fueltank.EDDBID);
                    modsSize += addModifications(ship.fueltank, position++, writer);
                    enableds += "1";
                    priorities += "1";
                    foreach (Hardpoint Hardpoint in ship.hardpoints)
                    {
                        if (Hardpoint.module == null)
                        {
                            uri += "-";
                            enableds += "1";
                            priorities += "0";
                        }
                        else
                        {
                            string id = CoriolisIDDefinitions.FromEDDBID(Hardpoint.module.EDDBID);
                            if (id == null)
                            {
                                uri += "-";
                                enableds += "1";
                                priorities += "0";
                            }
                            else
                            {
                                uri += id;
                                enableds += Hardpoint.module.enabled ? "1" : "0";
                                priorities += Hardpoint.module.priority;
                                modsSize += addModifications(Hardpoint.module, position, writer);
                            }
                        }
                        position++;
                    }
                    foreach (Compartment Compartment in ship.compartments)
                    {
                        if (Compartment.module == null)
                        {
                            uri += "-";
                            enableds += "1";
                            priorities += "0";
                        }
                        else
                        {
                            string id = CoriolisIDDefinitions.FromEDDBID(Compartment.module.EDDBID);
                            if (id == null)
                            {
                                uri += "-";
                                enableds += "1";
                                priorities += "0";
                            }
                            else
                            {
                                uri += id;
                                enableds += Compartment.module.enabled ? "1" : "0";
                                priorities += Compartment.module.priority;
                                modsSize += addModifications(Compartment.module, position, writer);
                            }
                        }
                        position++;
                    }

                    writer.Write((byte)0xff);
                    modsSize++;

                    uri += ".";
                    uri += LZString.compressToBase64(enableds).Replace('/', '-');
                    uri += ".";
                    uri += LZString.compressToBase64(priorities).Replace('/', '-');
                    uri += ".";

                }
                uri += Convert.ToBase64String(stream.ToArray()).Replace('/', '-');
                Logging.Debug("Sizes are " + modsSize + "/" + stream.Length + "/" + Convert.ToBase64String(stream.ToArray()).Length);

                string bn;
                if (ship.name == null)
                {
                    bn = ship.role + " " + ship.model;
                }
                else
=======
                using (var gzipStream = new GZipStream(streamOut, CompressionLevel.Optimal, true))
>>>>>>> feature/coriolis
                {
                    streamIn.CopyTo(gzipStream);
                }
                uri += "data=" + Uri.EscapeDataString(Convert.ToBase64String(streamOut.ToArray()));
            }

            // Add the ship's name
            string bn;
            if (ship.name == null)
            {
                bn = ship.role + " " + ship.model;
            }
            else
            {
                bn = ship.name;
            }
            uri += "&bn=" + Uri.EscapeDataString(bn);

            return uri;
        }
    }
}
