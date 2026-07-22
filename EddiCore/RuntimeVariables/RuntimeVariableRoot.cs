using System;

namespace EddiCore.RuntimeVariables
{
    /// <summary>
    /// Describes an already-compiled variable root by name and CLR type so shared inventory generation can inspect its
    /// metadata without depending on a renderer-specific value type such as Cottle.Value.
    /// </summary>
    public sealed record RuntimeVariableRoot (
        string Name,
        Type Type );
}
