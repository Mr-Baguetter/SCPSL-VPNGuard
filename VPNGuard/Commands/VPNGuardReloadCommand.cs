using System;
using CommandSystem;
using EO.WebBrowser;

namespace VPNGuard.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class VPNGuardReloadCommand : ICommand
    {
        public string Command { get; } = "vg_reload";
        public string[] Aliases { get; } = new string[] { "vg_r" };
        public string Description { get; } = "Reload VPNGuard's IP Address and User ID data.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            //Reload data.
            Filesystem.Init();

            response = "Reloaded VPNGuard's data.";
            return true;
        }
    }
}
