using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Threading;
using Utilities;

namespace EddiDataDefinitions
{
    public class Guardian : ResourceBasedLocalizedEDName<Guardian>
    {
        public static ResourceManager rmGuardianDesc = new ResourceManager("EddiDataDefinitions.Properties.GuardianDesc", Assembly.GetExecutingAssembly());

        public static readonly IDictionary<string, long?> GUARDIANS = new Dictionary<string, long?>();
        public static readonly IDictionary<long, Guardian> ENTRYIDS = new Dictionary<long, Guardian>();

        public bool exists;                 // This item exists and has been populated with information
        public long? value;
        public string description;

        static Guardian ()
        {
            resourceManager = Properties.Guardian.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new Guardian( edname );

            ENTRYIDS.Add( 3200800, new Guardian( "Guardian_Beacons", null ) );
            ENTRYIDS.Add( 3200200, new Guardian( "Guardian_Data_Logs", null ) );
            ENTRYIDS.Add( 3200400, new Guardian( "Guardian_Pylon", null ) );
            ENTRYIDS.Add( 3200600, new Guardian( "Guardian_Sentinel", null ) );
            ENTRYIDS.Add( 3200300, new Guardian( "Guardian_Terminal", null ) );
            ENTRYIDS.Add( 3200500, new Guardian( "Relic_Tower", null ) );

            GUARDIANS.Add( "Guardian_Beacons", 3200800 );
            GUARDIANS.Add( "Guardian_Data_Logs", 3200200 );
            GUARDIANS.Add( "Guardian_Pylon", 3200400 );
            GUARDIANS.Add( "Guardian_Sentinel", 3200600 );
            GUARDIANS.Add( "Guardian_Terminal", 3200300 );
            GUARDIANS.Add( "Relic_Tower", 3200500 );
        }

        // dummy used to ensure that the static constructor has run
        public Guardian () : this( "" )
        { }

        private Guardian ( string name ) : base( name, name )
        {
            this.exists = false;
            this.value = 0;
            this.description = "";
        }

        private Guardian ( string name, long? value ) : base( name, name )
        {
            this.exists = true;
            this.value = value;
            this.description = rmGuardianDesc.GetString( name );
        }

        /// <summary>
        /// Try getting data from the entryid first, then use name as a fallback
        /// </summary>
        public static Guardian Lookup ( long? entryId, string name )
        {
            Guardian item;
            item = LookupByEntryId( entryId );
            
            // EntryId doesn't exist, try name
            if ( item == null )
            {
                item = LookupByName( name );
            }

            if ( item == null )
            {
                item = new Guardian();
            }

            return item;
        }

        /// <summary>
        /// Preferred method of lookup
        /// </summary>
        public static Guardian LookupByEntryId ( long? entryId )
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
        public static Guardian LookupByName ( string name )
        {
            if ( name != "" )
            {
                if ( GUARDIANS.ContainsKey( name ) )
                {
                    long? entryid = GUARDIANS[ name ];
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
