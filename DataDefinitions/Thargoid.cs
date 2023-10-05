using System.Linq;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    public class Thargoid : ResourceBasedLocalizedEDName<Thargoid>
    {
        static Thargoid ()
        {
            resourceManager = Properties.Thargoid.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new Thargoid( edname );
        }

        public static readonly Thargoid Basilisk = new Thargoid( "Basilisk", 3100402, null );
        public static readonly Thargoid Berserker = new Thargoid( "Berserker", 3100802, null );
        public static readonly Thargoid Caustic_Generator = new Thargoid( "Caustic_Generator", 3101300, null );
        public static readonly Thargoid Cyclops = new Thargoid( "Cyclops", 3100401, null );
        public static readonly Thargoid Hydra = new Thargoid( "Hydra", 3100404, null );
        public static readonly Thargoid Inciter = new Thargoid( "Inciter", 3100803, null );
        public static readonly Thargoid Marauder = new Thargoid( "Marauder", 3100801, null );
        public static readonly Thargoid Medusa = new Thargoid( "Medusa", 3100403, null );
        public static readonly Thargoid Orthrus = new Thargoid( "Orthrus", 3100406, null );
        public static readonly Thargoid Regenerator = new Thargoid( "Regenerator", 3100804, null );
        public static readonly Thargoid Scavengers = new Thargoid( "Scavengers", 3100700, null );
        public static readonly Thargoid TG_DataScan = new Thargoid( "TG_DataScan", 3101000, null );
        public static readonly Thargoid TG_Pod = new Thargoid( "TG_Pod", 3101100, null );
        public static readonly Thargoid TG_Transmitter = new Thargoid( "TG_Transmitter", 3101200, null );
        public static readonly Thargoid Wrecked_Interceptor = new Thargoid( "Wrecked_Interceptor", 3100405, null );
        public static readonly Thargoid Wrecked_Scout = new Thargoid( "Wrecked_Scout", 3100805, null );
        //missing Thargoid Glaive Hunter
        //missing Thargoid Scythe Hunter

        public long? entryID;

        [PublicAPI]
        public long? value;

        [PublicAPI]
        public string description => Properties.ThargoidDesc.ResourceManager.GetString( edname );

        // dummy used to ensure that the static constructor has run
        public Thargoid () : this( "" )
        { }

        private Thargoid ( string edname ) : base( edname, edname )
        { }

        private Thargoid ( string edname, long? entryID, long? value ) : base( edname, edname )
        {
            this.entryID = entryID;
            this.value = value;
        }

        /// <summary>
        /// Try getting data from the entryid first, then use edname as a fallback
        /// </summary>
        public static Thargoid Lookup ( long? entryId, string edName )
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
                    Logging.Error( $"Duplicate EntryID value {entryId} in {nameof( Thargoid )}.", e );
                }
                else if ( AllOfThem.All( a => a.entryID != entryId ) )
                {
                    Logging.Error( $"Unknown EntryID value {entryId} with edname {edName} in {nameof( Thargoid )}.", e );
                }
            }

            return FromEDName( edName ) ?? new Thargoid( edName ); // No match.
        }
    }
}
