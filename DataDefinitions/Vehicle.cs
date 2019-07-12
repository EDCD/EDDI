using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    public class Vehicle : ResourceBasedLocalizedEDName<Vehicle>
    {
        static Vehicle()
        {
            resourceManager = Properties.Vehicle.ResourceManager;
            resourceManager.IgnoreCase = false;

            var Empire = new Vehicle("Empire");
            var Federation = new Vehicle("Federation");
            var GdnHybridV1 = new Vehicle("GdnHybridV1");
            var GdnHybridV2 = new Vehicle("GdnHybridV2");
            var GdnHybridV3 = new Vehicle("GdnHybridV3");
            var Independent = new Vehicle("Independent");
            var TestBuggy = new Vehicle("TestBuggy");
        }

        // Definition of the vehicle
        public int subslot { get; set; }
        public string loadout { get; set; }
        public int rebuilds { get; set; }

        // dummy used to ensure that the static constructor has run
        public Vehicle() : this("")
        { }

        private Vehicle(string edname) : base(edname, edname)
        { }

        public new static Vehicle FromEDName(string edName)
        {
            if (edName == null)
            {
                return null;
            }

            string tidiedName = edName.ToLowerInvariant().Replace("_fighter", "").Replace("_", "");
            return AllOfThem.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedName);
        }

        public static Vehicle FromLoadoutName(string loadout)
        {
            if (loadout == null) { return null; }

            Vehicle vehicle = new Vehicle();
            string tidiedLoadout = loadout.ToLowerInvariant()
                .Replace("_fighter_loadout", "")
                .Replace("_name", "");
            List<string> parts = tidiedLoadout.Split('_').ToList();
            if (parts.Count == 2)
            {
                vehicle = AllOfThem.FirstOrDefault(v => v.edname.ToLowerInvariant() == parts[0]);
                vehicle.loadout = parts[1];
            }

            return vehicle;
        }

        public static Vehicle FromJson(int subslot, dynamic json)
        {
            if (json is null) { return null; }

            string name = (string)json["name"];
            string tidiedName = name.ToLowerInvariant().Replace("_fighter", "").Replace("_", "");
            Vehicle vehicle = AllOfThem.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedName);
            vehicle.loadout = (string)json["loadout"];
            vehicle.rebuilds = (int)json["rebuilds"];
            vehicle.subslot = subslot;

            return vehicle;
        }

    }
}
