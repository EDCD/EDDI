using Cottle;
using EddiCore;
using EddiDataDefinitions;
using JetBrains.Annotations;
using System;
using Utilities;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class CommanderName : ICustomFunction
    {
        public string name => "CommanderName";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.CommanderName;
        public Type ReturnType => typeof( string );

        public IFunction function => Function.CreateNative0( ( runtime, writer ) =>
        {
            var commanderMonitorVariables = EDDI.Instance.ObtainMonitor( "Commander Monitor" ).GetVariableValues();
            if ( commanderMonitorVariables.TryGetValue( "cmdr", out Commander Cmdr ) )
            {
                return Cmdr.SpokenName();
            }

            return "";
        } );
    }
}
