using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEDDNResponder
{
    abstract class EDDNMessage
    {
        public DateTime timestamp;
        public string systemName;
        public string stationName;
    }
}
