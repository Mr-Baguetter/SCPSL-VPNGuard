using System;
using System.Net;
using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using VPNShield.Objects;

namespace VPNShield.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class WhitelistIP : ICommand
    {
        public string Command { get; } = "vs_whitelistip";
        public string[] Aliases { get; } = { "vs_wi" };

        public string Description { get; } = "Whitelist an IP address.";
        internal const string Usage = "Usage: vs_whitelistip (add/remove) (ip)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender playerSender)
            {
                if (!playerSender.CheckPermission("VPNShield.whitelist.ip"))
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

            if (!IPAddress.TryParse(arguments.At(1), out IPAddress ipAddress))
            {
                response = $"{arguments.At(1)} is not a valid IP address.";
                return false;
            }

            VPNShieldIP vpnShieldIP = DbManager.GetIP(arguments.At(1));

            

            switch (arguments.At(0).ToUpper())
            {
                case "ADD":
                    if (vpnShieldIP == null)
                    {
                        vpnShieldIP = new VPNShieldIP();
                    }

                    else if (!vpnShieldIP.Blacklisted)
                    {
                        response = $"{arguments.At(1)} is already whitelisted.";
                        return false;
                    }

                    vpnShieldIP.IPAddress = ipAddress.ToString();
                    vpnShieldIP.Blacklisted = false;
                    DbManager.SaveIP(vpnShieldIP);

                    response = $"{arguments.At(1)} is now whitelisted.";
                    return true;

                case "REMOVE":
                case "DELETE":
                    if (vpnShieldIP == null)
                    {
                        response = $"You cannot remove whitelisted status from {arguments.At(1)}. {arguments.At(1)} has not been seen on this server.";
                        return false;
                    }

                    if (vpnShieldIP.Blacklisted)
                    {
                        response = $"{arguments.At(1)} is not blacklisted.";
                        return false;
                    }

                    if (DbManager.DeleteIP(vpnShieldIP))
                    {
                        response = $"{arguments.At(1)} is has been deleted from memory and is no longer whitelisted. {arguments.At(1)} will be checked once it makes a connection to this server.";
                        return true;
                    }

                    else
                    {
                        response = $"An error occurred when trying to remove whitelisted status from {arguments.At(1)}. Please try again later.";
                        return false;
                    }

                default:
                    response = Usage;
                    return false;
            }
        }
    }
}
