using Exiled.API.Features;
using System.Net.Http;
using System.Threading.Tasks;
using VPNShield.Objects;
using Utf8Json;

namespace VPNShield
{
    public static class UpdateCheck
    {
        public static async Task CheckForUpdate()
        {
            Log.Info("Checking for update...");

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("User-Agent", $"VPNShield Update Checker - Running VPNShield v{Plugin.version}");
            HttpResponseMessage response = await client.GetAsync($"https://scpsl.somewhatsane.co.uk/plugins/vpnshield/checkForUpdate.php?CurrentVersion={Plugin.version}");
            string data = await response.Content.ReadAsStringAsync();

            try
            {
                VPNShieldUpdateCheckResponse vpnShieldUpdateResponse = JsonSerializer.Deserialize<VPNShieldUpdateCheckResponse>(data);

                //All bad API requests have negative status codes so we can do a check for bad API response status codes by checking to see if they are less than 0.
                if (vpnShieldUpdateResponse.StatusCode < 0)
                {
                    Log.Error($"An error occurred when trying to check for update.\nStatus code: {vpnShieldUpdateResponse.StatusCode}\nMessage: {vpnShieldUpdateResponse.Message}");
                    return;
                }

                switch (vpnShieldUpdateResponse.StatusCode)
                {
                    case 0:
                        Log.Info("You are running the latest version of VPNShield.");
                        break;
                    case 1:
                        Log.Info($"A new version of VPNShield (v{vpnShieldUpdateResponse.LatestVersion}) is available. Download it from: {vpnShieldUpdateResponse.DownloadUrls[0]} or {vpnShieldUpdateResponse.DownloadUrls[1]} .");
                        break;
                }
            }

            catch
            {
                Log.Error($"An error occurred when trying to check for update. Could not deserilize response from server.\nHTTP Status Code: {response.StatusCode}\nResponse from server: {data}");
            }
        }
    }
}
