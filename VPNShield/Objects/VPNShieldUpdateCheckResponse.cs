namespace VPNShield.Objects
{
    public class VPNShieldUpdateCheckResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string LatestVersion { get; set; }
        public string[] DownloadUrls { get; set; }
    }
}
