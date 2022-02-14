namespace EddiDataDefinitions
{
    public class NavWaypoint
    {
        public string systemName { get; set; }
        public ulong? systemAddress { get; set; }
        public decimal? x { get; set; }
        public decimal? y { get; set; }
        public decimal? z { get; set; }

        public decimal distanceTravelled { get; set; }
        public decimal distanceRemaining { get; set; }

        // Spansh Neutron and Galaxy plotters only
        public bool hasNeutronStar { get; set; }

        // Spansh Galaxy plotter only
        public bool? isScoopable { get; set; }
        public bool? refuelRecommended { get; set; }

        // Spansh Carrier plotter only
        public bool? hasIcyRing { get; set; }
        public bool? hasPristineMining { get; set; }
        public bool? isDesiredDestination { get; set; } // If this is one of the destinations prescribed by the commander for the route
        public int? fuelUsed { get; set; } // The amount of tritium used to jump to this location
    }
}
