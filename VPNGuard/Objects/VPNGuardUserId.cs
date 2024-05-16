using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPNGuard.Objects
{
    public class VPNGuardUserId
    {
        public string userId { get; set; }
        public uint accountCreationTime { get; set; }
        public bool passedAccountAgeCheck { get; set; }
        public bool passedAccountPlaytimeCheck { get; set; }
        public bool whitelisted { get; set; }

        public VPNGuardUserId(string userId, uint accountCreationTime, bool passedAccountAgeCheck = false, bool passedAccountPlaytimeCheck = false, bool whitelisted = false)
        {
            this.userId = userId;
            this.accountCreationTime = accountCreationTime;
            this.passedAccountAgeCheck = passedAccountAgeCheck;
            this.passedAccountPlaytimeCheck = passedAccountPlaytimeCheck;
            this.whitelisted = whitelisted;
        }
    }
}
