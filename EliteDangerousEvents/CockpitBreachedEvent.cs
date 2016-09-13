using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class CockpitBreachedEvent : Event
    {
        public const string NAME = "Cockpit breached";

        public CockpitBreachedEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
