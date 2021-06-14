using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class JetConeBoostEvent : Event
    {
        public const string NAME = "Jet cone boost";
        public const string DESCRIPTION = "Triggered when enough material has been collected from a solar jet cone (at a white dwarf or neutron star) for a jump boost";
        public const string SAMPLE = "{ \"timestamp\":\"2017-11-13T00:43:39Z\", \"event\":\"JetConeBoost\", \"BoostValue\":4.000000 }";

        [PublicAPI("the value of the boost")]
        public decimal boost { get; private set; }

        public JetConeBoostEvent(DateTime timestamp, decimal boost) : base(timestamp, NAME)
        {
            this.boost = boost;
        }
    }
}
