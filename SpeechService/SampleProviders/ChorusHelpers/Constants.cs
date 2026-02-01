using System;

namespace EddiSpeechService.SampleProviders.ChorusHelpers
{
    // ===== Chorus tuning and safety bounds =====
    internal static class Constants
    {
        public const float Zero = 0f;
        public const float Pi = (float)Math.PI;
        public const float TwoPi = (float)(2.0 * Math.PI);
        public const float Phi = 0.6180339887f; // golden ratio conjugate

        // Define the master backbone for effects
        public const float SigmoidCenter = 0.33f;
        public const float SigmoidSteepL = 0.95f;
        public const float SigmoidSteepR = 1.45f;

        // Wet/Dry Mixing
        public const float TrueDryThreshold = 2f; // below this, force dry
        public static readonly float[] DryGainFx =
        {
            0f, 10f, 20f, 25f, 28f, 30f, 32f, 35f, 37f, 40f, 45f, 50f, 60f, 70f, 80f, 90f, 100f
        };
        public static readonly float[] DryGainY =
        {
            1.000f, //   0
            0.930f, //  10
            0.800f, //  20
            0.660f, //  25
            0.550f, //  28
            0.400f, //  30
            0.368f, //  32
            0.320f, //  35
            0.260f, //  37
            0.220f, //  40
            0.200f, //  45
            0.150f, //  50
            0.070f, //  60
            0.030f, //  70
            0.015f, //  80
            0.0125f,//  90
            0.010f  // 100
        };
        public static readonly float[] WetGainFx =
        {
            0f, 10f, 20f, 25f, 28f, 30f, 32f, 35f, 37f, 40f, 42f, 45f, 50f, 60f, 70f, 80f, 90f, 100f
        };
        public static readonly float[] WetGainY =
        {
            0.000f,  //   0
            0.050f,  //  10
            0.300f,  //  20
            0.420f,  //  25
            0.480f,  //  28
            0.550f,  //  30
            0.578f,  //  32
            0.620f,  //  35
            0.740f,  //  37
            0.800f,  //  40
            0.820f,  //  42
            0.850f,  //  45
            0.900f,  //  50
            0.940f,  //  60
            0.980f,  //  70
            0.990f,  //  80
            0.9925f, //  90
            0.995f   // 100
        };
        public static readonly float[] MakeupDbFx = 
        {
            0f, 10f, 20f, 25f, 28f, 30f, 32f, 35f, 37f, 40f, 42f, 45f, 50f, 60f, 70f, 80f, 90f, 100f
        };
        public static readonly float[] MakeupDbY =
        {
            0.000f, //   0 fx
            1.049f, //  10 fx
            1.757f, //  20 fx
            3.044f, //  25 fx
            3.031f, //  28 fx
            5.020f, //  30 fx
            5.154f, //  32 fx
            4.969f, //  35 fx
            4.325f, //  37 fx
            3.995f, //  40 fx
            3.746f, //  42 fx
            3.787f, //  45 fx
            3.725f, //  50 fx
            3.020f, //  60 fx
            2.947f, //  70 fx
            2.343f, //  80 fx
            2.666f, //  90 fx
            2.933f, // 100 fx
        }; // Non-monotonic

        // Coherence
        public static readonly float[] DetuneJitterFx = { 0f, 20f, 30f, 32f, 35f, 37f, 40f, 60f, 80f, 100f };
        public static readonly float[] DetuneJitterY  = { 0f, 0.0040f, 0.0048f, 0.0050f, 0.0052f, 0.0054f, 0.0055f, 0.0075f, 0.0095f, 0.0100f };
        public const float DepthJitterPct = 0.020f; // As a percentage
        public static readonly float[] PhaseJitterFx = { 0f, 20f, 30f, 32f, 35f, 37f, 40f, 60f, 80f, 100f };
        public static readonly float[] PhaseJitterY  =
        {
            0.0000f, //   0 fx
            0.0140f, //  20 fx
            0.0200f, //  30 fx
            0.0220f, //  32 fx
            0.0360f, //  35 fx
            0.0380f, //  37 fx
            0.0340f, //  40 fx
            0.0300f, //  60 fx 
            0.0400f, //  80 fx
            0.0500f  // 100 fx
        };

        // Delay spread
        public const float StepMinMs = 1.10f; // tight ensemble at low fx
        public const float StepMaxMs = 3.25f; // max ensemble breadth at high fx
        public const float StepEdge0 = 0.03f;
        public const float StepEdge1 = 0.70f;
        public static readonly float[] StepX =
        {
            0.00f, 0.20f, 0.28f, 0.34f, 0.40f, 0.60f, 1.00f
        };
        public static readonly float[] StepY =
        {
            0.54f, // 0.00
            0.64f, // 0.20
            0.70f, // 0.28
            0.74f, // 0.34
            0.78f, // 0.40
            0.83f, // 0.60
            0.88f  // 1.00
        };
        public static readonly float[] CenterDelaySplineX =
        {
            0f, 10f, 20f, 30f, 35f, 40f, 45f, 50f, 70f, 100f
        };

        public static readonly float[] CenterDelaySplineY =
        {
            6.5f, 7f, 8f, 15.5f, 16f, 16.2f, 16.5f, 17f, 18.5f, 20f
        };
        public const float DelayJitterPct = 0.025f;

        // Depth shaping
        public const float DepthFloor = 0.00f; // minimum depth
        public const float DepthPosMin = 0.62f; // inner voices
        public const float DepthPosMax = 1.16f; // outer voices
        public const float MaxDepthFractionOfBase = 0.70f;
        public static readonly float[] ChorusDepthSplineX =
        {
            0f, 10f, 20f, 28f, 30f, 32f, 35f, 37f, 40f, 42f, 45f, 50f, 60f, 80f, 100f
        };
        public static readonly float[] ChorusDepthSplineY =
        {
            0f, 0.55f, 1.35f, 1.6f, 1.7f, 1.8f, 2.2f, 2.4f,
            2.6f, 2.8f, 3.2f, 4f, 5.8f, 8f, 9.6f
        };

        // Detune
        public const float DetuneSpanMin = 0.16f;
        public const float DetuneSpanMax = 0.30f;
        public const float DetuneEasePower = 1.15f;
        public const float DynamicDetuneDepth = 0.0007f; // instantaneous pitch variance
        public const float DynamicDetuneRateHz = 0.40f; // slow modulation of detune jitter
        public static readonly float[] DynamicDetuneMixFx =
        {
            0f, 20f, 30f, 40f, 60f, 80f, 100f
        };
        public static readonly float[] DynamicDetuneMixY =
        {
            0.00f, //   0 fx
            0.00f, //  20 fx
            0.00f, //  30 fx
            0.00f, //  40 fx
            0.02f, //  60 fx
            0.05f, //  80 fx
            0.06f  // 100 fx
        };

        // Feedback
        public const float FeedbackMax = 0.32f;
        public const float FeedbackMin = 0.03f;
        public const float FeedbackCenter = 35f;
        public const float FeedbackWidth = 60f;
        public const float FeedbackLateStart = 0.60f;
        public const float FeedbackLateEnd = 0.92f;
        public const float FeedbackLateMinFactor = 1.00f;
        public const float FeedbackAbsoluteMax = 0.38f;
        public const float FeedbackBloomStartFx = 55f;
        public const float FeedbackBloomEndFx   = 100f;
        public const float FeedbackBloomMax     = 0.38f;
        
        // Modulation Signal Shaping (per voice)
        public const float LfoMinHz = 0.78f; // the minimum amplitude of the Low Frequency Oscillator (LFO) signal controlling delay time.
        public const float LfoMaxHz = 1.55f; // the maximum amplitude of the Low Frequency Oscillator (LFO) signal controlling delay time.
        public const float LfoPercent = 0.60f; // The percent of the LFO from the signal curve
        public const float LfoPhaseSpreadPower = 0.72f;
        public const float LfoPhaseDrift = 0.04f;
        public const float LfoRamp = 0.78f;
        public const float LfoJitterDepth = 0.0016f;
        public const float LfoHarmonic2 = 2f;
        public const float LfoH2Cap = 0.70f;
        public const float LfoHarmonic3 = 3f;
        public const float LfoH3Cap = 0.24f;
        public const float LfoSkew = 0.36f;
        public const float LfoNormShapePower = 2f;
        public const float LfoH2RampScale = 0.90f;
        public const float LfoH3BaseScale = 0.25f;
        public const float MetaJitterDepth = 0.0010f; // LFO rate wobble depth
        public const float MetaJitterRateHz = 0.45f; // very slow modulator
        public const float MetaJitterVoiceRateSpread = 0.18f; // start here; tune 0.12–0.25
        public static readonly float[] LfoJitterScaleX =
        {
            0f, 10f, 20f, 30f, 35f, 40f, 50f, 60f, 70f, 80f, 90f, 100f
        };
        public static readonly float[] LfoJitterScaleY =
        {
            0.12f, //   0 fx
            0.18f, //  10 fx
            0.20f, //  20 fx
            0.20f, //  30 fx
            0.20f, //  35 fx
            0.23f, //  40 fx
            0.40f, //  50 fx
            0.55f, //  60 fx
            0.70f, //  70 fx
            0.86f, //  80 fx
            0.98f, //  90 fx
            1.10f  // 100 fx
        };
        public static readonly float[] LfoRateScaleX =
        {
            30f, 40f, 60f, 80f, 100f
        };
        public static readonly float[] LfoRateScaleY =
        {
            1.00f, // 30 fx
            1.04f, // 40 fx
            1.06f, // 60 fx
            1.10f, // 80 fx
            1.12f  // 100 fx
        };

        // Micro-combs
        public const float MicroCombOffset = 0.19f; // In milliseconds
        public const float MicroCombPower = 0.95f;
        public static readonly float[] MicroCombScaleX =
        {
            0f, 10f, 20f, 28f, 30f, 32f, 35f, 37f, 40f, 
            42f, 45f, 50f, 60f, 70f, 80f, 100f
        };
        public static readonly float[] MicroCombScaleY =
        {
            0.140f, //   0 fx
            0.154f, //  10 fx
            0.160f, //  20 fx
            0.160f, //  28 fx
            0.160f, //  30 fx
            0.185f, //  32 fx
            0.215f, //  35 fx
            0.235f, //  37 fx
            0.290f, //  40 fx
            0.400f, //  42 fx
            0.950f, //  45 fx
            1.478f, //  50 fx
            1.937f, //  60 fx
            2.500f, //  70 fx
            2.700f, //  80 fx
            2.850f  // 100 fx
        };

        // Voice Shaping
        public const int VoiceCount = 6;
        public const float VoiceBaseWeight = 1.05f;
        public const float VoiceCorrelationBias = 1.00f;
        public const float VoiceOuterThreshold = 0.66f;
        public const float VoiceOuterDepthBoost = 1.10f; // extra depth scaling for outer voices
        public const float VoiceOuterDetuneBoost = 1.10f; // extra detune for outer voices
        public const float OuterLfoBias = 1.34f; // outer voices have slightly faster LFO for more motion

        // Fractional sub-voices (“ghost voices”)
        public const float GhostVoiceFreqBias = 1.12f;
        public const float GhostVoicePhaseOffset = 0.7854f; // 45°
        public const float GhostBaseDelayOffsetMs = 0.45f;
        public static readonly float[] GhostWeightSplineX =
        {
            0f, 10f, 20f, 25f, 30f, 32f, 35f, 40f, 45f, 50f, 60f, 100f
        };
        public static readonly float[] GhostWeightSplineY =
        {
            0.02f,  //  0 fx
            0.02f,  // 10 fx
            0.03f,  // 20 fx
            0.04f,  // 25 fx
            0.09f,  // 30 fx
            0.10f,  // 32 fx
            0.11f,  // 35 fx
            0.13f,  // 40 fx
            0.15f,  // 45 fx
            0.17f,  // 50 fx
            0.19f,  // 60 fx
            0.22f   // 100 fx
        };
        public static readonly float[] GhostDepthSplineX =
        {
            0f, 10f, 20f, 25f, 30f, 35f, 37f, 40f, 45f, 50f, 60f, 80f, 100f
        };
        public static readonly float[] GhostDepthSplineY =
        {
            0.00f,  //  0 fx
            0.16f,  // 10 fx
            0.32f,  // 20 fx
            0.42f,  // 25 fx
            0.45f,  // 30 fx
            0.48f,  // 35 fx
            0.50f,  // 37 fx
            0.52f,  // 40 fx
            0.54f,  // 45 fx
            0.56f,  // 50 fx
            0.58f,  // 60 fx
            0.60f,  // 80 fx
            0.62f   // 100 fx
        };

        // Feedback Shelf
        public const float FeedbackShelfCutoffHz = 8000f;
        public const float FeedbackShelfGainEarlyDb = -0.60f;
        public const float FeedbackShelfGainLateDb = -0.40f;
        public const float FeedbackShelfStart = 0.02f;
        public const float FeedbackShelfEnd = 1.10f;
        public const float FeedbackLpfLow = 11000f;
        public const float FeedbackLpfHigh = 15000f;
        public const float FeedbackBypassFxThresh = 5.00f;// skip feedback LPF below this to save CPU
        public static readonly float[] DryHpfFx =
        {
            0f, 20f, 25f, 28f, 30f, 35f, 40f, 50f, 70f, 100f
        };
        public static readonly float[] DryHpfHz =
        {
            0f,   //   0 fx (disabled)
            25f,  //  20 fx
            22f,  //  25 fx
            26f,  //  28 fx
            55f,  //  30 fx
            80f,  //  35 fx
            55f,  //  40 fx
            45f,  //  50 fx
            22f,  //  70 fx
            18f   // 100 fx
        };
        public static readonly float[] WetHpfFx =
        {
            0f, 20f, 25f, 28f, 30f, 32f, 35f, 37f, 40f, 50f, 70f, 80f, 90f, 100f
        };
        public static readonly float[] WetHpfHz =
        {
            0f,   //   0 fx  (disabled)
            40f,  //  20 fx
            32f,  //  25 fx
            36f,  //  28 fx
            55f,  //  30 fx
            60f,  //  32 fx
            95f,  //  35 fx
            88f, //  37 fx
            85f,  //  40 fx
            80f,  //  50 fx
            45f,  //  70 fx
            30f,  //  80 fx
            28f,  //  90 fx
            26f   // 100 fx
        };
        public static readonly float[] WetLpfFx =
        {
            0f, 20f, 30f, 35f, 40f, 45f, 50f, 70f, 100f
        };
        public static readonly float[] WetLpfHz =
        {
            10400f, //   0 fx
            9800f,  //  20 fx
            9000f,  //  30 fx
            8400f,  //  35 fx
            8200f,  //  40 fx
            7400f,  //  45 fx
            7800f,  //  50 fx
            6400f,  //  70 fx
            5600f   // 100 fx
        };
        // Feedback High Pass Filter (prevents LF runaway in the feedback loop)
        public static readonly float[] FeedbackHpfFx =
        {
            0f, 20f, 25f, 30f, 35f, 40f, 50f, 70f, 100f
        };
        public static readonly float[] FeedbackHpfHz =
        {
            0f,   // 0 fx  (disabled)
            70f,  // 20 fx
            55f,  // 25 fx
            95f,  // 30 fx
            130f, // 35 fx
            110f, // 40 fx
            100f, // 50 fx
            50f,  // 70 fx
            45f   // 100 fx
        };

        // Body Shelf
        public const float BodyShelfQ  = 1.2f;
        public static readonly float[] BodyGainSplineX =
        {
            0f, 20f, 25f, 30f, 40f, 60f, 70f, 80f, 100f
        };
        public static readonly float[] BodyGainSplineY =
        {
            0.0f,  //   0 fx
            0.5f,  //  20
            1.05f, //  25
            1.2f,  //  30
            1.3f,  //  40
            0.9f,  //  60
            0.8f,  //  70
            0.95f, //  80
            0.6f   // 100
        };
        public const float BodyShelfHz = 240f;

        // Mud cut
        public static readonly float[] MudCutGainSplineX =
        {
            0f, 20f, 25f, 28f, 30f, 32f, 35f, 37f, 40f, 45f, 60f, 70f, 100f
        };
        public static readonly float[] MudCutGainSplineY =
        {
            -0.0f,  //   0 fx
            -1.0f,  //  20
            -1.85f, //  25
            -3.3f,  //  28 
            -2.2f,  //  30
            -2.0f,  //  32
            -0.3f,  //  35
            -0.45f, //  37
            -0.35f, //  40
            -0.25f, //  45
            -0.55f, //  60
            -0.90f, //  70
            -1.30f  // 100
        };
        public const float MudCutHz = 450f;
        public const float MudCutQ = 1.00f;

        // Presence cut
        public const float PresenceCutHz = 2000f;
        public const float PresenceCutQ  = 1.00f;
        public static readonly float[] PresenceCutGainSplineX = { 0f, 20f, 30f, 32f, 35f, 37f, 40f, 45f, 70f, 100f };
        public static readonly float[] PresenceCutGainSplineY =
        {
            -0.00f, //   0 fx
            -0.80f, //  20 fx
            -1.10f, //  30 fx
            -1.25f, //  32 fx
            -1.15f, //  35 fx
            -1.25f, //  37 fx
            -1.35f, //  40 fx
            -1.45f, //  45 fx
            -1.65f, //  70 fx
            -1.85f  // 100 fx
        };

        // Resonance
        public const float ResonanceAllPassGain = 0.52f;
        public const float ResonanceAllPassLenMs = 0.32f;
        public static readonly float[] ResonanceAllPassFx = { 0f, 20f, 30f, 40f, 50f, 70f, 100f };
        public static readonly float[] ResonanceAllPassGainY = { 0.00f, 0.00f, 0.00f, 0.03f, 0.06f, 0.32f, 0.52f };
        
        // Voice activation curves - more outer emphasis at the top
        public static readonly float[] VoiceActX =
        {
            0f, 20f, 30f, 40f, 60f, 80f, 100f
        };
        public static readonly float[] Voice0ActY =
        {
            0.00f, 0.18f, 0.30f, 0.52f, 0.70f, 0.84f, 0.92f
        };
        public static readonly float[] Voice1ActY =
        {
            0.00f, 0.18f, 0.30f, 0.52f, 0.70f, 0.84f, 0.92f
        };
        public static readonly float[] Voice2ActY =
        {
            0.00f, 0.12f, 0.44f, 0.60f, 0.62f, 0.78f, 0.95f
        };
        public static readonly float[] Voice3ActY =
        {
            0.00f, 0.12f, 0.44f, 0.62f, 0.62f, 0.78f, 0.95f
        };
        public static readonly float[] Voice4ActY =
        {
            0.00f, 0.20f, 0.45f, 0.75f, 0.85f, 0.92f, 0.96f
        };
        public static readonly float[] Voice5ActY =
        {
            0.00f, 0.17f, 0.40f, 0.70f, 0.80f, 0.88f, 0.94f
        };

        // Safety: absolute delay and buffer sizing
        public const float MinVoiceDelayMs = 0.75f;
        public const float MaxVoiceDelayMs = 90.0f; // hard cap base+depth per voice
        public const float BufferHeadroomMs = 10.0f; // extra beyond (base+depth)

        // Baseline EQ
        public const float Baseline125BoostHz = 125f;
        public const float Baseline125BoostQ  = 0.90f;
        public const float Baseline125BoostDb = -1.30f;
        public const float Baseline250BoostHz = 250f;
        public const float Baseline250BoostQ  = 0.90f;
        public const float Baseline250BoostDb = 0.17f;
        public const float Baseline500BoostHz = 500f;
        public const float Baseline500BoostQ  = 0.90f;
        public const float Baseline500BoostDb = -0.16f;
        public const float Baseline1kBoostHz = 1000f;
        public const float Baseline1kBoostQ  = 0.90f;
        public const float Baseline1kBoostDb = -0.47f;
        public const float Baseline2kBoostHz = 2000f;
        public const float Baseline2kBoostQ  = 0.90f;
        public const float Baseline2kBoostDb = -0.36f;
        public const float Baseline4kBoostHz = 4200f;
        public const float Baseline4kBoostQ  = 0.90f;
        public const float Baseline4kBoostDb = -3.16f;
        public const float Baseline8kBoostHz = 8000f;
        public const float Baseline8kBoostQ  = 0.90f;
        public const float Baseline8kBoostDb = 9.72f;
        public const float BaselineHiShelfHz = 11000f;
        public const float BaselineHiShelfQ  = 0.70f;
        public const float BaselineHiShelfDb = -5.5f;
        public static readonly float[] Baseline4kTrimDbFx = { 0f, 60f, 70f, 80f, 100f };
        public static readonly float[] Baseline4kTrimDbY  = { 0.0f, 0.6f, 2.1f, 1.9f, 1.5f };
        public static readonly float[] Baseline8kTrimDbFx = { 0f, 20f, 30f, 40f, 45f, 60f, 70f, 80f, 90f, 100f };
        public static readonly float[] Baseline8kTrimDbY  = { 0f, -0.4f, -0.6f, -1.2f, -1.8f, -1.4f, -2.4f, -2.6f, -2.1f, -1.7f };

        // Safety clamps
        public const float FeedbackSoftLimitDriveMax = 1.80f;
        public const float FeedbackSoftLimitCeiling  = 0.96f; // absolute clamp for feedback state
        public const float MixSoftClipCeiling = 0.965f;
        public const float ModDetuneJitterMax = 0.010f;
        public const float ModPhaseJitterMaxRad = 0.24f;
        public const float VoiceSafetyLimiter = 0.97f;
    }
}