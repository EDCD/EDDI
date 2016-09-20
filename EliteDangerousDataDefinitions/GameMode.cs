using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// Game mode
    /// </summary>
    public class GameMode
    {
        private static readonly List<GameMode> MODES = new List<GameMode>();

        public string edname { get; private set; }

        public string name { get; private set; }

        private GameMode(string edname, string name)
        {
            this.edname = edname;
            this.name = name;

            MODES.Add(this);
        }

        public static readonly GameMode Open = new GameMode("Open", "Open");
        public static readonly GameMode Group = new GameMode("Group", "Private group");
        public static readonly GameMode Solo = new GameMode("Solo", "Solo");

        public static GameMode FromName(string from)
        {
            GameMode result = MODES.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown game mode name " + from);
            }
            return result;
        }

        public static GameMode FromEDName(string from)
        {
            GameMode result = MODES.FirstOrDefault(v => v.edname == from);
            if (result == null)
            {
                Logging.Report("Unknown game mode ED name " + from);
            }
            return result;
        }
    }
}
