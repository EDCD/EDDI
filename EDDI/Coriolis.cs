using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Utilities;

namespace Eddi
{
    public class Coriolis
    {
        // Translation from the names that we use to the names that coriolis uses
        private static Dictionary<string, string> shipModels = new Dictionary<string, string>()
        {
            { "Adder" , "adder"},
            { "Anaconda", "anaconda" },
            { "Asp Explorer", "asp" },
            { "Asp Scout", "asp_scout" },
            { "Beluga Liner", "beluga" },
            { "Cobra Mk. III", "cobra_mk_iii" },
            { "Cobra Mk. IV", "cobra_mk_iv" },
            { "Imperial Cutter", "imperial_cutter" },
            { "Diamondback Explorer", "diamondback_explorer" },
            { "Diamondback Scout", "diamondback" },
            { "Eagle", "eagle" },
            { "Imperial Courier", "imperial_courier" },
            { "Imperial Eagle", "imperial_eagle" },
            { "Imperial Clipper", "imperial_clipper" },
            { "Federal Corvette", "federal_corvette" },
            { "Federal Dropship", "federal_dropship" },
            { "Federal Assault Ship", "federal_assault_ship" },
            { "Federal Gunship", "federal_gunship" },
            { "Fer-de-Lance", "fer_de_lance" },
            { "Hauler", "hauler" },
            { "Keelback", "keelback" },
            { "Orca", "orca" },
            { "Python", "python" },
            { "SideWinder", "sidewinder" },
            { "Type-6 Transporter", "type_6_transporter" },
            { "Type-7 Transporter", "type-7_transporter" },
            { "Type-9 Heavy", "type_9_heavy" },
            { "Viper Mk. III", "viper" },
            { "Viper Mk. IV", "viper_mk_iv" },
            { "Vulture", "vulture" }
        };

        private static int addModifications(Module module, byte position, BinaryWriter writer)
        {
            int bytesWritten = 0;
            if (module.modifications != null)
            {
                writer.Write(position);
                bytesWritten++;
                foreach (Modification modification in module.modifications)
                {
                    writer.Write((byte)modification.id);
                    bytesWritten++;
                    writer.Write((int)(modification.value * 10000));
                    bytesWritten += 4;
                }
                writer.Write((byte)0xff);
                bytesWritten++;
            }
            return bytesWritten;
        }

        public static string ShipUri(Ship ship)
        {
            int modsSize = 0;
            string enableds = "";
            string priorities = "";

            string uri = "https://coriolis.edcd.io/outfit/";

            using (MemoryStream stream = new MemoryStream())
            {
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
                {
                    bn = ship.name;
                }
                uri += "?bn=" + Uri.EscapeDataString(bn);

                return uri;
            }
        }

        public static string ShipBulkheads(string bulkheads)
        {
            switch (bulkheads)
            {
                case "Lightweight Alloy":
                    return "0";
                case "Reinforced Alloy":
                    return "1";
                case "Military Grade Composite":
                    return "2";
                case "Mirrored Surface Composite":
                    return "3";
                case "Reactive Surface Composite":
                    return "4";
                default:
                    return "0";
            }
        }
    }
}
