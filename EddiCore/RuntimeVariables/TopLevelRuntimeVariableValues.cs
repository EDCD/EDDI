using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiCore.RuntimeVariables
{
    public static class TopLevelRuntimeVariableValues
    {
        public static IReadOnlyList<RuntimeVariableValue> Build ()
        {
            return Build( new EddiRuntimeVariableContext() );
        }

        internal static IReadOnlyList<RuntimeVariableValue> Build ( IRuntimeVariableContext context )
        {
            return
            [
                RuntimeVariableCatalog.CapiActive.WithValue( context.CapiActive ),
                RuntimeVariableCatalog.DestinationDistanceLy.WithValue( context.GameState.DestinationDistanceLy ),
                RuntimeVariableCatalog.Environment.WithValue( context.GameState.Environment ),
                RuntimeVariableCatalog.Horizons.WithValue( context.GameState.inHorizons ),
                RuntimeVariableCatalog.IcaoActive.WithValue( context.IcaoActive ),
                RuntimeVariableCatalog.IpaActive.WithValue( context.IpaActive ),
                RuntimeVariableCatalog.Odyssey.WithValue( context.GameState.inOdyssey ),
                RuntimeVariableCatalog.SearchDistanceLy.WithValue( context.GameState.SearchDistanceLy ),
                RuntimeVariableCatalog.VaActive.WithValue( context.FromVA ),
                RuntimeVariableCatalog.Vehicle.WithValue( context.GameState.Vehicle ),
                RuntimeVariableCatalog.Version.WithValue( Constants.EDDI_VERSION.ShortString, Constants.EDDI_VERSION.ToString() )
            ];
        }

        public static RuntimeVariableValue Get ( string name )
        {
            return Build().Single( value => value.Name == name );
        }

        internal static RuntimeVariableValue Get ( string name, IRuntimeVariableContext context )
        {
            return Build( context ).Single( value => value.Name == name );
        }
    }
}
