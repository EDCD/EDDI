using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipFsdEvent : Event
    {
        public const string NAME = "Ship fsd";
        public const string DESCRIPTION = "Triggered when there is a change to the status of your ship's fsd";
        public const string SAMPLE = null;
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipFsdEvent()
        {
            VARIABLES.Add("fsd_status", "The status of your ship's fsd ('cooldown', 'cooldown complete', 'charging', 'charging complete', 'masslock', or 'masslock cleared')");
        }

        [JsonProperty("fsd_status")]
        public string fsd_status { get; private set; }

        public ShipFsdEvent(DateTime timestamp, string fsdStatus) : base(timestamp, NAME)
        {
            this.fsd_status = fsdStatus;
        }
    }
}
