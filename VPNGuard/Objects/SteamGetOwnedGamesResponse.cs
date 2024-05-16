using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPNGuard.Objects
{
    public class SteamGetOwnedGamesResponse
    {
        public SteamGames response { get; set; }
    }

    public class SteamGames
    {
        public uint game_count { get; set; }
        public SteamGame[] games { get; set; }
    }

    public class SteamGame
    {
        public uint appid { get; set; }
        public uint playtime_forever { get; set; }
        public uint playtime_windows_forever { get; set; }
        public uint playtime_mac_forever { get; set; }
        public uint playtime_linux_forever { get; set; }
        public uint rtime_last_played { get; set; }
    }
}
