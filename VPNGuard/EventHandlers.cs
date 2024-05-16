using LiteNetLib;
using LiteNetLib.Utils;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using System;
using System.Threading.Tasks;
using VPNGuard.API;
using VPNGuard.Objects;

namespace VPNGuard
{
    public class EventHandlers
    {
        [PluginEvent(ServerEventType.PlayerPreauth)]
        PreauthCancellationData OnPlayerPreauth(string userid, string ipAddress, long expiration, CentralAuthPreauthFlags flags, string country, byte[] signature, ConnectionRequest req, Int32 index)
        {
            Plugin.Singleton.VerboseLog($"{userid} from {ipAddress} ({country}) is preauthenticating!");

            //Let Northwood Staff bypass checks.
            if (flags.HasFlag(CentralAuthPreauthFlags.NorthwoodStaff))
            {
                Plugin.Singleton.VerboseLog($"{userid} from {ipAddress} ({country}) is Northwood Staff. Bypassing account / VPN checks...");
                return PreauthCancellationData.Accept();
            }

            //Let whitelisted users bypass checks.

            if (Plugin.UserIds.ContainsKey(userid) && Plugin.UserIds[userid].whitelisted)
            {
                Plugin.Singleton.VerboseLog($"{userid} from {ipAddress} ({country}) is whitelisted from VPNGuard checks. Bypassing account / VPN checks...");
                return PreauthCancellationData.Accept();
            }

            //Perform checks for user USING EXISTING DATA.
            //Only done for IP as once an IP is whitelisted / blacklisted, its status never changes (by this plugin anyways)

            if (Plugin.Singleton.PluginConfig.VpnCheck && Plugin.IPAddresses.ContainsKey(ipAddress))
            {
                if (Plugin.IPAddresses[ipAddress].Block)
                {
                    Plugin.Singleton.VerboseLog($"{ipAddress} is already known to be a VPN / proxy. Rejecting connection...");
                    return PreauthCancellationData.Reject(Plugin.Singleton.PluginConfig.VpnCheckKickMessage, true);
                }
            }

            return PreauthCancellationData.Accept();
        }


        [PluginEvent(ServerEventType.PlayerJoined)]
        async void OnPlayerJoined(Player player)
        {
            Plugin.Singleton.VerboseLog($"{player.UserId} from {player.IpAddress} has joined the game.");

            //Let Northwood Staff bypass checks.
            if (player.IsNorthwoodStaff || player.UserId.EndsWith("@northwood"))
            {
                Plugin.Singleton.VerboseLog($"{player.UserId} from {player.IpAddress} is Northwood Staff. Bypassing account / VPN checks...");

                return;
            }

            //Let whitelisted users bypass checks.
            if (Plugin.UserIds.ContainsKey(player.UserId) && Plugin.UserIds[player.UserId].whitelisted)
            {
                Plugin.Singleton.VerboseLog($"{player.UserId} from {player.IpAddress} is whitelisted from VPNGuard checks. Bypassing account / VPN checks...");

                return;
            }

            //Perform VPN checks is VPN check is enabled.
            if (Plugin.Singleton.PluginConfig.VpnCheck)
            {
                
                VPNGuardIP vpnGuardIp = await VPN.CheckIP(player.IpAddress);

                if (!Plugin.IPAddresses.ContainsKey(player.IpAddress))
                    Plugin.IPAddresses.Add(player.IpAddress, vpnGuardIp);

                if (vpnGuardIp.Block)
                {
                    KickPlayer(player, Plugin.Singleton.PluginConfig.VpnCheckKickMessage);
                    return;
                }
            }

            //If account age checked is performed and player is kicked, end here.
            if (Plugin.Singleton.PluginConfig.AccountAgeCheck && (await AccountAgeCheck(player) == false))
                return;

            //If account playtime checked is performed and player is kicked, end here.
            if (Plugin.Singleton.PluginConfig.AccountPlaytimeCheck && (await AccountPlaytimeCheck(player) == false))
                return;

            //All done.
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        void OnWaitingForPlayers()
        {
            //Reinit file system so that new IPs and UserIDs are loaded into memory.
            Filesystem.SaveIPAddresses();
            Filesystem.SaveUserIds();
            Filesystem.Init();
        }

        void KickPlayer(Player player, string kickReason)
        {
            //Kick player.
            player.Kick(kickReason);

            //If send to Discord is setup, send to Discord.
            if (Plugin.Singleton.PluginConfig.SendToDiscordWebhook)
                _ = API.Discord.SendDiscordWebHookMsg(Plugin.Singleton.PluginConfig.SendToDiscordWebhookUrl, player, kickReason);
        }

        async Task<bool> AccountAgeCheck(Player player)
        {
            Plugin.Singleton.VerboseLog($"Performing an account age check for player {player.UserId} from {player.IpAddress}.");

            //Check if User ID has VPNGuardUserId obj. If not, create one.
            if (Plugin.UserIds.ContainsKey(player.UserId))
            {
                if (Plugin.UserIds[player.UserId].passedAccountAgeCheck)
                {
                    Plugin.Singleton.VerboseLog($"{player.UserId} from {player.IpAddress} is already known to have passed an account age check.");
                    return true;
                }
                    
            }

            else
            {
                VPNGuardUserId vpnGuardUserId = new VPNGuardUserId(player.UserId, 0);
                Plugin.UserIds.Add(player.UserId, vpnGuardUserId);
            }

            //Get info from Steam.
            SteamPlayer steamPlayer = await Steam.GetSteamPlayerData(player.UserId);

            //If error is returned from Steam and account_kick_on_steam_error is set, kick player.
            if (steamPlayer == null && Plugin.Singleton.PluginConfig.AccountKickOnSteamError)
            {
                Plugin.Singleton.VerboseLog($"An error occurred when contacting the Steam API. Kicking player as per config settings...");
                KickPlayer(player, Plugin.Singleton.PluginConfig.AccountSteamErrorKickMessage);
                return false;
            }

            //If player profile is private and we cannot get information, kick if set.

            if (steamPlayer.communityvisibilitystate == 1)
            {
                if (Plugin.Singleton.PluginConfig.AccountKickPrivate)
                {
                    Plugin.Singleton.VerboseLog($"The profile of {player.UserId} from {player.IpAddress} is set to private. Kicking as per config settings...");

                    KickPlayer(player, Plugin.Singleton.PluginConfig.AccountPrivateKickMessage);
                    return false;
                }
            }

            //Calculate Steam age.
            double steamAccountAge = Steam.CalculateAccountAge(steamPlayer.timecreated);

            //If player's steam account is not old enough, kick.
            if (steamAccountAge < Plugin.Singleton.PluginConfig.SteamMinAge)
            {
                Plugin.Singleton.VerboseLog($"{player.UserId} from {player.IpAddress} is not old enough to play on this server (account is {steamAccountAge} day(s) old). Kicking..."); ;
                KickPlayer(player, Plugin.Singleton.PluginConfig.AccountAgeCheckKickMessage.Replace("%MINIMUMAGE%", Plugin.Singleton.PluginConfig.SteamMinAge.ToString()));
                return false;
            }

            //Player has passed Steam account age check.
            Plugin.Singleton.VerboseLog($"{player.UserId} from {player.IpAddress} is old enough to play on this server (account is {steamAccountAge} day(s) old.");

            Plugin.UserIds[player.UserId].passedAccountAgeCheck = true;
            return true;
        }

        async Task<bool> AccountPlaytimeCheck(Player player)
        {
            Plugin.Singleton.VerboseLog($"Performing an account age check for player {player.UserId} from {player.IpAddress}.");

            //Check if User ID has VPNGuardUserId obj. If not, create one.
            if (Plugin.UserIds.ContainsKey(player.UserId))
            {
                if (Plugin.UserIds[player.UserId].passedAccountPlaytimeCheck)
                {
                    Plugin.Singleton.VerboseLog($"{player.UserId} from {player.IpAddress} is already known to have passed an account age check.");
                    return true;
                }
                    
            }

            else
            {
                VPNGuardUserId vpnGuardUserId = new VPNGuardUserId(player.UserId, 0);
                Plugin.UserIds.Add(player.UserId, vpnGuardUserId);
            }

            //Get info from Steam.
            SteamGame steamGame = await Steam.GetSteamGameData(player.UserId);

            //If error is returned from Steam and account_kick_on_steam_error is set, kick player.
            if (steamGame == null && Plugin.Singleton.PluginConfig.AccountKickOnSteamError)
            {
                Plugin.Singleton.VerboseLog($"An error occurred when contacting the Steam API. Kicking player as per config settings...");
                KickPlayer(player, Plugin.Singleton.PluginConfig.AccountSteamErrorKickMessage);
                return false;
            }

            //If player's steam account has not played enough SCP: SL, kick.
            if (steamGame.playtime_forever < Plugin.Singleton.PluginConfig.SteamMinPlaytime)
            {
                Plugin.Singleton.VerboseLog($"{player.UserId} from {player.IpAddress} has not played enough to be on this server (account has played {steamGame.playtime_forever} minute(s) of SCP: SL). Kicking...");
                KickPlayer(player, Plugin.Singleton.PluginConfig.AccountPlaytimeCheckKickMessage.Replace("%MINIMUMPLAYTIME%", Plugin.Singleton.PluginConfig.SteamMinPlaytime.ToString()));
                return false;
            }

            //Player has passed Steam account playtime check.
            Plugin.Singleton.VerboseLog($"{player.UserId} from {player.IpAddress} has played enough to be on this server (account has played {steamGame.playtime_forever} minute(s) of SCP: SL).");
            Plugin.UserIds[player.UserId].passedAccountPlaytimeCheck = true;

            return true;
        }
    }
}
