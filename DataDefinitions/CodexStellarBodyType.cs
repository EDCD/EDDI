namespace EddiDataDefinitions
{
    public class CodexStellarBodyType : ResourceBasedLocalizedEDName<CodexStellarBodyType>
    {
        static CodexStellarBodyType ()
        {
            resourceManager = Properties.CodexStellarBodyType.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new CodexStellarBodyType( edname );

            GasGiants = new CodexStellarBodyType( "Gas_Giants" );
            Stars = new CodexStellarBodyType( "Stars" );
            Terrestrials = new CodexStellarBodyType( "Terrestrials" );
        }

        public static readonly CodexStellarBodyType GasGiants;
        public static readonly CodexStellarBodyType Stars;
        public static readonly CodexStellarBodyType Terrestrials;

        // dummy used to ensure that the static constructor has run
        public CodexStellarBodyType () : this( "" )
        { }

        private CodexStellarBodyType ( string edname ) : base( edname, edname )
        { }
    }
}