using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using NorthwoodLib;
using LiteNetLib.Utils;
using VPNShield.Objects;
using VPNShield.Enums;
using Exiled.Events.EventArgs.Player;

namespace VPNShield.Handlers
{
    public class EventHandlers
    {
        private readonly Plugin plugin;
        public EventHandlers(Plugin plugin) => this.plugin = plugin;

        private static readonly NetDataWriter writer = new();
        private static readonly Dictionary<string, KickReason> ToKick = new();
        private const byte BypassFlags = (1 << 1) | (1 << 3); //IgnoreBans or IgnoreGeoblock

        public void PreAuthenticating(PreAuthenticatingEventArgs ev)
        {
            VPNShieldIP ipAddressObj = DbManager.GetIP(ev.Request.RemoteEndPoint.Address.ToString());
            VPNShieldUserId userIdObj = DbManager.GetUserId(ev.UserId);

            //Create IP and UserID objs if they do not exist and store in database.

            if (ipAddressObj == null)
            {
                ipAddressObj = new();
                ipAddressObj.IPAddress = ev.Request.RemoteEndPoint.Address.ToString();
                DbManager.SaveIP(ipAddressObj);

                if (plugin.Config.VerboseMode)
                    Log.Debug($"Created new IP address record for {ev.Request.RemoteEndPoint.Address}.");
            }

            if (userIdObj == null)
            {
                userIdObj = new();
                userIdObj.UserId = ev.UserId;
                DbManager.SaveUserId(userIdObj);

                if (plugin.Config.VerboseMode)
                    Log.Debug($"Created new User ID record for {ev.UserId}.");
            }

            if (userIdObj.Whitelisted)
            {
                if (plugin.Config.VerboseMode)
                    Log.Debug($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) is whitelisted from VPN and account age checks. Skipping checks.");
                return;
            }

            if (ev.UserId.Contains("@northwood", StringComparison.InvariantCultureIgnoreCase))
            {
                if (plugin.Config.VerboseMode)
                    Log.Debug($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) is a Northwood Studios member. Skipping checks.");
                return;
            }

            byte flags = (byte)ev.Flags;

            if ((flags & BypassFlags) > 0)
            {
                if (plugin.Config.VerboseMode)
                    Log.Debug($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) has bypass flags (flags: {(int)flags}). Skipping checks.");
                return;
            }

            if (plugin.Config.VerboseMode)
                Log.Debug($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) doesn't have bypass flags (flags: {(int)flags}).");

            if (plugin.Config.VpnCheck)
            {
                //Check subnets first.
                foreach (VPNShieldIPSubnet subnet in DbManager.GetSubnets().FindAll())
                {
                    if (Convenience.IpAddressIsInRange(ev.Request.RemoteEndPoint.Address, subnet.IPSubnet))
                    {
                        if (subnet.Blacklisted)
                        {
                            if (plugin.Config.VerboseMode)
                                Log.Debug($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) is within an IP address subnet that is blacklisted ({subnet.IPSubnet}). Kicking...");
                            writer.Reset();
                            writer.Put((byte)10);
                            writer.Put(plugin.Config.VpnCheckKickMessage); //Limit of 400 characters due to the limit of the UDP packet.
                            ev.Request.Reject(writer);
                        }

                        else
                        {
                            if (plugin.Config.VerboseMode)
                                Log.Debug($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) is within an IP address subnet that is whitelisted ({subnet.IPSubnet}).");
                        }

                        return;
                    }
                }

                //If already known to be blacklisted...
                if (ipAddressObj.Blacklisted)
                {
                    if (plugin.Config.VerboseMode)
                        Log.Debug($"UserID {ev.UserId} ({ev.Request.RemoteEndPoint.Address}) is already known to have failed a VPN check. Kicking...");

                    writer.Reset();
                    writer.Put((byte)10);
                    writer.Put(plugin.Config.VpnCheckKickMessage); //Limit of 400 characters due to the limit of the UDP packet.
                    ev.Request.Reject(writer);

                    return;
                }
               
                
            }

            _ = Check(ev); //Do checks on in task to prevent holding up the game.
        }

        public void Verified(VerifiedEventArgs ev)
        {
            //Kick player if found in ToKick dictionary.

            if (ToKick.ContainsKey(ev.Player.UserId))
            {
                //Send message to webhook if enabled.
                if (plugin.Config.SendToDiscordWebhook)
                {
                    VPNShieldIP ipAddressObj = DbManager.GetIP(ev.Player.IPAddress);
                    _ = plugin.WebhookHandler.SendWebhook(plugin.Config.SendToDiscordWebhookUrl, ev.Player, ToKick[ev.Player.UserId], ipAddressObj);
                }
                    

                switch (ToKick[ev.Player.UserId])
                {
                    case KickReason.None:
                        break;
                    case KickReason.AccountAge:
                        ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.AccountAgeCheckKickMessage.Replace("%MINIMUMAGE%", plugin.Config.SteamMinAge.ToString()));
                        break;
                    case KickReason.AccountPlaytime:
                        ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.AccountPlaytimeCheckKickMessage.Replace("%MINIMUMPLAYTIME%", plugin.Config.SteamMinPlaytime.ToString()));
                        break;
                    case KickReason.AccountPrivate:
                        ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.AccountPrivateKickMessage);
                        break;
                    case KickReason.AccountSteamError:
                        ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.AccountSteamErrorKickMessage);
                        break;
                    case KickReason.VPN:
                        ServerConsole.Disconnect(ev.Player.Connection, plugin.Config.VpnCheckKickMessage);
                        break;
                }

                ToKick.Remove(ev.Player.UserId);
            }
        }



        public async Task Check(PreAuthenticatingEventArgs ev)
        {
            //Account age check.
            if (plugin.Config.AccountAgeCheck)
            {
                if (!string.IsNullOrWhiteSpace(plugin.Config.SteamApiKey))
                {
                    string kickMessage = null;
                    KickReason kickReason = KickReason.None;

                    switch (await plugin.Account.CheckAccountAge(ev.Request.RemoteEndPoint.Address, ev.UserId))
                    {
                        case AccountCheckResult.Fail:
                            kickMessage = plugin.Config.AccountAgeCheckKickMessage.Replace("%MINIMUMAGE", plugin.Config.SteamMinAge.ToString());
                            kickReason = KickReason.AccountAge;
                            break;
                        case AccountCheckResult.Private:
                            kickMessage = plugin.Config.AccountPrivateKickMessage;
                            kickReason = KickReason.AccountPrivate;
                            break;
                        case AccountCheckResult.APIError:
                            kickMessage = plugin.Config.AccountSteamErrorKickMessage;
                            kickReason = KickReason.AccountSteamError;
                            break;
                    }

                    if (kickReason != KickReason.None)
                    {
                        Player player = Player.Get(ev.UserId);
                        if (player != null)
                        {
                            //Send message to webhook if enabled.
                            if (plugin.Config.SendToDiscordWebhook)
                                _ = plugin.WebhookHandler.SendWebhook(plugin.Config.SendToDiscordWebhookUrl, player, kickReason);
                                

                            ServerConsole.Disconnect(player.Connection, kickMessage);
                        }

                        else
                            ToKick.Add(ev.UserId, kickReason);

                        return;
                    }
                }

                else
                    Log.Warn($"An account age check cannot be performed for {ev.UserId} ({ev.Request.RemoteEndPoint.Address}). Steam API key is null.");

            }

            //Account playtime check.
            if (plugin.Config.AccountPlaytimeCheck)
            {
                if (!string.IsNullOrWhiteSpace(plugin.Config.SteamApiKey))
                {
                    string kickMessage = null;
                    KickReason kickReason = KickReason.None;

                    switch (await plugin.Account.CheckAccountPlaytime(ev.Request.RemoteEndPoint.Address, ev.UserId))
                    {
                        case AccountCheckResult.Fail:
                            kickMessage = plugin.Config.AccountPlaytimeCheckKickMessage.Replace("%MINIMUMPLAYTIME%", plugin.Config.SteamMinPlaytime.ToString());
                            kickReason = KickReason.AccountPlaytime;
                            break;
                        case AccountCheckResult.Private:
                            kickMessage = plugin.Config.AccountPrivateKickMessage;
                            kickReason = KickReason.AccountPrivate;
                            break;
                        case AccountCheckResult.APIError:
                            kickMessage = plugin.Config.AccountSteamErrorKickMessage;
                            kickReason = KickReason.AccountSteamError;
                            break;
                    }

                    if (kickReason != KickReason.None)
                    {
                        Player player = Player.Get(ev.UserId);
                        if (player != null)
                        {
                            //Send message to webhook if enabled.
                            if (plugin.Config.SendToDiscordWebhook)
                                _ = plugin.WebhookHandler.SendWebhook(plugin.Config.SendToDiscordWebhookUrl, player, kickReason);

                            ServerConsole.Disconnect(player.Connection, kickMessage);
                        }
                        else
                            ToKick.Add(ev.UserId, kickReason);

                        return;
                    }
                }

                else
                    Log.Warn($"An account playtime check cannot be performed for {ev.UserId} ({ev.Request.RemoteEndPoint.Address}). Steam API key is null.");
            }


            //VPN Check.
            if (plugin.Config.VpnCheck)
            {
                bool kick = false;

                switch (plugin.Config.VpnCheckService)
                {
                    case 0:
                        if (string.IsNullOrEmpty(plugin.Config.IphubApiKey))
                            Log.Warn($"A VPN check cannot be performed for {ev.Request.RemoteEndPoint.Address} ({ev.UserId}). IPHub API key is null.");
                        else
                            kick = await plugin.VPN.CheckIPHub(ev.Request.RemoteEndPoint.Address.ToString(), ev.UserId);
                        break;
                    case 1:
                        if (string.IsNullOrEmpty(plugin.Config.GetipintelSubdomain))
                            Log.Warn($"A VPN check cannot be performed for {ev.Request.RemoteEndPoint.Address} ({ev.UserId}). GetIpIntel subdomain is null.");
                        else if (string.IsNullOrEmpty(plugin.Config.GetipintelContactEmailAddress))
                            Log.Warn($"A VPN check cannot be performed for {ev.Request.RemoteEndPoint.Address} ({ev.UserId}). GetIpIntel contact email address is null.");
                        else
                            kick = await plugin.VPN.CheckGii(ev.Request.RemoteEndPoint.Address.ToString(), ev.UserId);
                        break;
                    default:
                        Log.Warn($"Invalid VPN check service setting ({plugin.Config.VpnCheckService})! Will not perform a VPN check.");
                        break;

                }

                if (kick)
                {
                    Player player = Player.Get(ev.UserId);
                    if (player != null)
                    {
                        //Send a message to webhook if enabled.
                        if (plugin.Config.SendToDiscordWebhook)
                        {
                            VPNShieldIP ipAddressObj = DbManager.GetIP(ev.Request.RemoteEndPoint.Address.ToString());
                            _ = plugin.WebhookHandler.SendWebhook(plugin.Config.SendToDiscordWebhookUrl, player, KickReason.VPN, ipAddressObj);
                        }
                            

                        ServerConsole.Disconnect(player.Connection, plugin.Config.VpnCheckKickMessage);
                    }

                    else
                        ToKick.Add(ev.UserId, KickReason.VPN);

                    return;
                }

            }

            //All checks have passed. Player can play.
        }

        public void WaitingForPlayers()
        {
            ToKick.Clear();

            if (plugin.Config.VerboseMode)
                Log.Debug("Cleared ToKick dictionary.");

            Filesystem.CheckFileSystem();

            Log.Info("This server is protected by VPNShield.");
        }
    }
}
