using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class OrganicPredictionEvent : Event
    {
        public const string NAME = "Organic Surface Signals Prediction";
        public const string DESCRIPTION = "Triggered when surface signal sources are predicted";
        public const string SAMPLE = null;

        [PublicAPI( "A list of the predicted genuses of the biologicals" )]
        public List<string> biosignals => bioSignals.Select(s => s.genus.localizedName).ToList();

        [PublicAPI( "The body that the surface signals are predicted to be found on" )]
        public Body body { get; private set; }

        [PublicAPI( "Full object data for the predicted biologicals" )]
        public HashSet<Exobiology> bioSignals { get; private set; }

        public OrganicPredictionEvent ( DateTime timestamp, Body body, HashSet<Exobiology> signals ) : base(timestamp, NAME)
        {
            this.body = body;
            this.bioSignals = signals;
        }
    }
}
