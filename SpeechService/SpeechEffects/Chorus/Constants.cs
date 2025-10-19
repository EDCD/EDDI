using System;

namespace EddiSpeechService.SpeechEffects.Chorus
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
            1.000f, //   0 fx
            0.980f, //  10 fx
            0.900f, //  20 fx
            0.780f, //  25 fx
            0.700f, //  28 fx
            0.400f, //  30 fx
            0.330f, //  32 fx
            0.220f, //  35 fx
            0.160f, //  37 fx
            0.120f, //  40 fx
            0.100f, //  45 fx
            0.060f, //  50 fx
            0.040f, //  60 fx
            0.020f, //  70 fx
            0.015f, //  80 fx
            0.012f, //  90 fx
            0.010f  // 100 fx
        };
        public static readonly float[] WetGainFx =
        {
            0f, 10f, 20f, 25f, 28f, 30f, 32f, 35f, 37f, 40f, 42f, 45f, 50f, 60f, 70f, 80f, 90f, 100f
        };
        public static readonly float[] WetGainY =
        {
            0.00f, 0.05f, 0.15f, 0.28f, 0.38f, 0.50f, 0.58f, 0.68f, 0.74f, 0.80f, 0.84f, 0.88f, 0.92f, 0.96f, 0.98f, 0.990f, 0.994f, 0.996f
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
            0.0150f, //  30 fx
            0.0151f, //  32 fx
            0.0210f, //  35 fx
            0.0240f, //  37 fx
            0.0250f, //  40 fx
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
            0.54f, 0.64f, 0.70f, 0.74f, 0.78f, 0.80f, 0.84f
        }; // relative multiplier to existing linear step
        public static readonly float[] CenterDelaySplineX =
        {
            0f, 10f, 20f, 30f, 35f, 40f, 45f, 50f, 70f, 100f
        };

        public static readonly float[] CenterDelaySplineY =
        {
            6.50f, 7.00f, 8.00f, 9.50f, 10.50f, 11.50f, 12.00f, 12.50f, 13.50f, 14.00f
        };
        public const float DelayJitterPct = 0.015f; // ±1.5%

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
            0.00f, //   0 fx
            0.35f, //  10 fx
            0.90f, //  20 fx
            1.30f, //  28 fx
            1.40f, //  30 fx
            1.40f, //  32 fx
            1.50f, //  35 fx
            1.62f, //  37 fx
            1.78f, //  40 fx
            1.90f, //  42 fx
            2.20f, //  45 fx
            3.00f, //  50 fx
            3.50f, //  60 fx
            4.30f, //  80 fx
            5.00f  // 100 fx
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
        public const float FeedbackMax = 0.28f;
        public const float FeedbackMin = 0.03f;
        public const float FeedbackCenter = 25f;
        public const float FeedbackWidth = 60f;
        public const float FeedbackLateStart = 0.60f;
        public const float FeedbackLateEnd = 0.92f;
        public const float FeedbackLateMinFactor = 1.00f;
        public const float FeedbackAbsoluteMax = 0.28f;

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
            0.140f, //  0 fx
            0.154f, // 10 fx
            0.161f, // 20 fx
            0.168f, // 28 fx
            0.182f, // 30 fx
            0.238f, // 32 fx
            0.266f, // 35 fx
            0.308f, // 37 fx
            0.420f, // 40 fx
            0.563f, // 42 fx
            1.047f, // 45 fx
            1.478f, // 50 fx
            1.937f, // 60 fx
            2.250f, // 70 fx
            2.350f, // 80 fx
            2.550f  // 100 fx
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
        public static readonly float[] WetHpfFx =
        {
            0f, 20f, 30f, 35f, 40f, 50f, 70f, 100f
        };
        public static readonly float[] WetHpfHz =
        {
            0f,   // 0 fx  (disabled)
            60f,  // 20 fx
            90f,  // 30 fx
            100f, // 35 fx
            110f, // 40 fx
            110f, // 50 fx
            110f, // 70 fx
            110f  // 100 fx
        };
        public static readonly float[] WetLpfFx =
        {
            0f, 20f, 30f, 40f, 50f, 70f, 100f
        };
        public static readonly float[] WetLpfHz =
        {
            10400f, //   0 fx
            10600f, //  20 fx
            10850f, //  30 fx
            10950f, //  40 fx
            11000f, //  50 fx
            11000f, //  70 fx
            11000f  // 100 fx
        };

        // Body Shelf
        public const float BodyShelfHz = 420f;
        public const float BodyShelfQ  = 0.80f;
        public static readonly float[] BodyGainSplineX = { 0f, 20f, 30f, 32f, 40f, 70f, 100f };
        public static readonly float[] BodyGainSplineY = { 0f, -0.4f, -0.8f, -0.9f, -1.0f, -1.0f, -1.0f };

        // Metallic Shelf
        public const float MetallicShelfHz = 2700f;
        public const float MetallicQ = 0.80f;
        public static readonly float[] MetallicGainSplineX =
        {
            0f, 10f, 20f, 30f, 32f, 35f, 37f, 40f, 45f, 60f, 80f, 100f
        };
        public static readonly float[] MetallicGainSplineY =
        {
            +0.00f,  //   0 fx
            -0.20f,  //  10 fx
            -0.40f,  //  20 fx
            -1.10f,  //  30 fx
            -1.00f,  //  32 fx
            -1.00f,  //  35 fx
            -1.00f,  //  37 fx
            -0.70f,  //  40 fx
            -0.30f,  //  45 fx
            +0.10f,  //  60 fx
            +0.80f,  //  80 fx
            +1.20f   // 100 fx
        };
        public const float MetallicGainAttenuationMax = 2.0f;
        public const float MetallicGainAttenuationStart = 60f;
        public const float MetallicGainAttenuationEnd = 100f;

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
            0.00f, 0.18f, 0.30f, 0.46f, 0.62f, 0.82f, 0.92f
        };
        public static readonly float[] Voice1ActY =
        {
            0.00f, 0.18f, 0.30f, 0.46f, 0.62f, 0.82f, 0.92f
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

        // Shimmer
        public const float ShimmerShelfHz = 7600f;
        public const float ShimmerShelfQ = 0.72f;
        public static readonly float[] ShimmerSplineX =
        {
            0f, 10f, 20f, 30f, 32f, 35f, 37f, 40f, 45f, 60f, 80f, 100f
        };
        public static readonly float[] ShimmerSplineY =
        {
            0.00f, //   0 fx
            2.20f, //  10 fx
            2.70f, //  20 fx
            11.6f, //  30 fx
            11.6f, //  32 fx
            11.6f, //  35 fx
            11.8f, //  37 fx
            12.0f, //  40 fx
            10.0f, //  45 fx
            11.0f, //  60 fx
            12.0f, //  80 fx
            13.0f  // 100 fx
        };

        // Safety: absolute delay and buffer sizing
        public const float MinVoiceDelayMs = 0.75f;
        public const float MaxVoiceDelayMs = 90.0f; // hard cap base+depth per voice
        public const float BufferHeadroomMs = 10.0f; // extra beyond (base+depth)

        // Safety clamps
        public const float FeedbackSoftLimitDriveMax = 1.30f;
        public const float FeedbackSoftLimitCeiling  = 0.96f; // absolute clamp for feedback state
        public const float MixSoftClipCeiling = 0.965f;
        public const float ModDetuneJitterMax = 0.010f;
        public const float ModPhaseJitterMaxRad = 0.24f;
        public const float VoiceSafetyLimiter = 0.97f;
    }
}