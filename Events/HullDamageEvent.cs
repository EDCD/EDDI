using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class HullDamagedEvent : Event
    {
        public const string NAME = "Hull damaged";
        public const string DESCRIPTION = "Triggered when your hull is damaged to a certain extent";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-29T10:26:07Z"", ""event"":""HullDamage"", ""Health"":0.615263, ""PlayerPilot"":false, ""Fighter"":true }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static HullDamagedEvent()
        {
            VARIABLES.Add("vehicle", "The vehicle that has been damaged (Ship, SRV, Fighter)");
            VARIABLES.Add("piloted", "True if the vehicle receiving damage is piloted by the player");
            VARIABLES.Add("health", "The percentage health of the hull");
        }

        public string vehicle{ get; private set; }

        public bool? piloted { get; private set; }

        public decimal health { get; private set; }

        public HullDamagedEvent(DateTime timestamp, string vehicle, bool? piloted, decimal health) : base(timestamp, NAME)
        {
            this.vehicle = vehicle;
            this.piloted = piloted;
            this.health = health;
        }
    }
}
