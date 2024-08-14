using LiteDB;

namespace VPNShield.Objects
{
    public class VPNShieldUserId
    {
        [BsonId]
        public string UserId { get; set; }
        public bool AccountAgePassed { get; set; }
        public bool AccountPlaytimePassed { get; set; }
        public bool Whitelisted { get; set; }
    }
}
