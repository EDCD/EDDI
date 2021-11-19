using Newtonsoft.Json;
using System;

namespace EddiConfigService
{
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class Config
    {
        [JsonIgnore]
        internal string dataPath { get; set; }
    }

    public class RelativePathAttribute : Attribute
    {
        public string relativePath { get; }
        public RelativePathAttribute(string relativePath) { this.relativePath = relativePath; }
    }
}