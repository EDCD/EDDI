using Cottle;
using Cottle.Builtins;
using Cottle.Documents;
using Cottle.Exceptions;
using Cottle.Settings;
using Cottle.Stores;
using Cottle.Values;
using Eddi;
using EddiCompanionAppService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
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
        private readonly Dictionary<string, Script> scripts;
        private readonly CustomSetting setting;
        internal readonly DataProviderService dataProviderService;

        // The file to log speech
        [UsedImplicitly] public static readonly string LogFile = Constants.DATA_DIR + @"\speechresponder.out";

        public ScriptResolver(Dictionary<string, Script> scripts)
        {
            dataProviderService = new DataProviderService();
            this.scripts = scripts ?? new Dictionary<string, Script>();
            setting = new CustomSetting
            {
                Trimmer = BuiltinTrimmers.CollapseBlankCharacters
            };
        }

        public int priority(string name)
        {
            scripts.TryGetValue(name, out Script script);
            return script?.Priority ?? 3;
        }

        /// <summary> From a custom dictionary of variable values in the default store </summary>
        public string resolveFromName(string name, Dictionary<string, Tuple<Type, Value>> vars, bool isTopLevelScript)
        {
            BuiltinStore store = buildStore(vars);
            return resolveFromName(name, store, isTopLevelScript);
        }

        /// <summary> From a custom store </summary>
        public string resolveFromName(string name, BuiltinStore store, bool isTopLevelScript)
        {
            if (!scripts.TryGetValue(name, out Script script) || 
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

            return resolveFromValue(script.Value, store, isTopLevelScript, script);
        }

        /// <summary> From the default dictionary of variable values in the default store </summary>
        public string resolveFromValue(string scriptValue, bool isTopLevelScript)
        {
            var vars = CompileVariables();
            BuiltinStore store = buildStore(vars);
            return resolveFromValue(scriptValue, store, isTopLevelScript);
        }

        /// <summary> From a custom store </summary>
        public string resolveFromValue(string script, BuiltinStore store, bool isTopLevelScript, Script scriptObject = null)
        {
            try
            {
                Logging.Debug( $"Resolving {( isTopLevelScript ? "top level " : "" )}script {scriptObject?.Name}: {script}", store );

                var document = new SimpleDocument( script, setting );
                var result = document.Render( store );
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
        protected internal Dictionary<string, Tuple<Type, Value>> CompileVariables(Event theEvent = null)
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
                ["ipa_active"] = new Tuple<Type, Value>(typeof(bool), !SpeechService.Instance.Configuration.DisableIpa)

            };

            // Standard objects
            if ( EDDI.Instance.Cmdr != null )
            {
                dict[ "cmdr" ] = new Tuple<Type, Value>( typeof( Commander ), new ReflectionValue( EDDI.Instance.Cmdr ) );
            }

            if ( EDDI.Instance.HomeStarSystem != null )
            {
                dict[ "homesystem" ] = new Tuple<Type, Value>( typeof( StarSystem ), new ReflectionValue( EDDI.Instance.HomeStarSystem ) );
            }

            if ( EDDI.Instance.HomeStation != null )
            {
                dict[ "homestation" ] = new Tuple<Type, Value>( typeof( Station ), new ReflectionValue( EDDI.Instance.HomeStation ) );
            }

            if ( EDDI.Instance.SquadronStarSystem != null )
            {
                dict[ "squadronsystem" ] = new Tuple<Type, Value>( typeof( StarSystem ), new ReflectionValue( EDDI.Instance.SquadronStarSystem ) );
            }

            if ( EDDI.Instance.CurrentStarSystem != null )
            {
                dict[ "system" ] = new Tuple<Type, Value>( typeof( StarSystem ), new ReflectionValue( EDDI.Instance.CurrentStarSystem ) );
            }

            if ( EDDI.Instance.LastStarSystem != null )
            {
                dict[ "lastsystem" ] = new Tuple<Type, Value>( typeof( StarSystem ), new ReflectionValue( EDDI.Instance.LastStarSystem ) );
            }

            if ( EDDI.Instance.NextStarSystem != null )
            {
                dict[ "nextsystem" ] = new Tuple<Type, Value>( typeof( StarSystem ), new ReflectionValue( EDDI.Instance.NextStarSystem ) );
            }

            if ( EDDI.Instance.DestinationStarSystem != null )
            {
                dict[ "destinationsystem" ] = new Tuple<Type, Value>( typeof( StarSystem ), new ReflectionValue( EDDI.Instance.DestinationStarSystem ) );
            }

            if ( NavigationService.Instance.SearchStarSystem != null )
            {
                dict[ "searchsystem" ] = new Tuple<Type, Value>( typeof( StarSystem ), new ReflectionValue( NavigationService.Instance.SearchStarSystem ) );
            }

            if ( NavigationService.Instance.SearchStation != null )
            {
                dict[ "searchstation" ] = new Tuple<Type, Value>( typeof( Station ), new ReflectionValue( NavigationService.Instance.SearchStation ) );
            }

            if ( EDDI.Instance.CurrentStation != null )
            {
                dict[ "station" ] = new Tuple<Type, Value>( typeof( Station ), new ReflectionValue( EDDI.Instance.CurrentStation ) );
            }

            if ( EDDI.Instance.CurrentStellarBody != null )
            {
                dict[ "body" ] = new Tuple<Type, Value>( typeof( Body ), new ReflectionValue( EDDI.Instance.CurrentStellarBody ) );
            }

            if ( EDDI.Instance.FleetCarrier != null )
            {
                dict[ "carrier" ] = new Tuple<Type, Value>( typeof( FleetCarrier ), new ReflectionValue( EDDI.Instance.FleetCarrier ) );
            }

            if ( theEvent != null )
            {
                dict[ "event" ] = new Tuple<Type, Value>( typeof( Event ), new ReflectionValue( theEvent ) );
            }

            if ( EDDI.Instance.State != null )
            {
                dict[ "state" ] = new Tuple<Type, Value>( typeof( IDictionary<string, object>), buildState() );
                Logging.Debug( "State is: ", EDDI.Instance.State );
            }

            // Obtain additional variables from each monitor
            foreach ( IEddiMonitor monitor in EDDI.Instance.monitors )
            {
                var monitorVariables = monitor.GetVariables();
                if ( monitorVariables != null )
                {
                    foreach ( string key in monitorVariables.Keys )
                    {
                        if ( monitorVariables[ key ].Item2 == null )
                        {
                            dict.Remove( key );
                        }
                        else
                        {
                            dict[ key ] = new Tuple<Type, Value>( monitorVariables[ key ].Item1, new ReflectionValue( monitorVariables[ key ]?.Item2 ) );
                        }
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// Build a store from a list of variables
        /// </summary>
        public BuiltinStore buildStore(Dictionary<string, Tuple<Type, Value>> vars = null)
        {
            BuiltinStore store = new BuiltinStore();
            
            // Loop through our custom functions and add them to the store.
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && t.GetInterface(nameof(ICustomFunction)) != null))
            {
                // Create an instance of the function and add it to the store,
                // either with no .ctor or passing data to a .ctor as required (for classes implementing `ResolverInstance`).
                var function = (ICustomFunction)(type.GetConstructor(Type.EmptyTypes) != null 
                    ? Activator.CreateInstance(type) : 
                    Activator.CreateInstance(type, this, store));
                if ( function != null )
                {
                    store.Set( function.name, function.function, StoreMode.Global );
                }
            }

            // Variables
            if (vars != null)
            {
                foreach (var entry in vars)
                {
                    store[ entry.Key ] = entry.Value.Item2;
                }
            }

            return store;
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

        internal List<ICustomFunction> GetCustomFunctions ()
        {
            var functionsList = new List<ICustomFunction>();
            var assy = Assembly.GetAssembly(typeof(ScriptResolver));
            if ( assy != null )
            {
                foreach ( var type in assy.GetTypes()
                             .Where( t => t.IsClass && t.GetInterface( nameof( ICustomFunction ) ) != null ) )
                {
                    var function = (ICustomFunction)( type.GetConstructor( Type.EmptyTypes ) != null
                        ? Activator.CreateInstance( type )
                        : Activator.CreateInstance( type, this, buildStore() ) );

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
