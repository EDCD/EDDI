using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// State types for systems and factions
    /// </summary>
    public class State
    {
        private static readonly List<State> STATES = new List<State>();

        public string name { get; private set; }

        public string edname { get; private set; }

        private State(string edname, string name)
        {
            this.edname = edname;
            this.name = name;

            STATES.Add(this);
        }

        public static readonly State None = new State("None", "None");
        public static readonly State Retreat = new State("Retreat", "Retreat");
        public static readonly State War = new State("War", "War");
        public static readonly State Lockdown = new State("Lockdown", "Lockdown");
        public static readonly State CivilUnrest = new State("CivilUnrest", "Civil Unrest");
        public static readonly State CivilWar = new State("CivilWar", "Civil War");
        public static readonly State Boom = new State("Boom", "Boom");
        public static readonly State Expansion = new State("Expansion", "Expansion");
        public static readonly State Bust = new State("Bust", "Bust");
        public static readonly State Outbreak = new State("Outbreak", "Outbreak");
        public static readonly State Famine = new State("Famine", "Famine");
        public static readonly State Election = new State("Election", "Election");
        public static readonly State Investment = new State("Investment", "Investment");

        public static State FromName(string from)
        {
            foreach (State s in STATES)
            {
                if (from == s.name)
                {
                    return s;
                }
            }
            Logging.Report("Unknown state name " + from);
            return null;
        }

        public static State FromEDName(string from)
        {
            foreach (State s in STATES)
            {
                if (from == s.edname)
                {
                    return s;
                }
            }
            Logging.Report("Unknown state ED name " + from);
            return null;
        }
    }
}
