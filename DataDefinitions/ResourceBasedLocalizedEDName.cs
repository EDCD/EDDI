using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Utilities;

namespace EddiDataDefinitions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ResourceBasedLocalizedEDName<T> where T : ResourceBasedLocalizedEDName<T>, new()
    {
        static ResourceBasedLocalizedEDName()
        {
            AllOfThem = new List<T>();
        }
        public static readonly List<T> AllOfThem;
        protected static ResourceManager resourceManager;
        protected static Func<string, T> missingEDNameHandler;

        [JsonProperty]
        public readonly string edname;

        public readonly string basename;

        public string invariantName
        {
            get
            {
                string invariantName = resourceManager.GetString(basename, CultureInfo.InvariantCulture) ?? basename;
                return invariantName;
            }
        }

        public string fallbackLocalizedName { get; set; } = null;
        public string localizedName => resourceManager.GetString(basename) ?? fallbackLocalizedName ?? basename;

        [Obsolete("Please be explicit and use localizedName or invariantName")]
        public string name => localizedName;

        public override string ToString()
        {
            return localizedName;
        }

        protected ResourceBasedLocalizedEDName(string edname, string basename)
        {
            this.edname = edname;
            this.basename = basename;

            if (!string.IsNullOrEmpty(edname))
            {
                AllOfThem.Add(this as T);
            }
        }

        private static void EnsureSubClassStaticConstructorHasRun()
        {
            if (AllOfThem.Count == 0)
            {
                T dummy = new T();
            }
        }

        public static T FromName(string from)
        {
            EnsureSubClassStaticConstructorHasRun();
            if (from == null)
            {
                return null;
            }

            from = from.ToLowerInvariant();
            T result = AllOfThem.FirstOrDefault(
                v => 
                v.localizedName.ToLowerInvariant() == from 
                || v.invariantName.ToLowerInvariant() == from);
            return result;
        }

        public static T FromEDName(string from)
        {
            EnsureSubClassStaticConstructorHasRun();
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from?.Replace(";", "").Replace(" ", "").ToLowerInvariant();
            T result = AllOfThem.FirstOrDefault(
                v => v.edname
                .ToLowerInvariant()
                .Replace(";", "") == tidiedFrom);
            if (result == null)
            {
                Logging.Report($"Unknown ED name {from} in resource {resourceManager.BaseName}");
                if (missingEDNameHandler != null)
                {
                    result = missingEDNameHandler(from);
                }
            }
            return result;
        }
    }
}
