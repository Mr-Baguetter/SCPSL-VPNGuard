using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PluginAPI.Core;
using Utf8Json;

namespace VPNGuard.API
{
    public class Discord
    {
        private readonly static HttpClient httpClient = new HttpClient();

        public static async Task<bool> SendDiscordWebHookMsg(string discordWebhookUrl, Player player, string kickReason)
        {
            try
            {
                object webhookData = new
                {
                    content = "",
                    embeds = new List<object>
                    {
                        new
                        {
                            title = "[VPNGuard] Player kicked!",
                            color = "3447003",
                            fields = new List<object>
                            {
                                new
                                {
                                    name = "Server",
                                    value = StripRichText(GameCore.ConfigFile.ServerConfig.GetString("server_name", "My Server Name"))
                                },
                                new
                                {
                                    name = "Nickname",
                                    value = player.Nickname
                                },
                                new
                                {
                                    name = "User ID",
                                    value = player.UserId
                                },
                                new
                                {
                                    name = "IP Address",
                                    value = player.IpAddress
                                },
                                new
                                {
                                    name = "Kick reason",
                                    value = kickReason
                                }
                            },
                            timestamp = DateTime.UtcNow.ToString("o")
                        }
                    }
                };

                StringContent webhookStringContent = new StringContent(Encoding.UTF8.GetString(JsonSerializer.Serialize(webhookData)), Encoding.UTF8, "application/json");
                HttpResponseMessage responseMessage = await httpClient.PostAsync(discordWebhookUrl, webhookStringContent);
                string responseMessageString = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    Log.Error($"[{(int)responseMessage.StatusCode} - {responseMessage.StatusCode}] A non-successful status code was returned by Discord when trying to post to webhook regarding {player.UserId}'s ({player.IpAddress}) kick. Response Message: {responseMessageString}.");
                    return false;
                }

                return true;
            }
            
            catch (Exception ex)
            {
                Log.Error($"An exception occurred when trying to send to Discord webhook. Exception: {ex}");
                return false;
            }
        }

        private static string StripRichText(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}
