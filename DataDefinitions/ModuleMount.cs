namespace EddiDataDefinitions
{
    /// <summary> Asteroid material content </summary>
    public class ModuleMount : ResourceBasedLocalizedEDName<ModuleMount>
    {
        static ModuleMount()
        {
            resourceManager = Properties.ModuleMount.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new ModuleMount(edname);
        }

        public static readonly ModuleMount Fixed = new("Fixed");
        public static readonly ModuleMount Gimballed = new("Gimballed");
        public static readonly ModuleMount Turreted = new("Turreted");

        // dummy used to ensure that the static constructor has run
        public ModuleMount() : this("")
        { }

        public ModuleMount(string edName) : base(edName, edName)
        { }
    }
}
