using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public T Clone<T> () where T : Config
        {
            var serialized = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<T>( serialized );
        }
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class RelativePathAttribute : Attribute
    {
        public string relativePath { get; }

        public RelativePathAttribute(string relativePath) { this.relativePath = relativePath; }
    }
}