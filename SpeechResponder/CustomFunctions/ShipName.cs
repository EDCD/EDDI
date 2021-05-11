using Cottle.Functions;
using EddiCore;
using EddiDataDefinitions;
using EddiShipMonitor;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ShipName : ICustomFunction
    {
        public string name => "ShipName";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.ShipName;
        public NativeFunction function => new NativeFunction((values) =>
        {
            int? localId = (values.Count == 0 ? (int?)null : (int)values[0].AsNumber);
            string model = (values.Count == 2 ? values[1].AsString : null);

            if (localId is null && model is null)
            {
                if (EDDI.Instance.Vehicle == Constants.VEHICLE_TAXI)
                {
                    return EddiDataDefinitions.Properties.Ship.yourTransport;
                }
                if (EDDI.Instance.Vehicle == Constants.VEHICLE_MULTICREW)
                {
                    return EddiDataDefinitions.Properties.Ship.yourShip;
                }
            }
            ShipMonitor shipMonitor = (ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor");
            Ship ship = shipMonitor.GetShip(localId, model);
            return ship.SpokenName();
        }, 0, 2);
    }
}
