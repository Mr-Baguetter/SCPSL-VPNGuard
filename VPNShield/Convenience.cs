using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VPNShield
{
    public class Convenience
    {
        //THIS FUNCTION IS FROM https://gist.github.com/a-luna/bd93686ace9f6f22acf0a7032fc41777#file-networkutilities_2_ipaddressisinrange-cs
        //Many thanks to a-luna on GitHub for the function.
        //I (SomewhatSane, Mr. Baguetter) assume NO credit for it.

        public static bool IpAddressIsInRange(string checkIp, string cidrIp)
        {
            if (string.IsNullOrEmpty(checkIp))
            {
                throw new ArgumentException("Input string must not be null", checkIp);
            }

            var ipAddress = ParseIPv4Addresses(checkIp)[0];

            return IpAddressIsInRange(ipAddress, cidrIp);
        }

        public static bool IpAddressIsInRange(IPAddress checkIp, string cidrIp)
        {
            if (string.IsNullOrEmpty(cidrIp))
            {
                throw new ArgumentException("Input string must not be null", cidrIp);
            }

            var cidrAddress = ParseIPv4Addresses(cidrIp)[0];

            var parts = cidrIp.Split('/');
            if (parts.Length != 2)
            {
                throw new FormatException($"cidrMask was not in the correct format:\nExpected: a.b.c.d/n\nActual: {cidrIp}");
            }

            if (!Int32.TryParse(parts[1], out var netmaskBitCount))
            {
                throw new FormatException($"Unable to parse netmask bit count from {cidrIp}");
            }

            if (0 > netmaskBitCount || netmaskBitCount > 32)
            {
                throw new ArgumentOutOfRangeException($"Netmask bit count value of {netmaskBitCount} is invalid, must be in range 0-32");
            }

            var ipAddressBytes = BitConverter.ToInt32(checkIp.GetAddressBytes(), 0);
            var cidrAddressBytes = BitConverter.ToInt32(cidrAddress.GetAddressBytes(), 0);
            var cidrMaskBytes = IPAddress.HostToNetworkOrder(-1 << (32 - netmaskBitCount));

            var ipIsInRange = (ipAddressBytes & cidrMaskBytes) == (cidrAddressBytes & cidrMaskBytes);

            return ipIsInRange;
        }

        public static List<IPAddress> ParseIPv4Addresses(string input)
        {
            const string ipV4Pattern =
                @"(?:(?:1\d\d|2[0-5][0-5]|2[0-4]\d|0?[1-9]\d|0?0?\d)\.){3}(?:1\d\d|2[0-5][0-5]|2[0-4]\d|0?[1-9]\d|0?0?\d)";

            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input string must not be null", input);
            }

            var ips = new List<IPAddress>();
            try
            {
                var regex = new Regex(ipV4Pattern);
                foreach (Match match in regex.Matches(input))
                {
                    var ip = ParseSingleIPv4Address(match.Value);
                    ips.Add(ip);
                }
            }
            catch (Exception) { }

            return ips;
        }

        public static IPAddress ParseSingleIPv4Address(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input string must not be null", input);
            }

            var addressBytesSplit = input.Trim().Split('.').ToList();
            if (addressBytesSplit.Count != 4)
            {
                throw new ArgumentException("Input string was not in valid IPV4 format \"a.b.c.d\"", input);
            }

            var addressBytes = new byte[4];
            foreach (var i in Enumerable.Range(0, addressBytesSplit.Count))
            {
                if (!int.TryParse(addressBytesSplit[i], out var parsedInt))
                {
                    throw new FormatException($"Unable to parse integer from {addressBytesSplit[i]}");
                }

                if (0 > parsedInt || parsedInt > 255)
                {
                    throw new ArgumentOutOfRangeException($"{parsedInt} not within required IP address range [0,255]");
                }

                addressBytes[i] = (byte)parsedInt;
            }

            return new IPAddress(addressBytes);
        }

        public static bool CheckIfCIDR(string cidrString)
        {
            //Only works for IPv4. (Fine atm as SCPSL only supports IPv4 but this should be updated later down the line.)
            string[] segments = cidrString.Split('/');

            if (segments.Length != 2)
                return false;

            if (!IPAddress.TryParse(segments[0], out _))
                return false;

            if (!int.TryParse(segments[1], out int maskSize))
                return false;

            if (maskSize < 1 || maskSize > 32)
                return false;

            return true;
        }
    }
}
