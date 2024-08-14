using System;
using System.IO;
using System.Collections.Generic;
using LiteDB;
using VPNShield.Objects;

namespace VPNShield
{
    public static class DbManager
    {
        public static readonly string databaseLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED", "Plugins", "VPNShield", "data.db");
        private static readonly LiteDatabase db = new($"Filename={databaseLocation};Connection=shared");

        public static VPNShieldIP GetIP(string IPAddress)
        {
            try
            {
                ILiteCollection<VPNShieldIP> collection = db.GetCollection<VPNShieldIP>("IPAddresses");
                return collection.FindOne(x => x.IPAddress == IPAddress);
            }
            
            catch(Exception)
            {
                return null;
            }
        }


        public static void SaveIP(VPNShieldIP IPAddress)
        {
            ILiteCollection<VPNShieldIP> collection = db.GetCollection<VPNShieldIP>("IPAddresses");
            collection.Upsert(IPAddress);
        }

        public static void SaveIP(IEnumerable<VPNShieldIP> IPAddresses)
        {
            ILiteCollection<VPNShieldIP> collection = db.GetCollection<VPNShieldIP>("IPAddresses");
            collection.Upsert(IPAddresses);
        }

        public static bool DeleteIP(VPNShieldIP IPAddress)
        {
            ILiteCollection<VPNShieldIP> collection = db.GetCollection<VPNShieldIP>("IPAddresses");
            return collection.Delete(IPAddress.IPAddress);
        }

        public static void ClearIPs()
        {
            ILiteCollection<VPNShieldIP> collection = db.GetCollection<VPNShieldIP>("IPAddresses");
            collection.DeleteAll();
        }

        public static ILiteCollection<VPNShieldIPSubnet> GetSubnets()
        {
            ILiteCollection<VPNShieldIPSubnet> collection = db.GetCollection<VPNShieldIPSubnet>("IPAddressSubnets");
            return collection;
        }

        public static void SaveSubnet(VPNShieldIPSubnet subnet)
        {
            ILiteCollection<VPNShieldIPSubnet> collection = db.GetCollection<VPNShieldIPSubnet>("IPAddressSubnets");
            collection.Upsert(subnet);
        }

        public static bool DeleteSubnet(VPNShieldIPSubnet subnet)
        {
            ILiteCollection<VPNShieldIPSubnet> collection = db.GetCollection<VPNShieldIPSubnet>("IPAddressSubnets");
            return collection.Delete(subnet.IPSubnet);
        }

        public static void ClearSubnets()
        {
            ILiteCollection<VPNShieldIPSubnet> collection = db.GetCollection<VPNShieldIPSubnet>("IPAddressSubnets");
            collection.DeleteAll();
        }


        public static VPNShieldUserId GetUserId(string userId)
        {
            try
            {
                ILiteCollection<VPNShieldUserId> collection = db.GetCollection<VPNShieldUserId>("UserIds");
                return collection.FindOne(x => x.UserId == userId);
            }
            
            catch (Exception)
            {
                return null;
            }
        }

        public static void SaveUserId(VPNShieldUserId userId)
        {
            //For some reason, Upsert returns true on insert and false on update. Why???
            //You'd expect true = success, false = failure.

            ILiteCollection<VPNShieldUserId> collection = db.GetCollection<VPNShieldUserId>("UserIds");
            collection.Upsert(userId);
        }

        public static void SaveUserId(IEnumerable<VPNShieldUserId> userId)
        {
            ILiteCollection<VPNShieldUserId> collection = db.GetCollection<VPNShieldUserId>("UserIds");
            collection.Upsert(userId);
        }

        public static bool DeleteUserId(VPNShieldUserId userId)
        {
            ILiteCollection<VPNShieldUserId> collection = db.GetCollection<VPNShieldUserId>("UserIds");
            return collection.Delete(userId.UserId);
        }

        public static void ClearUserIds()
        {
            ILiteCollection<VPNShieldUserId> collection = db.GetCollection<VPNShieldUserId>("UserIds");
            collection.DeleteAll();
        }
    }
}
