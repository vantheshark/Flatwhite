using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Flatwhite.WebApi
{
    internal static class ExtensionMethods
    {
        public static int ToEpoch(this DateTime dateTime)
        {
            var t = (dateTime.ToUniversalTime() - new DateTime(1970, 1, 1));
            return (int)t.TotalSeconds;
        }

        public static string ToHex(this byte[] data)
        {
            var shb = new SoapHexBinary(data);
            return shb.ToString();
        }

        public static byte[] FromHex(this string data)
        {
            var shb = SoapHexBinary.Parse(data);
            return shb.Value;
        }
    }
}
