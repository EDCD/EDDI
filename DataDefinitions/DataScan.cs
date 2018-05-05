using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Datalink scanned
    /// </summary>
    public class DataScan : ResourceBasedLocalizedEDName<DataScan>
    {
        static DataScan()
        {
            resourceManager = Properties.DataScans.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = edname => new DataScan(edname);

            var AbandonedDataLog = new DataScan("AbandonedDataLog");
            var AncientCodex = new DataScan("ANCIENTCODEX");
            var DataLink = new DataScan("DataLink");
            var DataPoint = new DataScan("DataPoint");
            var DataPointHVT = new DataScan("DataPointHVT");
            var ListeningPost = new DataScan("ListeningPost");
            var SettlementUnknown = new DataScan("Settlement_Unknown");
            var ShipUplink = new DataScan("ShipUplink");
            var TgTransmitter = new DataScan("TGTRANSMITTER");
            var TouristBeacon = new DataScan("TouristBeacon");
            var UnknownUplink = new DataScan("Unknown_Uplink");
            var WreckedShip = new DataScan("WreckedShip");
        }

        // dummy used to ensure that the static constructor has run
        public DataScan() : this("")
        {}

        private DataScan(string edname) : base(edname, edname)
        {}
    }
}
