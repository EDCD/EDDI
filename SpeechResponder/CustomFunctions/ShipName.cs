using Cottle;
using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.ScriptResolverService;
using JetBrains.Annotations;
using System;
using System.Linq;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ShipName : ICustomFunction
    {
        public string name => "ShipName";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.ShipName;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            int? localId = (values.Count == 0 ? (int?) null : (int) values[0].AsNumber);
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

            var shipyard = ConfigService.Instance.shipMonitorConfiguration?.shipyard;
            var ship = localId is null
                ? EDDI.Instance.CurrentShip
                : shipyard?.FirstOrDefault(s => s.LocalId == localId)
                  ?? ShipDefinitions.FromModel(model)
                  ?? ShipDefinitions.FromEDModel(model);
            return ship?.SpokenName();
        }, 0, 2);
    }
}
