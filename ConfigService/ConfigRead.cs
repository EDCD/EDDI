using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using Utilities;

namespace EddiConfigService
{
    public sealed partial class ConfigService
    {
        /// <summary> Obtain configuration from a json (for unit testing). </summary>
        [UsedImplicitly]
        internal static T FromJson<T>(dynamic json) where T : new()
        {
            T configuration = new T();
            if (json != null)
            {
                try
                {
                    configuration = JsonConvert.DeserializeObject<T>(json);
                }
                catch (Exception ex)
                {
                    Logging.Debug($"Failed to obtain {typeof(T).Name}", ex);
                }
            }
            return configuration;
        }

        /// <summary> Obtain configuration from a file.  If the file name is not supplied then a default path is used </summary>
        internal static T FromFile<T>(string directory = null) where T : Config, new()
        {
            if (directory == null) { directory = Constants.DATA_DIR; }
            string filename = directory + (typeof(T).GetCustomAttribute(typeof(RelativePathAttribute)) as RelativePathAttribute)?.relativePath;
            T configuration = new T();
            if (File.Exists(filename))
            {
                string json = Files.Read(filename);
                if (json != null)
                {
                    try
                    {
                        configuration = FromJsonString<T>(json);
                    }
                    catch (Exception ex)
                    {
                        Logging.Debug($"Failed to read {typeof(T).Name}", ex);
                    }
                }
            }
            if (configuration == null)
            {
                configuration = new T();
            }

            return configuration;
        }

        public static T FromJsonString<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
