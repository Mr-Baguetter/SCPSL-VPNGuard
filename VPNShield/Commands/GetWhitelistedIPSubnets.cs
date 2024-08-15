using System;
using System.Collections.Generic;
using System.Net;
using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using VPNShield.Objects;

namespace VPNShield.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class GetWhitelistedSubnets : ICommand
    {
        public string Command { get; } = "vs_getwhitelistedsubnets";
        public string[] Aliases { get; } = { "vs_getsubnets", "vs_getwsubnets", "vs_gwsub" };

        public string Description { get; } = "Get a list of subnets that have been whitelisted.";
        internal const string Usage = "Usage: vs_getwhitelistedsubnets";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender playerSender)
            {
                if (!playerSender.CheckPermission("VPNShield.get.whitelistedsubnets"))
                {
                    response = "You do not have permission to run this command.";
                    return false;
                }
            }

            response = "Whitelisted subnets: ";

            foreach (VPNShieldIPSubnet subnet in DbManager.GetSubnets().FindAll())
            {
                response += $"{subnet.IPSubnet}, ";
            }

            return true;
        }
    }
}
