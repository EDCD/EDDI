using EddiCompanionAppService.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using JetBrains.Annotations;
using Utilities;

namespace EddiCompanionAppService
{
    public abstract class Endpoint
    {
        protected JObject GetEndpoint(string endpointURL)
        {
            JObject newJson = null;
            try
            {
                var result = CompanionAppService.Instance.obtainData(CompanionAppService.Instance.ServerURL() + endpointURL);
                var data = result.Item1;
                var timestamp = result.Item2;

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
                        if (data == null || !data.StartsWith("{"))
                        {
                            // No luck with a re-login; give up
                            CompanionAppService.Instance.CurrentState = CompanionAppService.State.ConnectionLost;
                            CompanionAppService.Instance.Logout();
                            throw new EliteDangerousCompanionAppException(
                                "Failed to obtain data from Frontier server (" + CompanionAppService.Instance.CurrentState + ")");
                        }
                    }
                }
                else
                {
                    try
                    {
                        Logging.Debug($"{endpointURL} endpoint returned " + data);
                        newJson = JObject.Parse(data);
                        newJson.Add("timestamp", timestamp);
                    }
                    catch (JsonException ex)
                    {
                        Logging.Error($"Failed to parse Frontier server {endpointURL} data", ex);
                        newJson = null;
                    }
                }

            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as telemetry is getting spammed when the server is down
                Logging.Info(ex.Message);
            }

            return newJson;
        }
    }

    public class CompanionApiEventArgs : EventArgs
    {
        [UsedImplicitly] private JObject json;

        public CompanionApiEventArgs(JObject json)
        {
            this.json = json;
        }
    }
}