using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using VPNShield.Objects;

namespace VPNShield.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class WhitelistGlobal : ICommand
    {
        public string Command { get; } = "vs_whitelist";
        public string[] Aliases { get; } = { "vs_w" };

        public string Description { get; } = "Exempt players from VPNShield checks.";
        internal const string Usage = "Usage: vs_whitelist (add/remove) (id)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender playerSender)
            {
                if (!playerSender.CheckPermission("VPNShield.whitelist"))
                {
                    response = "You do not have permission to run this command.";
                    return false;
                }
            }

            if (arguments.Count < 2)
            {
                response = Usage;
                return false;
            }

            string userId;

            if (!arguments.At(1).Contains("@"))
            {
                Player whitelistPlayer = Player.Get(arguments.At(1).ToLower());

                if (whitelistPlayer == null)
                {
                    response = $"A player identified by {arguments.At(1)} cannot be found on this server.";
                    return false;
                }

                userId = whitelistPlayer.UserId;
            }

            else
                userId = arguments.At(1).ToLower();

            VPNShieldUserId vpnShieldUserId = DbManager.GetUserId(userId);

#if DEBUG
            Log.Debug($"Is null?: {vpnShieldUserId == null}");
#endif

            if (vpnShieldUserId == null)
            {
                vpnShieldUserId = new();
                vpnShieldUserId.UserId = userId;
                vpnShieldUserId.AccountAgePassed = false;
                vpnShieldUserId.AccountPlaytimePassed = false;
                vpnShieldUserId.Whitelisted = false;
            }

#if DEBUG
            Log.Debug($"User ID: {vpnShieldUserId.UserId}");
            Log.Debug($"Account Age Passed: {vpnShieldUserId.AccountAgePassed}");
            Log.Debug($"Account Playtime Passed: {vpnShieldUserId.AccountPlaytimePassed}");
            Log.Debug($"Whitelisted: {vpnShieldUserId.Whitelisted}");
#endif

            switch (arguments.At(0).ToUpper())
            {
                case "ADD":
                    if (vpnShieldUserId.Whitelisted)
                    {
                        response = $"{arguments.At(1)} is already whitelisted from all VPNShield checks.";
                        return false;
                    }

                    vpnShieldUserId.Whitelisted = true;
                    DbManager.SaveUserId(vpnShieldUserId);
                    response = $"{arguments.At(1)} is now whitelisted and exempt from all VPNShield checks.";
                    return true;
                case "REMOVE":
                case "DELETE":

                    if (!vpnShieldUserId.Whitelisted)
                    {
                        response = $"{arguments.At(1)} is not whitelisted and exempt from all VPNShield checks.";
                        return false;
                    }

                    vpnShieldUserId.Whitelisted = false;

#if DEBUG
                    Log.Debug($"User ID: {vpnShieldUserId.UserId}");
                    Log.Debug($"Account Age Passed: {vpnShieldUserId.AccountAgePassed}");
                    Log.Debug($"Account Playtime Passed: {vpnShieldUserId.AccountPlaytimePassed}");
                    Log.Debug($"Whitelisted: {vpnShieldUserId.Whitelisted}");
#endif

                    DbManager.SaveUserId(vpnShieldUserId);
                    response = $"{arguments.At(1)} is no longer whitelisted and exempt from all VPNShield checks.";
                    return true;
                default:
                    response = Usage;
                    return false;
            }
        }
    }
}
