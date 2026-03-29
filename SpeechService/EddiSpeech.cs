using EddiDataDefinitions;

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
        public string message { get; private set; } = message;
        public int priority { get; private set; } = priority;
        public string voice { get; private set; } = voice;
        public bool radio { get; private set; } = radio;
        public string eventType { get; private set; } = eventType;

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
