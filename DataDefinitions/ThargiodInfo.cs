using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace EddiDataDefinitions
{
    public class ThargoidItem
    {
        public bool exists;   // This item exists and has been populated with information
        public string name;
        public string description;

        public ThargoidItem ()
        {
            exists = false;
            this.name = "";
            this.description = "";
        }

        public ThargoidItem ( string name, string desc )
        {
            exists = true;
            this.name = name;
            this.description = desc;
        }

        public bool Exists ()
        {
            return this.exists;
        }

        public void SetExists( bool exists ) {
            this.exists = exists;
        }
    }

    static class ThargoidInfo
    {
        public static ResourceManager rmThargoidName = new ResourceManager("EddiDataDefinitions.Properties.ThargoidName", Assembly.GetExecutingAssembly());
        public static ResourceManager rmThargoidDesc = new ResourceManager("EddiDataDefinitions.Properties.ThargoidDesc", Assembly.GetExecutingAssembly());

        public class LookupEntryId
        {
            public string edname;

            public LookupEntryId ( string edname )
            {
                this.edname = edname;
            }
        }

        public class LookupName
        {
            public long? entryid;

            public LookupName ( long? entryid )
            {
                this.entryid = entryid;
            }
        }

        public static Dictionary<long, LookupEntryId> EntryIdData = new Dictionary<long, LookupEntryId>();
        public static Dictionary<string, LookupName> NameData = new Dictionary<string, LookupName>();
        public static Dictionary<string, string> SubCategory = new Dictionary<string, string>();

        static ThargoidInfo ()
        {
            EntryIdData.Add( 3100402, new LookupEntryId( "Basilisk" ) );
            EntryIdData.Add( 3100802, new LookupEntryId( "Berserker" ) );
            EntryIdData.Add( 3101300, new LookupEntryId( "Caustic_Generator" ) );
            EntryIdData.Add( 3100401, new LookupEntryId( "Cyclops" ) );
            EntryIdData.Add( 3100404, new LookupEntryId( "Hydra" ) );
            EntryIdData.Add( 3100803, new LookupEntryId( "Inciter" ) );
            EntryIdData.Add( 3100801, new LookupEntryId( "Marauder" ) );
            EntryIdData.Add( 3100403, new LookupEntryId( "Medusa" ) );
            EntryIdData.Add( 3100406, new LookupEntryId( "Orthrus" ) );
            EntryIdData.Add( 3100804, new LookupEntryId( "Regenerator" ) );
            EntryIdData.Add( 3100700, new LookupEntryId( "Scavengers" ) );
            EntryIdData.Add( 3101000, new LookupEntryId( "TG_DataScan" ) );
            EntryIdData.Add( 3101100, new LookupEntryId( "TG_Pod" ) );
            EntryIdData.Add( 3101200, new LookupEntryId( "TG_Transmitter" ) );
            EntryIdData.Add( 3100405, new LookupEntryId( "Wrecked_Interceptor" ) );
            EntryIdData.Add( 3100805, new LookupEntryId( "Wrecked_Scout" ) );
        }

        public static ThargoidItem Lookup ( long? entryId, string edname )
        {
            ThargoidItem item = new ThargoidItem();
            item = LookupByEntryId( entryId );

            if ( !item.exists )
            {
                item = LookupByName( edname );
            }

            return item;
        }

        public static ThargoidItem LookupByEntryId ( long? entryId )
        {
            ThargoidItem item = new ThargoidItem();

            if ( entryId != null )
            {
                if ( EntryIdData.ContainsKey( (long)entryId ) )
                {
                    LookupEntryId data = EntryIdData[ (long)entryId ];

                    item.name = rmThargoidName.GetString( data.edname );
                    item.description = rmThargoidDesc.GetString( data.edname );

                    item.SetExists( true );
                }
            }

            return item;
        }

        public static ThargoidItem LookupByName ( string edname )
        {
            ThargoidItem item = new ThargoidItem();

            if ( edname != "" )
            {
                item.name = rmThargoidName.GetString( edname );
                item.description = rmThargoidDesc.GetString( edname );

                item.SetExists( true );
            }

            // If the above fails to find an entry then we return the empty item
            // We could modify the item to say that we could not find an entry as well
            return item;
        }
    }
}
