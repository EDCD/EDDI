using EliteDangerousCompanionAppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousDataProviderVAPlugin
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
            { "Cobra MkIII", "cobra_mk_iii" },
            { "Cobra MkIV", "cobra_mk_iv" },
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
            { "Viper MkIII", "viper" },
            { "Viper MkIV", "viper_mk_iv" },
            { "Vulture", "vulture" }
        };

        public static string ShipUri(Ship ship)
        {
            string uri = "http://coriolis.io/outfit/";
            uri += shipModels[ship.Model];
            uri += "/";
            uri += ShipBulkheads(ship.Bulkheads.Name);
            uri += ship.PowerPlant.Class + ship.PowerPlant.Grade;
            uri += ship.Thrusters.Class + ship.Thrusters.Grade;
            uri += ship.FrameShiftDrive.Class + ship.FrameShiftDrive.Grade;
            uri += ship.LifeSupport.Class + ship.LifeSupport.Grade;
            uri += ship.PowerDistributor.Class + ship.PowerDistributor.Grade;
            uri += ship.Sensors.Class + ship.Sensors.Grade;
            uri += ship.FuelTank.Class + ship.FuelTank.Grade;
            foreach (Hardpoint Hardpoint in ship.Hardpoints)
            {
                if (Hardpoint.Module == null)
                {
                    uri += "-";
                }
                else
                {
                    string id = CoriolisIDDefinitions.FromEDDBID(Hardpoint.Module.EDDBID);
                    if (id == null)
                    {
                        uri += "-";
                    }
                    else
                    {
                        uri += id;
                    }
                }
            }
            foreach (Compartment Compartment in ship.Compartments)
            {
                if (Compartment.Module == null)
                {
                    uri += "-";
                }
                else
                {
                    string id = CoriolisIDDefinitions.FromEDDBID(Compartment.Module.EDDBID);
                    if (id == null)
                    {
                        uri += "-";
                    }
                    else
                    {
                        uri += id;
                    }
                }

            }
            uri += ".Iw18ZwAzkA==.CwBhrSunIZjtIA==";
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
