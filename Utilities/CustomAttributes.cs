using JetBrains.Annotations;
using System;

namespace Utilities
{
    /// <summary>
    /// This attribute is intended to mark publicly available API which should not be removed and so is treated as used.
    /// It inherits from the JetBrains annotation but unlike the JetBrains annotation it is accessible via reflection.
    /// </summary>
    [JetBrains.Annotations.PublicAPI]
    [MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class PublicAPIAttribute : Attribute
    {
        public PublicAPIAttribute() 
        { }

        public PublicAPIAttribute([NotNull] string comment)
        {
            Comment = comment;
        }

        [CanBeNull]
        public string Comment { get; }
    }
}
