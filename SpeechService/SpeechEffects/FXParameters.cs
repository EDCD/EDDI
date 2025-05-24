namespace EddiSpeechService.SpeechEffects
{
    public class FxParameters
    {
        internal static int DamageAdjustedFxLevel ( decimal distortionLevel, int configFxLevel )
        {
            // Effects level can be increased, e.g. by damage if distortion is enabled
            var bonusFX = 0;
            if ( distortionLevel > 0 )
            {
                bonusFX = (int)decimal.Round( distortionLevel / 100M * ( 100M - configFxLevel ) );
            }

            return configFxLevel + bonusFX;
        }
    }
}
