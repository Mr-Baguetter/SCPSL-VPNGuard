using System.ComponentModel;

namespace VPNGuard
{
    public class Config
    {
        [Description("Should account age checking be enabled?")]
        public bool AccountAgeCheck { get; private set; } = false;

        [Description("Should account playtime checking be enabled?")]
        public bool AccountPlaytimeCheck { get; private set; } = false;

        [Description("Should accounts that cannot be checked (eg. private Steam accounts) be kicked?")]
        public bool AccountKickPrivate { get; private set; } = true;

        [Description("Should accounts that cannot be checked due to a Steam API error be kicked? In most cases, you should keep this set to false.")]
        public bool AccountKickOnSteamError { get; private set; } = false;

        [Description("Steam API key for account age checking.")]
        public string SteamApiKey { get; private set; } = null;

        [Description("Minimum Steam account age (if account age checking is enabled - in days).")]
        public int SteamMinAge { get; private set; } = 14;

        [Description("Minimum required SCPSL playtime required (if account playtime checking is enabled - in minutes).")]
        public int SteamMinPlaytime { get; private set; } = 0;

        [Description("Message shown to players who are kicked by an account age check. You may use %MINIMUMAGE% to insert the minimum age in days set into your kick message.")]
        public string AccountAgeCheckKickMessage { get; private set; } = "Your account must be at least %MINIMUMAGE% day(s) old to play on this server.";

        [Description("Message shown to players who are kicked by an account playtime check. You may use %MINIMUMPLAYTIME% to insert the minimum playtime in minutes set into your kick message.")]
        public string AccountPlaytimeCheckKickMessage { get; private set; } = "Your account must have played SCP: SL for atleast %MINIMUMPLAYTIME% minute(s) to play on this server.";

        [Description("Message shown to players who are kicked because they account cannot be checked due to privacy settings.")]
        public string AccountPrivateKickMessage { get; private set; } = "An account check could not be performed as your Steam profile is set to private. Please make your profile public and try connecting again!";

        [Description("Message shown to players who are kicked as there was a Steam API error (only needed if account_kick_on_steam_error).")]
        public string AccountSteamErrorKickMessage { get; private set; } = "An error occurred when trying to check your Steam account. Due to the policy set on this server, you were kicked. Please contact the server administration about this and try joining again later.";

        [Description("Should VPN checking be enabled?")]
        public bool VpnCheck { get; private set; } = true;

        [Description("What VPN service would you like to use for VPN checking? (0 for IPHub, 1 for GetIPIntel)")]
        public int VpnCheckService { get; private set; } = 0;

        [Description("IF vpn_check_service IS 0: IPHub API key for VPN checking. Get one for free at https://iphub.info .")]
        public string IphubApiKey { get; private set; } = null;

        [Description("IF vpn_check_service IS 0: IPHub supports 'strict blocking'. Should it be enabled? Strict blocking will catch more VPN / hosting IP addresses but may cause false positives. It is generally recommended to keep this disabled.")]
        public bool IphubStrictBlocking { get; private set; } = false;

        [Description("IF vpn_check_service IS 1: GetIPIntel.net API subdomain. If you have a specific plan, you can specify your subdomain. If you are using the free plan or don't know what this is, leave it as it is.")]
        public string GetipintelSubdomain { get; private set; } = "check";

        [Description("IF vpn_check_service IS 1: GetIPIntel.net Contact Email Address. The API requires a VALID contact email.")]
        public string GetipintelContactEmailAddress { get; private set; } = null;

        [Description("IF vpn_check_service IS 1: GetIPIntel.net optional flags. Flags m, b, f and n are supported. If you don't know what this is, leave this blank / null or check the GetIPIntel website.")]
        public string GetipintelOptionalFlags { get; private set; }

        [Description("IF vpn_check_service IS 1: GetIPIntel.net max score. Any IP Address with a score above this value will be blocked. 0.995 is recommended by the API.")]
        public float GetipintelMaxScore { get; private set; } = 0.995f;

        [Description("IF vpn_check_service IS 1: Block mobile ISPs? (Players playing on mobile data providers such as 3, EE, Vodafone, T-Mobile, Verizon, AT&T etc...)")]
        public bool GetipintelBlockMobileIsps { get; private set; } = false;

        [Description("Message shown to players who are kicked by a VPN check or a mobile ISP check (if enabled).")]
        public string VpnCheckKickMessage { get; private set; } = "VPNs and proxies are forbidden on this server.";

        [Description("Send a message to Discord via webhooks when someone is kicked by VPNShield?")]
        public bool SendToDiscordWebhook { get; private set; } = false;

        [Description("Discord Webhook URL for send_to_discord_webhook (only needed if kick_to_discord is true).")]
        public string SendToDiscordWebhookUrl { get; private set; }

        [Description("Check for VPNShield updates on startup?")]
        public bool CheckForUpdates { get; private set; } = true;

        [Description("Verbose mode. Prints more console messages.")]
        public bool VerboseMode { get; private set; } = false;
    }
}