using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using VPNGuard.Objects;

namespace VPNGuard
{
    public class Plugin
    {
        private EventHandlers EventHandlers;

        public static PluginHandler Handler;
        public static Plugin Singleton;

        //Dictionary containing all IP addresses loaded from file.
        public static Dictionary<string, VPNGuardIP> IPAddresses = new Dictionary<string, VPNGuardIP>();

        //Dicrionary containing all UserIDs loaded from file.
        public static Dictionary<string, VPNGuardUserId> UserIds = new Dictionary<string, VPNGuardUserId>();

        [PluginEntryPoint("VPNGuard", "1.0.2.0", "A VPN blocking plugin for SCP: SL servers running NWAPI.", "SomewhatSane#0979")]
        private void LoadPlugin()
        {
            Handler = PluginHandler.Get(this);
            Singleton = this;

            Log.Info($"{Handler.PluginName} v{Handler.PluginVersion} by {Handler.PluginAuthor}.");
            Log.Info(Handler.PluginDescription);

            //If update checker is enabled, check for updates.
            if (PluginConfig.CheckForUpdates)
                UpdateChecker.CheckForUpdate();

            //Get filesystem ready.
            Filesystem.Init();

            //Register events.
            Log.Info("Registering event handlers...");

            EventHandlers = new EventHandlers();
            EventManager.RegisterEvents(EventHandlers);

            Log.Info("Done!");
        }

        [PluginConfig]
        public Config PluginConfig;

        public void VerboseLog(string message)
        {
            if (PluginConfig.VerboseMode)
                Log.Debug(message);
        }
    }
}
