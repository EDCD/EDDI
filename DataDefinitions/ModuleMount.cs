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

        public static readonly ModuleMount Fixed = new ModuleMount("Fixed");
        public static readonly ModuleMount Gimballed = new ModuleMount("Gimballed");
        public static readonly ModuleMount Turreted = new ModuleMount("Turreted");

        // dummy used to ensure that the static constructor has run
        public ModuleMount() : this("")
        { }

        public ModuleMount(string edName) : base(edName, edName)
        { }
    }
}
