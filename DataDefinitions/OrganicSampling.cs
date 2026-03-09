using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class OrganicSampling
    {
        private class OrganicSample
        {
            public readonly ulong systemAddress;
            public readonly int bodyId;
            public readonly decimal latitude;
            public readonly decimal longitude;

            public OrganicSample ( ulong systemAddress, int bodyId, decimal latitude, decimal longitude )
            {
                this.systemAddress = systemAddress;
                this.bodyId = bodyId;
                this.latitude = latitude;
                this.longitude = longitude;
            }
        }

        public Organic organic { get; set; }
        private List<OrganicSample> _samples { get; } = new();
        public bool wasNearPriorSample { get; set; }

        public void LogSample ( Organic loggedOrganic, ulong systemAddress, int bodyId, decimal latitude, decimal longitude )
        {
            if ( organic?.variant.edname != loggedOrganic?.variant.edname )
            {
                organic = loggedOrganic;
                _samples.Clear();
            }
            _samples.Add( new OrganicSample( systemAddress, bodyId, latitude, longitude ) );
            wasNearPriorSample = true;
        }

        public bool IsNearby ( ulong systemAddress, int bodyId, decimal? planetRadiusMeters, decimal? latitude, decimal? longitude )
        {
            foreach ( var sample in _samples )
            {
                if ( sample.systemAddress == systemAddress && sample.bodyId == bodyId )
                {
                    var distanceMeters = Functions.SurfaceDistanceKm( planetRadiusMeters, sample.latitude, sample.longitude, latitude, longitude ) * 1000;
                    if ( distanceMeters < organic?.minimumDistanceMeters )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Reset ()
        {
            organic = null;
            _samples.Clear();
            wasNearPriorSample = false;
        }
    }
}