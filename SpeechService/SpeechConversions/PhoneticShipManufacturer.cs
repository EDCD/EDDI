using EddiDataDefinitions;

namespace EddiSpeechService.SpeechConversions
{
    public static partial class SpeechConversions
    {
        public static string getPhoneticShipManufacturer(string val2)
        {
            var phoneticManufacturer = Manufacturer.SpokenManufacturer(val2);
            if (!string.IsNullOrEmpty( phoneticManufacturer ) )
            {
                return phoneticManufacturer.Trim();
            }
            return val2;
        }
    }
}
