using Cottle;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.ScriptResolverService;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
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
            var commanderMonitorVariables = EDDI.Instance.ObtainMonitor( "Commander Monitor" ).GetVariables();
            if ( commanderMonitorVariables.TryGetValue( "cmdr", out var tuple ) && tuple.Item2 is Commander Cmdr )
            {
                return Cmdr.SpokenName();
            }

            return "";
        } );
    }
}
