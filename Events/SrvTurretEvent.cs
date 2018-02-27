using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SRVTurretEvent : Event
    {
        public const string NAME = "SRV turret";
        public const string DESCRIPTION = "Triggered when you deploy or retract your SRV's turret";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SRVTurretEvent()
        {
            VARIABLES.Add("deployed", "A boolean value. True if the turret is being deployed and false if it is being retracted");
        }

        [JsonProperty("deployed")]
        public bool deployed { get; private set; }

        public SRVTurretEvent(DateTime timestamp, bool srvTurretDeployed) : base(timestamp, NAME)
        {
            this.deployed = srvTurretDeployed;
        }
    }
}
