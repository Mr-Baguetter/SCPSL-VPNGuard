using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utf8Json;
using VPNShield.Objects;
using VPNShield.Enums;
using System.Text.RegularExpressions;

namespace VPNShield
{
    public class WebhookHandler
    {
        private readonly Plugin plugin;
        private readonly HttpClient httpClient = new();
        public WebhookHandler(Plugin plugin) => this.plugin = plugin;
        public async Task<bool> SendWebhook(string url, Player player, KickReason kickReason, VPNShieldIP checkResponse = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Log.Warn("Discord Webhook URL is null / whitespace! Not sending.");
                return false;
            }

            object webhookData = null;

            switch (kickReason)
            {
                case KickReason.AccountAge:
                    webhookData = new
                    {
                        content = "",
                        embeds = new List<object>
                        {
                            new
                            {
                                title = "[VPNShield] Player Kicked!",
                                color = "3447003",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "Server",
                                        value = StripRichText(Server.Name)
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
                                        value = player.IPAddress
                                    },
                                    new
                                    {
                                        name = "Kick reason",
                                        value = $"Player has not met / exceeded the minimum Steam account age set on this server ({plugin.Config.SteamMinAge})."
                                    }
                                },
                                timestamp = DateTime.UtcNow.ToString("o")
                            }
                        }
                    };
                    break;
                case KickReason.AccountPlaytime:
                    webhookData = new
                    {
                        content = "",
                        embeds = new List<object>
                        {
                            new
                            {
                                title = "[VPNShield] Player Kicked!",
                                color = "3066993",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "Server",
                                        value = StripRichText(Server.Name)
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
                                        value = player.IPAddress
                                    },
                                    new
                                    {
                                        name = "Kick reason",
                                        value = $"Player has not met the minimum required playtime set on this server ({plugin.Config.SteamMinPlaytime})."
                                    }
                                },
                                timestamp = DateTime.UtcNow.ToString("o")
                            }
                        }
                    };
                    break;
                case KickReason.AccountPrivate:
                    webhookData = new
                    {
                        content = "",
                        embeds = new List<object>
                        {
                            new
                            {
                                title = "[VPNShield] Player Kicked!",
                                color = "10181046",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "Server",
                                        value = StripRichText(Server.Name)
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
                                        value = player.IPAddress
                                    },
                                    new
                                    {
                                        name = "Kick reason",
                                        value = "The player's Steam account could not be checked at it is set to private."
                                    }
                                },
                                timestamp = DateTime.UtcNow.ToString("o")
                            }
                        }
                    };
                    break;

                case KickReason.AccountSteamError:
                    webhookData = new
                    {
                        content = "",
                        embeds = new List<object>
                        {
                            new
                            {
                                title = "[VPNShield] Player Kicked!",
                                color = "15105570",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "Server",
                                        value = StripRichText(Server.Name)
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
                                        value = player.IPAddress
                                    },
                                    new
                                    {
                                        name = "Kick reason",
                                        value = "An error occurred whilst checking Steam for Steam account details."
                                    }
                                },
                                timestamp = DateTime.UtcNow.ToString("o")
                            }
                        }
                    };
                    break;

                case KickReason.VPN:

                    switch (plugin.Config.VpnCheckService)
                    {
                        case 0:
                            webhookData = new
                            {
                                content = "",
                                embeds = new List<object>
                        {
                            new
                            {
                                title = "[VPNShield] Player Kicked!",
                                color = "15158332",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "Server",
                                        value = StripRichText(Server.Name)
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
                                        value = player.IPAddress
                                    },
                                    new
                                    {
                                        name = "Kick reason",
                                        value = "Failed VPN check."
                                    }
                                },
                                timestamp = DateTime.UtcNow.ToString("o")
                            }
                        }
                            };
                            break;
                        case 1:
                            webhookData = new
                            {
                                content = "",
                                embeds = new List<object>
                        {
                            new
                            {
                                title = "[VPNShield] Player Kicked!",
                                color = "15158332",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "Server",
                                        value = StripRichText(Server.Name)
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
                                        value = player.IPAddress
                                    },
                                    new
                                    {
                                        name = "GII Score",
                                        value = checkResponse.GiiScore.ToString()
                                    },
                                    new
                                    {
                                        name = "Mobile ISP? (-1 if unknown)",
                                        value = checkResponse.GiiMobile.ToString()
                                    },
                                    new
                                    {
                                        name = "GII checked at",
                                        value = new DateTime(checkResponse.CheckedAt).ToString()
                                    },
                                    new
                                    {
                                        name = "Kick reason",
                                        value = "Failed VPN check."
                                    }
                                },
                                timestamp = DateTime.UtcNow.ToString("o")
                            }
                        }
                            };
                            break;
                    }
                    
                    break;
                case KickReason.None:
                    webhookData = new
                    {
                        content = "",
                        embeds = new List<object>
                        {
                            new
                            {
                                title = "[VPNShield] Player Kicked!",
                                color = "9936031",
                                fields = new List<object>
                                {
                                    new
                                    {
                                        name = "Server",
                                        value = StripRichText(Server.Name)
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
                                        value = player.IPAddress
                                    },
                                    new
                                    {
                                        name = "Kick reason",
                                        value = "None / Unknown."
                                    }
                                },
                                timestamp = DateTime.UtcNow.ToString("o")
                            }
                        }
                    };
                    break;

            }

            StringContent webhookStringContent = new(Encoding.UTF8.GetString(JsonSerializer.Serialize(webhookData)), Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = await httpClient.PostAsync(url, webhookStringContent);
            string responseMessageString = await responseMessage.Content.ReadAsStringAsync();

            if (!responseMessage.IsSuccessStatusCode)
            {
                Log.Error($"[{(int)responseMessage.StatusCode} - {responseMessage.StatusCode}] A non-successful status code was returned by Discord when trying to post to webhook regarding {player.UserId}'s ({player.IPAddress}) kick. Response Message: {responseMessageString}.");
                return false;
            }

            if (plugin.Config.VerboseMode)
                Log.Debug($"Posted to Discord webhook regarding {player.UserId}'s ({player.IPAddress}) kick successfully!");

            return true;
        }

        private static string StripRichText(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}