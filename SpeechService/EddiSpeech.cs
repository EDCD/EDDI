using EddiDataDefinitions;

namespace EddiSpeechService
{
    public class EddiSpeech
    {
        public string message { get; private set; }
        public int priority { get; private set; }
        public string voice { get; private set; }
        public bool radio { get; private set; }
        public string eventType { get; private set; }

        // Calculated SpeechFX data
        public int echoDelay { get; set; }
        public int chorusLevel { get; set; }
        public int reverbLevel { get; set; }
        public int distortionLevel { get; set; }

        public EddiSpeech ( string message, string voice = null, int priority = 3,
            string eventType = null, int configFxLevel = 0, LandingPadSize shipSize = null,
            decimal? shipHealth = 100M, bool radio = false, bool distortOnDamage = false )
        {
            this.message = message;
            this.priority = priority;
            this.voice = voice;
            this.radio = radio;
            this.eventType = eventType;

            // Resolve the SpeechFX settings
            this.echoDelay = GetEchoDelay( shipSize );
            this.chorusLevel = GetChorusLevel( configFxLevel );
            this.reverbLevel = GetReverbLevel( configFxLevel );
            this.distortionLevel = GetDistortionLevel( distortOnDamage, shipHealth );
        }

        private static int GetChorusLevel ( int configFxLevel )
        {
            // This is not affected by ship parameters
            return (int)( 60 * ( configFxLevel / 100M ) );
        }

        private static int GetDistortionLevel ( bool distortOnDamage, decimal? shipHealth )
        {
            // This is affected by ship health
            var distortionLevel = 0;
            if ( shipHealth != null && distortOnDamage )
            {
                distortionLevel = ( 100 - (int)shipHealth );
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
                    echoDelayMs = 150;
                }
                else if ( size == LandingPadSize.Medium )
                {
                    echoDelayMs = 200;
                }
                else if ( size == LandingPadSize.Large )
                {
                    echoDelayMs = 400;
                }
            }

            return echoDelayMs;
        }

        internal static int GetReverbLevel ( int configFxLevel )
        {
            // This is not affected by ship parameters
            return (int)( 80 * ( configFxLevel / 100M ) );
        }
    }
}
