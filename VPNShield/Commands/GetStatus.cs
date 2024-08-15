using System;
using System.Net;
using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using VPNShield.Objects;

namespace VPNShield.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class GetStatus : ICommand
    {
        public string Command { get; } = "vs_getstatus";
        public string[] Aliases { get; } = { "vs_get", "vs_g", "vs_gs" };

        public string Description { get; } = "Get information that VPNShield has on an IP address or User ID.";
        internal const string Usage = "Usage: vs_getstatus (ip/userid)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender playerSender)
            {
                if (!playerSender.CheckPermission("VPNShield.get.status"))
                {
                    response = "You do not have permission to run this command.";
                    return false;
                }
            }

            if (arguments.Count < 1)
            {
                response = Usage;
                return false;
            }

            if (IPAddress.TryParse(arguments.At(0), out _))
            {
                VPNShieldIP ipAddress = DbManager.GetIP(arguments.At(0));

                if (ipAddress == null)
                {
                    response = $"The IP address {arguments.At(0)} is unknown.";
                    return true;
                }

                else
                {
                    response = $"\nIP Address: {ipAddress.IPAddress}\nBlacklisted?: {ipAddress.Blacklisted}\nGII Score: {ipAddress.GiiScore}\nIs mobile connection?: {ipAddress.GiiMobile}\nIP address checked at: {new DateTime(ipAddress.CheckedAt)} UTC";
                    return true;
                }
            }

            else if (arguments.At(0).Contains("@"))
            {
                VPNShieldUserId userId = DbManager.GetUserId(arguments.At(0));

                if (userId == null)
                {
                    response = $"The User ID {arguments.At(0)} is unknown.";
                    return false;
                }

                else
                {
                    response = $"\nUser ID: {userId.UserId}\nIs whitelisted from all checks?: {userId.Whitelisted}\nPassed account age check?: {userId.AccountAgePassed}\nPassed account playtime check?: {userId.AccountPlaytimePassed}";
                    return true;
                }
            }

            else
            {
                response = $"{arguments.At(0)} is not a valid IP address or User ID.";
                return false;
            }
        }
    }
}
