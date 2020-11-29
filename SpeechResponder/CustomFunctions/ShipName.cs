using Cottle.Functions;
using EddiCore;
using EddiDataDefinitions;
using EddiShipMonitor;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ShipName : ICustomFunction
    {
        public string name => "ShipName";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => @"
This function will provide the name of your ship.

If you have set up a phonetic name for your ship it will return that, otherwise if you have set up a name for your ship it will return that. The phonetic name uses SSML tags.

ShipName() takes an optional ship ID for which to provide the name. If no argument is supplied then it provides the name for your current ship.

If you have not set up a name for your ship it will just return ""your ship"".";
        public NativeFunction function => new NativeFunction((values) =>
        {
            int? localId = (values.Count == 0 ? (int?)null : (int)values[0].AsNumber);
            string model = (values.Count == 2 ? values[1].AsString : null);
            ShipMonitor shipMonitor = (ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor");
            Ship ship = shipMonitor.GetShip(localId, model);
            return ship.SpokenName();
        }, 0, 2);
    }
}
