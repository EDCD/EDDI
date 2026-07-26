using Cottle;
using EddiSpeechService;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class GetPendingSpeech : ICustomFunction
    {
        public string name => "GetPendingSpeech";
        public FunctionCategory Category => FunctionCategory.Voice;
        public string description => Properties.CustomFunctions_Untranslated.GetPendingSpeech;
        public Type ReturnType => typeof( List<EddiSpeech> );
        public IFunction function => Function.CreateNative1( ( _, _, _ ) =>
        {
            var pendingSpeech = SpeechService.Instance.speechQueue.priorityQueues
                .SelectMany( s => s )
                .ToList();

            return Value.FromReflection( pendingSpeech, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        } );
    }
}
