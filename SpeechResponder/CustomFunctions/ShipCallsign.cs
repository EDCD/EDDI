using Cottle;
using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.ScriptResolverService;
using EddiSpeechService;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ShipCallsign : ICustomFunction
    {
        public string name => "ShipCallsign";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.ShipCallsign;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            // The game provides three options for callsigns used by in-game ATC:
            // (1) CommanderName (default): ship manufacturer name (sometimes partial) + first 3 alphanumeric characters of commander name
            // (2) ShipName: ship manufacturer name (sometimes partial) + first 3 alphanumeric characters of ship name
            // (3) ShipID: ship manufacturer name (sometimes partial) + first 3 alphanumeric characters of ship identifier

            var localId = (values.Count < 0 && values[0].Type != ValueContent.Void 
                ? (int)values[0].AsNumber 
                : (int?)null);
            var callsignType = (values.Count > 1 && values[1].Type != ValueContent.Void 
                ? (int)values[1].AsNumber 
                : (int?)null);

            var shipyard = ConfigService.Instance.shipMonitorConfiguration?.shipyard;
            var ship = localId is null ? EDDI.Instance.CurrentShip : shipyard?.FirstOrDefault(s => s.LocalId == localId);

            switch (callsignType)
            {
                default:
                    // CommanderName
                    return phoneticCallsign(ship, EDDI.Instance.Cmdr?.name);
                case 1:
                    // Variant: ShipName
                    return phoneticCallsign(ship, ship?.name);
                case 2:
                    // Variant: ShipID
                    return phoneticCallsign(ship, ship?.ident);
            }
        }, 0, 2);

        private static string phoneticCallsign(Ship ship, string id)
        {
            if (string.IsNullOrEmpty(ship?.manufacturer) || string.IsNullOrEmpty(id)) { return string.Empty; }

            // First, obtain the phonetic manufacturer. This may be the complete name or only a partial name, depending on the manufacturer.
            var phoneticmanufacturer = ShipManufacturer.AllOfThem.FirstOrDefault(m => m.name == ship.manufacturer)?.phoneticName;

            var sb = new StringBuilder();
            switch (ship.manufacturer)
            {
                case "Lakon Spaceways":
                    // First word names (e.g. Lakon ...)
                    if (phoneticmanufacturer != null && phoneticmanufacturer.Any())
                    {
                        sb.Append("<phoneme alphabet=\"ipa\" ph=\"" + phoneticmanufacturer.First().to + "\">" + phoneticmanufacturer.First().from + "</phoneme> ");
                    }
                    else
                    {
                        sb.Append($"{ship.manufacturer.Split(' ').First()} ");
                    }
                    break;
                case "Faulcon DeLacy":
                    // Last word names (e.g. DeLacy ...)
                    if (phoneticmanufacturer != null && phoneticmanufacturer.Any())
                    {
                        sb.Append("<phoneme alphabet=\"ipa\" ph=\"" + phoneticmanufacturer.Last().to + "\">" + phoneticmanufacturer.Last().from + "</phoneme> ");
                    }
                    else
                    {
                        sb.Append($"{ship.manufacturer.Split(' ').Last()} ");
                    }
                    break;
                default:
                    // Full names (e.g. Core Dynamics ..., Gutamaya ..., Saud Kruger ..., Zorgon Peterson ...)
                    if (phoneticmanufacturer != null && phoneticmanufacturer.Any())
                    {
                        foreach (Translation item in phoneticmanufacturer) { sb.Append("<phoneme alphabet=\"ipa\" ph=\"" + item.to + "\">" + item.from + "</phoneme> "); }
                    }
                    else
                    {
                        sb.Append($"{ship.manufacturer} ");
                    }
                    break;
            }
            sb.Append(Get3LeadingCharacters(id));
            return sb.ToString();
        }

        private static string Get3LeadingCharacters(string input)
        {
            // Obtain the first three characters of the input string, zero padded and converted to ICAO (e.g. "A" becomes "Alpha Zero Zero")
            return Translations.ICAO(new Regex("[^a-zA-Z0-9]")
                .Replace(input, "")
                .ToUpperInvariant()
                .PadRight(3, '0')
                .Substring(0, 3));
        }
    }
}
