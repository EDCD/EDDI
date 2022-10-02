using System;

namespace EddiConfigService
{
    public abstract class Config
    { }

    public class RelativePathAttribute : Attribute
    {
        public string relativePath { get; }
        public RelativePathAttribute(string relativePath) { this.relativePath = relativePath; }
    }
}