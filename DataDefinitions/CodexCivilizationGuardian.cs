using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class CodexCivilizationGuardian : ResourceBasedLocalizedEDName<CodexCivilizationGuardian>
    {
        public static readonly IDictionary<string, long?> GUARDIANS = new Dictionary<string, long?>();
        public static readonly IDictionary<long, CodexCivilizationGuardian> ENTRYIDS = new Dictionary<long, CodexCivilizationGuardian>();

        static CodexCivilizationGuardian ()
        {
            resourceManager = Properties.CodexCivilizationGuardian.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new CodexCivilizationGuardian( edname );
        }

        public static readonly CodexCivilizationGuardian Guardian_Beacons = new CodexCivilizationGuardian( "Guardian_Beacons", 3200800 );
        public static readonly CodexCivilizationGuardian Guardian_Data_Logs = new CodexCivilizationGuardian( "Guardian_Data_Logs", 3200200 );
        public static readonly CodexCivilizationGuardian Guardian_Pylon = new CodexCivilizationGuardian( "Guardian_Pylon", 3200400 );
        public static readonly CodexCivilizationGuardian Guardian_Sentinel = new CodexCivilizationGuardian( "Guardian_Sentinel", 3200600 );
        public static readonly CodexCivilizationGuardian Guardian_Terminal = new CodexCivilizationGuardian( "Guardian_Terminal", 3200300 );
        public static readonly CodexCivilizationGuardian Relic_Tower = new CodexCivilizationGuardian( "Relic_Tower", 3200500 );

        public long? entryID;

        [PublicAPI]
        public string localizedDescription => Properties.CodexCivilizationGuardianDesc.ResourceManager.GetString( edname );

        // dummy used to ensure that the static constructor has run
        public CodexCivilizationGuardian () : this( "" )
        { }

        private CodexCivilizationGuardian ( string edname ) : base( edname, edname )
        { }

        private CodexCivilizationGuardian ( string edname, long? entryID ) : base( edname, edname )
        {
            this.entryID = entryID;
        }

        /// <summary>
        /// Try getting data from the entryid first, then use edname as a fallback
        /// </summary>
        public static CodexCivilizationGuardian Lookup ( long? entryId, string edName )
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
                    Logging.Error( $"Duplicate EntryID value {entryId} in {nameof( CodexCivilizationGuardian )}.", e );
                }
                else if ( AllOfThem.All( a => a.entryID != entryId ) )
                {
                    Logging.Error( $"Unknown EntryID value {entryId} with edname {edName} in {nameof( CodexCivilizationGuardian )}.", e );
                }
            }

            return FromEDName( edName ) ?? new CodexCivilizationGuardian( edName ); // No match.
        }
    }
}