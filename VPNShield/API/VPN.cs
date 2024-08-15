using Exiled.API.Features;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VPNShield.Objects;
using Utf8Json;

namespace VPNShield.API
{
    public class VPN
    {
        private readonly Plugin plugin;
        public VPN(Plugin plugin) => this.plugin = plugin;
        private readonly HttpClient client = new();

        public async Task<bool> CheckGii(string ipAddress, string userID)
        {
            try
            {
                VPNShieldIP ipAddressObj = DbManager.GetIP(ipAddress);

                if (ipAddressObj.CheckedAt != 0)
                {
                    if (ipAddressObj.Blacklisted)
                    {
                        Log.Debug($"{ipAddress} ({userID}) is already known to be a VPN / is blacklisted. Kicking...");
                        return true;
                    }

                    else
                    {
                        Log.Debug($"{ipAddress} ({userID}) has already passed a VPN check / is whitelisted.");
                        return false;
                    }

                }

                string url = $"http://{plugin.Config.GetipintelSubdomain}.getipintel.net/check.php?ip={ipAddress}&contact={plugin.Config.GetipintelContactEmailAddress}";
                if (!string.IsNullOrWhiteSpace(plugin.Config.GetipintelOptionalFlags))
                    url += $"&flags={plugin.Config.GetipintelOptionalFlags}";
                if (plugin.Config.GetipintelBlockMobileIsps)
                    url += $"&oflags=m";


                HttpResponseMessage webRequest = await client.GetAsync(url);

                if (!webRequest.IsSuccessStatusCode)
                {
                    Log.Error(webRequest.StatusCode == (HttpStatusCode)429
                        ? "VPN check could not complete. You have reached your API key's limit."
                        : $"VPN API connection error: {webRequest.StatusCode} - {webRequest.Content.ReadAsStringAsync().Result}");
                    return false;
                }

                string apiResponse = await webRequest.Content.ReadAsStringAsync();

                if (plugin.Config.GetipintelBlockMobileIsps)
                {
                    string[] mobileResponse = apiResponse.Split(',');

                    if (mobileResponse[1] == "1")
                    {
                        if (plugin.Config.VerboseMode)
                            Log.Debug($"{ipAddress} ({userID}) is a playing on a mobile data connection. Kicking...");

                        ipAddressObj.Blacklisted = true;
                        ipAddressObj.GiiScore = float.Parse(mobileResponse[0]);
                        ipAddressObj.GiiMobile = int.Parse(mobileResponse[1]);
                        ipAddressObj.CheckedAt = DateTime.UtcNow.Ticks;

                        DbManager.SaveIP(ipAddressObj);

                        return true;
                    }
                    else
                        apiResponse = mobileResponse[0];

                }

                if (float.TryParse(apiResponse, out float score))
                {
                    switch (score)
                    {
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
                            ipAddressObj.GiiScore = score;
                            ipAddressObj.CheckedAt = DateTime.UtcNow.Ticks;

                            if (plugin.Config.GetipintelMaxScore < score)
                            {
                                ipAddressObj.Blacklisted = true;
                                if (plugin.Config.VerboseMode)
                                    Log.Debug($"{ipAddress} ({userID}) is a detectable VPN (score is {score}). Kicking...");

                                DbManager.SaveIP(ipAddressObj);

                                return true;
                            }

                            else
                            {
                                if (plugin.Config.VerboseMode)
                                    Log.Debug($"{ipAddress} ({userID}) is not a detectable VPN (score is {score}).");

                                DbManager.SaveIP(ipAddressObj);
                                return false;
                            }
                    }
                }

                else
                    Log.Error($"VPN check could not complete. Unexpected response from API. Response from API: {score}.");

                return false;
            }

            catch (Exception ex)
            {
                Log.Error($"An error occurred when trying to check GetIPIntel for {ipAddress}. Exception: {ex}.");
                return false;
            }
            
        }

        public async Task<bool> CheckIPHub(string ipAddress, string userID) //A result of TRUE will kick.
        {
            try
            {
                VPNShieldIP ipAddressObj = DbManager.GetIP(ipAddress);

                if (ipAddressObj.CheckedAt != 0)
                {
                    if (ipAddressObj.Blacklisted)
                    {
                        Log.Debug($"{ipAddress} ({userID}) is already known as a VPN / is blacklisted. Kicking...");
                        return true;
                    }

                    else
                    {
                        Log.Debug($"{ipAddress} ({userID}) has already passed a VPN check / is whitelisted.");
                        return false;
                    }

                }

                if (!client.DefaultRequestHeaders.Contains("x-key"))
                    client.DefaultRequestHeaders.Add("x-key", plugin.Config.IphubApiKey);

                HttpResponseMessage webRequest = await client.GetAsync($"https://v2.api.iphub.info/ip/{ipAddress}");
                ipAddressObj.CheckedAt = DateTime.UtcNow.Ticks;

                if (!webRequest.IsSuccessStatusCode)
                {
                    string errorResponse = await webRequest.Content.ReadAsStringAsync();
                    Log.Error(webRequest.StatusCode == (HttpStatusCode)429
                        ? "VPN check could not complete. You have reached your API key's limit."
                        : $"VPN API connection error: {webRequest.StatusCode} - {errorResponse}");
                    return false;
                }

                string apiResponse = await webRequest.Content.ReadAsStringAsync();
                IpHubApiResponse ipHubApiResponse = JsonSerializer.Deserialize<IpHubApiResponse>(apiResponse);


                switch (ipHubApiResponse.block)
                {
                    case 0:
                        {
                            if (plugin.Config.VerboseMode)
                                Log.Debug($"{ipAddress} ({userID}) is not a detectable VPN.");


                            DbManager.SaveIP(ipAddressObj);
                            return false;
                        }

                    case 1:
                        {
                            if (plugin.Config.VerboseMode)
                                Log.Debug($"{ipAddress} ({userID}) is a detectable VPN. Kicking...");

                            ipAddressObj.Blacklisted = true;
                            DbManager.SaveIP(ipAddressObj);
                            return true;
                        }
                    case 2:
                        {
                            if (plugin.Config.IphubStrictBlocking)
                            {
                                if (plugin.Config.VerboseMode)
                                    Log.Debug($"{ipAddress} ({userID}) is a detectable VPN (detected by strict blocking). Kicking...");

                                ipAddressObj.Blacklisted = true;
                                DbManager.SaveIP(ipAddressObj);
                                return true;
                            }

                            if (plugin.Config.VerboseMode)
                                Log.Debug($"{ipAddress} ({userID}) is not a detectable VPN.");

                            DbManager.SaveIP(ipAddressObj);
                            return false;
                        }
                }
                return false;
            }

            catch (Exception ex)
            {
                Log.Error($"An exception occurred whilst checking IPHub. Exception: {ex}.");
                return false;
            }
                
        }
    }
}
