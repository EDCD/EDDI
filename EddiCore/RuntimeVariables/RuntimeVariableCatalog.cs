using EddiCompanionAppService;
using EddiConfigService;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiCore.RuntimeVariables
{
    /// <summary>
    /// Declares top-level runtime variables, their user-facing descriptions, and their optional VoiceAttack projection.
    /// This is the source catalog for simple live values such as `environment`; it does not compose the full script
    /// variable inventory.
    /// </summary>
    public static class RuntimeVariableCatalog
    {
        private static Func<decimal> searchDistanceLyProvider;

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

        /// <summary>Registers the NavigationService-owned provider without adding an EDDICore dependency on NavigationService.</summary>
        public static void RegisterSearchDistanceLyProvider ( Func<decimal> valueProvider )
        {
            searchDistanceLyProvider = valueProvider;
        }

        [PublicAPI( "True if the Frontier companion API is active." )]
        public static RuntimeVariableDefinition CapiActive => new(
            CapiActiveVariable,
            typeof(bool),
            () => CompanionAppService.Instance?.active ?? false,
            RuntimeVariableSourceKind.TopLevelRuntime,
            "cAPI active",
            true );

        [PublicAPI( "The distance to the destination system, in light years." )]
        public static RuntimeVariableDefinition DestinationDistanceLy => new(
            DestinationDistanceLyVariable,
            typeof(decimal),
            () => EDDI.Instance.GameState.DestinationDistanceLy,
            RuntimeVariableSourceKind.TopLevelRuntime,
            "Destination system distance",
            true );

        [PublicAPI( "The commander's current environment." )]
        public static RuntimeVariableDefinition Environment => new(
            EnvironmentVariable,
            typeof(string),
            () => EDDI.Instance.GameState.Environment,
            RuntimeVariableSourceKind.TopLevelRuntime,
            "Environment",
            true );

        [PublicAPI( "True if the current game session is Horizons." )]
        public static RuntimeVariableDefinition Horizons => new(
            HorizonsVariable,
            typeof(bool),
            () => EDDI.Instance.GameState.inHorizons,
            RuntimeVariableSourceKind.TopLevelRuntime,
            "horizons",
            true );

        [PublicAPI( "True if ICAO speech processing is enabled." )]
        public static RuntimeVariableDefinition IcaoActive => new(
            IcaoActiveVariable,
            typeof(bool),
            () => ConfigService.Instance.speechServiceConfiguration.EnableIcao,
            RuntimeVariableSourceKind.TopLevelRuntime,
            "icao active",
            true );

        [PublicAPI( "True if IPA speech processing is enabled." )]
        public static RuntimeVariableDefinition IpaActive => new(
            IpaActiveVariable,
            typeof(bool),
            () => !ConfigService.Instance.speechServiceConfiguration.DisableIpa,
            RuntimeVariableSourceKind.TopLevelRuntime,
            "ipa active",
            true );

        [PublicAPI( "True if the current game session is Odyssey." )]
        public static RuntimeVariableDefinition Odyssey => new(
            OdysseyVariable,
            typeof(bool),
            () => EDDI.Instance.GameState.inOdyssey,
            RuntimeVariableSourceKind.TopLevelRuntime,
            "odyssey",
            true );

        [PublicAPI( "The active search distance, in light years." )]
        public static RuntimeVariableDefinition SearchDistanceLy => new(
            SearchDistanceLyVariable,
            typeof(decimal),
            () => searchDistanceLyProvider?.Invoke() ?? 0m,
            RuntimeVariableSourceKind.TopLevelRuntime,
            "Search system distance",
            true );

        [PublicAPI( "True if the VoiceAttack plug-in is active." )]
        public static RuntimeVariableDefinition VaActive => new(
            VaActiveVariable,
            typeof(bool),
            () => EDDI.Instance.FromVA,
            RuntimeVariableSourceKind.TopLevelRuntime );

        [PublicAPI( "The commander's current vehicle." )]
        public static RuntimeVariableDefinition Vehicle => new(
            VehicleVariable,
            typeof(string),
            () => EDDI.Instance.GameState.Vehicle,
            RuntimeVariableSourceKind.TopLevelRuntime,
            "Vehicle",
            true );

        [PublicAPI( "The current EDDI version." )]
        public static RuntimeVariableDefinition Version => new(
            VersionVariable,
            typeof(string),
            () => Constants.EDDI_VERSION.ShortString,
            RuntimeVariableSourceKind.TopLevelRuntime,
            "EDDI version",
            true,
            () => Constants.EDDI_VERSION.ToString() );

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
