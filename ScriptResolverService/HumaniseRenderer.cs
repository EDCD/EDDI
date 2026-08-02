using Cottle;
using System.Collections.Generic;
using System.Threading;

namespace EddiScriptResolverService
{
    public static class HumaniseRenderer
    {
        private const string ScriptName = "Humanise";
        private static readonly AsyncLocal<bool> RenderingScript = new();

        public static string Render (
            decimal? number,
            IReadOnlyDictionary<string, IScriptDefinition> scripts,
            IContext context = null )
        {
            if ( number is null )
            {
                return string.Empty;
            }

            if ( RenderingScript.Value )
            {
                return "Cottle speech system configuration error: Recursive Humanise() calls are not supported.";
            }

            if ( scripts is null || !scripts.TryGetValue( ScriptName, out var script ) )
            {
                return "Cottle speech system configuration error: Humanise script not found.";
            }

            if ( script?.Enabled != true )
            {
                return "Cottle speech system configuration error: Humanise script is disabled.";
            }

            if ( string.IsNullOrWhiteSpace( script.Value ) )
            {
                return "Cottle speech system configuration error: Humanise script is empty.";
            }

            var renderContext = context ?? ScriptResolver.buildContext( null, scripts );
            Value args = new Dictionary<Value, Value> { [ "number" ] = number.Value };

            try
            {
                RenderingScript.Value = true;
                var result = ScriptInvoker.ResolveFromName(
                    ScriptName,
                    scripts,
                    renderContext,
                    false,
                    true,
                    true,
                    args );
                return result?.Trim();
            }
            finally
            {
                RenderingScript.Value = false;
            }
        }
    }
}
