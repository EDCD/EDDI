using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommanderContinuedEvent : Event
    {
        public const string NAME = "Commander continued";
        public const string DESCRIPTION = "Triggered when you continue an existing game";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"LoadGame\",\"Commander\":\"HRC1\",\"FID\":\"F44396\",\"Horizons\":true,\"Ship\":\"CobraMkIII\",\"ShipID\":1,\"GameMode\":\"Group\",\"Group\":\"Mobius\",\"Credits\":600120,\"Loan\":0,\"ShipName\":\"jewel of parhoon\",\"ShipIdent\":\"hr-17f\",\"FuelLevel\":3.964024,\"FuelCapacity\":8}";

        [PublicAPI("The commander's name")]
        public string commander { get; private set; }

        [PublicAPI("The game version includes the 'Horizons' DLC")]
        public bool horizons { get; private set; }

        [PublicAPI("The game version includes the 'Odyssey' DLC")]
        public bool odyssey { get; private set; }

        [PublicAPI("The commander's ship")]
        public string ship => shipEDModel == "TestBuggy" ? Utilities.Constants.VEHICLE_SRV
            : shipEDModel.ToLowerInvariant().Contains("fighter") ? Utilities.Constants.VEHICLE_FIGHTER
            : shipEDModel.ToLowerInvariant().Contains("suit") ? Utilities.Constants.VEHICLE_LEGS
            : shipEDModel.ToLowerInvariant().Contains("taxi") ? Utilities.Constants.VEHICLE_TAXI
            : ShipDefinitions.FromEDModel(shipEDModel, false)?.model;

        [PublicAPI("The ID of the commander's ship")]
        public long? shipid { get; private set; } // shipid serves double duty in the journal - for ships it is the localId (an integer value). For suits, it is the suit ID (a long).

        [PublicAPI("The game mode (Open, Group or Solo)")]
        public string mode { get; private set; }

        [PublicAPI("The name of the group (only if mode == Group)")]
        public string group { get; private set; }

        [PublicAPI("The number of credits the commander has")]
        public long credits { get; private set; }

        [PublicAPI("The current loan the commander has")]
        public long loan { get; private set; }

        [PublicAPI("The current fuel level of the commander's vehicle")]
        public decimal? fuel { get; private set; }

        [PublicAPI("The total fuel capacity of the commander's vehicle")]
        public decimal? fuelcapacity { get; private set; }

        [PublicAPI("True if the commander is starting landed")]
        public bool? startlanded { get; private set; }

        [PublicAPI("True if the commander is starting dead / at the rebuy screen")]
        public bool? startdead { get; private set; }

        // Not intended to be user facing

        public string shipname { get; private set; }

        public string shipident { get; private set; }

        public string frontierID { get; private set; }

        public string shipEDModel { get; private set; }

        public CommanderContinuedEvent(DateTime timestamp, string commander, string frontierID, bool horizons, bool odyssey, long? shipId, string shipEdModel, string shipName, string shipIdent, bool? startedLanded, bool? startDead, GameMode mode, string group, long credits, long loan, decimal? fuel, decimal? fuelcapacity) : base(timestamp, NAME)
        {
            this.commander = commander;
            this.frontierID = frontierID;
            this.horizons = horizons;
            this.odyssey = odyssey;
            this.shipid = shipId;
            this.shipEDModel = shipEdModel;
            this.shipname = shipName;
            this.shipident = shipIdent;
            this.startlanded = startedLanded;
            this.startdead = startDead;
            this.mode = (mode == null ? null : mode.localizedName);
            this.group = group;
            this.credits = credits;
            this.loan = loan;
            this.fuel = fuel;
            this.fuelcapacity = fuelcapacity;
        }
    }
}
