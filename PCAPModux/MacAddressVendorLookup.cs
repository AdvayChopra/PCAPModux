using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCAPModux
{
    public static class MacAddressVendorLookup
    {
        private static readonly Dictionary<string, string> ouiToVendor;

        static MacAddressVendorLookup()
        {
            ouiToVendor = LoadOuiToVendorMappings(@"C:\Users\chopr\Downloads\oui_all.csv");
        }

        private static Dictionary<string, string> LoadOuiToVendorMappings(string filePath)
        {
            var dictionary = new Dictionary<string, string>();

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines.Skip(1)) // Skip the header line
            {
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    var oui = parts[1].Trim().ToUpper();
                    var vendor = parts[2].Trim().Trim('"'); // Remove any surrounding quotes
                    dictionary[oui] = vendor;
                }
            }

            return dictionary;
        }

        public static string GetVendorName(string macAddress)
        {
            string oui = macAddress.Substring(0, 6).ToUpper();
            return ouiToVendor.TryGetValue(oui, out string vendor) ? vendor : "Unknown";
        }
    }
}