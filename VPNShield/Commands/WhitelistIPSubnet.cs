using System;
using System.Net;
using CommandSystem;
using Exiled.Permissions.Extensions;
using LiteDB;
using RemoteAdmin;
using Exiled.API.Features;
using VPNShield.Objects;

namespace VPNShield.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class WhitelistIPSubnet : ICommand
    {
        public string Command { get; } = "vs_whitelistipsubnet";
        public string[] Aliases { get; } = { "vs_wips", "vs_wipsub" };

        public string Description { get; } = "Whitelist an IP address subnet. Expects CIDR notation.";
        internal const string Usage = "Usage: vs_whitelistipsubnet (add/remove) (ip subnet in CIDR notation)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender playerSender)
            {
                if (!playerSender.CheckPermission("VPNShield.whitelist.ipsubnet"))
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

            if (!Convenience.CheckIfCIDR(arguments.At(1)))
            {
                response = $"{arguments.At(1)} is not a valid IP subnet in CIDR notation.";
                return false;
            }

            ILiteCollection<VPNShieldIPSubnet> subnets = DbManager.GetSubnets();
            VPNShieldIPSubnet subnet;

            switch (arguments.At(0).ToUpper())
            {
                case "ADD":
                    if (subnets.Exists(arguments.At(1)))
                    {
                        response = $"{arguments.At(1)} is already whitelisted.";
                        return false;
                    }

                    subnet = new()
                    {
                        IPSubnet = arguments.At(1),
                        Blacklisted = false
                    };

                    DbManager.SaveSubnet(subnet);
                    response = $"Players connecting from the subnet {arguments.At(1)} are now whitelisted from VPN checks.";
                    return true;
                case "DELETE":
                case "REMOVE":
                    if (!subnets.Exists(arguments.At(1)))
                    {
                        response = $"{arguments.At(1)} is not whitelisted.";
                        return false;
                    }

                    subnet = new()
                    {
                        IPSubnet = arguments.At(1),
                        Blacklisted = false
                    };

                    if (DbManager.DeleteSubnet(subnet))
                    {
                        response = $"Players connecting from the subnet {arguments.At(1)} will now be subject to VPN checks if not whitelisted otherwise.";
                        return true;
                    }
                    else
                    {
                        response = "An error occurred when trying to remove whitelisted IP subnet. Details may have been printed into the console.";
                        return false;
                    }
                default:
                    response = Usage;
                    return false;
            }
        }
    }
}
