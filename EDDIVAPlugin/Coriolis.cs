using EliteDangerousCompanionAppService;
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
            string uri = "http://coriolis.io/outfit/";
            uri += shipModels[ship.Model];
            uri += "/";
            uri += ShipBulkheads(ship.Bulkheads.Name);
            enableds += "1";
            priorities += "4";
            uri += ship.PowerPlant.Class + ship.PowerPlant.Grade;
            enableds += "1";
            priorities += "0";
            uri += ship.Thrusters.Class + ship.Thrusters.Grade;
            enableds += ship.Thrusters.Enabled ? "1" : "0";
            priorities += ship.Thrusters.Priority;
            uri += ship.FrameShiftDrive.Class + ship.FrameShiftDrive.Grade;
            enableds += ship.FrameShiftDrive.Enabled ? "1" : "0";
            priorities += ship.FrameShiftDrive.Priority;
            uri += ship.LifeSupport.Class + ship.LifeSupport.Grade;
            enableds += ship.LifeSupport.Enabled ? "1" : "0";
            priorities += ship.LifeSupport.Priority;
            uri += ship.PowerDistributor.Class + ship.PowerDistributor.Grade;
            enableds += ship.PowerDistributor.Enabled ? "1" : "0";
            priorities += ship.PowerDistributor.Priority;
            uri += ship.Sensors.Class + ship.Sensors.Grade;
            enableds += ship.Sensors.Enabled ? "1" : "0";
            priorities += ship.Sensors.Priority;
            uri += ship.FuelTank.Class + ship.FuelTank.Grade;
            enableds += "1";
            priorities += "1";
            foreach (Hardpoint Hardpoint in ship.Hardpoints)
            {
                if (Hardpoint.Module == null)
                {
                    uri += "-";
                    enableds += "1";
                    priorities += "0";
                }
                else
                {
                    string id = CoriolisIDDefinitions.FromEDDBID(Hardpoint.Module.EDDBID);
                    if (id == null)
                    {
                        uri += "-";
                        enableds += "1";
                        priorities += "0";
                    }
                    else
                    {
                        uri += id;
                        enableds += Hardpoint.Module.Enabled ? "1" : "0";
                        priorities += Hardpoint.Module.Priority;
                    }
                }
            }
            foreach (Compartment Compartment in ship.Compartments)
            {
                if (Compartment.Module == null)
                {
                    uri += "-";
                    enableds += "1";
                    priorities += "0";
                }
                else
                {
                    string id = CoriolisIDDefinitions.FromEDDBID(Compartment.Module.EDDBID);
                    if (id == null)
                    {
                        uri += "-";
                        enableds += "1";
                        priorities += "0";
                    }
                    else
                    {
                        uri += id;
                        enableds += Compartment.Module.Enabled ? "1" : "0";
                        priorities += Compartment.Module.Priority;
                    }
                }

            }

            uri += ".";
            uri += LZString.compressToBase64(enableds).Replace('/', '-');
            uri += ".";
            uri += LZString.compressToBase64(priorities).Replace('/', '-');

            string bn;
            if (ship.Name == null)
            {
                bn = ship.CallSign;
            }
            else
            {
                bn = ship.Name + " (" + ship.CallSign + ")";
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
