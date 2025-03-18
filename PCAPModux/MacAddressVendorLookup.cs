using System.Collections.Generic;

namespace PCAPModux
{
    public static class MacAddressVendorLookup
    {
        private static readonly Dictionary<string, string> ouiToVendor = new Dictionary<string, string>
        {
            { "00070DAFF454", "Cisco" },
            { "000000000000", "Broadcast" },
            // Add more mappings as needed
        };

        public static string GetVendorName(string macAddress)
        {
            string oui = macAddress.Substring(0, 12).ToUpper();
            return ouiToVendor.TryGetValue(oui, out string vendor) ? vendor : "Unknown";
        }
    }
}