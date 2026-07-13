using System;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class CodexCivilizationThargoid : ResourceBasedLocalizedEDName<CodexCivilizationThargoid>
    {
        static CodexCivilizationThargoid ()
        {
            resourceManager = Properties.CodexCivilizationThargoid.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new CodexCivilizationThargoid( edname );
        }

        public static readonly CodexCivilizationThargoid Basilisk = new( "Basilisk", 3100402 );
        public static readonly CodexCivilizationThargoid Berserker = new( "Berserker", 3100802 );
        public static readonly CodexCivilizationThargoid Caustic_Generator = new( "Caustic_Generator", 3101300 );
        public static readonly CodexCivilizationThargoid Coral_Root = new( "Thargoid_Coral_Root", 3100602 );
        public static readonly CodexCivilizationThargoid Cyclops = new( "Cyclops", 3100401 );
        public static readonly CodexCivilizationThargoid Glaive = new( "Glaive", 3100501 );
        public static readonly CodexCivilizationThargoid Hydra = new( "Hydra", 3100404 );
        public static readonly CodexCivilizationThargoid Inciter = new( "Inciter", 3100803 );
        public static readonly CodexCivilizationThargoid Marauder = new( "Marauder", 3100801 );
        public static readonly CodexCivilizationThargoid Medusa = new( "Medusa", 3100403 );
        public static readonly CodexCivilizationThargoid Orthrus = new( "Orthrus", 3100406 );
        public static readonly CodexCivilizationThargoid Regenerator = new( "Regenerator", 3100804 );
        public static readonly CodexCivilizationThargoid Scavengers = new( "Scavengers", 3100700 );
        public static readonly CodexCivilizationThargoid TG_DataScan = new( "TG_DataScan", 3101000 );
        public static readonly CodexCivilizationThargoid TG_Pod = new( "TG_Pod", 3101100 );
        public static readonly CodexCivilizationThargoid TG_Transmitter = new( "TG_Transmitter", 3101200 );
        public static readonly CodexCivilizationThargoid Wrecked_Interceptor = new( "Wrecked_Interceptor", 3100405 );
        public static readonly CodexCivilizationThargoid Wrecked_Scout = new( "Wrecked_Scout", 3100805 );
        //missing Thargoid Scythe Hunter

        public long? entryID;

        [PublicAPI( "localized description" )]
        public string localizedDescription => Properties.CodexCivilizationThargoidDesc.ResourceManager.GetString( edname );

        // dummy used to ensure that the static constructor has run
        public CodexCivilizationThargoid () : this( "" )
        { }

        private CodexCivilizationThargoid ( string edname ) : base( edname, edname )
        { }

        private CodexCivilizationThargoid ( string edname, long? entryID ) : base( edname, edname )
        {
            this.entryID = entryID;
        }

        /// <summary>
        /// Try getting data from the entryid first, then use edname as a fallback
        /// </summary>
        public static CodexCivilizationThargoid Lookup ( long? entryId, string edName )
        {
            try
            {
                if ( entryId != null )
                {
                    return AllOfThem.Single( a => a.entryID == entryId );
                }
            }
            catch ( InvalidOperationException e )
            {
                if ( AllOfThem.Count( a => a.entryID == entryId ) > 1 )
                {
                    Logging.Error( $"Duplicate EntryID value {entryId} in {nameof( CodexCivilizationThargoid )}.", e );
                }
                else if ( AllOfThem.All( a => a.entryID != entryId ) )
                {
                    Logging.Error( $"Unknown EntryID value {entryId} with edname {edName} in {nameof( CodexCivilizationThargoid )}.", e );
                }
            }

            return FromEDName( edName ) ?? new CodexCivilizationThargoid( edName ); // No match.
        }
    }
}