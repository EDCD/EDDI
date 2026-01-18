/*
   Derived from: 
   https://web.archive.org/web/20230127225616/http://disc.thargoid.space/ID64, 
   https://github.com/klightspeed/EliteDangerousRegionMap/, 
   https://bitbucket.org/Esvandiary/edts/src/develop/edtslib/

   MIT License
   
   Copyright (c) 2020 Ben Peddell
   
   Permission is hereby granted, free of charge, to any person obtaining a copy
   of this software and associated documentation files (the "Software"), to deal
   in the Software without restriction, including without limitation the rights
   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
   copies of the Software, and to permit persons to whom the Software is
   furnished to do so, subject to the following conditions:
   
   The above copyright notice and this permission notice shall be included in all
   copies or substantial portions of the Software.
   
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
   SOFTWARE. 
 */

using Newtonsoft.Json;
using System;

namespace EddiDataDefinitions
{
    public class StarSystemId64
    {
        [Utilities.PublicAPI( "Boxel data, as an object." ), JsonIgnore ]
        public Boxel boxel { get; set; }

        // The masscode is the letter conversion of the size class & yields 0 for H up to 7 for A
        [ Utilities.PublicAPI(
              "The mass code for the star system (ranging from 'H' to 'A' with 'H' tending to contain high mass star systems and 'A' tending to contain low mass systems)." ),
          JsonIgnore ]
        public string massCode => "HGFEDCBA"[ sizeClass ].ToString();

        [ Utilities.PublicAPI( "Region data, as an object." ), JsonIgnore ]
        public StarSystemRegion region { get; set; }

        // The size class is stored in the lowest 3 bits of the ID64.
        [ Utilities.PublicAPI(
              "The integer equivalent of the mass code for the star system (ranging from 0 to 7 with 0 tending to contain high mass star systems and 7 tending to contain low mass systems)." ),
          JsonIgnore ]
        public int sizeClass => (int)( _systemAddress & 7 );

        private ulong _systemAddress { get; set; }

        #region Subclasses

        public class Boxel
        {
            [ Utilities.PublicAPI( "Boxel X coordinate." ), JsonIgnore ]
            public int boxelX { get; set; }

            [ Utilities.PublicAPI( "Boxel Y coordinate." ), JsonIgnore ]
            public int boxelY { get; set; }

            [ Utilities.PublicAPI( "Boxel Z coordinate." ), JsonIgnore ]
            public int boxelZ { get; set; }

            [ Utilities.PublicAPI( "Boxel width in light years." ), JsonIgnore ]
            public int boxelWidthLy { get; set; }

            [ Utilities.PublicAPI( "The numeric boxel ID." ), JsonIgnore ]
            public int boxelId => boxelX + ( boxelY << 7 ) + ( boxelZ << 14 );

            public Boxel ( int x, int y, int z, int masscode )
            {
                boxelX = x;
                boxelY = y;
                boxelZ = z;
                boxelWidthLy = Convert.ToInt32( 1280 / Math.Pow( 2, masscode ) );
            }
        }

        #endregion

        public StarSystemId64 ( ulong systemAddress )
        {
            _systemAddress = systemAddress;
            var id64 = Convert.ToInt64( systemAddress );
            var x = (int)( ( ( ( id64 >> ( 30 - ( sizeClass * 2 ) ) ) & ( (int)0x3FFF >> sizeClass ) ) << sizeClass ) *
                           10 ) + StarSystemRegion.x0;
            var y = (int)( ( ( ( id64 >> ( 17 - sizeClass ) ) & ( (int)0x1FFF >> sizeClass ) ) << sizeClass ) * 10 ) +
                    StarSystemRegion.y0;
            var z = (int)( ( ( ( id64 >> 3 ) & ( (int)0x3FFF >> sizeClass ) ) << sizeClass ) * 10 ) + StarSystemRegion.z0;

            boxel = new Boxel( x, y, z, sizeClass );
            region = StarSystemRegion.FromXZCoordinates( x, z );
        }
    }
}