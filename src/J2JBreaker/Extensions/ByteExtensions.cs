﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2JBreaker.Extensions
{
    internal static class ByteExtensions
    {
        internal static string BytesToHexString(this byte[] hexBytes)
        {
            string result = string.Empty;

            foreach (byte c in hexBytes)
                result += c.ToString("x2").ToUpper();

            return result;
        }

        internal static byte ReadByte(this byte[] array, int offset)
        {
            byte[] skipped = array.Skip(offset).ToArray();
            return skipped[0];
        }

        internal static byte[] ReadByte(this byte[] array, int offset, int length)
        {
            byte[] skipped = array.Skip(offset).ToArray();
            byte[] result = new byte[length];

            Array.Copy(skipped, 0, result, 0, length);

            return result;
        }

        internal static byte[] ReadUInt32LE(this byte[] array, int offset)
        {
            byte[] skipped = array.Skip(offset).ToArray();
            byte[] result = new byte[4];

            Array.Copy(skipped, 0, result, 0, 4);
            Array.Reverse(result);

            return result;
        }

        internal static byte[] ReadUInt32BE(this byte[] array, int offset)
        {
            byte[] skipped = array.Skip(offset).ToArray();
            byte[] result = new byte[4];

            Array.Copy(skipped, 0, result, 0, 4);
            Array.Sort(result);

            return result;
        }
    }
}
