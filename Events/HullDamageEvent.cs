using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class HullDamagedEvent : Event
    {
        public const string NAME = "Hull damaged";
        public const string DESCRIPTION = "Triggered when your hull is damaged to a certain extent";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-29T10:26:07Z"", ""event"":""HullDamage"", ""Health"":0.615263, ""PlayerPilot"":false, ""Fighter"":true }";

        [PublicAPI("The vehicle that has been damaged (Ship, SRV, Fighter)")]
        public string vehicle { get; private set; }

        [PublicAPI("True if the vehicle receiving damage is piloted by the player")]
        public bool? piloted { get; private set; }

        [PublicAPI("The percentage health of the hull")]
        public decimal health { get; private set; }

        public HullDamagedEvent(DateTime timestamp, string vehicle, bool? piloted, decimal health) : base(timestamp, NAME)
        {
            this.vehicle = vehicle;
            this.piloted = piloted;
            this.health = health;
        }
    }
}
