using LiteDB;

namespace VPNShield.Objects
{
    public class VPNShieldIP
    { 
        [BsonId]
        public string IPAddress { get; set; }
        public bool Blacklisted { get; set; } = false;
        public float GiiScore { get; set; } = 0;
        public int GiiMobile { get; set; } = -1;
        public long CheckedAt { get; set; } = 0;

    }
}
