using EddiCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;

// Number of worker threads is automatic because `Workers` is set to 0.
// There are 3 scopes of parallelization:
// (1) ClassLevel – each thread executes a TestClass worth of tests. Within the TestClass, the test methods execute serially.
//     This is the default – tests within a class might have interdependency, and we don’t want to be too aggressive.
// (2) MethodLevel – each thread executes a TestMethod.
// (3) Custom – the user can provide a plugin implementing the required execution semantics. 
// source: https://devblogs.microsoft.com/devops/mstest-v2-in-assembly-parallel-test-execution/
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]

namespace UnitTests
{
    public class TestBase
    {
        internal void MakeSafe()
        {
            // Prevent telemetry data from being reported based on test results
            Utilities._Rollbar.TelemetryEnabled = false;

            // Don't write to permanent storage (do this before we initialize our EDDI instance)
            Utilities.Files.unitTesting = true;

            // Set ourselves as in a beta game session to stop automatic sending of data to remote systems
            PrivateObject privateObject = new PrivateObject(EDDI.Instance);
            privateObject.SetFieldOrProperty("gameIsBeta", true);
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
