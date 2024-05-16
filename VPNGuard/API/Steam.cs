using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PlayerStatsSystem;
using PluginAPI.Core;
using Utf8Json;
using VPNGuard.Objects;

namespace VPNGuard.API
{
    public class Steam
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public static async Task<SteamPlayer> GetSteamPlayerData(string userid)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={Plugin.Singleton.PluginConfig.SteamApiKey}&steamids={userid.Split('@')[0]}&format=json");

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    string errorResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                    Log.Error(httpResponseMessage.StatusCode == (HttpStatusCode)429
                        ? $"Could not get Steam player data for player with User ID {userid}. You have reached your API key's limit."
                        : $"Steam API connection error: {httpResponseMessage.StatusCode} - {errorResponse}");
                    return null;
                }
                string apiResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SteamGetPlayerSummariesResponse>(apiResponse).response.players[0];
            }

            catch(Exception ex)
            {
                Log.Error($"An exception occurred when trying to get Steam account data for {userid}. Exception: {ex}");
                return null;
            }
        }

        public static async Task<SteamGame> GetSteamGameData(string userid)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={Plugin.Singleton.PluginConfig.SteamApiKey}&format=json&steamid={userid}&appids_filter[0]=700330");

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    string errorResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                    Log.Error(httpResponseMessage.StatusCode == (HttpStatusCode)429
                        ? $"Could not get Steam game statistics for player with User ID {userid}. You have reached your API key's limit."
                        : $"Steam API connection error: {httpResponseMessage.StatusCode} - {errorResponse}");
                    return null;
                }
                string apiResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SteamGetOwnedGamesResponse>(apiResponse).response.games[0];
            }

            catch (Exception ex)
            {
                Log.Error($"An exception occurred when trying to get Steam account data for {userid}. Exception: {ex}");
                return null;
            }
        }

        public static double CalculateAccountAge(uint accountCreationUnix)
        {
            DateTime accountCreationDt = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(accountCreationUnix);
            return Math.Truncate(DateTime.Now.Subtract(accountCreationDt).TotalDays);
        }
    }
}
