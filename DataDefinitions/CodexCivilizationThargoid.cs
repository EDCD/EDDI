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

        public static readonly CodexCivilizationThargoid Basilisk = new CodexCivilizationThargoid( "Basilisk", 3100402 );
        public static readonly CodexCivilizationThargoid Berserker = new CodexCivilizationThargoid( "Berserker", 3100802 );
        public static readonly CodexCivilizationThargoid Caustic_Generator = new CodexCivilizationThargoid( "Caustic_Generator", 3101300 );
        public static readonly CodexCivilizationThargoid Cyclops = new CodexCivilizationThargoid( "Cyclops", 3100401 );
        public static readonly CodexCivilizationThargoid Glaive = new CodexCivilizationThargoid( "Glaive", 3100501 );
        public static readonly CodexCivilizationThargoid Hydra = new CodexCivilizationThargoid( "Hydra", 3100404 );
        public static readonly CodexCivilizationThargoid Inciter = new CodexCivilizationThargoid( "Inciter", 3100803 );
        public static readonly CodexCivilizationThargoid Marauder = new CodexCivilizationThargoid( "Marauder", 3100801 );
        public static readonly CodexCivilizationThargoid Medusa = new CodexCivilizationThargoid( "Medusa", 3100403 );
        public static readonly CodexCivilizationThargoid Orthrus = new CodexCivilizationThargoid( "Orthrus", 3100406 );
        public static readonly CodexCivilizationThargoid Regenerator = new CodexCivilizationThargoid( "Regenerator", 3100804 );
        public static readonly CodexCivilizationThargoid Scavengers = new CodexCivilizationThargoid( "Scavengers", 3100700 );
        public static readonly CodexCivilizationThargoid TG_DataScan = new CodexCivilizationThargoid( "TG_DataScan", 3101000 );
        public static readonly CodexCivilizationThargoid TG_Pod = new CodexCivilizationThargoid( "TG_Pod", 3101100 );
        public static readonly CodexCivilizationThargoid TG_Transmitter = new CodexCivilizationThargoid( "TG_Transmitter", 3101200 );
        public static readonly CodexCivilizationThargoid Wrecked_Interceptor = new CodexCivilizationThargoid( "Wrecked_Interceptor", 3100405 );
        public static readonly CodexCivilizationThargoid Wrecked_Scout = new CodexCivilizationThargoid( "Wrecked_Scout", 3100805 );
        //missing Thargoid Scythe Hunter

        public long? entryID;

        [PublicAPI]
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