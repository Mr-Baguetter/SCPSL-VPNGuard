namespace VPNShield.Objects
{
    public class IpHubApiResponse
    {
        public string ip { get; set; }
        public string countryCode { get; set; }
        public string countryName { get; set; }
        public int asn { get; set; }
        public string isp { get; set; }
        public int block { get; set; }
        public string hostname { get; set; }
    }
}
