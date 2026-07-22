using Cottle;
using Cottle.Exceptions;
using EddiCore;
using EddiCore.RuntimeVariables;
using EddiDataDefinitions;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiSpeechResponder.ScriptResolverService
{
    public class ScriptResolver ( Dictionary<string, Script> scripts = null )
    {
        public readonly Dictionary<string, Script> Scripts = scripts ?? new Dictionary<string, Script>();
        public static readonly DocumentConfiguration documentConfiguration = new()
        {
            Trimmer = DocumentConfiguration.TrimRepeatedWhitespaces, NbCycleMax = 100000
        };

        // The file to log speech
        [UsedImplicitly] public static readonly string LogFile = Constants.DATA_DIR + @"\speechresponder.out";

        public int priority(string name)
        {
            Scripts.TryGetValue(name, out var script);
            return script?.Priority ?? 3;
        }

        /// <summary> From a custom dictionary of variable values in the default context </summary>
        public string resolveFromName(string name, Dictionary<string, Tuple<Type, Value>> vars, bool isTopLevelScript)
        {
            var context = buildContext(vars, Scripts);
            return resolveFromName(name, Scripts, context, isTopLevelScript);
        }

        /// <summary> From a custom context </summary>
        public static string resolveFromName(string name, IDictionary<string, Script> scripts, IContext context, bool isTopLevelScript)
        {
            if (!scripts.TryGetValue(name, out var script) || 
                script?.Value is null)
            {
                Logging.Debug($"No {name} script found");
                return null;
            }
            if (!script.Enabled)
            {
                Logging.Debug($"{name} script disabled");
                return null;
            }
            var scriptValue = script.Value;

            // Prepend included scripts as appropriate
            var includedScriptNames = (script.includes ?? string.Empty).Split( ';' ).Select( i => i.Trim() ).ToList();
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

            return resolveFromValue(scriptValue, context, isTopLevelScript, script, includedScripts );
        }

        /// <summary> From the default dictionary of variable values in the default context </summary>
        public string resolveFromValue(string scriptValue, bool isTopLevelScript)
        {
            var vars = CompileVariables();
            var context = buildContext(vars);
            return resolveFromValue(scriptValue, context, isTopLevelScript);
        }

        /// <summary> From a custom context </summary>
        public static string resolveFromValue(string script, IContext context, bool isTopLevelScript, Script scriptObject = null, Dictionary<string, string> includedScripts = null)
        {
            var templateBuilder = new TemplateBuilder();

            try
            {
                // Combine any included scripts with our main script
                if ( includedScripts != null )
                {
                    foreach ( var includedScript in includedScripts )
                    {
                        templateBuilder.Append(includedScript.Key, includedScript.Value, true);
                    }
                }
                templateBuilder.Append(scriptObject?.Name, script, false);
                script = templateBuilder.Render();

                //If this is not a top level script then we need to preserve escape sequence characters (\).
                if ( !isTopLevelScript )
                {
                    script = GeneratedRegex.EscapeCharacterRegex().Replace( script, @"\\\\" );
                }

                var documentResult = Document.CreateDefault( script, documentConfiguration );
                if ( !documentResult.Success )
                {
                    foreach ( var report in documentResult.Reports )
                    {
                        // Errors will be handled through the ParseException class so we're only concerned with warnings and notices here.
                        if ( report.Severity is DocumentSeverity.Warning )
                        {
                            Logging.Warn( @"Cottle Parser Warning:", report );
                        }

                        if ( report.Severity is DocumentSeverity.Notice )
                        {
                            Logging.Debug( @"Cottle Parser Suggestion:", report );
                        }
                    }
                }

                var document = documentResult.DocumentOrThrow;

                var result = document.Render( context );

                // Tidy up the output script
                if ( isTopLevelScript )
                {
                    result = GeneratedRegex.TabsOrTwoOrMoreSpacesRegex().Replace( result, " " )
                        .Replace( " ,", "," )
                        .Replace( " .", "." ).Trim();
                    result = result.Trim() == "" ? null : result.Trim();
                }

                Logging.Debug( $"Turned {( isTopLevelScript ? $"top level '{scriptObject?.Name}" : "" )}' script '{script}' into speech '{result}'" );

                if ( isTopLevelScript && result != null )
                {
                    var stored = result;
                    // Remove any leading pause
                    if ( stored.StartsWith( "<break" ) )
                    {
                        var pattern = "^<break[^>]*>";
                        var replacement = "";
                        var rgx = new Regex( pattern );
                        stored = rgx.Replace( stored, replacement );
                    }

                    EDDI.Instance.State[ "eddi_context_last_speech" ] = stored;
                }

                return result;
            }
            catch ( ParseException e )
            {
                // Report the failing the script name, if it is available
                string scriptName;
                int scriptLine;
                if ( e.Line > 0 )
                {
                    templateBuilder.FetchTemplateItemFromLine( e.Line, out scriptName, out scriptLine );
                }
                else
                {
                    templateBuilder.FetchTemplateItemFromOffset( script, e.LocationStart, out scriptName, out scriptLine );
                }
                if ( !string.IsNullOrEmpty(scriptName) )
                {
                    scriptName = "the script \"" + scriptName + "\"";
                }
                else
                {
                    scriptName = "this script";
                }

                var rejectedSubstring = e.LocationStart >= 0 && script.Length >= (e.LocationStart + e.LocationLength)
                    ? script.Substring( e.LocationStart, e.LocationLength )
                    : string.Empty;
                Logging.Warn( $"Failed to resolve {scriptName} at line {scriptLine}. {e}" );
                return $"There is a problem with {scriptName} at line {scriptLine}. {errorTranslation( e.Message + rejectedSubstring )}";
            }
            catch ( ArgumentOutOfRangeException aoore )
            {
                Logging.Warn( aoore.Message, aoore );
                return $"Error with {scriptObject?.Name ?? "this"} script: {aoore.Message}";
            }
            catch ( ConfigException ce )
            {
                Logging.Error( ce.Message, ce );
                return $"Cottle speech system configuration error: {ce.Message}";
            }
            catch ( IndexOutOfRangeException ioore )
            {
                Logging.Warn( ioore.Message, ioore );
                return $"Error with {scriptObject?.Name ?? "this"} script: {ioore.Message}";
            }
            catch ( TargetParameterCountException tpce )
            {
                Logging.Warn( tpce.Message, tpce );
                return $"Error with {scriptObject?.Name ?? "this"} script: {tpce.Message}";
            }
            catch ( Exception e )
            {
                Logging.Error( e.Message, e );
                return $"Error with {scriptObject?.Name ?? "this"} script: {e.Message}";
            }
        }

        private static string errorTranslation(string msg)
        {
            // Give human readable descriptions for select cottle errors
            return msg
                    .Replace("'", "")
                    .Replace("<EOF>", "opening curly bracket")
                    .Replace("<eof>", "incomplete expression")
                    .Replace("{", "opening curly bracket")
                    .Replace("}", "closing curly bracket");
        }

        // Compile variables from the EDDI information
        protected internal Dictionary<string, Tuple<Type, Value>> CompileVariables ( dynamic theEvent = null )
        {
            try
            {
                var dict = new Dictionary<string, Tuple<Type, Value>>
                {
                    // Boolean constants
                    [ "true" ] = new( typeof(bool), true ),
                    [ "false" ] = new( typeof(bool), false ),
                };

                AddRuntimeVariables( dict, TopLevelRuntimeVariableValues.Build() );

                // Standard objects

                if ( EDDI.Instance.GameState.CurrentStarSystem != null )
                {
                    dict[ "system" ] = new Tuple<Type, Value>( typeof(StarSystem),
                        Value.FromReflection( EDDI.Instance.GameState.CurrentStarSystem,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
                }

                if ( EDDI.Instance.GameState.LastStarSystem != null )
                {
                    dict[ "lastsystem" ] = new Tuple<Type, Value>( typeof(StarSystem),
                        Value.FromReflection( EDDI.Instance.GameState.LastStarSystem,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
                }

                if ( EDDI.Instance.GameState.NextStarSystem != null )
                {
                    dict[ "nextsystem" ] = new Tuple<Type, Value>( typeof(StarSystem),
                        Value.FromReflection( EDDI.Instance.GameState.NextStarSystem,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
                }

                if ( EDDI.Instance.GameState.DestinationStarSystem != null )
                {
                    dict[ "destinationsystem" ] = new Tuple<Type, Value>( typeof(StarSystem),
                        Value.FromReflection( EDDI.Instance.GameState.DestinationStarSystem,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
                }

                if ( EDDI.Instance.SearchStarSystem != null )
                {
                    dict[ "searchsystem" ] = new Tuple<Type, Value>( typeof(StarSystem),
                        Value.FromReflection( EDDI.Instance.SearchStarSystem,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
                }

                if ( EDDI.Instance.SearchStation != null )
                {
                    dict[ "searchstation" ] = new Tuple<Type, Value>( typeof(Station),
                        Value.FromReflection( EDDI.Instance.SearchStation,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
                }

                if ( EDDI.Instance.GameState.CurrentStation != null )
                {
                    dict[ "station" ] = new Tuple<Type, Value>( typeof(Station),
                        Value.FromReflection( EDDI.Instance.GameState.CurrentStation,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
                }

                if ( EDDI.Instance.GameState.CurrentStellarBody != null )
                {
                    dict[ "body" ] = new Tuple<Type, Value>( typeof(Body),
                        Value.FromReflection( EDDI.Instance.GameState.CurrentStellarBody,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
                }

                if ( theEvent != null ) // A dynamic type is used so that Value.FromReflection 
                {
                    dict[ "event" ] = new Tuple<Type, Value>( theEvent.GetType(),
                        Value.FromReflection( theEvent,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
                }

                if ( EDDI.Instance.State != null )
                {
                    dict[ "state" ] = new Tuple<Type, Value>( typeof(IDictionary<string, object>), buildState() );
                    Logging.Debug( "State is: ", EDDI.Instance.State );
                }

                // Obtain additional variables from each monitor
                foreach ( var monitor in EDDI.Instance.monitors )
                {
                    AddRuntimeVariables( dict, monitor.GetVariableValues() );
                }

                return dict;

            }
            catch ( FileLoadException ex ) when ( (uint)ex.HResult == 0x800711C7 ) // Application Control (Smart App Control / WDAC / AppLocker) blocked a DLL
            {
                var blockedAssembly = ex.FileName ?? "an assembly required for script variables";

                // Telemetry: keep the original exception intact
                Logging.Warn( $"Failed to compile Speech Responder variables. Windows Application Control blocked access to {blockedAssembly}. This usually happens on systems with Smart App Control or corporate security policies.", ex );

                // Fail as gracefully as possible: return an empty variable set.
                return new Dictionary<string, Tuple<Type, Value>>();
            }
        }

        private static void AddRuntimeVariables (
            Dictionary<string, Tuple<Type, Value>> dict,
            IEnumerable<RuntimeVariableValue> values )
        {
            foreach ( var variableValue in values )
            {
                if ( variableValue.Value == null )
                {
                    dict.Remove( variableValue.Name );
                    continue;
                }

                dict[ variableValue.Name ] = new Tuple<Type, Value>( variableValue.Type, ToCottleValue( variableValue.Value ) );
            }
        }

        private static Value ToCottleValue ( object value )
        {
            return value switch
            {
                bool boolValue => boolValue,
                decimal decimalValue => decimalValue,
                double doubleValue => doubleValue,
                float floatValue => (decimal)floatValue,
                int intValue => intValue,
                long longValue => longValue,
                string stringValue => stringValue,
                _ => Value.FromReflection(
                    (dynamic)value,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
            };
        }

        /// <summary>
        /// Build a context from a list of variables
        /// </summary>
        public static IContext buildContext (Dictionary<string, Tuple<Type, Value>> vars = null, IDictionary<string, Script> scripts = null )
        {
            var context = new Dictionary<Value, Value>();

            // Variables
            if (vars != null)
            {
                foreach (var entry in vars)
                {
                    context[ entry.Key ] = entry.Value.Item2;
                }
            }

            // Loop through our custom functions and add them to the context.
            foreach ( var function in GetCustomFunctions( context, scripts ) )
            {
                context[ function.name ] = Value.FromFunction( function.function );
            }

            return Context.CreateBuiltin( context );
        }

        public static Dictionary<Value, Value> buildState()
        {
            if (EDDI.Instance.State == null)
            {
                return null;
            }

            var state = new Dictionary<Value, Value>();
            foreach (var key in EDDI.Instance.State.Keys)
            {
                var value = EDDI.Instance.State[key];
                if (value == null)
                {
                    // Null values should not be included in our Cottle state
                    continue;
                }
                var valueType = value.GetType();
                if (valueType == typeof(string))
                {
                    state[key] = (string)value;
                }
                else if (valueType == typeof(int))
                {
                    state[key] = (int)value;
                }
                else if (valueType == typeof(bool))
                {
                    state[key] = (bool)value;
                }
                else if (valueType == typeof(decimal))
                {
                    state[key] = (decimal)value;
                }
            }
            return state;
        }

        public static List<ICustomFunction> GetCustomFunctions ( Dictionary<Value, Value> dict, IDictionary<string, Script> scripts = null )
        {
            return GetCustomFunctions( Context.CreateBuiltin( dict ), scripts );
        }

        public static List<ICustomFunction> GetCustomFunctions ( IContext context = null, IDictionary<string, Script> scripts = null )
        {
            var functionsList = new List<ICustomFunction>();
            var assy = Assembly.GetAssembly( typeof(ScriptResolver) );
            if ( assy != null )
            {
                foreach ( var type in assy.GetTypes()
                             .Where( t => t.IsClass && t.GetInterface( nameof(ICustomFunction) ) != null ) )
                {
                    var function = (ICustomFunction)( type.GetConstructor( Type.EmptyTypes ) != null
                            ? Activator.CreateInstance( type )
                            : Activator.CreateInstance( type, context ?? Context.Empty, scripts ?? new Dictionary<string, Script>() ) );

                    if ( function != null )
                    {
                        functionsList.Add( function );
                    }
                }
            }

            return functionsList;
        }
    }
}
