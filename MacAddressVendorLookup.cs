using System.Collections.Generic;

namespace PCAPModux
{
    public static class MacAddressVendorLookup
    {
        private static readonly Dictionary<string, string> ouiToVendor = new Dictionary<string, string>
        {
            { "00:1A:2B", "Cisco" },
            { "00:1B:63", "Apple" },
            { "00:1C:BF", "Dell" },
            // Add more mappings as needed
        };

        public static string GetVendorName(string macAddress)
        {
            string oui = macAddress.Substring(0, 8).ToUpper();
            return ouiToVendor.TryGetValue(oui, out string vendor) ? vendor : "Unknown";
        }
    }
}
