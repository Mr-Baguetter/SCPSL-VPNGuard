using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VPNGuard.Objects
{
    public class VPNGuardIP
    {
        public string IPAddress { get; set; }
        public bool Block { get; set; }
        public float GetIpIntelScore { get; set; }
        public bool GetIpIntelIsMobile { get; set; }

        public VPNGuardIP(string IPAddress, bool Block, float GetIpIntelScore = -1, bool GetIpIntelIsMobile = false)
        {
            this.IPAddress = IPAddress;
            this.Block = Block;
            this.GetIpIntelScore = GetIpIntelScore;
            this.GetIpIntelIsMobile = GetIpIntelIsMobile;
        }
    }
}
