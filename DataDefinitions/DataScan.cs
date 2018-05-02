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

            var DataLink = new DataScan("DataLink");
            var DataPoint = new DataScan("DataPoint");
            var ListeningPost = new DataScan("ListeningPost");
            var AbandonedDataLog = new DataScan("AbandonedDataLog");
            var WreckedShip = new DataScan("WreckedShip");
        }

        // dummy used to ensure that the static constructor has run
        public DataScan() : this("")
        {}

        private DataScan(string edname) : base(edname, edname)
        {}
    }
}
