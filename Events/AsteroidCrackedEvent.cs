using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class AsteroidCrackedEvent : Event
    {
        public const string NAME = "Asteroid cracked";
        public const string DESCRIPTION = "Triggered when you break up a 'Motherlode' asteroid for mining";
        public const string SAMPLE = "{ \"timestamp\":\"2020-05-12T17:10:21Z\", \"event\":\"AsteroidCracked\", \"Body\":\"Corona Austr. Dark Region OX-U b2-3 6 A Ring\" }";

        [PublicAPI("The name of the nearest body (normally the ring where the asteroid was found)")]
        public string bodyname { get; private set; }

        public AsteroidCrackedEvent(DateTime timestamp, string bodyName) : base(timestamp, NAME)
        {
            this.bodyname = bodyName;
        }
    }
}