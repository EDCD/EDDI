using Newtonsoft.Json;
using System.Reflection;
using Utilities;

namespace EddiConfigService
{
    public sealed partial class ConfigService
    {
        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, unless it too is null, for example
        /// a test config purely based on JSON, in which case nothing will be written.
        /// </summary>
        private static void ToFile(Config value, string directory = null)
        {
            if (directory == null) { directory = Constants.DATA_DIR; }
            var filename = directory + (value.GetType().GetCustomAttribute(typeof(RelativePathAttribute)) as RelativePathAttribute)?.relativePath;
            var json = JsonConvert.SerializeObject(value, Formatting.Indented);
            Logging.Debug($"{value.GetType()} to file: " + json);
            LockManager.GetLock(value.GetType().Name, () =>
            {
                Files.Write(filename, json);
            });
        }
    }
}