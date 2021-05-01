﻿using Cottle;
using Cottle.Functions;
using EddiCore;
using EddiDataDefinitions;
using EddiShipMonitor;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using JetBrains.Annotations;
using System.Linq;
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
        public NativeFunction function => new NativeFunction((values) =>
        {
            // The game provides three options for callsigns used by in-game ATC:
            // (1) CommanderName (default): ship manufacturer name (sometimes partial) + first 3 alphanumeric characters of commander name
            // (2) ShipName: ship manufacturer name (sometimes partial) + first 3 alphanumeric characters of ship name
            // (3) ShipID: ship manufacturer name (sometimes partial) + first 3 alphanumeric characters of ship identifier

            int? localId = (values.Count < 0 && values[0].Type != ValueContent.Void 
                ? (int)values[0].AsNumber 
                : (int?)null);
            int? callsignType = (values.Count > 1 && values[1].Type != ValueContent.Void 
                ? (int)values[1].AsNumber 
                : (int?)null);

            ShipMonitor shipMonitor = (ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor");
            Ship ship = localId is null ? shipMonitor.GetCurrentShip() : shipMonitor.GetShip(localId);

            switch (callsignType)
            {
                default:
                    // CommanderName
                    return phoneticCallsign(ship, EDDI.Instance.Cmdr.name);
                case 1:
                    // Variant: ShipName
                    return phoneticCallsign(ship, ship.name);
                case 2:
                    // Variant: ShipID
                    return phoneticCallsign(ship, ship.ident);
            }
        }, 0, 2);

        private static string phoneticCallsign(Ship ship, string id)
        {
            if (string.IsNullOrEmpty(ship?.manufacturer) || string.IsNullOrEmpty(id)) { return string.Empty; }

            // First, obtain the phonetic manufacturer. This may be the complete name or only a partial name, depending on the manufacturer.
            var phoneticmanufacturer = ShipDefinitions.ManufacturerPhoneticNames.FirstOrDefault(m => m.Key == ship.manufacturer).Value;

            string result = "";
            switch (ship.manufacturer)
            {
                case "Core Dynamics":
                case "Gutamaya":
                case "Saud Kruger":
                case "Zorgon Peterson":
                    // Full names
                    foreach (Translation item in phoneticmanufacturer) { result += "<phoneme alphabet=\"ipa\" ph=\"" + item.to + "\">" + item.from + "</phoneme> "; }
                    break;
                case "Lakon Spaceways":
                    // First word names
                    result += "<phoneme alphabet=\"ipa\" ph=\"" + phoneticmanufacturer.First().to + "\">" + phoneticmanufacturer.First().from + "</phoneme> ";
                    break;
                case "Faulcon DeLacy":
                    // Last word names
                    result += "<phoneme alphabet=\"ipa\" ph=\"" + phoneticmanufacturer.Last().to + "\">" + phoneticmanufacturer.Last().from + "</phoneme> ";
                    break;
                default:
                    // Ship model isn't here
                    if (ship.phoneticmanufacturer != null && ship.phoneticmanufacturer.Any())
                    {
                        foreach (Translation item in phoneticmanufacturer) { result += "<phoneme alphabet=\"ipa\" ph=\"" + item.to + "\">" + item.from + "</phoneme> "; }
                    }
                    else
                    {
                        result += ship.manufacturer;
                    }
                    break;
            }

            result += Get3LeadingCharacters(id);

            return result;
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
