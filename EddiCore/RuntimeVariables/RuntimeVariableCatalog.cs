using System.Collections.Generic;
using Utilities;

namespace EddiCore.RuntimeVariables
{
    /// <summary>
    /// Declares top-level runtime variable metadata, including user-facing descriptions and optional VoiceAttack projection.
    /// Live values are supplied separately by runtime owners.
    /// </summary>
    public static class RuntimeVariableCatalog
    {
        public const string CapiActiveVariable = "capi_active";
        public const string DestinationDistanceLyVariable = "destinationdistance";
        public const string EnvironmentVariable = "environment";
        public const string HorizonsVariable = "horizons";
        public const string IcaoActiveVariable = "icao_active";
        public const string IpaActiveVariable = "ipa_active";
        public const string OdysseyVariable = "odyssey";
        public const string SearchDistanceLyVariable = "searchdistance";
        public const string VaActiveVariable = "va_active";
        public const string VehicleVariable = "vehicle";
        public const string VersionVariable = "version";

        [PublicAPI( "True if the Frontier companion API is active." )]
        public static RuntimeVariableDefinition CapiActive => new(
            CapiActiveVariable,
            typeof(bool),
            RuntimeVariableSourceKind.TopLevelRuntime,
            "cAPI active",
            true,
            true );

        [PublicAPI( "The distance to the destination system, in light years." )]
        public static RuntimeVariableDefinition DestinationDistanceLy => new(
            DestinationDistanceLyVariable,
            typeof(decimal),
            RuntimeVariableSourceKind.TopLevelRuntime,
            "Destination system distance",
            true,
            true );

        [PublicAPI( "The commander's current environment." )]
        public static RuntimeVariableDefinition Environment => new(
            EnvironmentVariable,
            typeof(string),
            RuntimeVariableSourceKind.TopLevelRuntime,
            "Environment",
            true,
            true );

        [PublicAPI( "True if the current game session is Horizons." )]
        public static RuntimeVariableDefinition Horizons => new(
            HorizonsVariable,
            typeof(bool),
            RuntimeVariableSourceKind.TopLevelRuntime,
            "horizons",
            true,
            true );

        [PublicAPI( "True if ICAO speech processing is enabled." )]
        public static RuntimeVariableDefinition IcaoActive => new(
            IcaoActiveVariable,
            typeof(bool),
            RuntimeVariableSourceKind.TopLevelRuntime,
            "icao active",
            true,
            true );

        [PublicAPI( "True if IPA speech processing is enabled." )]
        public static RuntimeVariableDefinition IpaActive => new(
            IpaActiveVariable,
            typeof(bool),
            RuntimeVariableSourceKind.TopLevelRuntime,
            "ipa active",
            true,
            true );

        [PublicAPI( "True if the current game session is Odyssey." )]
        public static RuntimeVariableDefinition Odyssey => new(
            OdysseyVariable,
            typeof(bool),
            RuntimeVariableSourceKind.TopLevelRuntime,
            "odyssey",
            true,
            true );

        [PublicAPI( "The active search distance, in light years." )]
        public static RuntimeVariableDefinition SearchDistanceLy => new(
            SearchDistanceLyVariable,
            typeof(decimal),
            RuntimeVariableSourceKind.TopLevelRuntime,
            "Search system distance",
            true,
            true );

        [PublicAPI( "True if the VoiceAttack plug-in is active." )]
        public static RuntimeVariableDefinition VaActive => new(
            VaActiveVariable,
            typeof(bool),
            RuntimeVariableSourceKind.TopLevelRuntime,
            "VA active",
            true );

        [PublicAPI( "The commander's current vehicle." )]
        public static RuntimeVariableDefinition Vehicle => new(
            VehicleVariable,
            typeof(string),
            RuntimeVariableSourceKind.TopLevelRuntime,
            "Vehicle",
            true,
            true );

        [PublicAPI( "The current EDDI version." )]
        public static RuntimeVariableDefinition Version => new(
            VersionVariable,
            typeof(string),
            RuntimeVariableSourceKind.TopLevelRuntime,
            "EDDI version",
            true,
            true );

        public static IReadOnlyList<RuntimeVariableDefinition> TopLevelVariables =>
        [
            CapiActive,
            DestinationDistanceLy,
            Environment,
            Horizons,
            IcaoActive,
            IpaActive,
            Odyssey,
            SearchDistanceLy,
            VaActive,
            Vehicle,
            Version
        ];
    }
}
