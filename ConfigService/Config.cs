using System;

namespace EddiConfigService
{
    public abstract class Config
    { }

    [AttributeUsage( AttributeTargets.Class )]
    public class RelativePathAttribute : Attribute
    {
        public string relativePath { get; }
        public RelativePathAttribute(string relativePath) { this.relativePath = relativePath; }
    }
}