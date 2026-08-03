using Cottle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiScriptResolverService
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

        protected IReadOnlyDictionary<string, IScriptDefinition> Scripts { get; }

        protected RecursiveFunction ( IContext context, IReadOnlyDictionary<string, IScriptDefinition> scripts )
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

        protected string InvokeScript ( IMap globals, IReadOnlyList<Value> values )
        {
            var scriptName = values[ 0 ].AsString;
            if ( values.Count > 1 && values[ 1 ].Type != ValueContent.Map )
            {
                throw new ArgumentException ( $"The function invoking {scriptName} has arguments which are not a map value." );
            }

            return ScriptInvoker.ResolveFromName(
                scriptName,
                Scripts,
                GetContext( globals ),
                false,
                false,
                values.Count > 1,
                values.Count > 1 ? values[ 1 ] : default )?.Trim();
        }

        protected string RenderApproximateNumber ( IMap globals, Value input )
        {
            var number = (decimal?)Convert.ToDecimal( input.AsNumber );
            return ApproximateNumber.Render( number, Scripts, GetContext( globals ) );
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
