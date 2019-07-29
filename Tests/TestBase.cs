using Eddi;
using EddiEvents;
using Newtonsoft.Json.Linq;
using Rollbar;
using System;
using System.IO;

namespace UnitTests
{
    public class TestBase
    {
        internal void MakeSafe()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;

            // Set ourselves as in beta to stop sending data to remote systems
            EDDI.Instance.enqueueEvent(new FileHeaderEvent(DateTime.Now, "JournalBeta.txt", "beta", "beta"));

            // Don't write to permanent storage
            Utilities.Files.unitTesting = true;
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
