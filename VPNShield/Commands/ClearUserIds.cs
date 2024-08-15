using System;
using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin;

namespace VPNShield.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ClearUserIds : ICommand
    {
        public string Command { get; } = "vs_clearuserids";
        public string[] Aliases { get; } = { "vs_clearuserid" };

        public string Description { get; } = "Clear all user IDs from VPNShield's database.";
        internal const string Usage = "Usage: vs_clearuserids";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender playerSender)
            {
                if (!playerSender.CheckPermission("VPNShield.clear.userids"))
                {
                    response = "You do not have permission to run this command.";
                    return false;
                }
            }

            if (arguments.Count == 0)
            {
                response = "Are you sure you want to delete ALL user IDs from VPNShield's database? If so, run the command 'vs_clearuserids confirm'.";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = Usage;
                return false;
            }



            if (arguments.At(0).ToUpper() == "CONFIRM")
            {
                DbManager.ClearUserIds();
                response = "Deleted all user IDs from VPNShield's database!";
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
