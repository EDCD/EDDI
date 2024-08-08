using Cottle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiSpeechResponder.Service
{
    public interface ICustomFunction
    {
        string name { get; }
        FunctionCategory Category { get; }
        string description { get; }
        Type ReturnType { get; }
        IFunction function { get; }
    }

    public class RecursiveFunction
    {
        private IContext TopLevelContext { get; }

        private static Dictionary<Value, Value> RuntimeGlobals { get; set; } = new Dictionary<Value, Value>();

        protected Dictionary<string, Script> Scripts { get; }

        protected RecursiveFunction ( IContext context, Dictionary< string, Script > scripts )
        {
            this.TopLevelContext = context;
            this.Scripts = scripts;
            RuntimeGlobals.Clear();
        }

        protected IContext GetContext ( IMap globals )
        {
            RuntimeGlobals = new[]
                {
                    globals.ToDictionary( g => g.Key, g => g.Value ),
                    RuntimeGlobals.Where( g => !globals.Contains( g.Key ) )
                }
                .SelectMany( dict => dict )
                .ToDictionary( pair => pair.Key, pair => pair.Value );
            var runtimeState = new Dictionary<Value, Value> { [ "state" ] = ScriptResolver.buildState() }
                .Where( pair => !RuntimeGlobals.ContainsKey( pair.Key ) );
            var latestContext = Context.CreateBuiltin( new[] { RuntimeGlobals, runtimeState }
                .SelectMany( dict => dict )
                .ToDictionary( pair => pair.Key, pair => pair.Value ) );
            return Context.CreateCascade( latestContext, TopLevelContext );
        }
    }

    public enum FunctionCategory
    {
        Details,
        Dynamic,
        Galnet,
        Hidden,
        Phonetic,
        Tempo,
        Utility,
        Voice
    }
}
