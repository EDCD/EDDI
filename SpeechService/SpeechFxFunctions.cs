using System;
using System.Linq;

namespace EddiSpeechService
{
    internal static class SpeechFxFunctions
    {
        /// <summary>
        /// Asymmetric sigmoid function
        /// </summary>
        public static float AsymSigmoid ( float x, float center, float steepL, float steepR )
        {
            if ( x < center )
            {
                return 0.5f * (float)Math.Pow( x / center, steepL );
            }

            return 1f - ( 0.5f * (float)Math.Pow( ( 1f - x ) / ( 1f - center ), steepR ) );
        }

        public static int Clamp ( int value, int min, int max )
        {
            if ( value < min )
            { return min; }

            if ( value > max )
            { return max; }

            return value;
        }

        public static float Clamp ( float value, float min, float max )
        {
            if ( value < min )
            { return min; }

            if ( value > max )
            { return max; }

            return value;
        }

        public static float DecibalsToLinear ( float dbValue )
        {
            return (float)Math.Pow( 10.0, dbValue / 20.0 );
        }

        public static float EaseInPow ( float t, float p )
        {
            // Ease-in curve t^p on [0..1], clamped for safety
            return (float)Math.Pow( Clamp( t, 0f, 1f ), p );
        }

        public static float Hash0To1 ( int[] parameters, int userSeed = 0 )
        {
            unchecked
            {
                // 32-bit mix (stable across runs)
                var x = 2166136261u; // FNV offset basis
                foreach ( var param in parameters.Append( userSeed ) )
                {
                    x = ( x ^ (uint)param ) * 16777619u;
                }

                // avalanche
                x ^= x >> 16;
                x *= 0x7FEB352Du;
                x ^= x >> 15;
                x *= 0x846CA68Bu;
                x ^= x >> 16;

                // return hash value between 0 and 1 inclusive
                return ( x & 0x00FFFFFFu ) / 16777216f;
            }
        }

        /// <summary>
        /// Linear interpolation between two values based on a given weight
        /// </summary>
        public static float LinearInterpolate ( float a, float b, float t )
        {
            return a + ( ( b - a ) * t );
        }

        public static float LinearRescale ( float x, float inMin, float inMax )
        {
            return Clamp( ( x - inMin ) / ( inMax - inMin ), 0f, 1f );
        }

        /// <summary>
        /// Linear scaling helper for fxCount‑based parameters.
        /// </summary>
        public static float LinearScale ( float fxLevel, float minFx, float maxFx, float minVal, float maxVal )
        {
            var t = Clamp( ( fxLevel - minFx ) / ( maxFx - minFx ), 0f, 1f );
            return minVal + ( ( maxVal - minVal ) * t );
        }

        /// <summary>
        /// Easier to tune than a true sigmoid.
        /// Applies a Windowed cubic Hermite polynomial to create a smooth threshold between points defined using a center and width.
        /// </summary>
        public static float SoftStep ( float x, float center, float width )
        {
            // width controls how wide the transition band is
            var t = Clamp( ( x - ( center - ( width * 0.5f ) ) ) / width, 0f, 1f );
            return t * t * ( 3f - ( 2f * t ) ); // cubic Hermite
        }

        /// <summary>
        /// Applies a piecewise Cubic Hermite Interpolating Polynomial to create a smooth spline between data points.
        /// Local control: Y[i] only affects its two neighboring segments.
        /// It can overshoot when adjacent points differ sharply
        /// Natural “ease-in/ease-out” transitions between control points.
        /// </summary>
        public static float SmoothSpline ( float[] xArray, float[] yArray, float x )
        {
            if ( xArray is null )
            {
                throw new ArgumentNullException( nameof(xArray), @"Array cannot be null." );
            }
            if ( yArray is null )
            {
                throw new ArgumentNullException( nameof( yArray ), @"Array cannot be null." );
            }
            if ( xArray.Length != yArray.Length )
            {
                throw new InvalidOperationException( $"Spline input arrays must have equal length (got xArray={xArray.Length}, yArray={yArray.Length})." );
            }
            if ( xArray.Length < 2 )
            {
                throw new InvalidOperationException( "Spline requires at least two control points." );
            }

            // Enforce monotonic X (sort if necessary)
            if ( xArray[ 0 ] > xArray[ xArray.Length - 1 ] )
            {
                Array.Reverse( xArray );
                Array.Reverse( yArray );
            }

            var n = xArray.Length;

            if ( x <= xArray[ 0 ] )
            {
                return yArray[ 0 ];
            }

            if ( x >= xArray[ n - 1 ] )
            {
                return yArray[ n - 1 ];
            }

            var i = 0;
            while ( i < (n - 1) && x > xArray[ i + 1 ] )
            {
                i++;
            }

            var h = xArray[i + 1] - xArray[i];
            if ( h <= 0f )
            {
                throw new InvalidOperationException( "Non-monotonic X array." );
            }

            var t = (x - xArray[i]) / h;
            var t2 = t * t;
            var t3 = t2 * t;

            var m0 = i == 0
                ? (yArray[1] - yArray[0]) / (xArray[1] - xArray[0])
                : (yArray[i + 1] - yArray[i - 1]) / (xArray[i + 1] - xArray[i - 1]);
            var m1 = i == (n - 2)
                ? (yArray[n - 1] - yArray[n - 2]) / (xArray[n - 1] - xArray[n - 2])
                : (yArray[i + 2] - yArray[i]) / (xArray[i + 2] - xArray[i]);

            var h00 = (2f * t3) - (3f * t2) + 1f;
            var h10 = t3 - (2f * t2) + t;
            var h01 = (-2f * t3) + (3f * t2);
            var h11 = t3 - t2;

            return ( h00 * yArray[ i ] ) + ( h * h10 * m0 ) + ( h01 * yArray[ i + 1 ] ) + ( h * h11 * m1 );
        }

        public static float SmoothSplineClamped ( float[] xs, float[] ys, float x )
        {
            var y = SmoothSpline(xs, ys, x );

            var i = 0;
            while ( i < ( xs.Length - 2 ) && x > xs[ i + 1 ] )
            {
                i++;
            }

            var lo = Math.Min(ys[i], ys[i + 1]);
            var hi = Math.Max(ys[i], ys[i + 1]);
            return Clamp( y, lo, hi );
        }

        /// <summary>
        /// Applies a Single cubic Hermite polynomial to create an S shaped curve with zero slope at either edge and max slope at the center.
        /// Can be used for "fade in/out" or "ease" transitions.
        /// </summary>
        public static float SmoothStep ( float edge0, float edge1, float x )
        {
            var t = Clamp( ( x - edge0 ) / ( edge1 - edge0 ), 0f, 1f );
            return t * t * ( 3f - ( 2f * t ) );
        }

        public static float SoftClipCeiling ( float x, float ceiling )
        {
            var ax = Math.Abs(x);
            if ( ax <= ceiling )
            {
                return x;
            }

            var sign = Math.Sign(x);
            var over = (ax - ceiling) / (1f - ceiling); // 0..inf
            var shaped = (float)Math.Tanh(over);        // 0..1
            return sign * ( ceiling + ( ( 1f - ceiling ) * shaped ) );
        }
    }

    /// <summary>
    /// Applies a piecewise Cubic Hermite Interpolating Polynomial to create a smooth spline between data points.
    /// Local control: Y[i] only affects its two neighboring segments.
    /// It is monotonic and will not overshoot when adjacent points differ sharply (but can flatten)
    /// Natural “ease-in/ease-out” transitions between control points.
    /// </summary>
    public class SmoothMonotonicSpline
    {
        private readonly float[] _x;
        private readonly float[] _y;
        private readonly float[] _h;
        private readonly float[] _m;

        public SmoothMonotonicSpline ( float[] x, float[] y )
        {
            if ( x.Length != y.Length )
            {
                throw new ArgumentException( "X and Y arrays must have the same length." );
            }

            if ( x.Length < 2 )
            {
                throw new ArgumentException( "At least two data points are required." );
            }

            var n = x.Length;
            _x = (float[])x.Clone();
            _y = (float[])y.Clone();
            _h = new float[ n - 1 ];
            _m = new float[ n ];

            ComputeSplineCoefficients();
        }

        private void ComputeSplineCoefficients ()
        {
            var n = _x.Length;
            var d = new float[n - 1];

            for ( var i = 0; i < (n - 1); i++ )
            {
                _h[ i ] = _x[ i + 1 ] - _x[ i ];
                d[ i ] = ( _y[ i + 1 ] - _y[ i ] ) / _h[ i ];
            }

            if ( n == 2 )
            {
                _m[ 0 ] = _m[ 1 ] = d[ 0 ];
                return;
            }

            for ( var i = 1; i < (n - 1); i++ )
            {
                if ( d[ i - 1 ] == 0f || d[ i ] == 0f )
                    _m[ i ] = 0f;
                else
                {
                    var w1 = (2f * _h[i]) + _h[i - 1];
                    var w2 = _h[i] + (2f * _h[i - 1]);
                    _m[ i ] = ( w1 + w2 ) / ( ( w1 / d[ i - 1 ] ) + ( w2 / d[ i ] ) );
                }
            }

            // Endpoint slopes (Fritsch–Carlson)
            if ( n >= 4 )
            {
                _m[ 0 ] = ComputeEndpointSlope( d[ 0 ], d[ 1 ], _h[ 0 ], _h[ 1 ] );
                _m[ n - 1 ] = ComputeEndpointSlope( d[ n - 2 ], d[ n - 3 ], _h[ n - 2 ], _h[ n - 3 ] );
            }
            else
            {
                // For 3-point case, approximate endpoints using the nearest slope
                _m[ 0 ] = d[ 0 ];
                _m[ n - 1 ] = d[ n - 2 ];
            }
        }

        private static float ComputeEndpointSlope ( float d0, float d1, float h0, float h1 )
        {
            var m = ((((2f * h0) + h1) * d0) - (h0 * d1)) / (h0 + h1);
            if ( m < 0f || d0 == 0f )
            {
                return 0f;
            }
            if ( m > (3f * d0) )
            {
                return 3f * d0;
            }
            return m;
        }

        public float Evaluate ( float x )
        {
            var n = _x.Length;

            if ( x <= _x[ 0 ] )
            {
                return _y[ 0 ];
            }

            if ( x >= _x[ n - 1 ] )
            {
                return _y[ n - 1 ];
            }

            var i = 0;
            while ( i < (n - 1) && x > _x[ i + 1 ] )
            {
                i++;
            }

            var h = _h[i];
            var t = (x - _x[i]) / h;
            var t2 = t * t;
            var t3 = t2 * t;

            var y0 = _y[i];
            var y1 = _y[i + 1];
            var m0 = _m[i];
            var m1 = _m[i + 1];

            var h00 = (2f * t3) - (3f * t2) + 1f;
            var h10 = t3 - (2f * t2) + t;
            var h01 = (-2f * t3) + (3f * t2);
            var h11 = t3 - t2;

            return ( h00 * y0 ) + ( h * h10 * m0 ) + ( h01 * y1 ) + ( h * h11 * m1 );
        }
    }
}