using System;

namespace EddiSpeechService.SampleProviders.ChorusHelpers
{
    internal static class Curve
    {
        // Ensemble effect parameter curves
        private static readonly SmoothMonotonicSpline BodyGainDbSpline =
            new SmoothMonotonicSpline( Constants.BodyGainSplineX, Constants.BodyGainSplineY );
        private static readonly SmoothMonotonicSpline CenterDelaySpline =
            new SmoothMonotonicSpline(Constants.CenterDelaySplineX, Constants.CenterDelaySplineY);
        private static readonly SmoothMonotonicSpline ChorusDepthSpline =
            new SmoothMonotonicSpline(Constants.ChorusDepthSplineX, Constants.ChorusDepthSplineY);
        private static readonly SmoothMonotonicSpline DryGainSpline =
            new SmoothMonotonicSpline(Constants.DryGainFx, Constants.DryGainY);
        private static readonly SmoothMonotonicSpline DynamicDetuneSpline =
            new SmoothMonotonicSpline( Constants.DynamicDetuneMixFx, Constants.DynamicDetuneMixY );
        private static readonly SmoothMonotonicSpline GhostDepthSpline = 
            new SmoothMonotonicSpline(Constants.GhostDepthSplineX, Constants.GhostDepthSplineY);
        private static readonly SmoothMonotonicSpline GhostWeightSpline = 
            new SmoothMonotonicSpline(Constants.GhostWeightSplineX, Constants.GhostWeightSplineY);
        private static readonly SmoothMonotonicSpline LfoRateSpline =
            new SmoothMonotonicSpline(Constants.LfoRateScaleX, Constants.LfoRateScaleY);
        private static readonly SmoothMonotonicSpline ModDetuneJitterSpline =
            new SmoothMonotonicSpline(Constants.DetuneJitterFx, Constants.DetuneJitterY);
        private static readonly SmoothMonotonicSpline ModPhaseJitterSpline =
            new SmoothMonotonicSpline(Constants.PhaseJitterFx, Constants.PhaseJitterY);
        private static readonly SmoothMonotonicSpline ResonanceAllPassGainSpline =
            new SmoothMonotonicSpline(Constants.ResonanceAllPassFx, Constants.ResonanceAllPassGainY);
        public static readonly SmoothMonotonicSpline StepSpline = 
            new SmoothMonotonicSpline( Constants.StepX, Constants.StepY );
        private static readonly SmoothMonotonicSpline WetGainSpline =
            new SmoothMonotonicSpline(Constants.WetGainFx, Constants.WetGainY);
        private static readonly SmoothMonotonicSpline WetHpfHzSpline =
            new SmoothMonotonicSpline( Constants.WetHpfFx, Constants.WetHpfHz );
        private static readonly SmoothMonotonicSpline WetLpfHzSpline =
            new SmoothMonotonicSpline(Constants.WetLpfFx, Constants.WetLpfHz);
        
        // Voice activation curves
        public static readonly SmoothMonotonicSpline Voice0ActSpline = 
            new SmoothMonotonicSpline(Constants.VoiceActX, Constants.Voice0ActY);
        public static readonly SmoothMonotonicSpline Voice1ActSpline = 
            new SmoothMonotonicSpline(Constants.VoiceActX, Constants.Voice1ActY);
        public static readonly SmoothMonotonicSpline Voice2ActSpline = 
            new SmoothMonotonicSpline(Constants.VoiceActX, Constants.Voice2ActY);
        public static readonly SmoothMonotonicSpline Voice3ActSpline = 
            new SmoothMonotonicSpline(Constants.VoiceActX, Constants.Voice3ActY);
        public static readonly SmoothMonotonicSpline Voice4ActSpline = 
            new SmoothMonotonicSpline(Constants.VoiceActX, Constants.Voice4ActY);
        public static readonly SmoothMonotonicSpline Voice5ActSpline = 
            new SmoothMonotonicSpline(Constants.VoiceActX, Constants.Voice5ActY);

        internal struct ModStruct
        {
            public float DetuneJitter; // fractional ± multiplier
            public float PhaseJitterRad; // radians
        }

        internal struct GhostStruct
        {
            public float GhostWeight; // 0..1 of paired weight
            public float GhostDepthFactor; // 0..1 of paired depth
        }

        public static ModStruct ModulationProfile ( float fxLevel )
        {
            return new ModStruct
            {
                DetuneJitter = Functions.Clamp( ModDetuneJitterSpline.Evaluate( fxLevel ), 0f, Constants.ModDetuneJitterMax ),
                PhaseJitterRad = Functions.Clamp( ModPhaseJitterSpline.Evaluate( fxLevel ), 0f, Constants.ModPhaseJitterMaxRad ),
            };
        }

        public static float BodyGainDb ( float fxLevel )
        {
            return BodyGainDbSpline.Evaluate( fxLevel );
        }

        public static float CenterDelayMs ( float fxLevel )
        {
            return CenterDelaySpline.Evaluate( fxLevel );
        }

        public static float ChorusDepth ( float fxLevel )
        {
            return ChorusDepthSpline.Evaluate( fxLevel );
        }

        public static float DetuneSpan ( float fxLevel )
        {
            var x = Functions.Clamp( fxLevel / 100f, 0f, 1f );
            return Functions.LinearInterpolate( Constants.DetuneSpanMin, Constants.DetuneSpanMax,
                Functions.EaseInPow( x, Constants.DetuneEasePower ) );
        }

        public static float DryGain ( float fxLevel )
        {
            return DryGainSpline.Evaluate( fxLevel );
        }

        public static float DynamicDetuneMix ( float fxLevel )
        {
            return DynamicDetuneSpline.Evaluate( fxLevel );
        }

        public static float Feedback ( float master, float fxLevel )
        {
            // Early fade gives slight taper below 0.1 master; constant thereafter.
            var t = Functions.SoftStep( fxLevel, Constants.FeedbackCenter, Constants.FeedbackWidth );
            var fb = Functions.LinearInterpolate( Constants.FeedbackMin, Constants.FeedbackMax, t );
            
            // Taper to reduce late-range cancellation
            var late = Functions.LinearRescale( master, Constants.FeedbackLateStart, Constants.FeedbackLateEnd );
            var lateAtten = Functions.LinearInterpolate( 1.00f, Constants.FeedbackLateMinFactor, late );
            var baseVal = Math.Min( fb * lateAtten, Constants.FeedbackAbsoluteMax );
            
            return baseVal;
        }

        public static float FeedbackGainDb ( float master )
        {
            var shelfNorm = Functions.LinearRescale( master, Constants.FeedbackShelfStart, Constants.FeedbackShelfEnd );
            var baseCut = Functions.LinearInterpolate( Constants.FeedbackShelfGainEarlyDb, Constants.FeedbackShelfGainLateDb, shelfNorm );
            // As feedback rises, open the filter slightly to avoid "buzz" and let resonance bloom
            return baseCut * ( 1f + ( 0.15f * master ) );
        }

        public static float FeedbackLpfCutoff ( float master )
        {
            return Functions.LinearInterpolate( Constants.FeedbackLpfLow, Constants.FeedbackLpfHigh, master );
        }

        public static GhostStruct GhostProfile ( float fxLevel )
        {
            var ghostWeight = GhostWeightSpline.Evaluate(fxLevel);
            var ghostDepth = GhostDepthSpline.Evaluate(fxLevel);

            return new GhostStruct
            {
                GhostWeight = ghostWeight,
                GhostDepthFactor = ghostDepth
            };
        }

        public static float LfoJitterScale ( float fxLevel )
        {
            return Functions.SmoothSpline( Constants.LfoJitterScaleX, Constants.LfoJitterScaleY, fxLevel );
        }

        public static float LfoRateScale ( float fxLevel )
        {
            return LfoRateSpline.Evaluate( fxLevel );
        }

        public static float MetallicGainDb ( float fxLevel )
        {
            var baseDb = Functions.SmoothSpline( Constants.MetallicGainSplineX, Constants.MetallicGainSplineY, fxLevel );
            var t = Functions.SmoothStep(Constants.MetallicGainAttenuationStart, Constants.MetallicGainAttenuationEnd, fxLevel);
            var atten = t * Constants.MetallicGainAttenuationMax;
            return baseDb - atten;
        }

        public static float MicroCombScale ( float fxLevel )
        {
            return Functions.SmoothSpline( Constants.MicroCombScaleX, Constants.MicroCombScaleY, fxLevel );
        }

        public static float MixMakeupDb ( float fxLevel )
        {
            var x = Constants.MakeupDbFx;
            var y = Constants.MakeupDbY;

            // clamp endpoints
            if ( fxLevel <= x[ 0 ] )
            {
                return y[ 0 ];
            }

            if ( fxLevel >= x[ x.Length - 1 ] )
            {
                return y[ y.Length - 1 ];
            }

            // find segment
            var i = 0;
            while ( i < (x.Length - 1) && fxLevel > x[ i + 1 ] )
            { i++; }

            var raw = Functions.SmoothSpline( x, y, fxLevel );

            // clamp segment to prevent overshoots
            var lo = Math.Min( y[i], y[i + 1] );
            var hi = Math.Max( y[i], y[i + 1] );
            return Math.Max( lo, Math.Min( hi, raw ) );
        }


        public static float ResonanceAllPassGain ( float fxLevel )
        {
            return ResonanceAllPassGainSpline.Evaluate( fxLevel );
        }

        public static float ShimmerGainDb ( float fxLevel )
        {
            return Functions.SmoothSpline( Constants.ShimmerSplineX, Constants.ShimmerSplineY, fxLevel );
        }

        public static float WetGain ( float fxLevel )
        {
            return WetGainSpline.Evaluate( fxLevel );
        }

        public static float WetHpfHz ( float fxLevel )
        {
            return WetHpfHzSpline.Evaluate( fxLevel );
        }

        public static float WetLpfCutoffHz ( float fxLevel )
        {
            return WetLpfHzSpline.Evaluate( fxLevel );
        }
    }
}