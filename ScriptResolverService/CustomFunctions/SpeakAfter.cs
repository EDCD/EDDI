using Cottle;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    [method: UsedImplicitly]
    public class SpeakAfter ( IContext context, IReadOnlyDictionary<string, IScriptDefinition> scripts ) : ICustomFunction
    {
        public string name => "SpeakAfter";
        public FunctionCategory Category => FunctionCategory.Voice;
        public string description => Properties.CustomFunctions_Untranslated.SpeakAfter;
        public Type ReturnType => typeof( string );

        // Supplied for this render only by the Speech Responder. No global callback or background Cottle runtime.
        public Action<DeferredSpeechRequest> Schedule { get; set; }

        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            var scriptName = values[ 0 ].AsString;
            if ( values[ 0 ].Type != ValueContent.String || string.IsNullOrWhiteSpace( scriptName ) ||
                 !scripts.ContainsKey( scriptName ) )
            {
                throw new ArgumentException( "SpeakAfter requires the name of an existing script." );
            }
            if ( values[ 1 ].Type != ValueContent.Number || values[ 1 ].AsNumber < 0 ||
                 values[ 1 ].AsNumber > 30000 || values[ 1 ].AsNumber != Math.Truncate( values[ 1 ].AsNumber ) )
            {
                throw new ArgumentException( "SpeakAfter requires a whole-number delay from 0 to 30000 milliseconds." );
            }

            var waitFor = values.Count > 2 ? values[ 2 ].AsString : string.Empty;
            if ( values.Count > 2 && ( values[ 2 ].Type != ValueContent.String ||
                                      ( waitFor.Length > 0 && !scripts.ContainsKey( waitFor ) ) ) )
            {
                throw new ArgumentException( "SpeakAfter requires an existing script to wait for, or an empty string." );
            }
            var args = values.Count > 3 ? values[ 3 ] : Value.EmptyMap;
            if ( args.Type != ValueContent.Void && args.Type != ValueContent.Map )
            {
                throw new ArgumentException( "SpeakAfter arguments must be a map." );
            }
            if ( Schedule is null )
            {
                throw new InvalidOperationException( "SpeakAfter is only available during ordinary Speech Responder script evaluation; deferred reports cannot schedule more reports." );
            }
            Schedule( new DeferredSpeechRequest( scriptName, (int)values[ 1 ].AsNumber, waitFor, args ) );
            return string.Empty;
        }, 2, 4 );
    }
}
