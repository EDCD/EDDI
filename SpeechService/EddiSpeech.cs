using EddiDataDefinitions;
using EddiSpeechService.SpeechPreparation;
using Utilities;

namespace EddiSpeechService
{
    public class EddiSpeech (
        string message,
        string voice = null,
        int priority = 3,
        string eventType = null,
        LandingPadSize shipSize = null,
        decimal? shipHealth = 100M,
        bool radio = false,
        bool distortOnDamage = false )
    {
        [PublicAPI]
        public string message { get; private set; } = message;
        
        [PublicAPI]
        public int priority { get; private set; } = priority;
        
        [PublicAPI]
        public string voice { get; private set; } = voice;
        
        [PublicAPI]
        public bool radio { get; private set; } = radio;
        
        [PublicAPI]
        public string eventType { get; private set; } = eventType;

        [ PublicAPI ]
        public int wordCount { get; private set; } = GeneratedRegex.WordsRegex()
            .Matches( SpeechFormatter.StripSSML( message ?? string.Empty ) ).Count;

        // Calculated SpeechFX data
        public int echoDelay { get; } = GetEchoDelay( shipSize );
        public int distortionLevel { get; } = GetDistortionLevel( distortOnDamage, shipHealth );

        // Resolve the SpeechFX settings

        private static int GetDistortionLevel ( bool distortOnDamage, decimal? shipHealth )
        {
            // This is affected by ship health
            var distortionLevel = 0;
            if ( shipHealth != null && distortOnDamage )
            {
                distortionLevel = 100 - (int)shipHealth;
            }

            return distortionLevel;
        }

        private static int GetEchoDelay ( LandingPadSize size )
        {
            // this is affected by ship size
            var echoDelayMs = 0; // Default
            if ( size != null )
            {
                if ( size == LandingPadSize.Small )
                {
                    echoDelayMs = 50;
                }
                else if ( size == LandingPadSize.Medium )
                {
                    echoDelayMs = 60;
                }
                else if ( size == LandingPadSize.Large )
                {
                    echoDelayMs = 120;
                }
            }

            return echoDelayMs;
        }
    }
}
