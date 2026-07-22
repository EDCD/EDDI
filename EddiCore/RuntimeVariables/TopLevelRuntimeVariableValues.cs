using EddiCompanionAppService;
using EddiConfigService;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiCore.RuntimeVariables
{
    public static class TopLevelRuntimeVariableValues
    {
        public static IReadOnlyList<RuntimeVariableValue> Build ( decimal searchDistanceLy )
        {
            return
            [
                new( RuntimeVariableCatalog.CapiActiveVariable, typeof(bool), CompanionAppService.Instance?.active ?? false ),
                new( RuntimeVariableCatalog.DestinationDistanceLyVariable, typeof(decimal), EDDI.Instance.GameState.DestinationDistanceLy ),
                new( RuntimeVariableCatalog.EnvironmentVariable, typeof(string), EDDI.Instance.GameState.Environment ),
                new( RuntimeVariableCatalog.HorizonsVariable, typeof(bool), EDDI.Instance.GameState.inHorizons ),
                new( RuntimeVariableCatalog.IcaoActiveVariable, typeof(bool), ConfigService.Instance.speechServiceConfiguration.EnableIcao ),
                new( RuntimeVariableCatalog.IpaActiveVariable, typeof(bool), !ConfigService.Instance.speechServiceConfiguration.DisableIpa ),
                new( RuntimeVariableCatalog.OdysseyVariable, typeof(bool), EDDI.Instance.GameState.inOdyssey ),
                new( RuntimeVariableCatalog.SearchDistanceLyVariable, typeof(decimal), searchDistanceLy ),
                new( RuntimeVariableCatalog.VaActiveVariable, typeof(bool), EDDI.Instance.FromVA ),
                new( RuntimeVariableCatalog.VehicleVariable, typeof(string), EDDI.Instance.GameState.Vehicle ),
                new( RuntimeVariableCatalog.VersionVariable, typeof(string), Constants.EDDI_VERSION.ShortString, Constants.EDDI_VERSION.ToString() )
            ];
        }

        public static RuntimeVariableValue Get ( string name, decimal searchDistanceLy )
        {
            return Build( searchDistanceLy ).Single( value => value.Name == name );
        }
    }
}
