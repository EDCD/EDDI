using System;
using System.Collections.Generic;
using System.Reflection;

namespace EddiEvents
{
    public abstract class Events
    {
        public static readonly Dictionary<string, Type> TYPES = [ ];
        private static readonly Dictionary<string, IList<object>> SAMPLES = [ ];
        private static readonly Dictionary<string, string> DEFAULTS = [ ];
        public static readonly Dictionary<string, string> DESCRIPTIONS = [ ];

        static Events()
        {
            lock (SAMPLES)
            {
                try
                {
                    foreach ( var type in typeof(Event).Assembly.GetTypes() )
                    {
                        if ( type.IsInterface ||
                             type.IsAbstract ||
                             !type.IsSubclassOf( typeof(Event) ) )
                        {
                            continue;
                        }

                        // Ensure that the static constructor of the class has been run
                        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( type.TypeHandle );

                        if ( type.GetField( "NAME" ) is var nameField &&
                             nameField?.GetValue( null ) is string eventName )
                        {
                            TYPES.Add( eventName, type );

                            if ( type.GetField( "DESCRIPTION" ) is var descriptionField &&
                                 descriptionField?.GetValue( null ) is string eventDescription )
                            {
                                DESCRIPTIONS.Add( eventName, eventDescription );
                            }

                            if ( type.GetField( "DEFAULT" ) is var defaultField &&
                                 defaultField?.GetValue( null ) is string eventDefault )
                            {
                                DEFAULTS.Add( eventName, eventDefault );
                            }

                            if ( type.GetField( "SAMPLES" ) is var samplesField &&
                                 samplesField?.GetValue( null ) is IList<object> eventSamples )
                            {
                                SAMPLES.Add( eventName, eventSamples );
                            }
                            else if ( type.GetField( "SAMPLE" ) is var sampleField &&
                                 sampleField?.GetValue( null ) is var eventSample )
                            {
                                SAMPLES.Add( eventName, new List<object> { eventSample } );
                            }
                        }
                    }
                }
                catch ( ReflectionTypeLoadException )
                {
                    // DLL we can't parse; ignore
                }
            }
        }

        public static Type TypeByName(string name)
        {
            TYPES.TryGetValue(name, out var value);
            return value;
        }

        public static object SampleByName(string name)
        {
            SAMPLES.TryGetValue(name, out var value);
            if ( value?.Count == 1 )
            {
                return value[ 0 ];
            }
            if ( value?.Count > 1 )
            {
                var rand = new Random();
                var randIndex = rand.Next( value.Count );
                return value[ randIndex ];
            }
            return string.Empty;
        }

        public static string DescriptionByName(string name)
        {
            DESCRIPTIONS.TryGetValue(name, out var value);
            return value;
        }

        public static string DefaultByName(string name)
        {
            DEFAULTS.TryGetValue(name, out var value);
            return value;
        }
    }
}
