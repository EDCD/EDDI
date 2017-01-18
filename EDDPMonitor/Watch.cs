using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEddpMonitor
{
    /// <summary>
    /// The parameters to match EDDP messages
    /// </summary>
    public class Watch
    {
        public string name { get; private set; }
        public string system { get; private set; }
        public string station { get; private set; }
        public string faction { get; private set; }
        public string state { get; private set; }
        public decimal maxdistancefromship { get; private set; }
        public decimal maxdistancefromhome { get; private set; }
    }
}
