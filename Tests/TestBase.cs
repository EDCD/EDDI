using Eddi;
using Newtonsoft.Json.Linq;
using Rollbar;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    public class TestBase
    {
        internal void MakeSafe()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;

            // Don't write to permanent storage (do this before we initialize our EDDI instance)
            Utilities.Files.unitTesting = true;

            // Set ourselves as in a beta game session to stop automatic sending of data to remote systems
            PrivateObject privateObject = new PrivateObject(EDDI.Instance);
            privateObject.SetFieldOrProperty("gameIsBeta", true );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static T DeserializeJsonResource<T>(byte[] data) where T : class
        {
            using (var stream = new MemoryStream(data))
            {
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    if (typeof(T) == typeof(string))
                    {
                        return Newtonsoft.Json.JsonSerializer.Create().Deserialize(reader, typeof(JObject)).ToString() as T;
                    }
                    else
                    {
                        return Newtonsoft.Json.JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
                    }
                }
            }
        }
    }
}
