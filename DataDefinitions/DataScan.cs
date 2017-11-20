using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Datalink scanned
    /// </summary>
    public class DataScan
    {
        private static readonly List<DataScan> DATASCANS = new List<DataScan>();

        public string name { get; private set; }

        public string edname { get; private set; }

        private DataScan(string edname, string name)
        {
            this.edname = edname;
            this.name = name;

            DATASCANS.Add(this);
        }

        public static readonly DataScan DataLink = new DataScan("DataLink", "Data Link");
        public static readonly DataScan DataPoint = new DataScan("DataPoint", "Data Point");
        public static readonly DataScan ListeningPost = new DataScan("ListeningPost", "Listening Post");
        public static readonly DataScan AbandonedDataLog = new DataScan("AbandonedDataLog", "Abandoned Data Log");
        public static readonly DataScan WreckedShip = new DataScan("WreckedShip", "Wrecked Ship");

        public static DataScan FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            DataScan result = DATASCANS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Data Link name " + from);
            }
            return result;
        }

        public static DataScan FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from == null ? null : from.Replace(";", "").ToLowerInvariant();
            DataScan result = DATASCANS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown Data Link ED name " + from);
                result = new DataScan(from, tidiedFrom);
            }
            return result;
        }
    }
}
