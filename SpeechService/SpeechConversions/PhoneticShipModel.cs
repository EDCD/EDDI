using EddiDataDefinitions;

namespace EddiSpeechService.SpeechConversions
{
    public static partial class SpeechConversions
    {
        public static string getPhoneticShipModel(string val)
        {
            var ship = ShipDefinitions.FromModel( val );
            if ( ship != null && !string.IsNullOrEmpty( ship.model ) )
            {
                return ship.SpokenModel().Trim();
            }

            return val;
        }
    }
}
