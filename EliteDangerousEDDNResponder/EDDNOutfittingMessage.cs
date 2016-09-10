using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEDDNResponder
{
    class EDDNCommoditiesMessage : EDDNMessage
    {
        public List<EDDNCommodity> commodities;

        public EDDNCommoditiesMessage()
        {
            commodities = new List<EDDNCommodity>();
        }
    }
}
