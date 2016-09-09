using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEDDNResponder
{
    abstract class EDDNMessage
    {
        public string timestamp; // Timestamp in YYYY-MM-DDTHH:mm:SSZ format
        public string systemName;
        public string stationName;
    }
}
