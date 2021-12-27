using System;
using System.Text;

namespace CloudAutomation.Utils.Extensions
{
    public static class StringEncodingExtension
    {
        public static string BasicEncoding(this string pat)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));
        }

        public static string BasicDecoding(this string basicAuth)
        {
            if (!basicAuth.Contains("Basic ")) throw new ArgumentException("Invalid Basic Token");

            var basicToken = basicAuth.Split(" ")[1];

            var basicTokenDecodedBytes = Convert.FromBase64String(basicToken);

            var pat = Encoding.ASCII.GetString(basicTokenDecodedBytes);
            pat = pat.Split(":")[1];
            return pat;
        }
    }
}