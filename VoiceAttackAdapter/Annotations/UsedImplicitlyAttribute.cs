#nullable enable

using System;

namespace EddiVoiceAttackAdapter.Annotations
{
    [AttributeUsage( AttributeTargets.All )]
    internal sealed class UsedImplicitlyAttribute : Attribute
    {
        public string? Reason { get; init; }
    }
}
