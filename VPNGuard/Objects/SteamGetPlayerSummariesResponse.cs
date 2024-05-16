using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPNGuard.Objects
{
    public class SteamGetPlayerSummariesResponse
    {
        public SteamPlayers response { get; set; }
    }

    public class SteamPlayers
    {
        public SteamPlayer[] players { get; set; }
    }

    public class SteamPlayer
    {
        public string steamid { get; set; }
        public int communityvisibilitystate { get; set; }
        public int profilestate { get; set; }
        public string personaname { get; set; }
        public int commentpermission { get; set; }
        public string profileurl { get; set; }
        public string avatar { get; set; }
        public string avatarmedium { get; set; }
        public string avatarfull { get; set; }
        public string avatarhash { get; set; }
        public uint lastlogoff { get; set; }
        public int personastate { get; set; }
        public string primaryclanid { get; set; }
        public uint timecreated { get; set; }
        public int personastateflags { get; set; }
        public string loccountrycode { get; set; }
    }
}
