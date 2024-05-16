using PluginAPI.Core;
using System.Net.Http;
using System.Reflection;

namespace VPNGuard
{
    public class UpdateChecker
    {
        public static async void CheckForUpdate()
        {
            Log.Info("Checking for update...");

            using (HttpClient httpClient = new HttpClient())
            {
                // Add user agent.
                httpClient.DefaultRequestHeaders.Add("User-Agent", $"{Plugin.Handler.PluginName} Update Checker - Running version {Plugin.Handler.PluginVersion} .");
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("https://scpsl.somewhatsane.co.uk/plugins/vpnguard/latest.html");

                // If non-successful response is returned from server, output error message.
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    Log.Error($"Unsuccessful status code received from update server. Status code: {(int)httpResponseMessage.StatusCode} {httpResponseMessage.StatusCode}.");
                    return;
                }

                string[] response = (await httpResponseMessage.Content.ReadAsStringAsync()).Split(';');

                // If response downloaded is not what is expected, output message.
                if (response.Length != 2)
                {
                    Log.Error($"Unexpected response from update server. Expected 2 parts, got {response.Length} parts.");
                    return;
                }

                // Check current version against latest reported by server and output message accordingly.

                if (response[0] != Assembly.GetExecutingAssembly().GetName().Version.ToString())
                {
                    Log.Info($"An update to VPNGuard is available! (Your version: {Plugin.Handler.PluginVersion}, Latest version: {response[0]}).");
                    Log.Info($"Download the latest VPNGuard at: {response[1]} .");
                }

                else
                    Log.Info("You are running the latest version of VPNGuard.");
            }
            
        }
    }
}
