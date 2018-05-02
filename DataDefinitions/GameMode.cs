using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Game mode
    /// </summary>
    public class GameMode : ResourceBasedLocalizedEDName<GameMode>
    {
        static GameMode()
        {
            resourceManager = Properties.GameModes.ResourceManager;
            resourceManager.IgnoreCase = false;

            var Open = new GameMode("Open");
            var Group = new GameMode("Group");
            var Solo = new GameMode("Solo");
        }

        // dummy used to ensure that the static constructor has run
        public GameMode() : this("")
        {}

        private GameMode(string edname) : base(edname, edname)
        {}
    }
}
