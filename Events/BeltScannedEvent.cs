using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiEvents
{
    public class BeltScannedEvent : Event
    {
        public const string NAME = "Belt scanned";
        public const string DESCRIPTION = "Triggered when you complete a scan of a belt";
        public const string SAMPLE = @"{ ""timestamp"":""2017-09-05T08:35:15Z"", ""event"":""Scan"", ""BodyName"":""Synuefe RE-G c27-9 ABC A Belt Cluster 4"", ""DistanceFromArrivalLS"":223.959274}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BeltScannedEvent()
        {
            VARIABLES.Add("name", "The name of the belt that has been scanned");
            VARIABLES.Add("distancefromarrival", "The distance in LS from the main star");
        }

        public string name { get; private set; }

        public decimal distancefromarrival { get; private set; }

        public BeltScannedEvent(DateTime timestamp, string name, decimal distancefromarrival) : base(timestamp, NAME)
        {
            this.name = name;
            this.distancefromarrival = distancefromarrival;
        }

    }
}