using Cottle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiSpeechResponder.ScriptResolverService
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
        private IContext ParentContext { get; }

        private static Dictionary<Value, Value> RuntimeGlobals { get; set; } = new();
        private static readonly object globalsLock = new();

        protected Dictionary<string, Script> Scripts { get; }

        protected RecursiveFunction ( IContext context, Dictionary< string, Script > scripts )
        {
            this.ParentContext = context;
            this.Scripts = scripts;
            lock ( globalsLock )
            {
                RuntimeGlobals.Clear();
            }
        }

        protected IContext GetContext ( IMap globals )
        {
            IContext latestContext;
            lock ( globalsLock )
            {
                RuntimeGlobals = new[]
                    {
                        globals.ToDictionary( g => g.Key, g => g.Value ),
                        RuntimeGlobals.Where( g => !globals.Contains( g.Key ) )
                    }
                    .SelectMany( dict => dict )
                    .ToDictionary( pair => pair.Key, pair => pair.Value );
                RuntimeGlobals[ "state" ] = ScriptResolver.buildState();
                latestContext = Context.CreateBuiltin( RuntimeGlobals );
            }
            return Context.CreateCascade( latestContext, ParentContext );
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
