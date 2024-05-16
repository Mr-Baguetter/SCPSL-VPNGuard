using System;
using CommandSystem;
using PluginAPI.Core;
using VPNGuard.Objects;

namespace VPNGuard.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class VPNGuardWhitelistCommand : ICommand
    {
        public string Command { get; } = "vg_whitelist";
        public string[] Aliases { get; } = new string[] { "vg_w" };
        public string Description { get; } = "Whitelist a UserID to be exempt from VPNGuard checks.";
        internal const string Usage = "Usage: vg_whitelist (add/remove) <Player ID / User ID>";
        internal string[] ValidOperations = new string[2] { "add", "remove" };
       

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            //Check to see if a valid number of arguments have been provided.

            if (arguments.Count != 2 || !ValidOperations.Contains(arguments.At(0).ToLower()))
            {
                response = Usage;
                return false;
            }

            //Init variables.
            string userId;
            bool whitelistedStatus;

            //Check to see if the argument passed through is a userid or a player id.
            Player player = Player.Get<Player>(arguments.At(1));

            //If the player is not connected to the server.
            if (player == null)
            {
                //Check to make sure the user ID entered is valid.
                if (!arguments.At(1).Contains("@"))
                {
                    //If not, complain to user.
                    response = $"Invalid Player ID / User ID. {Usage}";
                    return false;
                }
                else
                    //Otherwise, accept argument as valid player ID.
                    userId = arguments.At(1);
            }

            //If player is connected to server, get their user ID.
            else
                userId = player.UserId;

            
            if (arguments.At(0).ToLower() == "add")
                whitelistedStatus = true;
            else
                whitelistedStatus = false;

            //If player is already known, update their whitelisted status.
            if (Plugin.UserIds.ContainsKey(userId))
                Plugin.UserIds[userId].whitelisted = whitelistedStatus;

            //Otherwise, add user and set their whitelisted status to true.
            else
                Plugin.UserIds.Add(userId, new VPNGuardUserId(userId, 0, false, false, whitelistedStatus));

            //Output success message to user, respective to operation.

            if (arguments.At(0).ToLower() == "add")
                response = $"Whitelisted {userId} successfully!";
            else
                response = $"Removed whitelisted status from {userId} successfully!";

            return true;
        }
    }
}
