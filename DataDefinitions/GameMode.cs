using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
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
        public static readonly GameMode Group = new GameMode("Group", "Group");
        public static readonly GameMode Solo = new GameMode("Solo", "Solo");

        public static GameMode FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            GameMode result = MODES.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown game mode name " + from);
            }
            return result;
        }

        public static GameMode FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from.ToLowerInvariant();
            GameMode result = MODES.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown game mode ED name " + from);
                result = new GameMode(from, tidiedFrom);
            }
            return result;
        }
    }
}
