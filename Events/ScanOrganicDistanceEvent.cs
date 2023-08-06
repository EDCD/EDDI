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

        [PublicAPI("The current sample distance")]
        public int sample_distance { get; private set; }

        [PublicAPI( "Player has moved inside the sample distance for sample 1" )]
        public bool sample1_inside { get; private set; }

        [PublicAPI( "Player has moved outside the sample distance for sample 1" )]
        public bool sample1_outside { get; private set; }

        [PublicAPI( "Player has moved inside the sample distance for sample 2" )]
        public bool sample2_inside { get; private set; }

        [PublicAPI( "Player has moved outside the sample distance for sample 2" )]
        public bool sample2_outside { get; private set; }

        //[PublicAPI( "The current latitude." )]
        //public decimal? latitude { get; private set; }

        //[PublicAPI( "The current longitude." )]
        //public decimal? longitude { get; private set; }

        //[PublicAPI( "Do we have Longitude and Latitude?" )]
        //public int reset { get; private set; }

        //[PublicAPI( "Are we near the surface of a planet?" )]
        //public int near_surface { get; private set; }

        //[PublicAPI( "Have we entered supercruise?" )]
        //public int supercruise { get; private set; }

        //[PublicAPI( "Have we entered hyperspace?" )]
        //public int hyperspace { get; private set; }

        // Not intended to be user facing

        public ScanOrganicDistanceEvent ( DateTime timestamp, int distance, int state1, int state2 ) : base( timestamp, NAME )
        {
            this.sample_distance = distance;

            if ( state1 == 1 )          // Transitioned to inside sample distance radius for Sample 1
            {
                this.sample1_inside = true;
            }
            else if ( state1 == 2 )     // Transitioned to outside sample distance radius for Sample 1
            {
                this.sample1_outside = true;
            }

            if ( state2 == 1 )          // Transitioned to inside sample distance radius for Sample 2
            {
                this.sample2_inside = true;
            }
            else if ( state2 == 2 )     // Transitioned to outside sample distance radius for Sample 2
            {
                this.sample2_outside = true;
            }

            //this.near_surface = near_surface;
            //this.near_surface = 2;
            //switch ( near_surface )
            //{
            //    case false:
            //        this.near_surface = 0;
            //        break;
            //    case true:
            //        this.near_surface = 1;
            //        break;
            //}

            ////this.supercruise = supercruise;
            //this.supercruise = 2;
            //switch ( supercruise )
            //{
            //    case false:
            //        this.supercruise = 0;
            //        break;
            //    case true:
            //        this.supercruise = 1;
            //        break;
            //}

            ////this.hyperspace = hyperspace;
            //this.hyperspace = 2;
            //switch ( hyperspace )
            //{
            //    case false:
            //        this.hyperspace = 0;
            //        break;
            //    case true:
            //        this.hyperspace = 1;
            //        break;
            //}

            ////this.reset = ( !near_surface || hyperspace );
            //this.reset = 2;
            //if ( this.near_surface == 0 || (this.supercruise == 0 && this.hyperspace == 1 ) )
            //{
            //    this.reset = 1;
            //}
            //else
            //{
            //    this.reset = 0;
            //}

            //this.latitude = latitude;
            //this.longitude = longitude;

        }
    }
}
