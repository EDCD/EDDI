using Cottle;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiScriptResolverService
{
    public static class ScriptInvoker
    {
        public static string ResolveFromName (
            string name,
            IReadOnlyDictionary<string, IScriptDefinition> scripts,
            IContext context,
            bool isTopLevelScript,
            bool reportConfigurationErrors = false,
            bool hasArgs = false,
            Value args = default )
        {
            if ( scripts is null || !scripts.TryGetValue( name, out var script ) || script?.Value is null )
            {
                Logging.Debug( $"No {name} script found" );
                return reportConfigurationErrors
                    ? $"Cottle speech system configuration error: {name} script not found."
                    : null;
            }

            if ( !script.Enabled )
            {
                Logging.Debug( $"{name} script disabled" );
                return reportConfigurationErrors
                    ? $"Cottle speech system configuration error: {name} script is disabled."
                    : null;
            }

            if ( !isTopLevelScript && reportConfigurationErrors && string.IsNullOrWhiteSpace( script.Value ) )
            {
                Logging.Warn( $"{name} script disabled" );
                return $"Cottle speech system configuration error: {name} script is empty.";
            }

            var renderContext = hasArgs
                ? Context.CreateCascade(
                    Context.CreateBuiltin( new Dictionary<Value, Value> { [ "args" ] = args } ),
                    context )
                : context;

            return ScriptResolver.resolveFromValue(
                script.Value,
                renderContext,
                isTopLevelScript,
                script,
                GetIncludedScripts( script, scripts ) );
        }

        private static Dictionary<string, string> GetIncludedScripts (
            IScriptDefinition script,
            IReadOnlyDictionary<string, IScriptDefinition> scripts )
        {
            var includedScriptNames = ( script.includes ?? string.Empty ).Split( ';' ).Select( i => i.Trim() );
            var includedScripts = new Dictionary<string, string>();
            foreach ( var scriptName in includedScriptNames )
            {
                var includedScript = scripts.FirstOrDefault( s =>
                    s.Key.Equals( scriptName, StringComparison.InvariantCultureIgnoreCase ) ).Value;
                if ( includedScript != null )
                {
                    includedScripts.Add( includedScript.Name, includedScript.Value );
                }
            }

            return includedScripts;
        }
    }
}
