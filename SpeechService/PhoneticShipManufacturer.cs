using EddiDataDefinitions;

namespace EddiSpeechService
{
    public partial class Translations
    {
        public static string getPhoneticShipManufacturer(string val2)
        {
            string phoneticManufacturer = ShipDefinitions.SpokenManufacturer(val2);
            if (phoneticManufacturer != null)
            {
                return phoneticManufacturer;
            }
            return val2;
        }
    }
}
