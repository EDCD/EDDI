using Cottle;
using Cottle.Exceptions;
using Eddi;
using EddiCompanionAppService;
using EddiCore;
using EddiDataDefinitions;
using EddiNavigationService;
using EddiSpeechService;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiSpeechResponder.Service
{
    public class ScriptResolver
    {
        public readonly Dictionary<string, Script> Scripts;
        public static readonly DocumentConfiguration documentConfiguration = new DocumentConfiguration
        {
            Trimmer = DocumentConfiguration.TrimRepeatedWhitespaces, NbCycleMax = 100000
        };

        // The file to log speech
        [UsedImplicitly] public static readonly string LogFile = Constants.DATA_DIR + @"\speechresponder.out";

        public ScriptResolver(Dictionary<string, Script> scripts = null)
        {
            this.Scripts = scripts ?? new Dictionary<string, Script>();
        }

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
            if (script.Enabled == false)
            {
                Logging.Debug($"{name} script disabled");
                return null;
            }

            return resolveFromValue(script.Value, context, isTopLevelScript, script);
        }

        /// <summary> From the default dictionary of variable values in the default context </summary>
        public string resolveFromValue(string scriptValue, bool isTopLevelScript)
        {
            var vars = CompileVariables();
            var context = buildContext(vars);
            return resolveFromValue(scriptValue, context, isTopLevelScript);
        }

        /// <summary> From a custom context </summary>
        public static string resolveFromValue(string script, IContext context, bool isTopLevelScript, Script scriptObject = null)
        {
            try
            {
                Logging.Debug( $"Resolving {( isTopLevelScript ? "top level " : "" )}script {scriptObject?.Name}: {script}", context );

                //If this is not a top level script then we need to preserve escape sequence characters (\).
                if ( !isTopLevelScript )
                {
                    script = Regex.Replace( script, @"\\", @"\\\\" );
                }

                var documentResult = Document.CreateDefault( script, documentConfiguration );
                if ( !documentResult.Success )
                {
                    foreach ( var report in documentResult.Reports )
                    {
                        // Errors will be handled through the ParseException class so we're only concerned with warnings and notices here.
                        if ( report.Severity is DocumentSeverity.Warning )
                        {
                            Logging.Warn( @"Cottle Parser Warning", report );
                        }
                        if ( report.Severity is DocumentSeverity.Notice )
                        {
                            Logging.Debug( @"Cottle Parser Suggestion", report );
                        }
                    }
                }
                var document = documentResult.DocumentOrThrow;

                var result = document.Render( context );

                // Tidy up the output script
                if ( isTopLevelScript )
                {
                    result = Regex.Replace( result, " +", " " ).Replace( " ,", "," ).Replace( " .", "." ).Trim();
                    Logging.Debug( $"Turned {scriptObject?.Name} script into speech '{result}'" );
                    result = result.Trim() == "" ? null : result.Trim();
                }

                if ( isTopLevelScript && result != null )
                {
                    string stored = result;
                    // Remove any leading pause
                    if ( stored.StartsWith( "<break" ) )
                    {
                        string pattern = "^<break[^>]*>";
                        string replacement = "";
                        Regex rgx = new Regex( pattern );
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
                if ( scriptObject != null )
                {
                    scriptName = "the script \"" + scriptObject.Name + "\"";
                }
                else
                {
                    scriptName = "this script";
                }

                Logging.Warn( $"Failed to resolve {scriptName} at line {e.Line}. {e}" );
                return $"There is a problem with {scriptName} at line {e.Line}. {errorTranslation( e.Message )}";
            }
            catch ( ConfigException ce )
            {
                Logging.Error( ce.Message, ce );
                return $"Cottle speech system configuration error: {ce.Message}";
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
        protected internal Dictionary<string, Tuple<Type, Value>> CompileVariables(dynamic theEvent = null)
        {
            var dict = new Dictionary<string, Tuple<Type, Value>>
            {
                // Boolean constants
                ["true"] = new Tuple<Type, Value>(typeof(bool), true),
                ["false"] = new Tuple<Type, Value>(typeof(bool), false),

                // Standard simple variables
                ["capi_active"] = new Tuple<Type, Value>(typeof(bool), CompanionAppService.Instance?.active ?? false),
                ["destinationdistance"] = new Tuple<Type, Value>(typeof(decimal), EDDI.Instance.DestinationDistanceLy),
                ["searchdistance"] = new Tuple<Type, Value>(typeof(decimal), NavigationService.Instance.SearchDistanceLy),
                ["environment"] = new Tuple<Type, Value>(typeof(string), EDDI.Instance.Environment),
                ["horizons"] = new Tuple<Type, Value>(typeof(bool), EDDI.Instance.inHorizons),
                ["odyssey"] = new Tuple<Type, Value>(typeof(bool), EDDI.Instance.inOdyssey),
                ["va_active"] = new Tuple<Type, Value>(typeof(bool), App.FromVA),
                ["vehicle"] = new Tuple<Type, Value>(typeof(string), EDDI.Instance.Vehicle),
                ["icao_active"] = new Tuple<Type, Value>(typeof(bool), SpeechService.Instance.Configuration.EnableIcao),
                ["ipa_active"] = new Tuple<Type, Value>(typeof(bool), !SpeechService.Instance.Configuration.DisableIpa),
                ["version"] = new Tuple<Type, Value>(typeof(string), Constants.EDDI_VERSION.ShortString)
            };

            // Standard objects
            if ( EDDI.Instance.Cmdr != null )
            {
                dict[ "cmdr" ] = new Tuple<Type, Value>( typeof( Commander ), Value.FromReflection( EDDI.Instance.Cmdr, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( EDDI.Instance.HomeStarSystem != null )
            {
                dict[ "homesystem" ] = new Tuple<Type, Value>( typeof( StarSystem ), Value.FromReflection(EDDI.Instance.HomeStarSystem, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( EDDI.Instance.HomeStation != null )
            {
                dict[ "homestation" ] = new Tuple<Type, Value>( typeof( Station ), Value.FromReflection(EDDI.Instance.HomeStation, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( EDDI.Instance.SquadronStarSystem != null )
            {
                dict[ "squadronsystem" ] = new Tuple<Type, Value>( typeof( StarSystem ), Value.FromReflection(EDDI.Instance.SquadronStarSystem, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( EDDI.Instance.CurrentStarSystem != null )
            {
                dict[ "system" ] = new Tuple<Type, Value>( typeof( StarSystem ), Value.FromReflection(EDDI.Instance.CurrentStarSystem, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( EDDI.Instance.LastStarSystem != null )
            {
                dict[ "lastsystem" ] = new Tuple<Type, Value>( typeof( StarSystem ), Value.FromReflection(EDDI.Instance.LastStarSystem, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( EDDI.Instance.NextStarSystem != null )
            {
                dict[ "nextsystem" ] = new Tuple<Type, Value>( typeof( StarSystem ), Value.FromReflection(EDDI.Instance.NextStarSystem, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( EDDI.Instance.DestinationStarSystem != null )
            {
                dict[ "destinationsystem" ] = new Tuple<Type, Value>( typeof( StarSystem ), Value.FromReflection(EDDI.Instance.DestinationStarSystem, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( NavigationService.Instance.SearchStarSystem != null )
            {
                dict[ "searchsystem" ] = new Tuple<Type, Value>( typeof( StarSystem ), Value.FromReflection(NavigationService.Instance.SearchStarSystem, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( NavigationService.Instance.SearchStation != null )
            {
                dict[ "searchstation" ] = new Tuple<Type, Value>( typeof( Station ), Value.FromReflection(NavigationService.Instance.SearchStation, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( EDDI.Instance.CurrentStation != null )
            {
                dict[ "station" ] = new Tuple<Type, Value>( typeof( Station ), Value.FromReflection(EDDI.Instance.CurrentStation, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( EDDI.Instance.CurrentStellarBody != null )
            {
                dict[ "body" ] = new Tuple<Type, Value>( typeof( Body ), Value.FromReflection(EDDI.Instance.CurrentStellarBody, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( EDDI.Instance.FleetCarrier != null )
            {
                dict[ "carrier" ] = new Tuple<Type, Value>( typeof( FleetCarrier ), Value.FromReflection(EDDI.Instance.FleetCarrier, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( theEvent != null ) // A dynamic type is used so that Value.FromReflection 
            {
                dict[ "event" ] = new Tuple<Type, Value>( theEvent.GetType(), Value.FromReflection( theEvent, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
            }

            if ( EDDI.Instance.State != null )
            {
                dict[ "state" ] = new Tuple<Type, Value>( typeof( IDictionary<string, object>), buildState() );
                Logging.Debug( "State is: ", EDDI.Instance.State );
            }
            
            // Obtain additional variables from each monitor
            foreach ( var monitor in EDDI.Instance.monitors )
            {
                var monitorVariables = monitor.GetVariables();
                if ( monitorVariables != null )
                {
                    foreach ( var key in monitorVariables.Keys )
                    {
                        if ( monitorVariables[ key ].Item2 == null )
                        {
                            dict.Remove( key );
                        }
                        else
                        {
                            dict[ key ] = new Tuple<Type, Value>( monitorVariables[ key ].Item1, Value.FromReflection((dynamic)monitorVariables[key]?.Item2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) );
                        }
                    }
                }
            }

            return dict;
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
            foreach (string key in EDDI.Instance.State.Keys)
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
