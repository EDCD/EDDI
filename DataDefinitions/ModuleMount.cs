namespace EddiDataDefinitions
{
    /// <summary> Asteroid material content </summary>
    public class ModuleMount ( string edName ) : ResourceBasedLocalizedEDName<ModuleMount>( edName, edName )
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
    }
}
