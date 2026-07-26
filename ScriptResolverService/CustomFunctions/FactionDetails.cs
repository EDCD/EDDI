using Cottle;
using EddiCore;
using EddiDataDefinitions;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class FactionDetails : ICustomFunction
    {
        public string name => "FactionDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.FactionDetails;
        public Type ReturnType => typeof( Faction );

        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            Faction result;
            if (values.Count == 0)
            {
                result = EDDI.Instance.GameState.CurrentStarSystem?.Faction;
            }
            else if (values.Count == 1)
            {
                result = EDDI.Instance.DataProvider.FetchFactionByNameAsync( values[ 0 ].AsString )?.GetAwaiter().GetResult();
            }
            else
            {
                result = EDDI.Instance.DataProvider.FetchFactionByNameAsync( values[0].AsString, values[1].AsString )?.GetAwaiter().GetResult();
            }
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        }, 1, 2);
    }
}
