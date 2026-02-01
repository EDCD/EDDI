using System;

namespace EddiSpeechService.SampleProviders.ChorusHelpers
{
    internal static class Curve
    {
        // Ensemble effect parameter curves
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
                DetuneJitter = SpeechFxFunctions.Clamp( ModDetuneJitterSpline.Evaluate( fxLevel ), 0f, Constants.ModDetuneJitterMax ),
                PhaseJitterRad = SpeechFxFunctions.Clamp( ModPhaseJitterSpline.Evaluate( fxLevel ), 0f, Constants.ModPhaseJitterMaxRad ),
            };
        }

        public static float BodyGainDb ( float fxLevel )
        {
            return SpeechFxFunctions.SmoothSplineClamped( Constants.BodyGainSplineX, Constants.BodyGainSplineY, fxLevel );
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
            var x = SpeechFxFunctions.Clamp( fxLevel / 100f, 0f, 1f );
            return SpeechFxFunctions.LinearInterpolate( Constants.DetuneSpanMin, Constants.DetuneSpanMax,
                SpeechFxFunctions.EaseInPow( x, Constants.DetuneEasePower ) );
        }

        public static float DryGain ( float fxLevel )
        {
            return DryGainSpline.Evaluate( fxLevel );
        }

        public static float DryHpfHz ( float fxLevel )
        {
            return SpeechFxFunctions.SmoothSplineClamped( Constants.DryHpfFx, Constants.DryHpfHz, fxLevel );
        }

        public static float DynamicDetuneMix ( float fxLevel )
        {
            return DynamicDetuneSpline.Evaluate( fxLevel );
        }

        public static float Feedback ( float master, float fxLevel )
        {
            // Early fade gives slight taper below 0.1 master; constant thereafter.
            var t = SpeechFxFunctions.SoftStep( fxLevel, Constants.FeedbackCenter, Constants.FeedbackWidth );
            var fb = SpeechFxFunctions.LinearInterpolate( Constants.FeedbackMin, Constants.FeedbackMax, t );
            
            // Taper to reduce late-range cancellation
            var late = SpeechFxFunctions.LinearRescale( master, Constants.FeedbackLateStart, Constants.FeedbackLateEnd );
            var lateAtten = SpeechFxFunctions.LinearInterpolate( 1.00f, Constants.FeedbackLateMinFactor, late );
            var baseVal = Math.Min( fb * lateAtten, Constants.FeedbackAbsoluteMax );

            var bloomT = SpeechFxFunctions.SmoothStep(Constants.FeedbackBloomStartFx, Constants.FeedbackBloomEndFx, fxLevel);
            var bloomed = SpeechFxFunctions.LinearInterpolate(baseVal, Constants.FeedbackBloomMax, bloomT);
            return Math.Min( bloomed, Constants.FeedbackAbsoluteMax );
        }

        public static float FeedbackHpfHz ( float fxLevel )
        {
            return SpeechFxFunctions.SmoothSplineClamped( Constants.FeedbackHpfFx, Constants.FeedbackHpfHz, fxLevel );
        }

        public static float FeedbackGainDb ( float master )
        {
            var shelfNorm = SpeechFxFunctions.LinearRescale( master, Constants.FeedbackShelfStart, Constants.FeedbackShelfEnd );
            var baseCut = SpeechFxFunctions.LinearInterpolate( Constants.FeedbackShelfGainEarlyDb, Constants.FeedbackShelfGainLateDb, shelfNorm );
            // As feedback rises, open the filter slightly to avoid "buzz" and let resonance bloom
            return baseCut * ( 1f + ( 0.15f * master ) );
        }

        public static float FeedbackLpfCutoff ( float master )
        {
            return SpeechFxFunctions.LinearInterpolate( Constants.FeedbackLpfLow, Constants.FeedbackLpfHigh, master );
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
            return SpeechFxFunctions.SmoothSpline( Constants.LfoJitterScaleX, Constants.LfoJitterScaleY, fxLevel );
        }

        public static float LfoRateScale ( float fxLevel )
        {
            return LfoRateSpline.Evaluate( fxLevel );
        }

        public static float MicroCombScale ( float fxLevel )
        {
            return SpeechFxFunctions.SmoothSpline( Constants.MicroCombScaleX, Constants.MicroCombScaleY, fxLevel );
        }

        public static float MixMakeupDb ( float fxLevel )
        {
            return SpeechFxFunctions.SmoothSplineClamped( Constants.MakeupDbFx, Constants.MakeupDbY, fxLevel );
        }
        
        public static float MudCutGainDb ( float fxLevel )
        {
            return SpeechFxFunctions.SmoothSplineClamped( Constants.MudCutGainSplineX, Constants.MudCutGainSplineY, fxLevel );
        }

        public static float PresenceCutGainDb ( float fxLevel )
        {
            return SpeechFxFunctions.SmoothSplineClamped( Constants.PresenceCutGainSplineX, Constants.PresenceCutGainSplineY, fxLevel );
        }
        
        public static float ResonanceAllPassGain ( float fxLevel )
        {
            return ResonanceAllPassGainSpline.Evaluate( fxLevel );
        }

        public static float WetGain ( float fxLevel )
        {
            return WetGainSpline.Evaluate( fxLevel );
        }

        public static float WetHpfHz ( float fxLevel )
        {
            return SpeechFxFunctions.SmoothSplineClamped( Constants.WetHpfFx, Constants.WetHpfHz, fxLevel );
        }

        public static float WetLpfCutoffHz ( float fxLevel )
        {
            return SpeechFxFunctions.SmoothSplineClamped( Constants.WetLpfFx, Constants.WetLpfHz, fxLevel );
        }
    }
}