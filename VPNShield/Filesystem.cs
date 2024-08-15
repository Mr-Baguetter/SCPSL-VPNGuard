using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using VPNShield.Objects;

namespace VPNShield
{
    public static class Filesystem
    {
        public static void CheckFileSystem()
        {
            //Check to see if VPNShield directory exists and if it doesn't, create it.
            if (!Directory.Exists($"{Plugin.exiledPath}/VPNShield"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield directory does not exist. Creating.");
                Directory.CreateDirectory($"{Plugin.exiledPath}/VPNShield");
            }

            if (File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt exists! Upgrading data... This may take a while depending on how much data you have. Please wait.");
                UpgradeBlacklistIPs();
            }

            if (File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt exists! Upgrading data... This may take a while depending on how much data you have. Please wait.");
                UpgradeWhitelistIPs();
            }

            if (File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt exists! Upgrading data... This may take a while depending on how much data you have. Please wait.");
                UpgradeAccountWhitelist();
            }

            if (File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt exists! Upgrading data... This may take a while depending on how much data you have. Please wait.");
                UpgradeAccountAgeCheck();
            }

            if (File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt exists! Upgrading data... This may take a while depending on how much data you have. Please wait.");
                UpgradeAccountPlaytimeCheck();
            }

            Log.Info($"File system check complete.\nWorking directory is: {Path.Combine(Plugin.exiledPath, "VPNShield")}.\nDatabase path is: {DbManager.databaseLocation}.");
        }

        public static void UpgradeWhitelistIPs()
        {
            if (File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt.lock"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt is locked. Skipping...");
                return;
            }

            List<VPNShieldIP> ipAddresses = new();

            File.Create($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt.lock");

            foreach (string record in File.ReadAllLines($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt"))
            {
                VPNShieldIP iPAddressObj = new();
                iPAddressObj.IPAddress = record;
                iPAddressObj.GiiScore = 0;
                iPAddressObj.GiiMobile = -1;
                iPAddressObj.Blacklisted = false;
                iPAddressObj.CheckedAt = DateTime.UtcNow.Ticks;

                ipAddresses.Add(iPAddressObj);
            }

            DbManager.SaveIP(ipAddresses);

            File.Move($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt", $"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt.bak");
            File.Delete($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt.lock");

            Log.Info($"Upgraded {Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt to new format. Backup has been saved as {Plugin.exiledPath}/VPNShield/VPNShield-WhitelistIPs.txt.bak."); ;

        }

        public static void UpgradeBlacklistIPs()
        {
            if (File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt.lock"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt is locked. Skipping...");
                return;
            }

            List<VPNShieldIP> ipAddresses = new();

            File.Create($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt.lock");

            foreach (string record in File.ReadAllLines($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt"))
            {
                VPNShieldIP iPAddressObj = new();
                iPAddressObj.IPAddress = record;
                iPAddressObj.GiiScore = 0;
                iPAddressObj.GiiMobile = -1;
                iPAddressObj.Blacklisted = true;
                iPAddressObj.CheckedAt = DateTime.UtcNow.Ticks;

                ipAddresses.Add(iPAddressObj);
            }

            DbManager.SaveIP(ipAddresses);

            File.Move($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt", $"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt.bak");
            File.Delete($"{Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt.lock");

            Log.Info($"Upgraded {Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt to new format. Backup has been saved as {Plugin.exiledPath}/VPNShield/VPNShield-BlacklistIPs.txt.bak");
        }

        public static void UpgradeAccountWhitelist()
        {
            if (File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt.lock"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt is locked. Skipping...");
                return;
            }

            List<VPNShieldUserId> userIds = new();

            File.Create($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt.lock");

            foreach (string record in File.ReadAllLines($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt"))
            {
                VPNShieldUserId userIdObj = DbManager.GetUserId(record);

                if (userIdObj == null)
                {
                    userIdObj = new VPNShieldUserId
                    {
                        UserId = record,
                        AccountAgePassed = false,
                        AccountPlaytimePassed = false,
                        Whitelisted = false
                    };
                }

                userIdObj.UserId = record;
                userIdObj.Whitelisted = true;
                userIds.Add(userIdObj);
            }

            DbManager.SaveUserId(userIds);

            File.Move($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt", $"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt.bak");
            File.Delete($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt.lock");

            Log.Info($"Upgraded {Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt to new format. Backup has been saved as {Plugin.exiledPath}/VPNShield/VPNShield-WhitelistUserIDs.txt.bak.");

        }

        public static void UpgradeAccountAgeCheck()
        {
            if (File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt.lock"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt is locked. Skipping...");
                return;
            }

            List<VPNShieldUserId> userIds = new();

            File.Create($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt.lock");

            foreach (string record in File.ReadAllLines($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt"))
            {
                VPNShieldUserId userIdObj = DbManager.GetUserId(record);

                if (userIdObj == null)
                {
                    userIdObj = new VPNShieldUserId
                    {
                        UserId = record,
                        AccountAgePassed = false,
                        AccountPlaytimePassed = false,
                        Whitelisted = false
                    };
                }

                userIdObj.UserId = record;
                userIdObj.AccountAgePassed = true;
                userIds.Add(userIdObj);
            }

            DbManager.SaveUserId(userIds);

            File.Move($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt", $"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt.bak");
            File.Delete($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt.lock");

            Log.Info($"Upgraded {Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt to new format. Backup has been saved as {Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountAgeCheck.txt.bak.");
        }

        public static void UpgradeAccountPlaytimeCheck()
        {
            if (File.Exists($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt.lock"))
            {
                Log.Warn($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt is locked. Skipping...");
                return;
            }

            List<VPNShieldUserId> userIds = new();

            File.Create($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt.lock");

            foreach (string record in File.ReadAllLines($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt"))
            {
                VPNShieldUserId userIdObj = DbManager.GetUserId(record);

                if (userIdObj == null)
                {
                    userIdObj = new();
                    userIdObj.UserId = record;
                    userIdObj.AccountAgePassed = false;
                    userIdObj.AccountPlaytimePassed = false;
                    userIdObj.Whitelisted = false;
                }

                userIdObj.UserId = record;
                userIdObj.AccountPlaytimePassed = true;
                userIds.Add(userIdObj);
            }

            DbManager.SaveUserId(userIds);

            File.Move($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt", $"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt.bak");
            File.Delete($"{Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt.lock");

            Log.Info($"Upgraded {Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt to new format. Backup has been saved as {Plugin.exiledPath}/VPNShield/VPNShield-WhitelistAccountPlaytimeCheck.txt.bak.");
        }
    }
}