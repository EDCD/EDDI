using EddiEvents;
using EddiIPC_Service;
using EddiSpeechResponder.ScriptResolverService;
using EddiVoiceAttackResponder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilities;

namespace EddiDocumentationGenerator
{
    public static class DocumentationGenerator
    {
        private const string NewLine = "\r\n";

        public static IReadOnlyDictionary<string, string> RenderWikiEventPages ()
        {
            return Events.TYPES
                .OrderBy( i => i.Key )
                .ToDictionary(
                    entry => @"Wiki\events\" + entry.Key.Replace( " ", "-" ) + "-event.md",
                    entry => JoinLines( RenderWikiEventPage( entry.Key, entry.Value ) ) );
        }

        public static string RenderWikiEventsList ()
        {
            var output = new List<string>
            {
                "EDDI generates a large number of events, triggered from changes in-game as well as from a number of external sources (e.g. Galnet RSS feed).  " +
                "A brief description of all available events is below, along with a link to more detailed information about each event:",
                ""
            };

            foreach ( var entry in Events.TYPES.OrderBy( i => i.Key ) )
            {
                output.Add( "## [" + entry.Key + "](" + entry.Key.Replace( " ", "-" ) + "-event)" );
                output.Add( Events.DESCRIPTIONS[ entry.Key ] + "." );
                output.Add( "" );
            }

            return JoinLines( output );
        }

        public static string RenderEventVariableKeywords ()
        {
            var eventVars = new SortedSet<string>();

            foreach ( var type in Events.TYPES )
            {
                var vars = new MetaVariables( type.Value ).Descriptors;
                foreach ( var key in vars.SelectMany( v => v.KeysPath ) )
                {
                    eventVars.Add( key );
                }
            }

            return "      <Word>" + string.Join( "</Word>\r\n      <Word>", eventVars ) + " </Word>\r\n";
        }

        public static (string Help, string Functions) RenderFunctionsHelp ()
        {
            var functionsList = ScriptResolver.GetCustomFunctions()
                .Where( f => f.Category != FunctionCategory.Hidden )
                .OrderBy( f => f.name )
                .ToList();

            var help = new List<string>
            {
                "",
                EddiSpeechResponder.Properties.CustomFunctions_Untranslated.HelpHeader,
                ""
            };

            foreach ( var function in functionsList )
            {
                help.Add( $"### {function.name}()" );
                help.Add( "" );
                help.Add( function.description );
                help.Add( "" );
            }

            var functions = new List<string>
            {
                "",
                EddiSpeechResponder.Properties.CustomFunctions_Untranslated.FunctionsHeader,
                ""
            };

            foreach ( var function in functionsList )
            {
                functions.Add( $"* {function.name}()" );
            }

            return (JoinLines( help ), JoinLines( functions ));
        }

        public static void WriteWikiOutput ( string outputDirectory )
        {
            ArgumentException.ThrowIfNullOrWhiteSpace( outputDirectory );

            foreach ( var page in RenderWikiEventPages() )
            {
                WriteText( outputDirectory, page.Key, page.Value );
            }

            WriteText( outputDirectory, @"Wiki\Events.md", RenderWikiEventsList() );

            var (help, functions) = RenderFunctionsHelp();
            WriteText( outputDirectory, "Help.md", help );
            WriteText( outputDirectory, @"Wiki\Help.md", help );
            WriteText( outputDirectory, @"Wiki\Functions.md", functions );
            WriteText( outputDirectory, @"Cottle\Custom keywords.txt", RenderEventVariableKeywords() );
        }

        private static List<string> RenderWikiEventPage ( string eventName, Type eventType )
        {
            var output = new List<string>
            {
                Events.DESCRIPTIONS[ eventName ] + ".",
                ""
            };

            var metaVariables = new MetaVariables( eventType );
            var vars = metaVariables.Results;
            var cottleVars = vars.AsCottleVariables();
            var voiceAttackVars = VoiceAttackVariables.Convert( vars, "EDDI", eventName );

            if ( vars.Count == 0 )
            {
                output.Add( "This event has no variables." );
                output.Add( "To respond to this event in VoiceAttack, create a command entitled ((EDDI " + eventName.ToLowerInvariant() + "))." );
                output.Add( "" );
            }

            if ( vars.Any( v => v.keysPath.Any( k => k.Contains( @"<index" ) ) ) )
            {
                output.Add( "Where values are indexed (the compartments on a ship for example), the index will be represented by '*\\<index\\>*'." );
                if ( voiceAttackVars.Any( v => v.key.Contains( @"<index" ) ) )
                {
                    output.Add( "For VoiceAttack, a variable with the root name of the indexed array shall identify the total number of entries in the array. For example, if compartments 1 and 2 are available then the value of the corresponding 'compartments' variable will be 2." );
                }
                output.Add( "" );
            }

            if ( cottleVars.Count > 0 )
            {
                output.Add( "When using this event in the [Speech responder](Speech-Responder) the information about this event is available under the `event` object.  The available variables are as follows:" );
                output.Add( "" );
                output.Add( "" );

                foreach ( var cottleVariable in cottleVars.OrderBy( i => i.key ) )
                {
                    var description = !string.IsNullOrEmpty( cottleVariable.description ) ? $" - {cottleVariable.description}" : "";
                    output.Add( $"  - *{{event.{cottleVariable.key}}}* {description}" );
                    output.Add( "" );
                }
            }

            if ( voiceAttackVars.Count > 0 )
            {
                output.Add( "" );
                output.Add( "To respond to this event in VoiceAttack, create a command entitled ((EDDI " + eventName.ToLowerInvariant() + ")). VoiceAttack variables will be generated to allow you to access the event information." );
                output.Add( "" );
                output.Add( "The following VoiceAttack variables are available for this event:" );
                output.Add( "" );
                output.Add( "" );

                foreach ( var variable in voiceAttackVars.OrderBy( i => i.key ) )
                {
                    output.Add( RenderVoiceAttackVariable( variable ) );
                    output.Add( "" );
                }

                output.Add( "" );
                output.Add( "For more details on VoiceAttack integration, see https://github.com/EDCD/EDDI/wiki/VoiceAttack-Integration." );
                output.Add( "" );
            }

            return output;
        }

        private static string RenderVoiceAttackVariable ( VoiceAttackVariable variable )
        {
            var description = !string.IsNullOrEmpty( variable.description ) ? $" - {variable.description}" : "";
            return variable.variableType switch
            {
                Type type when type == typeof( string ) => $"  - *{{TXT:{variable.key}}}* {description}",
                Type type when type == typeof( int ) => $"  - *{{INT:{variable.key}}}* {description}",
                Type type when type == typeof( bool ) => $"  - *{{BOOL:{variable.key}}}* {description}",
                Type type when type == typeof( decimal ) => $"  - *{{DEC:{variable.key}}}* {description}",
                Type type when type == typeof( DateTime ) => $"  - *{{DATE:{variable.key}}}* {description}",
                Type type when type == typeof( IEnumerable<> ) => $"  - *{{INT:{variable.key}}}* {description}",
                _ => string.Empty
            };
        }

        private static string JoinLines ( IEnumerable<string> lines )
            => string.Join( NewLine, lines ) + NewLine;

        private static void WriteText ( string outputDirectory, string relativePath, string text )
        {
            var path = Path.Combine( outputDirectory, relativePath );
            Directory.CreateDirectory( Path.GetDirectoryName( path ) ?? outputDirectory );
            File.WriteAllText( path, text );
        }
    }
}
