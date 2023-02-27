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

            Open = new GameMode("Open");
            Group = new GameMode("Group");
            Solo = new GameMode("Solo");
        }

        public static readonly GameMode Open;
        public static readonly GameMode Group;
        public static readonly GameMode Solo;

        // dummy used to ensure that the static constructor has run
        public GameMode() : this("")
        { }

        private GameMode(string edname) : base(edname, edname)
        { }
    }
}
