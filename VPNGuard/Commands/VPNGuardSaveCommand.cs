using System;
using CommandSystem;


namespace VPNGuard.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class VPNGuardSaveCommand : ICommand
    {
        public string Command { get; } = "vg_save";
        public string[] Aliases { get; } = new string[] { "vg_s" };
        public string Description { get; } = "Force save VPNGuard's data.";
        internal const string Usage = "Usage: vg_save (ips/userids/all)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            //Save data.

            if (arguments.Count < 1)
            {
                response = Usage;
                return false;
            }

            switch (arguments.At(0).ToLower())
            {
                case "ips":
                    if (Filesystem.SaveIPAddresses())
                    {
                        response = "Saved VPNGuard's IP data successfully!";
                        return true;
                    }

                    else
                    {
                        response = "An error occurred when trying to save VPNGuard's IP data. Exception has been output into the server console.";
                        return false;
                    }
                case "userids":
                    if (Filesystem.SaveUserIds())
                    {
                        response = "Saved VPNGuard's UserID data successfully!";
                        return true;
                    }

                    else
                    {
                        response = "An error occurred when trying to save VPNGuard's UserID data. Exception has been output into the server console.";
                        return false;
                    }
                case "all":
                    if (Filesystem.SaveIPAddresses() && Filesystem.SaveUserIds())
                    {
                        response = "Saved VPNGuard's IP and UserID data successfully!";
                        return true;
                    }

                    else
                    {
                        response = "An error occurred when trying to save VPNGuard's IP and UserID data. Exception has been output into the server console.";
                        return false;
                    }
                default:
                    response = $"Invalid operation, {Usage}";
                    return false;
            }
        }
    }
}
