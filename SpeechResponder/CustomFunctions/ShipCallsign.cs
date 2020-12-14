using Cottle.Functions;
using EddiCore;
using EddiDataDefinitions;
using EddiShipMonitor;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using JetBrains.Annotations;
using System.Text.RegularExpressions;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ShipCallsign : ICustomFunction
    {
        public string name => "ShipCallsign";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.ShipCallsign;
        public NativeFunction function => new NativeFunction((values) =>
        {
            int? localId = (values.Count == 0 ? (int?)null : (int)values[0].AsNumber);
            ShipMonitor shipMonitor = (ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor");
            Ship ship = shipMonitor.GetShip(localId);

            string result;
            if (ship != null)
            {
                if (EDDI.Instance.Cmdr != null && EDDI.Instance.Cmdr.name != null)
                {
                    // Obtain the first three characters
                    string chars = new Regex("[^a-zA-Z0-9]").Replace(EDDI.Instance.Cmdr.name, "").ToUpperInvariant().Substring(0, 3);
                    result = ship.phoneticmanufacturer + " " + Translations.ICAO(chars);
                }
                else
                {
                    if (string.IsNullOrEmpty(ship.phoneticmanufacturer))
                    {
                        result = "unidentified ship";
                    }
                    else
                    {
                        result = "unidentified " + ship.phoneticmanufacturer + " " + ship.phoneticmodel;
                    }
                }
            }
            else
            {
                result = "unidentified ship";
            }
            return result;
        }, 0, 1);
    }
}
