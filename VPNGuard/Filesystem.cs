using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VPNGuard.Objects;
using System.Net;

namespace VPNGuard
{
    public class Filesystem
    {
        private readonly static string IPAddressCSVPath = Path.Combine(Plugin.Handler.PluginDirectoryPath, "ipaddresses.csv");
        private readonly static string UserIdCSVPath = Path.Combine(Plugin.Handler.PluginDirectoryPath, "userids.csv");
        public static void Init()
        {
            Log.Info("Initializing file system.");
            Plugin.IPAddresses.Clear();
            Plugin.UserIds.Clear();

            if (!CheckFileSystem())
            {
                Log.Error("Initialization of file system failed. Plugin may not work as expected! No data from ipaddresses.csv or userids.csv is being used.");
                return;
            }

            Dictionary<string, VPNGuardIP> ips = LoadIPAddresses();
            Dictionary<string, VPNGuardUserId> userids = LoadUserIds();

            //Check to see if IPs have been loaded successfully and if so, use them.
            if (ips != null)
                Plugin.IPAddresses = ips;
            else
                Log.Error($"Loading IPs from {IPAddressCSVPath} failed! No IP address data is being used.");

            //Check to see if UserIds have been loaded successfully and if so, use them.
            if (userids != null)
                Plugin.UserIds = userids;
            else
                Log.Error($"Loading UserIds from {UserIdCSVPath} failed! No User ID data is being used.");

            Log.Info("Initialized file system finished.");
        }

        private static bool CheckFileSystem()
        {
            try
            {
                //Check to make sure that ipaddresses.csv exists and if not, make it with headers.
                if (!File.Exists(IPAddressCSVPath))
                {
                    Log.Warning($"File {IPAddressCSVPath} does not exist! Creating...");
                    File.Create(IPAddressCSVPath);
                }

                if (!File.Exists(UserIdCSVPath))
                {
                    Log.Warning($"File {UserIdCSVPath} does not exist! Creating...");
                    File.Create(UserIdCSVPath);
                }

                return true;
            }

            catch(Exception ex)
            {
                Log.Error($"Exception occurred when checking file system! Exception: {ex}");
                return false;
            }
        }

        private static Dictionary<string, VPNGuardIP> LoadIPAddresses()
        {
            try
            {
                Dictionary<string, VPNGuardIP> ips = new Dictionary<string, VPNGuardIP>();

                string[] ipAddressLines = File.ReadAllLines(IPAddressCSVPath);

                for (int i = 0; i < ipAddressLines.Length; i++)
                {
                    //Skip lines that begin with # or CSV header Line
                    if (ipAddressLines[i].StartsWith("#") || ipAddressLines[i].StartsWith("IP_Address,"))
                        continue;

                    string[] ipAddressLineCells = ipAddressLines[i].Split(',');

                    //Check to make sure that IP Address and Block status are valid.

                    if (!IPAddress.TryParse(ipAddressLineCells[0], out _))
                    {
                        Log.Warning($"Error parsing IP Address \"{ipAddressLineCells[0]}\" on line {i} of {IPAddressCSVPath}. Skipping...");
                        continue;
                    }

                    if (!bool.TryParse(ipAddressLineCells[1], out bool block))
                    {
                        Log.Warning($"Error parsing IP Address block status \"{ipAddressLineCells[1]}\" on line {i} of {IPAddressCSVPath}. Skipping...");
                        continue;
                    }

                    if (!float.TryParse(ipAddressLineCells[2], out float getIpIntelScore))
                    {
                        Log.Warning($"Error parsing IP Address GetIpIntel score \"{ipAddressLineCells[2]}\" on line {i} of {IPAddressCSVPath}. Skipping...");
                        continue;
                    }

                    if (!bool.TryParse(ipAddressLineCells[3], out bool getIpIntelIsMobile))
                    {
                        Log.Warning($"Error parsing IP Address GetIpIntel IP address mobile status \"{ipAddressLineCells[3]}\" on line {i} of {IPAddressCSVPath}. Skipping...");
                        continue;
                    }

                    VPNGuardIP vpnGuardIP = new VPNGuardIP(ipAddressLineCells[0], block, getIpIntelScore, getIpIntelIsMobile);


                    ips.Add(ipAddressLineCells[0], vpnGuardIP);
                }

                return ips;
            }

            catch(Exception ex)
            {
                Log.Error($"An exception occurred when trying to load data from file system. Exception: {ex}");
                return null;
            }
        }

        public static bool SaveIPAddresses()
        {
            try
            {
                //We have to do this in a way so that we don't have duplicate data.

                //Get the IPs on Disk.
                Dictionary<string, VPNGuardIP> ipsOnDisk = LoadIPAddresses();

                //Create a new dictionary with IPs on disk and IPs in memory merged together with those in memory having priority if a duplicate is present.
                Dictionary<string, VPNGuardIP> merged = Plugin.IPAddresses.Concat(ipsOnDisk).GroupBy(i => i.Key).ToDictionary(group => group.Key, group => group.First().Value);


                //Convert data stored in merged dictionary to CSV format stored in a lsit.
                List<string> listOfIps = new List<string>();

                foreach (KeyValuePair<string, VPNGuardIP> ip in merged)
                {
                    listOfIps.Add(IPObjToCsv(ip.Value));
                }


                //Rename current file.
                File.Delete($"{IPAddressCSVPath}.bak");
                File.Move(IPAddressCSVPath, $"{IPAddressCSVPath}.bak");

                //Write to new file.
                File.WriteAllLines(IPAddressCSVPath, listOfIps);

                //Done.
                return true;
            }
            

            catch (Exception ex)
            {
                Log.Error($"An exception occurred when saving IPs to disk. Exception : {ex}");
                return false;
            }
        }

        public static Dictionary<string, VPNGuardUserId> LoadUserIds()
        {
            try
            {
                Dictionary<string, VPNGuardUserId> userids = new Dictionary<string, VPNGuardUserId>();

                string[] userIdLines = File.ReadAllLines(UserIdCSVPath);

                for (int i = 0; i < userIdLines.Length; i++)
                {
                    //Skip lines that begin with # or CSV header Line
                    if (userIdLines[i].StartsWith("#") || userIdLines[i].StartsWith("UserID,"))
                        continue;

                    string[] userIdLineCells = userIdLines[i].Split(',');

                    //Check to make sure that IP Address and Block status are valid.

                    if (!uint.TryParse(userIdLineCells[1], out uint accountCreationTime))
                    {
                        Log.Warning($"Error parsing User ID account creation time \"{userIdLineCells[1]}\" on line {i} of {UserIdCSVPath}. Skipping...");
                        continue;
                    }
                    if (!bool.TryParse(userIdLineCells[2], out bool passedAccountAgeCheck))
                    {
                        Log.Warning($"Error parsing User ID 'Passed account age check' status \"{userIdLineCells[2]}\" on line {i} of {UserIdCSVPath}. Skipping...");
                        continue;
                    }

                    if (!bool.TryParse(userIdLineCells[3], out bool passedAccountPlaytimeCheck))
                    {
                        Log.Warning($"Error parsing User ID 'Passed account playtime check' status \"{userIdLineCells[3]}\" on line {i} of {UserIdCSVPath}. Skipping...");
                        continue;
                    }

                    if (!bool.TryParse(userIdLineCells[4], out bool whitelisted))
                    {
                        Log.Warning($"Error parsing User ID whitelisted status \"{userIdLineCells[1]}\" on line {i} of {UserIdCSVPath}. Skipping...");
                        continue;
                    }

                    VPNGuardUserId userId = new VPNGuardUserId(userIdLineCells[0], accountCreationTime, passedAccountAgeCheck, passedAccountPlaytimeCheck, whitelisted);

                    userids.Add(userIdLineCells[0], userId);
                }

                return userids;
            }

            catch (Exception ex)
            {
                Log.Error($"An exception occurred when trying to load data from file system. Exception: {ex}");
                return null;
            }
        }

        public static bool SaveUserIds()
        {
            try
            {
                //We have to do this in a way so that we don't have duplicate data.

                //Get the IPs on Disk.
                Dictionary<string, VPNGuardUserId> userIdsOnDisk = LoadUserIds();

                //Create a new dictionary with IPs on disk and IPs in memory merged together with those in memory having priority if a duplicate is present.
                Dictionary<string, VPNGuardUserId> merged = Plugin.UserIds.Concat(userIdsOnDisk).GroupBy(i => i.Key).ToDictionary(group => group.Key, group => group.First().Value);


                //Convert data stored in merged dictionary to CSV format stored in a lsit.
                List<string> listOfUserIds = new List<string>();

                foreach (KeyValuePair<string, VPNGuardUserId> userid in merged)
                {
                    listOfUserIds.Add(UserIdObjToCsv(userid.Value));
                }


                //Rename current file.
                File.Delete($"{UserIdCSVPath}.bak");
                File.Move(UserIdCSVPath, $"{UserIdCSVPath}.bak");

                //Write to new file.
                File.WriteAllLines(UserIdCSVPath, listOfUserIds);

                //Done.
                return true;
            }


            catch (Exception ex)
            {
                Log.Error($"An exception occurred when saving IPs to disk. Exception : {ex}");
                return false;
            }
        }

        private static string IPObjToCsv(VPNGuardIP ip)
        {
            return $"{ip.IPAddress},{ip.Block},{ip.GetIpIntelScore},{ip.GetIpIntelIsMobile}";
        }

        private static string UserIdObjToCsv(VPNGuardUserId userid)
        {
            return $"{userid.userId},{userid.accountCreationTime},{userid.passedAccountAgeCheck},{userid.passedAccountPlaytimeCheck},{userid.whitelisted}";
        }
    }
}
