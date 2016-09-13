using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class EnteredSupercruiseEvent : Event
    {
        public const string NAME = "Entered supercruise";
        public static EnteredSupercruiseEvent SAMPLE = new EnteredSupercruiseEvent(DateTime.Now);

        public EnteredSupercruiseEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
