using EddiCompanionAppService;
using EddiConfigService;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiCore.RuntimeVariables
{
    public static class TopLevelRuntimeVariableValues
    {
        public static IReadOnlyList<RuntimeVariableValue> Build ()
        {
            return
            [
                RuntimeVariableCatalog.CapiActive.WithValue( CompanionAppService.Instance?.active ?? false ),
                RuntimeVariableCatalog.DestinationDistanceLy.WithValue( EDDI.Instance.GameState.DestinationDistanceLy ),
                RuntimeVariableCatalog.Environment.WithValue( EDDI.Instance.GameState.Environment ),
                RuntimeVariableCatalog.Horizons.WithValue( EDDI.Instance.GameState.inHorizons ),
                RuntimeVariableCatalog.IcaoActive.WithValue( ConfigService.Instance.speechServiceConfiguration.EnableIcao ),
                RuntimeVariableCatalog.IpaActive.WithValue( !ConfigService.Instance.speechServiceConfiguration.DisableIpa ),
                RuntimeVariableCatalog.Odyssey.WithValue( EDDI.Instance.GameState.inOdyssey ),
                RuntimeVariableCatalog.SearchDistanceLy.WithValue( EDDI.Instance.GameState.SearchDistanceLy ),
                RuntimeVariableCatalog.VaActive.WithValue( EDDI.Instance.FromVA ),
                RuntimeVariableCatalog.Vehicle.WithValue( EDDI.Instance.GameState.Vehicle ),
                RuntimeVariableCatalog.Version.WithValue( Constants.EDDI_VERSION.ShortString, Constants.EDDI_VERSION.ToString() )
            ];
        }

        public static RuntimeVariableValue Get ( string name )
        {
            return Build().Single( value => value.Name == name );
        }
    }
}
