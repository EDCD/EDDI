using EliteDangerousDataDefinitions;
using System;
using System.Collections.Generic;

namespace EDDIVAPlugin
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

        public static string ShipUri(Ship ship)
        {
            string enableds = "";
            string priorities = "";
            string uri = "https://coriolis.io/outfit/";
            uri += shipModels[ship.model];
            uri += "/";
            uri += ShipBulkheads(ship.bulkheads.name);
            enableds += "1";
            priorities += "4";
            uri += ship.powerplant.@class + ship.powerplant.grade;
            enableds += "1";
            priorities += "0";
            uri += ship.thrusters.@class + ship.thrusters.grade;
            enableds += ship.thrusters.enabled ? "1" : "0";
            priorities += ship.thrusters.priority;
            uri += ship.frameshiftdrive.@class + ship.frameshiftdrive.grade;
            enableds += ship.frameshiftdrive.enabled ? "1" : "0";
            priorities += ship.frameshiftdrive.priority;
            uri += ship.lifesupport.@class + ship.lifesupport.grade;
            enableds += ship.lifesupport.enabled ? "1" : "0";
            priorities += ship.lifesupport.priority;
            uri += ship.powerdistributor.@class + ship.powerdistributor.grade;
            enableds += ship.powerdistributor.enabled ? "1" : "0";
            priorities += ship.powerdistributor.priority;
            uri += ship.sensors.@class + ship.sensors.grade;
            enableds += ship.sensors.enabled ? "1" : "0";
            priorities += ship.sensors.priority;
            uri += ship.fueltank.@class + ship.fueltank.grade;
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
                    }
                }
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
                    }
                }

            }

            uri += ".";
            uri += LZString.compressToBase64(enableds).Replace('/', '-');
            uri += ".";
            uri += LZString.compressToBase64(priorities).Replace('/', '-');

            string bn;
            if (ship.name == null)
            {
                bn = ship.callsign;
            }
            else
            {
                bn = ship.name + " (" + ship.callsign + ")";
            }
            uri += "?bn=" + Uri.EscapeDataString(bn);

            return uri;
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
