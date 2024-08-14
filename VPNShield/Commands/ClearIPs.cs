using System;
using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin;

namespace VPNShield.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ClearIPs : ICommand
    {
        public string Command { get; } = "vs_clearips";
        public string[] Aliases { get; } = { "vs_clearip" };

        public string Description { get; } = "Clear all IP addresses from VPNShield's database.";
        internal const string Usage = "Usage: vs_clearips";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender playerSender)
            {
                if (!playerSender.CheckPermission("VPNShield.clear.ips"))
                {
                    response = "You do not have permission to run this command.";
                    return false;
                }
            }

            if (arguments.Count == 0)
            {
                response = "Are you sure you want to delete ALL IP addresses from VPNShield's database? If so, run the command 'vs_clearips confirm'.";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = Usage;
                return false;
            }



            if (arguments.At(0).ToUpper() == "CONFIRM")
            {
                DbManager.ClearIPs();
                response = "Deleted all IP addresses from VPNShield's database!";
                return true;
            }

            else
            {
                response = Usage;
                return false;
            }
        }
    }
}
