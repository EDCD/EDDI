using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class Guardian : ResourceBasedLocalizedEDName<Guardian>
    {
        public static readonly IDictionary<string, long?> GUARDIANS = new Dictionary<string, long?>();
        public static readonly IDictionary<long, Guardian> ENTRYIDS = new Dictionary<long, Guardian>();

        static Guardian ()
        {
            resourceManager = Properties.Guardian.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new Guardian( edname );
        }

        public static readonly Guardian Guardian_Beacons = new Guardian( "Guardian_Beacons", 3200800, null );
        public static readonly Guardian Guardian_Data_Logs = new Guardian( "Guardian_Data_Logs", 3200200, null );
        public static readonly Guardian Guardian_Pylon = new Guardian( "Guardian_Pylon", 3200400, null );
        public static readonly Guardian Guardian_Sentinel = new Guardian( "Guardian_Sentinel", 3200600, null );
        public static readonly Guardian Guardian_Terminal = new Guardian( "Guardian_Terminal", 3200300, null );
        public static readonly Guardian Relic_Tower = new Guardian( "Relic_Tower", 3200500, null );

        public long? entryID;
        public long? value;
        public string description => Properties.GuardianDesc.ResourceManager.GetString( edname );

        // dummy used to ensure that the static constructor has run
        public Guardian () : this( "" )
        { }

        private Guardian ( string edname ) : base( edname, edname )
        { }

        private Guardian ( string edname, long? entryID, long? value ) : base( edname, edname )
        {
            this.entryID = entryID;
            this.value = value;
        }

        /// <summary>
        /// Try getting data from the entryid first, then use edname as a fallback
        /// </summary>
        public static Guardian Lookup ( long? entryId, string edName )
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
                    Logging.Error( $"Duplicate EntryID value {entryId} in {nameof( Guardian )}.", e );
                }
                else if ( AllOfThem.All( a => a.entryID != entryId ) )
                {
                    Logging.Error( $"Unknown EntryID value {entryId} with edname {edName} in {nameof( Guardian )}.", e );
                }
            }

            return FromEDName( edName ) ?? new Guardian( edName ); // No match.
        }
    }
}
