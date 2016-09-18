using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// Signal source types
    /// </summary>
    public class SignalSource
    {
        private static readonly List<SignalSource> SIGNALSOURCES = new List<SignalSource>();

        public string name { get; private set; }

        public string edname { get; private set; }

        private SignalSource(string edname, string name)
        {
            this.edname = edname;
            this.name = name;

            SIGNALSOURCES.Add(this);
        }

        public static readonly SignalSource None = new SignalSource("none", "None");

        public static SignalSource FromName(string from)
        {
            SignalSource result = SIGNALSOURCES.First(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Signal Source name " + from);
            }
            return result;
        }

        public static SignalSource FromEDName(string from)
        {
            SignalSource result = SIGNALSOURCES.First(v => v.edname == from);
            if (result == null)
            {
                Logging.Report("Unknown Signal Source ED name " + from);
            }
            return result;
        }
    }
}
