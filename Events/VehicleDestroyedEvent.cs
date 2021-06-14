using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class VehicleDestroyedEvent : Event
    {
        public const string NAME = "Vehicle destroyed";
        public const string DESCRIPTION = "Triggered when your vehicle (fighter or SRV) is destroyed";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"FighterDestroyed\", \"ID\":13}";

        [PublicAPI("The vehicle that was destroyed (e.g. fighter or srv)")]
        public string vehicle { get; private set; }

        [PublicAPI("The vehicle's id")]
        public int id { get; private set; }

        public VehicleDestroyedEvent(DateTime timestamp, string vehicle, int id) : base(timestamp, NAME)
        {
            this.vehicle = vehicle;
            this.id = id;
        }
    }
}
