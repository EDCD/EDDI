using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using Utilities.MetaVariables;

namespace EddiCore.RuntimeVariables
{
    /// <summary>
    /// Builds the complete standard variable metadata surface by composing the top-level <see cref="RuntimeVariableCatalog"/>,
    /// standard object roots, monitor-provided variables, and optional runtime roots supplied by callers.
    /// </summary>
    public static class StandardVariableInventoryBuilder
    {
        private sealed record StandardObjectRoot (
            string Name,
            Type Type,
            string Description );

        private static readonly IReadOnlyList<StandardObjectRoot> StandardObjectRoots =
        [
            new( "system", typeof(StarSystem), "Details about the commander's current star system." ),
            new( "lastsystem", typeof(StarSystem), "Details about the commander's previous star system." ),
            new( "nextsystem", typeof(StarSystem), "Details about the next star system in the plotted route." ),
            new( "destinationsystem", typeof(StarSystem), "Details about the destination star system." ),
            new( "searchsystem", typeof(StarSystem), "Details about the active search star system." ),
            new( "searchstation", typeof(Station), "Details about the active search station." ),
            new( "station", typeof(Station), "Details about the commander's current station." ),
            new( "body", typeof(Body), "Details about the commander's current stellar body." )
        ];

        public static IReadOnlyList<RuntimeVariableDeclaration> GetTopLevelRuntimeDeclarations () =>
            RuntimeVariableDefinitionExtensions.DiscoverDeclarations( typeof(RuntimeVariableCatalog) );

        public static IReadOnlyList<RuntimeVariableDeclaration> GetCurrentMonitorRuntimeDeclarations ()
        {
            return EDDI.Instance?.monitors?
                .SelectMany( monitor => monitor.GetVariableDeclarations() )
                .ToList() ?? [];
        }

        public static IReadOnlyList<MetaVariable> BuildStandardMetaVariables (
            IEnumerable<RuntimeVariableRoot> runtimeVariableRoots,
            IEnumerable<RuntimeVariableDeclaration> monitorRuntimeDeclarations = null,
            MetaVariableDiscoveryOptions options = null )
        {
            options ??= MetaVariableDiscoveryOptions.Runtime;
            var variablesByPath = new SortedDictionary<string, MetaVariable>( StringComparer.OrdinalIgnoreCase );

            AddMetaVariables(
                variablesByPath,
                new MetaVariables( GetTopLevelRuntimeDeclarations(), options: options ).Results,
                options );

            AddMetaVariables(
                variablesByPath,
                new MetaVariables( GetStandardObjectRootDeclarations(), options: options ).Results,
                options );

            AddMetaVariables(
                variablesByPath,
                new MetaVariables( monitorRuntimeDeclarations ?? GetCurrentMonitorRuntimeDeclarations(), options: options ).Results,
                options );

            if ( runtimeVariableRoots is not null )
            {
                foreach ( var root in runtimeVariableRoots )
                {
                    if ( root.Type is null )
                    {
                        continue;
                    }

                    var vars = new MetaVariables( root.Type, null, null, options ).Descriptors
                        .Select( descriptor => new MetaVariable(
                            VariableDescriptor.Create(
                                descriptor.KeysPath.Prepend( root.Name ),
                                descriptor.VariableType,
                                descriptor.Description,
                                descriptor.Value,
                                descriptor.SourceType,
                                descriptor.SourceMemberName,
                                descriptor.IsObsolete
                                    ? new ObsoleteAttribute( descriptor.ObsoleteMessage )
                                    : null,
                                descriptor.IsCollectionRoot,
                                descriptor.IsObjectRoot,
                                descriptor.DeclaredType,
                                options ) ) );

                    AddMetaVariables( variablesByPath, vars, options );
                }
            }

            return variablesByPath.Values.ToList();
        }

        public static IReadOnlyList<MetaVariable> BuildStaticStandardMetaVariables (
            IEnumerable<RuntimeVariableDeclaration> monitorRuntimeDeclarations = null,
            MetaVariableDiscoveryOptions options = null )
        {
            return BuildStandardMetaVariables(
                null,
                monitorRuntimeDeclarations,
                options ?? MetaVariableDiscoveryOptions.StrictDocumentation );
        }

        private static List<RuntimeVariableDeclaration> GetStandardObjectRootDeclarations ()
        {
            return StandardObjectRoots
                .Select( root => new RuntimeVariableDeclaration(
                    new RuntimeVariableDefinition(
                        root.Name,
                        root.Type,
                        RuntimeVariableSourceKind.TopLevelRuntime ),
                    root.Description,
                    typeof(StandardVariableInventoryBuilder),
                    root.Name,
                    null ) )
                .ToList();
        }

        private static void AddMetaVariables (
            IDictionary<string, MetaVariable> variablesByPath,
            IEnumerable<MetaVariable> variables,
            MetaVariableDiscoveryOptions options )
        {
            foreach ( var variable in variables
                         .Where( variable => !string.IsNullOrEmpty( variable.Descriptor.CottlePath ) )
                         .GroupBy( variable => variable.Descriptor.CottlePath, StringComparer.OrdinalIgnoreCase )
                         .Select( group => group.First() ) )
            {
                var key = variable.Descriptor.CottlePath;
                if ( !variablesByPath.TryAdd( key, variable ) && options.Strict )
                {
                    throw new MetaVariableDiscoveryException( $"Duplicate runtime variable path '{key}'." );
                }
            }
        }
    }
}
