using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierStatsEvent : Event
    {
        public const string NAME = "Carrier stats";
        public const string DESCRIPTION = "Triggered when you open the carrier management screen";
        public const string SAMPLE = "{ \"timestamp\":\"2022-08-13T11:50:53Z\", \"event\":\"CarrierStats\", \"CarrierID\":3709999999, \"Callsign\":\"X9X-9XX\", \"Name\":\"Innominatus\", \"DockingAccess\":\"all\", \"AllowNotorious\":true, \"FuelLevel\":732, \"JumpRangeCurr\":500.000000, \"JumpRangeMax\":500.000000, \"PendingDecommission\":false, \"SpaceUsage\":{ \"TotalCapacity\":25000, \"Crew\":5670, \"Cargo\":1111, \"CargoSpaceReserved\":1664, \"ShipPacks\":0, \"ModulePacks\":0, \"FreeSpace\":16555 }, \"Finance\":{ \"CarrierBalance\":1882996398, \"ReserveBalance\":320862095, \"AvailableBalance\":1475795999, \"ReservePercent\":17 }, \"Crew\":[ { \"CrewRole\":\"BlackMarket\", \"Activated\":false }, { \"CrewRole\":\"Captain\", \"Activated\":true, \"Enabled\":true, \"CrewName\":\"Marilyn Erickson\" }, { \"CrewRole\":\"Refuel\", \"Activated\":false }, { \"CrewRole\":\"Repair\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Grace Hancock\" }, { \"CrewRole\":\"Rearm\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Nayla Travis\" }, { \"CrewRole\":\"Commodities\", \"Activated\":true, \"Enabled\":true, \"CrewName\":\"Arya Snyder\" }, { \"CrewRole\":\"VoucherRedemption\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Arnold Wilkinson\" }, { \"CrewRole\":\"Exploration\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Stan Salinas\" }, { \"CrewRole\":\"Shipyard\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Dayami Osborn\" }, { \"CrewRole\":\"Outfitting\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Guadalupe Solomon\" }, { \"CrewRole\":\"CarrierFuel\", \"Activated\":true, \"Enabled\":true, \"CrewName\":\"Craig Callahan\" }, { \"CrewRole\":\"VistaGenomics\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Scott Kemp\" }, { \"CrewRole\":\"PioneerSupplies\", \"Activated\":false }, { \"CrewRole\":\"Bartender\", \"Activated\":true, \"Enabled\":false, \"CrewName\":\"Rita Fernandez\" } ], \"ShipPacks\":[  ], \"ModulePacks\":[  ] }";

        // Carrier variables

        [PublicAPI("The callsign (alphanumeric designation) of the carrier")]
        public string callsign { get; private set; }

        [PublicAPI("The name of the carrier")]
        public string name { get; private set; }

        [PublicAPI("The carrier's current docking access (one of one of 'all', 'squadronfriends', 'friends', or 'none')")]
        public string dockingAccess { get; private set; }

        [PublicAPI("True if the carrier currently provides docking access to notorious commanders")]
        public bool notoriousAccess { get; private set; }

        [PublicAPI("The current tritium fuel level of the carrier")]
        public int fuel { get; private set; }

        [PublicAPI("The current total used capacity of the carrier")]
        public int usedCapacity { get; private set; }

        [PublicAPI("The current free capacity of the carrier")]
        public int freeCapacity { get; private set; }

        [PublicAPI("The current bank balance of the carrier")]
        public ulong bankBalance { get; private set; }

        [PublicAPI("The current reserved bank balance of the carrier")]
        public ulong bankReservedBalance { get; private set; }

        // These properties are not intended to be user facing

        public long? carrierId { get; private set; }

        public CarrierStatsEvent(DateTime timestamp, long? carrierId, string carrierCallsign, string carrierName, string dockingAccess, bool notoriousAccess, int fuelLevel, int usedSpace, int freeSpace, ulong bankBalance, ulong bankBalanceReserved) : base(timestamp, NAME)
        {
            // Carrier
            this.carrierId = carrierId;
            this.callsign = carrierCallsign;
            this.name = carrierName;
            this.dockingAccess = dockingAccess;
            this.notoriousAccess = notoriousAccess;
            this.fuel = fuelLevel;
            this.usedCapacity = usedSpace;
            this.freeCapacity = freeSpace;
            this.bankBalance = bankBalance;
            this.bankReservedBalance = bankBalanceReserved;
        }
    }
}