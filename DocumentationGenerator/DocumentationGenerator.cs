using EddiCore;
using EddiEvents;
using EddiIPC_Service;
using EddiSpeechResponder.ScriptResolverService;
using EddiVoiceAttackResponder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Utilities;

namespace DocumentationGenerator
{
    public static class DocumentationGenerator
    {
        private const string NewLine = "\r\n";

        private sealed record ObjectShape (
            Type Type,
            string DisplayName,
            IReadOnlyList<string> UsedByPaths,
            IReadOnlyList<ObjectOccurrence> Occurrences,
            IReadOnlyList<VariableDescriptor> Descriptors );

        private sealed record ObjectOccurrence (
            Type Type,
            IReadOnlyList<string> KeysPath,
            string CottlePath );

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

        public static string RenderVariablesPage ()
        {
            var monitorDeclarations = GetDocumentationMonitorRuntimeDeclarations();
            var variables = StandardVariableInventory
                .GetStaticStandardMetaVariables( monitorDeclarations, MetaVariableDiscoveryOptions.StrictDocumentation )
                .Select( v => v.Descriptor )
                .Where( d => !string.IsNullOrWhiteSpace( d.CottlePath ) )
                .OrderBy( d => d.CottlePath, StringComparer.OrdinalIgnoreCase )
                .ToList();

            var output = new List<string>
            {
                "# EDDI Variables",
                "",
                "EDDI provides variables that can be used by Speech Responder scripts.",
                "",
                "A variable can be a simple value, such as `environment`, or an object, such as `cmdr`. Object properties are accessed from the parent object with a period, for example `cmdr.name`. Array / list object values can be accessed using an index between square brackets, for example `inventory[<index\\>].name`.",
                "",
                "The root variable list below identifies the roots available to scripts. The object reference documents each object shape once and lists the roots that use it, so shared object types such as `system`, `lastsystem`, and `nextsystem` do not repeat the same property descriptions.",
                "",
                "Event-specific variables are available under the `event` object while editing an event script and are documented on each event page.",
                "",
                "---",
                "",
                "## Root Variables",
                ""
            };

            foreach ( var descriptor in variables
                         .Where( d => d.KeysPath.Count == 1 )
                         .OrderBy( d => d.CottlePath, StringComparer.OrdinalIgnoreCase ) )
            {
                output.Add( RenderCottleDescriptor( descriptor ) );
            }

            var objectShapes = BuildObjectShapes( variables );
            if ( objectShapes.Count > 0 )
            {
                output.Add( "" );
                output.Add( "---" );
                output.Add( "" );
                output.Add( "## Object reference" );
                output.Add( "" );
            }

            var documentedTypes = objectShapes.Select( s => s.Type ).ToHashSet();
            var objectOccurrences = objectShapes.SelectMany( s => s.Occurrences ).ToList();
            foreach ( var shape in objectShapes )
            {
                output.Add( $"### {shape.DisplayName}" );
                output.Add( "" );
                output.Add( "Used by: " + string.Join( ", ", shape.UsedByPaths.Select( path => $"`{path}`" ) ) );
                output.Add( "" );

                foreach ( var descriptor in shape.Descriptors )
                {
                    output.Add( RenderRelativeCottleDescriptor( descriptor, shape.Occurrences, objectOccurrences, documentedTypes ) );
                }

                output.Add( "" );
            }

            return JoinLines( output );
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
            WriteText( outputDirectory, "Variables.md", RenderVariablesPage() );
            WriteText( outputDirectory, @"Wiki\Variables.md", RenderVariablesPage() );

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

        private static List<ObjectShape> BuildObjectShapes ( IReadOnlyList<VariableDescriptor> descriptors )
        {
            var occurrences = BuildObjectOccurrences( descriptors );

            return occurrences
                .GroupBy( occurrence => occurrence.Type )
                .Select( g =>
                {
                    var shapeOccurrences = g
                        .OrderBy( occurrence => occurrence.CottlePath, StringComparer.OrdinalIgnoreCase )
                        .ToList();

                    var directDescriptors = shapeOccurrences
                        .SelectMany( occurrence => descriptors
                            .Where( descriptor => IsDirectChildOfOccurrence( descriptor, occurrence ) ) )
                        .GroupBy( descriptor => RenderRelativeCottlePath( descriptor, shapeOccurrences ), StringComparer.OrdinalIgnoreCase )
                        .Select( group => group.OrderBy( d => d.CottlePath, StringComparer.OrdinalIgnoreCase ).First() )
                        .Where( d => !string.IsNullOrWhiteSpace( RenderRelativeCottlePath( d, shapeOccurrences ) ) )
                        .OrderBy( d => RenderRelativeCottlePath( d, shapeOccurrences ), StringComparer.OrdinalIgnoreCase )
                        .ToList();

                    return new ObjectShape(
                        g.Key,
                        RenderTypeName( g.Key ),
                        shapeOccurrences
                            .Select( occurrence => RenderOccurrenceReference( occurrence, occurrences ) )
                            .Distinct( StringComparer.OrdinalIgnoreCase )
                            .OrderBy( path => path, StringComparer.OrdinalIgnoreCase )
                            .ToList(),
                        shapeOccurrences,
                        directDescriptors );
                } )
                .Where( shape => shape.Descriptors.Count > 0 )
                .OrderBy( s => s.DisplayName, StringComparer.OrdinalIgnoreCase )
                .ToList();
        }

        private static List<ObjectOccurrence> BuildObjectOccurrences ( IReadOnlyList<VariableDescriptor> descriptors )
        {
            var occurrencesByPath = new SortedDictionary<string, ObjectOccurrence>( StringComparer.OrdinalIgnoreCase );

            foreach ( var descriptor in descriptors.Where( d => d.IsObjectRoot && d.DeclaredType is not null ) )
            {
                AddOccurrence( occurrencesByPath, descriptor.DeclaredType, descriptor.KeysPath );
            }

            foreach ( var group in GetCollectionElementTypeCandidates( descriptors )
                         .GroupBy( candidate => candidate.KeysPath, new KeysPathComparer() ) )
            {
                var elementType = group
                    .Where( candidate => candidate.Type is not null )
                    .GroupBy( candidate => candidate.Type )
                    .OrderByDescending( g => g.Count() )
                    .ThenBy( g => g.Key.FullName, StringComparer.OrdinalIgnoreCase )
                    .Select( g => g.Key )
                    .FirstOrDefault();

                AddOccurrence( occurrencesByPath, elementType, group.Key );
            }

            return occurrencesByPath.Values
                .Where( occurrence => occurrence.Type is not null &&
                                      occurrence.Type != typeof( object ) &&
                                      !IsUndecomposedType( occurrence.Type ) )
                .ToList();
        }

        private static IEnumerable<(IReadOnlyList<string> KeysPath, Type Type)> GetCollectionElementTypeCandidates (
            IReadOnlyList<VariableDescriptor> descriptors )
        {
            foreach ( var descriptor in descriptors.Where( d => d.KeysPath.Count > 2 ) )
            {
                for ( var i = 0; i < descriptor.KeysPath.Count - 1; i++ )
                {
                    if ( descriptor.KeysPath[ i ] != MetaVariables.indexMarker )
                    {
                        continue;
                    }

                    if ( descriptor.KeysPath.Count != i + 2 )
                    {
                        continue;
                    }

                    yield return (descriptor.KeysPath.Take( i + 1 ).ToList(), descriptor.SourceType);
                }
            }
        }

        private static void AddOccurrence (
            IDictionary<string, ObjectOccurrence> occurrencesByPath,
            Type type,
            IEnumerable<string> keysPath )
        {
            if ( type is null )
            {
                return;
            }

            var path = keysPath.ToList();
            if ( path.Count == 0 )
            {
                return;
            }

            var cottlePath = VariablePathFormatter.RenderCottlePath( path );
            occurrencesByPath.TryAdd( cottlePath, new ObjectOccurrence( type, path, cottlePath ) );
        }

        private static bool IsDirectChildOfOccurrence (
            VariableDescriptor descriptor,
            ObjectOccurrence occurrence )
        {
            return descriptor.KeysPath.Count == occurrence.KeysPath.Count + 1 &&
                   StartsWith( descriptor.KeysPath, occurrence.KeysPath );
        }

        private static string RenderOccurrenceReference (
            ObjectOccurrence occurrence,
            IReadOnlyList<ObjectOccurrence> occurrences )
        {
            var parent = occurrences
                .Where( candidate => !ReferenceEquals( candidate, occurrence ) )
                .Where( candidate => candidate.KeysPath.Count < occurrence.KeysPath.Count )
                .Where( candidate => StartsWith( occurrence.KeysPath, candidate.KeysPath ) )
                .OrderByDescending( candidate => candidate.KeysPath.Count )
                .FirstOrDefault();

            if ( parent is null )
            {
                return occurrence.CottlePath;
            }

            var relativePath = VariablePathFormatter.RenderCottlePath(
                occurrence.KeysPath.Skip( parent.KeysPath.Count ) );
            return $"{RenderTypeName( parent.Type )}.{relativePath}";
        }

        private static bool StartsWith (
            IReadOnlyList<string> keysPath,
            IReadOnlyList<string> prefix )
        {
            return keysPath.Count >= prefix.Count &&
                   prefix.Select( ( key, index ) => string.Equals( keysPath[ index ], key, StringComparison.OrdinalIgnoreCase ) )
                       .All( matched => matched );
        }

        private static string RenderRelativeCottleDescriptor (
            VariableDescriptor descriptor,
            IReadOnlyList<ObjectOccurrence> occurrences,
            IReadOnlyList<ObjectOccurrence> allOccurrences,
            ISet<Type> documentedTypes )
        {
            var relativePath = RenderRelativeCottlePath( descriptor, occurrences );
            var referenceType = GetReferenceType( descriptor, allOccurrences, documentedTypes );
            var description = RenderDescriptorDescription( descriptor, includeAllowedValues: referenceType is null ).Trim();
            if ( referenceType is not null )
            {
                description = string.IsNullOrWhiteSpace( description )
                    ? $"See: `{RenderTypeName( referenceType )}`."
                    : $"{description} See: `{RenderTypeName( referenceType )}`.";
            }

            return $"  - *{relativePath}* - {description}";
        }

        private static string RenderRelativeCottlePath (
            VariableDescriptor descriptor,
            IReadOnlyList<ObjectOccurrence> occurrences )
        {
            var occurrence = occurrences
                .Where( o => StartsWith( descriptor.KeysPath, o.KeysPath ) )
                .OrderByDescending( o => o.KeysPath.Count )
                .FirstOrDefault();

            return occurrence is null
                ? string.Empty
                : VariablePathFormatter.RenderCottlePath( descriptor.KeysPath.Skip( occurrence.KeysPath.Count ) );
        }

        private static string RenderCottleDescriptor ( VariableDescriptor descriptor )
        {
            return $"  - *{descriptor.CottlePath}* - {RenderDescriptorDescription( descriptor ).Trim()}";
        }

        private static Type GetReferenceType (
            VariableDescriptor descriptor,
            IReadOnlyList<ObjectOccurrence> occurrences,
            ISet<Type> documentedTypes )
        {
            if ( descriptor.IsObjectRoot &&
                 descriptor.DeclaredType is not null &&
                 descriptor.DeclaredType != typeof( object ) &&
                 documentedTypes.Contains( descriptor.DeclaredType ) )
            {
                return descriptor.DeclaredType;
            }

            if ( !descriptor.IsCollectionRoot )
            {
                return null;
            }

            var collectionElementPath = descriptor.KeysPath
                .Append( MetaVariables.indexMarker )
                .ToList();
            var collectionElementCottlePath = VariablePathFormatter.RenderCottlePath( collectionElementPath );

            var referenceType = occurrences
                .FirstOrDefault( occurrence => string.Equals(
                    occurrence.CottlePath,
                    collectionElementCottlePath,
                    StringComparison.OrdinalIgnoreCase ) )
                ?.Type;

            return referenceType is not null && documentedTypes.Contains( referenceType )
                ? referenceType
                : null;
        }

        private static string RenderDescriptorDescription ( VariableDescriptor descriptor, bool includeAllowedValues = true )
        {
            var description = descriptor.Description ?? string.Empty;
            if ( descriptor.IsObsolete )
            {
                description = string.IsNullOrEmpty( descriptor.ObsoleteMessage )
                    ? $"{description} Obsolete."
                    : $"{description} Obsolete: {descriptor.ObsoleteMessage}";
            }

            if ( includeAllowedValues && descriptor.AllowedValues.Count > 0 )
            {
                var allowedValues = string.Join(
                    ", ",
                    descriptor.AllowedValues
                        .Select( v => v.LocalizedName ?? v.InvariantName ?? v.EdName )
                        .Where( v => !string.IsNullOrWhiteSpace( v ) ) );
                description = $"{description} Allowed values: {allowedValues}.";
            }
            else if ( includeAllowedValues && !string.IsNullOrEmpty( descriptor.AllowedValuesOmittedReason ) )
            {
                description = $"{description} {descriptor.AllowedValuesOmittedReason}";
            }

            return description;
        }

        private static bool IsUndecomposedType ( Type type )
        {
            return type == typeof( string ) ||
                   type == typeof( bool ) ||
                   type == typeof( int ) ||
                   type == typeof( uint ) ||
                   type == typeof( decimal ) ||
                   type == typeof( long ) ||
                   type == typeof( ulong ) ||
                   type == typeof( double ) ||
                   type == typeof( float ) ||
                   type == typeof( DateTime ) ||
                   type == typeof( TimeSpan );
        }

        private static string RenderTypeName ( Type type )
        {
            if ( !type.IsGenericType )
            {
                return type.Name;
            }

            var genericName = type.Name.Split( '`' )[ 0 ];
            var genericArguments = string.Join( ", ", type.GetGenericArguments().Select( RenderTypeName ) );
            return $"{genericName}<{genericArguments}>";
        }

        private sealed class KeysPathComparer : IEqualityComparer<IReadOnlyList<string>>
        {
            public bool Equals ( IReadOnlyList<string> x, IReadOnlyList<string> y )
            {
                if ( ReferenceEquals( x, y ) )
                {
                    return true;
                }

                if ( x is null || y is null || x.Count != y.Count )
                {
                    return false;
                }

                return x.SequenceEqual( y, StringComparer.OrdinalIgnoreCase );
            }

            public int GetHashCode ( IReadOnlyList<string> obj )
            {
                unchecked
                {
                    return obj.Aggregate( 17, ( hash, key ) =>
                        (hash * 31) + StringComparer.OrdinalIgnoreCase.GetHashCode( key ?? string.Empty ) );
                }
            }
        }

        private static List<RuntimeVariableDeclaration> GetDocumentationMonitorRuntimeDeclarations ()
        {
            return DiscoverMonitorTypes()
                .SelectMany( type =>
                {
                    var monitor = (IEddiMonitor)RuntimeHelpers.GetUninitializedObject( type );
                    return RuntimeVariableDefinitionExtensions.DiscoverDeclarations( type, monitor );
                } )
                .ToList();
        }

        private static List<Type> DiscoverMonitorTypes ()
        {
            var directory = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location )
                            ?? AppContext.BaseDirectory;

            return Directory
                .EnumerateFiles( directory, "*Monitor.dll", SearchOption.AllDirectories )
                .OrderBy( path => path, StringComparer.OrdinalIgnoreCase )
                .SelectMany( GetMonitorTypes )
                .OrderBy( type => type.FullName, StringComparer.OrdinalIgnoreCase )
                .ToList();
        }

        private static IEnumerable<Type> GetMonitorTypes ( string assemblyPath )
        {
            try
            {
                return Assembly.LoadFrom( assemblyPath )
                    .GetTypes()
                    .Where( type => !type.IsAbstract &&
                                    !type.IsInterface &&
                                    typeof(IEddiMonitor).IsAssignableFrom( type ) );
            }
            catch ( Exception ex ) when ( ex is BadImageFormatException or
                                           FileLoadException or
                                           ReflectionTypeLoadException )
            {
                throw new InvalidOperationException(
                    $"Unable to discover monitor variables from '{assemblyPath}'.",
                    ex );
            }
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
