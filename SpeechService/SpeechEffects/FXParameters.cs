using EddiDataDefinitions;
using Utilities;

namespace EddiSpeechService.SpeechEffects
{
    public class FxParameters
    {
        private static SpeechServiceConfiguration Configuration => SpeechService.Instance.Configuration;

        public static EddiSpeech GetSpeechFX ( EddiSpeech speech )
        {
            var ship = speech.ship;
            speech.echoDelay = echoDelayForShip( ship );
            speech.chorusLevel = chorusLevelForShip();
            speech.reverbLevel = reverbLevelForShip();
            speech.distortionLevel = distortionLevelForShip( ship );
            speech.compressionLevel = 0;
            return speech;
        }

        private static int echoDelayForShip ( Ship ship )
        {
            // this is affected by ship size
            var echoDelayMs = 0; // Default
            if ( ship != null )
            {
                if ( ship.Size == LandingPadSize.Small )
                {
                    echoDelayMs = 150;
                }
                else if ( ship.Size == LandingPadSize.Medium )
                {
                    echoDelayMs = 200;
                }
                else if ( ship.Size == LandingPadSize.Large )
                {
                    echoDelayMs = 400;
                }
            }

            return echoDelayMs;
        }

        private static int chorusLevelForShip ()
        {
            // This is not affected by ship parameters
            return (int)( 60 * ( Configuration.EffectsLevel / 100M ) );
        }

        private static int reverbLevelForShip ()
        {
            // This is not affected by ship parameters
            return (int)( 80 * ( Configuration.EffectsLevel / 100M ) );
        }

        private static int distortionLevelForShip ( Ship ship )
        {
            // This is affected by ship health
            var distortionLevel = 0;
            if ( ship != null && Configuration.DistortOnDamage )
            {
                distortionLevel = ( 100 - (int)ship.health );
            }

            return distortionLevel;
        }

        internal static int fxLevel ( decimal distortionLevel )
        {
            // Effects level is increased by damage if distortion is enabled
            var distortionFX = 0;
            if ( distortionLevel > 0 )
            {
                distortionFX = (int)decimal.Round( distortionLevel / 100M * ( 100M - Configuration.EffectsLevel ) );
                Logging.Debug( "Calculating effect of damage distortion on speech effects: +" + distortionFX );
            }

            return Configuration.EffectsLevel + distortionFX;
        }

    }
}
