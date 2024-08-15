using LiteDB;

namespace VPNShield.Objects
{
    public class VPNShieldIPSubnet
    {
        [BsonId]
        public string IPSubnet { get; set; }
        public bool Blacklisted { get; set; }
    }
}
