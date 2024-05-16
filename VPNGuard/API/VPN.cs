using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using PluginAPI.Core;
using VPNGuard.Objects;
using Utf8Json;

namespace VPNGuard.API
{
    public class VPN
    {
        //Returns TRUE if VPN, otherwise FALSE.
        private static readonly HttpClient httpClient = new HttpClient();
        public static async Task<VPNGuardIP> CheckIP(string ipAddress)
        {
            //If IP is already known, return to calling function.
            if (Plugin.IPAddresses.ContainsKey(ipAddress))
            {
                if (Plugin.Singleton.PluginConfig.VerboseMode)
                {
                    if (Plugin.IPAddresses[ipAddress].Block)
                        Log.Debug($"{ipAddress} is a known VPN / proxy.");
                    else
                        Log.Debug($"{ipAddress} is a known as not a VPN or proxy.");
                }

                return Plugin.IPAddresses[ipAddress];
            }

            //Otherwise, cheeck IP address against API.
            switch (Plugin.Singleton.PluginConfig.VpnCheckService)
            {
                //IPHub
                case 0:
                        
                    //Check to make sure we have all of the details we need to check IPHub.

                    if (string.IsNullOrWhiteSpace(Plugin.Singleton.PluginConfig.IphubApiKey))
                    {
                        Log.Warning($"Could not perform VPN check for IP {ipAddress}. IPHub API key is not set or invalid.");
                        return null;
                    }

                    //Add API key to request headers if it is not present already.

                    if (!httpClient.DefaultRequestHeaders.Contains("x-key"))
                        httpClient.DefaultRequestHeaders.Add("x-key", Plugin.Singleton.PluginConfig.IphubApiKey);

                    //Check IPHub.

                    HttpResponseMessage ipHubResponse = await httpClient.GetAsync($"https://v2.api.iphub.info/ip/{ipAddress}");
                    string apiResponse = await ipHubResponse.Content.ReadAsStringAsync();

                    //If check was not completed successfully, output a message.

                    if (!ipHubResponse.IsSuccessStatusCode)
                    {
                        Log.Error(ipHubResponse.StatusCode == (HttpStatusCode)429
                            ? $"Could not perform VPN check for IP {ipAddress}. IPHub - You have reached your API key's limit."
                            : $"Could not perform VPN check for IP {ipAddress}. VPN API connection error: {ipHubResponse.StatusCode} - {apiResponse}");
                        return null;
                    }

                    IpHubApiResponse ipHubApiResponse = JsonSerializer.Deserialize<IpHubApiResponse>(apiResponse);

                    VPNGuardIP vpnGuardIP = new VPNGuardIP(ipAddress, false);

                    //Depending on block status, mark IP as VPN.
                    switch (ipHubApiResponse.block)
                    {
                        case 0:
                            if (Plugin.Singleton.PluginConfig.VerboseMode)
                                Log.Debug($"{ipAddress} is not a detectable VPN.");

                            break;
                        case 1:
                            if (Plugin.Singleton.PluginConfig.VerboseMode)
                                Log.Debug($"{ipAddress} is a detectable VPN.");

                            vpnGuardIP = new VPNGuardIP(ipAddress, true);
                            break;
                        case 2:
                            if (Plugin.Singleton.PluginConfig.IphubStrictBlocking)
                            {
                                if (Plugin.Singleton.PluginConfig.VerboseMode)
                                    Log.Debug($"{ipAddress} is a detectable VPN, detected by IPHub strict blocking.");

                                vpnGuardIP = new VPNGuardIP(ipAddress, true);
                                break;
                            }

                            else
                            {
                                if (Plugin.Singleton.PluginConfig.VerboseMode)
                                    Log.Debug($"{ipAddress} is not a detectable VPN, even with IPHub strict blocking enabled.");

                                break;
                            }
                    }
                    return vpnGuardIP;

                //GetIPIntel
                case 1:
                    
                    //Create URL.

                    string url = $"http://{Plugin.Singleton.PluginConfig.GetipintelSubdomain}.getipintel.net/check.php?ip={ipAddress}&contact={Plugin.Singleton.PluginConfig.GetipintelContactEmailAddress}";
                    if (!string.IsNullOrWhiteSpace(Plugin.Singleton.PluginConfig.GetipintelOptionalFlags))
                        url += $"&flags={Plugin.Singleton.PluginConfig.GetipintelOptionalFlags}";
                    if (Plugin.Singleton.PluginConfig.GetipintelBlockMobileIsps)
                        url += $"&oflags=m";

                    //Check GetIpIntel.
                    HttpResponseMessage webRequest = await httpClient.GetAsync(url);

                    //If check was not completed successfully, output a message.
                    if (!webRequest.IsSuccessStatusCode)
                    {
                        Log.Error(webRequest.StatusCode == (HttpStatusCode)429
                            ? "VPN check could not complete. You have reached your API key's limit."
                            : $"VPN API connection error: {webRequest.StatusCode} - {webRequest.Content.ReadAsStringAsync().Result}");
                        return null;
                    }

                    string giiApiResponse = await webRequest.Content.ReadAsStringAsync();

                    //If block mobile ISPs is enabled...
                    if (Plugin.Singleton.PluginConfig.GetipintelBlockMobileIsps)
                    {
                        string[] mobileResponse = giiApiResponse.Split(',');

                        if (mobileResponse[1] == "1")
                        {
                            if (Plugin.Singleton.PluginConfig.VerboseMode)
                                Log.Debug($"{ipAddress} is a mobile data connection. Kicking...");

                            vpnGuardIP = new VPNGuardIP(ipAddress, true, float.Parse(mobileResponse[0]), true);
                            return vpnGuardIP;
                        }
                        else
                            giiApiResponse = mobileResponse[0];

                    }

                    if (float.TryParse(giiApiResponse, out float score))
                    {
                        switch (score)
                        {
                            //Negative scores are a result of bad API calls. Anything between 0 and 1 is a valid score.
                            case -1:
                                Log.Error($"VPN check could not complete. Error from API: {score} - No input.");
                                break;
                            case -2:
                                Log.Error($"VPN check could not complete. Error from API: {score} - Invalid IP Address.");
                                break;
                            case -3:
                                Log.Error($"VPN check could not complete. Error from API: {score} - Unroutable address / private address.");
                                break;
                            case -4:
                                Log.Error($"VPN check could not complete. Error from API: {score} - Unable to reach database.");
                                break;
                            case -5:
                                Log.Error($"VPN check could not complete. Error from API: {score} - Your IP Address has been banned from the system.");
                                break;
                            case -6:
                                Log.Error($"VPN check could not complete. Error from API: {score} - You did not provide any contact information.");
                                break;
                            default:
                                vpnGuardIP = new VPNGuardIP(ipAddress, false, score, false);

                                if (Plugin.Singleton.PluginConfig.GetipintelMaxScore < score)
                                {
                                    vpnGuardIP.Block = true;

                                    if (Plugin.Singleton.PluginConfig.VerboseMode)
                                        Log.Debug($"{ipAddress} is a detectable VPN (score is {score}). Kicking...");
                                }

                                else if (Plugin.Singleton.PluginConfig.VerboseMode)
                                        Log.Debug($"{ipAddress} is not a detectable VPN (score is {score}).");

                                return vpnGuardIP;
                        }
                    }

                    else
                        Log.Error($"VPN check could not complete. Unexpected response from API. Response from API: {score}.");

                    return null;
                
                //Unknown API
                default:
                    Log.Warning($"Could not perform VPN check for IP {ipAddress}. VPN check service value is invalid.");
                    return null;

            }
        }
    }
}
