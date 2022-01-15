using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2JBreaker.Extensions
{
    internal static class StringExtensions
    {
        internal static byte HexToByte(this string hex)
        {
            return Convert.ToByte(hex, 16);
        }

        internal static byte[] HexToBytes(this string hex)
        {
            byte[] result = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length; i += 2)
            {
                result[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return result;
        }

        internal static IEnumerable<string> SplitInParts(this string text, int partLength)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < text.Length; i += partLength)
                yield return text.Substring(i, Math.Min(partLength, text.Length - i));
        }

        internal static List<string> SplitByString(this string text, string seperator)
        {
            return text.Split(new string[1] { seperator }, StringSplitOptions.None).ToList();
        }
    }
}
