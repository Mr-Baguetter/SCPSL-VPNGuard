using Exiled.API.Features;
using NorthwoodLib;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Utf8Json;
using VPNShield.Objects;
using VPNShield.Enums;

namespace VPNShield.API
{
    public class Account
    {
        private readonly Plugin plugin;
        public Account(Plugin plugin) => this.plugin = plugin;
        private readonly HttpClient client = new();

        public async Task<AccountCheckResult> CheckAccountAge(IPAddress ipAddress, string userID) //A result of TRUE will kick.
        {
            try
            {
                if (!userID.Contains("@", StringComparison.InvariantCulture))
                    return AccountCheckResult.Pass; //Invalid UserID

                VPNShieldUserId userIdObj = DbManager.GetUserId(userID);

                if (userIdObj.AccountAgePassed)
                {
                    if (plugin.Config.VerboseMode)
                        Log.Debug($"UserID {userID} ({ipAddress}) has already passed an account age check.");

                    return AccountCheckResult.Pass;
                }
                    

                string[] userIdSplit = userID.Split('@');
                switch (userIdSplit[1].ToLower(CultureInfo.InvariantCulture))
                {
                    case "steam":
                        HttpResponseMessage webRequest = await client.GetAsync($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={plugin.Config.SteamApiKey}&steamids={userIdSplit[0]}&format=json");
                        if (!webRequest.IsSuccessStatusCode)
                        {
                            string errorResponse = await webRequest.Content.ReadAsStringAsync();
                            Log.Error(webRequest.StatusCode == (HttpStatusCode)429
                                ? "Steam account age check could not complete. You have reached your API key's limit."
                                : $"Steam API connection error: {webRequest.StatusCode} - {errorResponse}");
                            return AccountCheckResult.APIError;
                        }
                        string apiResponse = await webRequest.Content.ReadAsStringAsync();
                        SteamAgeApiResponse steamApiResponse = JsonSerializer.Deserialize<SteamAgeApiResponse>(apiResponse);

                        int communityvisibilitystate = steamApiResponse.response.players[0].communityvisibilitystate;

                        if (plugin.Config.AccountKickPrivate && communityvisibilitystate == 1)
                        {
                            if (plugin.Config.VerboseMode)
                                Log.Debug($"UserID {userID} ({ipAddress}) cannot have their account age checked due to their privacy settings. Kicking..");
                            return AccountCheckResult.Private;
                        }

                        int timecreated = steamApiResponse.response.players[0].timecreated;
                        DateTime creationDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timecreated);
                        int accountAge = (int)(DateTime.Today - creationDateTime).TotalDays;

                        if (accountAge < plugin.Config.SteamMinAge)
                        {
                            if (plugin.Config.VerboseMode)
                                Log.Debug($"UserID {userID} ({ipAddress}) is too young to be on this server (account is {accountAge} day(s) old). Kicking..");
                            return AccountCheckResult.Fail;
                        }

                        if (plugin.Config.VerboseMode)
                            Log.Debug($"UserID {userID} ({ipAddress}) is old enough to be on this server (account is {accountAge} day(s) old).");

                        userIdObj.AccountAgePassed = true;
                        DbManager.SaveUserId(userIdObj);

                        return AccountCheckResult.Pass;

                    case "discord":
                    case "northwood":
                    default:
                        return AccountCheckResult.Pass; //Ignore them
                }
            }
            
            catch (Exception ex)
            {
                Log.Error($"An exception occurred when checking Steam account age for {userID}. Exception: {ex}");
                return AccountCheckResult.APIError;
            }
        }

        public async Task<AccountCheckResult> CheckAccountPlaytime(IPAddress ipAddress, string userID) //A result of TRUE will kick.
        {
            try
            {
                if (!userID.Contains("@", StringComparison.InvariantCulture))
                    return AccountCheckResult.Pass; //Invalid UserID

                VPNShieldUserId userIdObj = DbManager.GetUserId(userID);

                if (userIdObj.AccountPlaytimePassed)
                {
                    if (plugin.Config.VerboseMode)
                        Log.Debug($"UserID {userID} ({ipAddress}) has already passed an SCP: SL playtime check.");
                    return AccountCheckResult.Pass;
                }
                    

                string[] userIdSplit = userID.Split('@');
                switch (userIdSplit[1].ToLower(CultureInfo.InvariantCulture))
                {
                    case "steam":
                        HttpResponseMessage webRequest = await client.GetAsync($"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={plugin.Config.SteamApiKey}&format=json&steamid={userIdSplit[0]}&appids_filter[0]=700330");
                        if (!webRequest.IsSuccessStatusCode)
                        {
                            string errorResponse = await webRequest.Content.ReadAsStringAsync();
                            Log.Error(webRequest.StatusCode == (HttpStatusCode)429
                                ? "Steam account playtime check could not complete. You have reached your API key's limit."
                                : $"Steam API connection error: {webRequest.StatusCode} - {errorResponse}");
                            return AccountCheckResult.APIError;
                        }
                        string apiResponse = await webRequest.Content.ReadAsStringAsync();
                        SteamPlaytimeApiResponse steamPlaytimeApiResponse = JsonSerializer.Deserialize<SteamPlaytimeApiResponse>(apiResponse);

                        if (plugin.Config.AccountKickPrivate && steamPlaytimeApiResponse.response.games == null)
                        {
                            if (plugin.Config.VerboseMode)
                                Log.Debug($"UserID {userID} ({ipAddress}) cannot have their SCP: SL playtime checked due to their privacy settings. Kicking..");
                            return AccountCheckResult.Private;
                        }

                        int totalPlaytime = steamPlaytimeApiResponse.response.games[0].playtime_forever;

                        if (totalPlaytime < plugin.Config.SteamMinPlaytime)
                        {
                            if (plugin.Config.VerboseMode)
                                Log.Debug($"UserID {userID} ({ipAddress}) has not exceeded the minimum SCP: SL playtime required for this server (account has played SCP: SL for {totalPlaytime} minute(s)). Kicking..");
                            return AccountCheckResult.Fail;
                        }

                        if (plugin.Config.VerboseMode)
                            Log.Debug($"UserID {userID} ({ipAddress}) has exceeded the minimum SCP: SL playtime required for this server (account has played SCP: SL for {totalPlaytime} minute(s)).");

                        userIdObj.AccountPlaytimePassed = true;
                        DbManager.SaveUserId(userIdObj);

                        return AccountCheckResult.Pass;
                    case "discord":
                    case "northwood":
                    default:
                        return AccountCheckResult.Pass; //Ignore them
                }
            }

            catch (Exception ex)
            {
                Log.Error($"An exception occurred when checking SCP: SL playtime for {userID}. Exception: {ex}");
                return AccountCheckResult.APIError;
            }
        }
    }
}