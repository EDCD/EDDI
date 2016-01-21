using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousDataProviderVAPlugin
{
    public class Coriolis
    {
        public static string ShipUri(Ship ship)
        {
            string uri = "http://coriolis.io/outfit/";
            uri += ShipModel(ship.model);
            uri += "/";
            uri += ShipBulkheads(ship.bulkheads);
            uri += ship.powerPlant;
            uri += ship.thrusters;
            uri += ship.frameShiftDrive;
            uri += ship.lifeSupport;
            uri += ship.powerDistributor;
            uri += ship.sensors;
            uri += ship.fuelTank;
            uri += "---1717----04044j030t--002h.Iw18eQ==.Aw18eQ==";
            return uri;
        }

        // Translate ship model to that used by coriolis
        public static string ShipModel(string model)
        {
            return "python";
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
