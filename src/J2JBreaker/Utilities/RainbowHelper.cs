using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2JBreaker.Utilities
{
    internal class RainbowHelper
    {
        private string _rainbowTablePath = string.Empty;

        internal string RainbowTablePath
        {
            get => _rainbowTablePath;
            set => _rainbowTablePath = value;
        }

        private List<string> _rainbowTable = new List<string>();

        internal List<string> RainbowTable
        {
            get => _rainbowTable;
            set => _rainbowTable = value;
        }

        internal RainbowHelper(string rainbowTablePath)
        {
            _rainbowTablePath = rainbowTablePath;

            InitializeRainbowHelper();
        }

        private void InitializeRainbowHelper()
        {
            if (!File.Exists(_rainbowTablePath))
            {
                throw new FileNotFoundException("The rainbow table file does not found.");
            }

            using (StreamReader reader = new StreamReader(_rainbowTablePath))
            {
                while (!reader.EndOfStream)
                {
                    _rainbowTable.Add(reader.ReadLine() ?? string.Empty);
                }
            }
        }

        internal List<string> Enhance(string text)
        {
            if (text.Length < 3)
            {
                throw new InvalidDataException("The text is too short to enhance.");
            }

            List<string> result = new List<string>();

            result.Add(text);

            // Step1. Checks connections of text and rainbow.
            // (inputted text) -> (rainbow text)
            for (int index=0; index<_rainbowTable.Count; index++)
            {
                string rainbow = _rainbowTable[index];

                for (int offset=2; offset< text.Length; offset++)
                {
                    int startIndex = (text.Length - 1) - offset;
                    string splittedText = text.Substring(startIndex, (text.Length - 1) - startIndex);

                    if (rainbow.StartsWith(splittedText, StringComparison.OrdinalIgnoreCase))
                    {
                        string processedText = text.Substring(0, startIndex) + rainbow;
                        result.Add(processedText);
                    }
                }
            }

            // Step2. Distinct.
            result = result.Distinct().ToList();

            return result;
        }

        internal List<string> BruteForce(byte[] originalBytes, byte[] targetBytes)
        {
            if (originalBytes.Length > 255)
            {
                throw new InvalidDataException("The byte array is too long.");
            }

            List<string> passwords = new List<string>();

            for (int index = 0; index < _rainbowTable.Count; index++)
            {
                string rainbow = _rainbowTable[index];

                // Step1. Interprte.
                // One char is 2 Bytes(in UTF-16).

                byte[] buffer = Encoding.Unicode.GetBytes(rainbow);

                int length = buffer.Length < originalBytes.Length ? buffer.Length : originalBytes.Length;

                byte[] rainbowBytes = new byte[length];
                Array.Copy(buffer, 0, rainbowBytes, 0, length);

                for (byte weight=0; weight<length; weight++)
                {
                    rainbowBytes[weight] += weight;
                }

                // Step2. XOR.
                byte[] xorBytes = new byte[length];

                for (int index2 = 0; index2 < length; index2++)
                {
                    xorBytes[index2] = (byte)(rainbowBytes[index2] ^ targetBytes[index2]);
                }

                // Step3. Compare.
                if (originalBytes.SequenceEqual(xorBytes))
                {
                    passwords.Add(rainbow);
                }
            }

            return passwords;
        }
    }
}
