using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEDDNResponder
{
    class EDDNShipyardMessage : EDDNMessage
    {
        public List<string> ships;

        public EDDNShipyardMessage()
        {
            ships = new List<string>();
        }
    }
}
