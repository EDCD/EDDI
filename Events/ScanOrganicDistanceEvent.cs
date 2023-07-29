using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ScanOrganicDistanceEvent : Event
    {
        public const string NAME = "Scan organic distance event";
        public const string DESCRIPTION = "Triggered when commander location updated and updates context distances from previous scans.";
        public const string SAMPLE = @"{ ""timestamp"":""2023-07-22T04:01:18Z"", ""event"":""ScanOrganic"", ""ScanType"":""Log"", ""Genus"":""$Codex_Ent_Shrubs_Genus_Name;"", ""Genus_Localised"":""Frutexa"", ""Species"":""$Codex_Ent_Shrubs_05_Name;"", ""Species_Localised"":""Frutexa Fera"", ""Variant"":""$Codex_Ent_Shrubs_05_F_Name;"", ""Variant_Localised"":""Frutexa Fera - Green"", ""SystemAddress"":34542299533283, ""Body"":42 }";

        [PublicAPI( "The current latitude." )]
        public decimal? latitude { get; private set; }

        [PublicAPI( "The current longitude." )]
        public decimal? longitude { get; private set; }

        [PublicAPI( "Do we have Longitude and Latitude?" )]
        public int reset { get; private set; }

        [PublicAPI( "Are we near the surface of a planet?" )]
        public int near_surface { get; private set; }

        [PublicAPI( "Have we entered supercruise?" )]
        public int supercruise { get; private set; }

        [PublicAPI( "Have we entered hyperspace?" )]
        public int hyperspace { get; private set; }

        // Not intended to be user facing

        public ScanOrganicDistanceEvent ( DateTime timestamp, bool near_surface, bool supercruise, bool hyperspace, decimal? latitude, decimal? longitude ) : base( timestamp, NAME )
        {
            //this.near_surface = near_surface;
            this.near_surface = 2;
            switch ( near_surface )
            {
                case false:
                    this.near_surface = 0;
                    break;
                case true:
                    this.near_surface = 1;
                    break;
            }

            //this.supercruise = supercruise;
            this.supercruise = 2;
            switch ( supercruise )
            {
                case false:
                    this.supercruise = 0;
                    break;
                case true:
                    this.supercruise = 1;
                    break;
            }

            //this.hyperspace = hyperspace;
            this.hyperspace = 2;
            switch ( hyperspace )
            {
                case false:
                    this.hyperspace = 0;
                    break;
                case true:
                    this.hyperspace = 1;
                    break;
            }

            //this.reset = ( !near_surface || hyperspace );
            this.reset = 2;
            if ( this.near_surface == 0 || (this.supercruise == 0 && this.hyperspace == 1 ) )
            {
                this.reset = 1;
            }
            else
            {
                this.reset = 0;
            }

            this.latitude = latitude;
            this.longitude = longitude;

        }
    }
}
