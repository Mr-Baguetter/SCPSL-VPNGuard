# VPNGuard
A VPN blocking plugin for SCP: SL servers running NWAPI (based on VPNShield-EXILED-Edition).

<h1>Configuration</h1>

```
# Should account age checking be enabled?
account_age_check: false
# Should account playtime checking be enabled?
account_playtime_check: false
# Should accounts that cannot be checked (eg. private Steam accounts) be kicked?
account_kick_private: true
# Should accounts that cannot be checked due to a Steam API error be kicked? In most cases, you should keep this set to false.
account_kick_on_steam_error: false
# Steam API key for account age checking.
steam_api_key:
# Minimum Steam account age (if account age checking is enabled - in days).
steam_min_age: 14
# Minimum required SCPSL playtime required (if account playtime checking is enabled - in minutes).
steam_min_playtime: 0
# Message shown to players who are kicked by an account age check. You may use %MINIMUMAGE% to insert the minimum age in days set into your kick message.
account_age_check_kick_message: Your account must be at least %MINIMUMAGE% day(s) old to play on this server.
# Message shown to players who are kicked by an account playtime check. You may use %MINIMUMPLAYTIME% to insert the minimum playtime in minutes set into your kick message.
account_playtime_check_kick_message: 'Your account must have played SCP: SL for atleast %MINIMUMPLAYTIME% minute(s) to play on this server.'
# Message shown to players who are kicked because they account cannot be checked due to privacy settings.
account_private_kick_message: An account check could not be performed as your Steam profile is set to private. Please make your profile public and try connecting again!
# Message shown to players who are kicked as there was a Steam API error (only needed if account_kick_on_steam_error).
account_steam_error_kick_message: An error occurred when trying to check your Steam account. Due to the policy set on this server, you were kicked. Please contact the server administration about this and try joining again later.
# Should VPN checking be enabled?
vpn_check: true
# What VPN service would you like to use for VPN checking? (0 for IPHub, 1 for GetIPIntel)
vpn_check_service: 0
# IF vpn_check_service IS 0: IPHub API key for VPN checking. Get one for free at https://iphub.info .
iphub_api_key:
# IF vpn_check_service IS 0: IPHub supports 'strict blocking'. Should it be enabled? Strict blocking will catch more VPN / hosting IP addresses but may cause false positives. It is generally recommended to keep this disabled.
iphub_strict_blocking: false
# IF vpn_check_service IS 1: GetIPIntel.net API subdomain. If you have a specific plan, you can specify your subdomain. If you are using the free plan or don't know what this is, leave it as it is.
getipintel_subdomain: check
# IF vpn_check_service IS 1: GetIPIntel.net Contact Email Address. The API requires a VALID contact email.
getipintel_contact_email_address: 
# IF vpn_check_service IS 1: GetIPIntel.net optional flags. Flags m, b, f and n are supported. If you don't know what this is, leave this blank / null or check the GetIPIntel website.
getipintel_optional_flags:
# IF vpn_check_service IS 1: GetIPIntel.net max score. Any IP Address with a score above this value will be blocked. 0.995 is recommended by the API.
getipintel_max_score: 0.995000005
# IF vpn_check_service IS 1: Block mobile ISPs? (Players playing on mobile data providers such as 3, EE, Vodafone, T-Mobile, Verizon, AT&T etc...)
getipintel_block_mobile_isps: false
# Message shown to players who are kicked by a VPN check or a mobile ISP check (if enabled).
vpn_check_kick_message: VPNs and proxies are forbidden on this server.
# Send a message to Discord via webhooks when someone is kicked by VPNShield?
send_to_discord_webhook: false
# Discord Webhook URL for send_to_discord_webhook (only needed if kick_to_discord is true).
send_to_discord_webhook_url: 
# Check for VPNShield updates on startup?
check_for_updates: true
# Verbose mode. Prints more console messages.
verbose_mode: false
```

<h1>Commands</h1>

`vs_reload` - Reload VPNGuard's data.
<br><br>
`vs_save (ips/userids/all)` - Save VPNGuard data to disk.
