using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiConfigService
{
    public abstract class Config
    {
        #region Gather data needed for legacy data conversions

        [JsonExtensionData]
        internal IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();

        #endregion
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class RelativePathAttribute : Attribute
    {
        public string relativePath { get; }

        public RelativePathAttribute(string relativePath) { this.relativePath = relativePath; }
    }
}