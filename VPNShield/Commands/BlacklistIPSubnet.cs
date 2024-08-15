using System;
using CommandSystem;
using Exiled.Permissions.Extensions;
using LiteDB;
using RemoteAdmin;
using VPNShield.Objects;

namespace VPNShield.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class BlacklistIPSubnet : ICommand
    {
        public string Command { get; } = "vs_blacklistipsubnet";
        public string[] Aliases { get; } = { "vs_bis", "vs_bipsub" };

        public string Description { get; } = "Blacklist an IP address subnet. Expects CIDR notation.";
        internal const string Usage = "Usage: vs_blacklistipsubnet (add/remove) (ip subnet in CIDR notation)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender playerSender)
            {
                if (!playerSender.CheckPermission("VPNShield.blacklistip.subnet"))
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

            VPNShieldIPSubnet subnet = new()
            {
                IPSubnet = arguments.At(1),
                Blacklisted = true
            };

            switch (arguments.At(0).ToUpper())
            {
                case "ADD":
                    if (subnets.Exists(arguments.At(1)))
                    {
                        response = $"{arguments.At(1)} is already blacklisted.";
                        return false;
                    }

                    DbManager.SaveSubnet(subnet);
                    response = $"Players connecting from the subnet {arguments.At(1)} are now blacklisted.";
                    return true;
                case "DELETE":
                case "REMOVE":
                    if (!subnets.Exists(arguments.At(1)))
                    {
                        response = $"{arguments.At(1)} is not blacklisted.";
                        return false;
                    }

                    if (DbManager.DeleteSubnet(subnet))
                    {
                        response = $"Players connecting from the subnet {arguments.At(1)} is no longer blacklisted.";
                        return true;
                    }
                    else
                    {
                        response = "An error occurred when trying to remove blacklisted IP subnet. Details may have been printed into the console.";
                        return false;
                    }
                default:
                    response = Usage;
                    return false;
            }
        }
    }
}
