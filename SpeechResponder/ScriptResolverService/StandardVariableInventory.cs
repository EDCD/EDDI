using Cottle;
using EddiCore;
using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiSpeechResponder.ScriptResolverService
{
    public static class StandardVariableInventory
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
                .SelectMany( monitor => RuntimeVariableDefinitionExtensions.DiscoverDeclarations(
                    monitor.GetType(),
                    monitor ) )
                .ToList() ?? [];
        }

        public static IReadOnlyList<MetaVariable> GetStandardMetaVariables (
            Dictionary<string, Tuple<Type, Value>> compiledVariables,
            IEnumerable<RuntimeVariableDeclaration> monitorRuntimeDeclarations = null,
            MetaVariableDiscoveryOptions options = null )
        {
            options ??= MetaVariableDiscoveryOptions.Runtime;
            var variablesByPath = new SortedDictionary<string, MetaVariable>( StringComparer.OrdinalIgnoreCase );

            AddMetaVariables(
                variablesByPath,
                new MetaVariables( GetTopLevelRuntimeDeclarations(), options: options ).Results );

            AddMetaVariables(
                variablesByPath,
                new MetaVariables( GetStandardObjectRootDeclarations(), options: options ).Results );

            AddMetaVariables(
                variablesByPath,
                new MetaVariables( monitorRuntimeDeclarations ?? GetCurrentMonitorRuntimeDeclarations(), options: options ).Results );

            if ( compiledVariables is not null )
            {
                foreach ( var kvp in compiledVariables )
                {
                    if ( kvp.Value.Item1 is null )
                    {
                        continue;
                    }

                    var vars = new MetaVariables( kvp.Value.Item1, null, null, options ).Results;
                    foreach ( var variable in vars )
                    {
                        variable.keysPath = variable.keysPath.Prepend( kvp.Key ).ToList();
                    }

                    AddMetaVariables( variablesByPath, vars );
                }
            }

            return variablesByPath.Values.ToList();
        }

        public static IReadOnlyList<MetaVariable> GetStaticStandardMetaVariables (
            IEnumerable<RuntimeVariableDeclaration> monitorRuntimeDeclarations = null,
            MetaVariableDiscoveryOptions options = null )
        {
            return GetStandardMetaVariables(
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
                        () => null,
                        RuntimeVariableSourceKind.TopLevelRuntime ),
                    root.Description,
                    typeof(StandardVariableInventory),
                    root.Name,
                    null ) )
                .ToList();
        }

        private static void AddMetaVariables (
            IDictionary<string, MetaVariable> variablesByPath,
            IEnumerable<MetaVariable> variables )
        {
            foreach ( var variable in variables )
            {
                var key = variable.Descriptor.CottlePath;
                if ( string.IsNullOrEmpty( key ) )
                {
                    continue;
                }

                variablesByPath.TryAdd( key, variable );
            }
        }
    }
}
