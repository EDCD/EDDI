using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierStatsEvent (
        DateTime timestamp,
        long carrierId,
        StationModel carrierType,
        string carrierCallsign,
        string carrierName,
        string dockingAccess,
        bool notoriousAccess,
        int fuelLevel,
        int usedSpace,
        int freeSpace,
        long bankBalance,
        long bankBalanceReserved,
        long bankAvailableBalance,
        decimal jumpRangeCurr,
        decimal jumpRangeMax,
        bool pendingDecommission )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Carrier stats";
        public const string DESCRIPTION = "Triggered when you open the carrier management screen";
        public const string SAMPLE = "{ \"timestamp\":\"2022-08-13T11:50:53Z\", \"event\":\"CarrierStats\", \"CarrierID\":3709999999, \"Callsign\":\"X9X-9XX\", \"Name\":\"Innominatus\", \"DockingAccess\":\"all\", \"AllowNotorious\":true, \"FuelLevel\":732, \"JumpRangeCurr\":500.000000, \"JumpRangeMax\":500.000000, \"PendingDecommission\":false, \"SpaceUsage\":{ \"TotalCapacity\":25000, \"Crew\":5670, \"Cargo\":1111, \"CargoSpaceReserved\":1664, \"ShipPacks\":0, \"ModulePacks\":0, \"FreeSpace\":16555 }, \"Finance\":{ \"CarrierBalance\":1882996398, \"ReserveBalance\":320862095, \"AvailableBalance\":1475795999, \"ReservePercent\":17 }, \"Crew\":[ { \"CrewRole\":\"BlackMarket\", \"Activated\":false }, { \"CrewRole\":\"Captain\", \"Activated\":true, \"Enabled\":true, \"CrewName\":\"Marilyn Erickson\" }, { \"CrewRole\":\"Refuel\", \"Activated\":false }, { \"CrewRole\":\"Repair\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Grace Hancock\" }, { \"CrewRole\":\"Rearm\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Nayla Travis\" }, { \"CrewRole\":\"Commodities\", \"Activated\":true, \"Enabled\":true, \"CrewName\":\"Arya Snyder\" }, { \"CrewRole\":\"VoucherRedemption\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Arnold Wilkinson\" }, { \"CrewRole\":\"Exploration\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Stan Salinas\" }, { \"CrewRole\":\"Shipyard\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Dayami Osborn\" }, { \"CrewRole\":\"Outfitting\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Guadalupe Solomon\" }, { \"CrewRole\":\"CarrierFuel\", \"Activated\":true, \"Enabled\":true, \"CrewName\":\"Craig Callahan\" }, { \"CrewRole\":\"VistaGenomics\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Scott Kemp\" }, { \"CrewRole\":\"PioneerSupplies\", \"Activated\":false }, { \"CrewRole\":\"Bartender\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Rita Fernandez\" } ], \"ShipPacks\":[  ], \"ModulePacks\":[  ] }";

        // Carrier variables

        [PublicAPI("The callsign (alphanumeric designation) of the carrier")]
        public string callsign { get; private set; } = carrierCallsign;

        [PublicAPI("The name of the carrier")]
        public string name { get; private set; } = carrierName;

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; } = carrierId;

        [PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" )]
        public StationModel carrierType { get; private set; } = carrierType;

        [PublicAPI("The carrier's current docking access (one of one of 'all', 'squadronfriends', 'friends', or 'none')")]
        public string dockingAccess { get; private set; } = dockingAccess;

        [PublicAPI("True if the carrier currently provides docking access to notorious commanders")]
        public bool notoriousAccess { get; private set; } = notoriousAccess;

        [PublicAPI( "True if the carrier is currently scheduled to be decommissioned" )]
        public bool pendingDecommission { get; set; } = pendingDecommission;

        [PublicAPI("The current tritium fuel level of the carrier")]
        public int fuel { get; private set; } = fuelLevel;

        [PublicAPI("The current single jump range of the carrier in light years")]
        public decimal jumpRange { get; set; } = jumpRangeCurr;

        [PublicAPI( "The maximum single jump range of the carrier in light years" )]
        public decimal jumpRangeMax { get; set; } = jumpRangeMax;

        [PublicAPI("The current total used capacity of the carrier")]
        public int usedCapacity { get; private set; } = usedSpace;

        [PublicAPI("The current free capacity of the carrier")]
        public int freeCapacity { get; private set; } = freeSpace;

        [PublicAPI("The current bank balance of the carrier")]
        public long bankBalance { get; private set; } = bankBalance;

        [PublicAPI("The current reserved bank balance of the carrier")]
        public long bankReservedBalance { get; private set; } = bankBalanceReserved;

        [PublicAPI("The current available bank balance of the carrier")]
        public long bankAvailableBalance { get; private set; } = bankAvailableBalance;

        // Carrier
    }
}