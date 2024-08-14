﻿using System.Collections.Generic;

namespace VPNShield.Objects
{
    public class SteamAgeApiResponse
    {
        public SteamAgeResponse response { get; set; }
    }

    public class SteamAgeResponse
    {
        public List<SteamAgePlayer> players { get; set; }
    }

    public class SteamAgePlayer
    {
        public string steamid { get; set; }
        public int communityvisibilitystate { get; set; }
        public int profilestate { get; set; }
        public string personaname { get; set; }
        public string profileurl { get; set; }
        public string avatar { get; set; }
        public string avatarmedium { get; set; }
        public string avatarfull { get; set; }
        public string avatarhash { get; set; }
        public int personastate { get; set; }
        public string realname { get; set; }
        public string primaryclanid { get; set; }
        public int timecreated { get; set; }
        public int personastateflags { get; set; }
        public string loccountrycode { get; set; }
        public string locstatecode { get; set; }
        public int loccityid { get; set; }
    }
}