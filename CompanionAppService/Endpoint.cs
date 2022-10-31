using EddiCompanionAppService.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Utilities;

namespace EddiCompanionAppService
{
    public abstract class Endpoint
    {
        protected Tuple<JObject, DateTime> GetEndpoint(string endpointURL)
        {
            JObject newJson = null;
            DateTime timestamp = DateTime.MinValue;

            try
            {
                var result = CompanionAppService.Instance.obtainData(CompanionAppService.Instance.ServerURL() + endpointURL);
                var data = result.Item1;
                timestamp = result.Item2;

                if (data == null || !data.StartsWith("{"))
                {
                    // Happens if there is a problem with the API.  Logging in again might clear this...
                    CompanionAppService.Instance.relogin();
                    if (CompanionAppService.Instance.CurrentState != CompanionAppService.State.Authorized)
                    {
                        // No luck; give up
                        CompanionAppService.Instance.CurrentState = CompanionAppService.State.ConnectionLost;
                        CompanionAppService.Instance.Logout();
                    }
                    else
                    {
                        // Looks like login worked; try again
                        result = CompanionAppService.Instance.obtainData(CompanionAppService.Instance.ServerURL() + endpointURL);
                        data = result.Item1;
                        timestamp = result.Item2;

                        if (data == null || !data.StartsWith("{"))
                        {
                            // No luck with a relogin; give up
                            CompanionAppService.Instance.CurrentState = CompanionAppService.State.ConnectionLost;
                            CompanionAppService.Instance.Logout();
                            throw new EliteDangerousCompanionAppException(
                                "Failed to obtain data from Frontier server (" + CompanionAppService.Instance.CurrentState + ")");
                        }
                    }
                }

                try
                {
                    Logging.Debug($"{endpointURL} endpoint returned " + data);
                    newJson = JObject.Parse(data);
                }
                catch (JsonException ex)
                {
                    Logging.Error($"Failed to parse Frontier server {endpointURL} data", ex);
                    newJson = null;
                }
            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as Rollbar is getting spammed when the server is down
                Logging.Info(ex.Message);
            }

            return new Tuple<JObject, DateTime>(newJson, timestamp);
        }
    }

    public class CompanionApiEventArgs : EventArgs
    {
        public JObject json;
        public DateTime timestamp;

        public CompanionApiEventArgs(JObject json, DateTime timestamp)
        {
            this.json = json;
            this.timestamp = timestamp;
        }
    }
}