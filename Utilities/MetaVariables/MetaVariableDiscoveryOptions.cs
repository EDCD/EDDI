using System;
using System.Collections.Generic;

namespace Utilities.MetaVariables
{
    public sealed class MetaVariableDiscoveryOptions
    {
        public static MetaVariableDiscoveryOptions Runtime { get; } = new();

        public static MetaVariableDiscoveryOptions StrictDocumentation { get; } = new()
        {
            Strict = true,
            RequireDescriptions = true
        };

        public bool Strict { get; init; }

        public bool RequireDescriptions { get; init; }

        public int MaxInlineAllowedValues { get; init; } = 64;

        public ISet<string> MissingDescriptionAllowlist { get; init; } =
            new HashSet<string>( StringComparer.Ordinal );
    }

    public class MetaVariableDiscoveryException : Exception
    {
        public MetaVariableDiscoveryException ( string message )
            : base( message )
        { }

        public MetaVariableDiscoveryException ( string message, Exception innerException )
            : base( message, innerException )
        { }
    }
}
