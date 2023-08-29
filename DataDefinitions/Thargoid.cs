using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Threading;
using Utilities;

namespace EddiDataDefinitions
{
    public class Thargoid : ResourceBasedLocalizedEDName<Thargoid>
    {
        public static ResourceManager rmThargoidDesc = new ResourceManager("EddiDataDefinitions.Properties.ThargoidDesc", Assembly.GetExecutingAssembly());

        public static readonly IDictionary<string, long?> THARGOIDS = new Dictionary<string, long?>();
        public static readonly IDictionary<long, Thargoid> ENTRYIDS = new Dictionary<long, Thargoid>();

        public bool exists;                 // This item exists and has been populated with information
        public long? value;
        public string description;

        static Thargoid ()
        {
            resourceManager = Properties.Thargoid.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new Thargoid( edname );

            ENTRYIDS.Add( (long)3100402, new Thargoid( "Basilisk", null ) );
            ENTRYIDS.Add( (long)3100802, new Thargoid( "Berserker", null ) );
            ENTRYIDS.Add( (long)3101300, new Thargoid( "Caustic_Generator", null ) );
            ENTRYIDS.Add( (long)3100401, new Thargoid( "Cyclops", null ) );
            ENTRYIDS.Add( (long)3100404, new Thargoid( "Hydra", null ) );
            ENTRYIDS.Add( (long)3100803, new Thargoid( "Inciter", null ) );
            ENTRYIDS.Add( (long)3100801, new Thargoid( "Marauder", null ) );
            ENTRYIDS.Add( (long)3100403, new Thargoid( "Medusa", null ) );
            ENTRYIDS.Add( (long)3100406, new Thargoid( "Orthrus", null ) );
            ENTRYIDS.Add( (long)3100804, new Thargoid( "Regenerator", null ) );
            ENTRYIDS.Add( (long)3100700, new Thargoid( "Scavengers", null ) );
            ENTRYIDS.Add( (long)3101000, new Thargoid( "TG_DataScan", null ) );
            ENTRYIDS.Add( (long)3101100, new Thargoid( "TG_Pod", null ) );
            ENTRYIDS.Add( (long)3101200, new Thargoid( "TG_Transmitter", null ) );
            ENTRYIDS.Add( (long)3100405, new Thargoid( "Wrecked_Interceptor", null ) );
            ENTRYIDS.Add( (long)3100805, new Thargoid( "Wrecked_Scout", null ) );
            //MISSINGIDS.Add( "", new Thargoid( "", null ) );       // Thargoid Glaive Hunter
            //MISSINGIDS.Add( "", new Thargoid( "", null ) );       // Thargoid Scythe Hunter

            THARGOIDS.Add( "Basilisk", 3100402 );
            THARGOIDS.Add( "Berserker", 3100802 );
            THARGOIDS.Add( "Caustic_Generator", 3101300 );
            THARGOIDS.Add( "Cyclops", 3100401 );
            THARGOIDS.Add( "Hydra", 3100404 );
            THARGOIDS.Add( "Inciter", 3100803 );
            THARGOIDS.Add( "Marauder", 3100801 );
            THARGOIDS.Add( "Medusa", 3100403 );
            THARGOIDS.Add( "Orthrus", 3100406 );
            THARGOIDS.Add( "Regenerator", 3100804 );
            THARGOIDS.Add( "Scavengers", 3100700 );
            THARGOIDS.Add( "TG_DataScan", 3101000 );
            THARGOIDS.Add( "TG_Pod", 3101100 );
            THARGOIDS.Add( "TG_Transmitter", 3101200 );
            THARGOIDS.Add( "Wrecked_Interceptor", 3100405 );
            THARGOIDS.Add( "Wrecked_Scout", 3100805 );
            //THARGOIDS.Add( "",  );      // Thargoid Glaive Hunter
            //THARGOIDS.Add( "",  );      // Thargoid Scythe Hunter
        }

        // dummy used to ensure that the static constructor has run
        public Thargoid () : this( "" )
        { }

        private Thargoid ( string name ) : base( name, name )
        {
            this.exists = false;
            this.value = 0;
            this.description = "";
        }

        private Thargoid ( string name, long? value ) : base( name, name )
        {
            this.exists = true;
            this.value = value;
            this.description = rmThargoidDesc.GetString( name );
        }

        /// <summary>
        /// Try getting data from the entryid first, then use name as a fallback
        /// </summary>
        public static Thargoid Lookup ( long? entryId, string name )
        {
            Thargoid item;
            item = LookupByEntryId( entryId );
            
            // EntryId doesn't exist, try name
            if ( item == null )
            {
                item = LookupByName( name );
            }

            if ( item == null )
            {
                item = new Thargoid();
            }

            return item;
        }

        /// <summary>
        /// Preferred method of lookup
        /// </summary>
        public static Thargoid LookupByEntryId ( long? entryId )
        {
            if ( entryId != null )
            {
                if ( ENTRYIDS.ContainsKey( (long)entryId ) )
                {
                    return ENTRYIDS[ (long)entryId ];
                }
            }
            return null;
        }

        /// <summary>
        /// Lookup data by name
        /// </summary>
        public static Thargoid LookupByName ( string name )
        {
            if ( name != "" )
            {
                if ( THARGOIDS.ContainsKey( name ) )
                {
                    long? entryid = THARGOIDS[ name ];
                    if ( entryid != null )
                    {
                        return LookupByEntryId( entryid );
                    }
                }
            }
            return null;
        }
    }
}
