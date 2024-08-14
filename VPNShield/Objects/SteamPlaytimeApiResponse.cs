using System.Collections.Generic;

namespace VPNShield.Objects
{
    public class SteamPlaytimeApiResponse
    {
        public SteamPlayTimeResponse response;
    }

    public class SteamPlayTimeResponse
    {
        public int game_count { get; set; }
        public List<SteamGame> games { get; set; }
    }

    public class SteamGame
    {
        public int appid { get; set; }
        public int playtime_forever { get; set; }
        public int playtime_windows_forever { get; set; }
        public int playtime_mac_forever { get; set; }
        public int playtime_linux_forever { get; set; }
    }
}
