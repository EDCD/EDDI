using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class ScanOrganicSchema : ISchema
    {
        public List<string> edTypes => new List<string> { "ScanOrganic" };

        public bool Handle ( string edType, ref IDictionary<string, object> data, EDDNState eddnState )
        {
            try
            {
                if ( !edTypes.Contains( edType ) ) { return false; }
                if ( eddnState?.Location is null || eddnState.GameVersion is null ) { return false; }
                if ( !eddnState.Location.CheckLocationData( edType, data ) ) { return false; }

                // No personal data to remove

                // Omit the `Analyse` scan type
                if ( data.TryGetValue( "ScanType", out var scanType ) && scanType.ToString() == "Analyse" )
                {
                    return false;
                }

                // Rename `Body` to `BodyID` to match EDDN and most event conventions
                if ( data.TryGetValue( "Body", out var body ) && body is int bodyID )
                {
                    data.Remove( "Body" );
                    data.Add( "BodyID", bodyID );
                }

                // Apply data augments
                data = eddnState.Location.AugmentBodyNameID( data );
                data = eddnState.Location.AugmentBodyLatLong( data, 60, true );
                data = eddnState.GameVersion.AugmentVersion( data );

                EDDNSender.SendToEDDN( "https://eddn.edcd.io/schemas/scanorganic/1", data, eddnState );
                return true;
            }
            catch ( Exception e )
            {
                Logging.Error( $"{GetType().Name} failed to handle journal data.", e );
                return false;
            }
        }
    }
}