using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// Details of a star class
    /// </summary>
    public class StarClass
    {
        private static readonly List<StarClass> STARS = new List<StarClass>();

        public string edname { get; private set; }

        public string name { get; private set; }

        public decimal commonality { get; private set; }

        private StarClass(string edname, string name, decimal commonality)
        {
            this.edname = edname;
            this.name = name;
            this.commonality = commonality;
        }

        public static readonly StarClass O = new StarClass("O", "O", 0.000003M);
        public static readonly StarClass B = new StarClass("B", "B", 0.13M);
        public static readonly StarClass A = new StarClass("A", "A", 0.6M);
        public static readonly StarClass F = new StarClass("F", "F", 3M);
        public static readonly StarClass G = new StarClass("G", "G", 7.6M);
        public static readonly StarClass K = new StarClass("K", "K", 12.1M);
        public static readonly StarClass M = new StarClass("M", "M", 76.45M);

        public static StarClass FromName(string from)
        {
            StarClass result = STARS.First(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown star name " + from);
            }
            return result;
        }

        public static StarClass FromEDName(string from)
        {
            StarClass result = STARS.First(v => v.edname == from);
            if (result == null)
            {
                Logging.Report("Unknown star ED name " + from);
            }
            return result;
        }
    }
}
