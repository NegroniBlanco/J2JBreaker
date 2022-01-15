using J2JBreaker.Breakers.Abstractions;
using J2JBreaker.Breakers.Enums;
using J2JBreaker.Extensions;
using J2JBreaker.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2JBreaker.Breakers
{
    internal class SevenZipBreaker : Breaker
    {
        #region ::Constants:

        /// <summary>
        /// The 7Z header.
        /// </summary>
        private readonly byte[] HEADER_7Z = new byte[6] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C };

        private readonly int MAX_MAJOR = 0;
        private readonly int MAX_MINOR = 4;

        #endregion

        internal SevenZipBreaker(FileStream fileStream) : base(fileStream)
        {
            // Set a default character set.
            _characterSet = VariableBuilder.GetCharacterSet();
        }

        private List<string> BreakUsingBruteForceMode(out bool result)
        {
            List<string> passwords = new List<string>();

            if (_fileStream == null)
            {
                throw new NullReferenceException("The file stream is null.");
            }

            // Step1. Brute-Force.
            Log.Information("STEP1. Brute-Forcing...");

            RainbowHelper helper = new RainbowHelper(_rainbowTablePath);

            // Default header with version bytes.
            byte[] headerWithVer = new byte[8];
            HEADER_7Z.CopyTo(headerWithVer, 0);

            byte[] buffer2 = new byte[8];
            _fileStream.Position = 0;
            _fileStream.Read(buffer2, 0, 8);

            for (int majorIndex = 0; majorIndex <= MAX_MAJOR; majorIndex++)
            {
                for (int minorIndex = 0; minorIndex <= MAX_MINOR; minorIndex++)
                {
                    headerWithVer[6] = (byte)majorIndex;
                    headerWithVer[7] = (byte)minorIndex;

                    passwords.AddRange(helper.BruteForce(headerWithVer, buffer2));
                }
            }

            // Distinct.
            passwords = passwords.Distinct().ToList();

            // Step2. Except abnormal passwords.
            Log.Information($"STEP2. Excepting abnormal passwords...");
            if (!_noExcept)
            {
                passwords = ExceptAbnormalStrings(passwords, _characterSet);
                passwords = ExceptLongStrings(passwords);
            }

            result = true;
            return passwords;
        }

        private List<string> BreakUsingSignatureMode(out bool result)
        {
            List<string> passwords = new List<string>();

            if (_fileStream == null)
            {
                throw new NullReferenceException("The file stream is null.");
            }

            // Step1. Analyze headers.
            Log.Information($"STEP1. Analyzing headers...");

            // Copy bytes.
            byte[] encryptedPasswordBytesWithVer = new byte[8];
            _fileStream.Position = 0;
            _fileStream.Read(encryptedPasswordBytesWithVer, 0, 8);

            // Interpret.

            byte[] headerWithVer = new byte[8];
            HEADER_7Z.CopyTo(headerWithVer, 0);

            for (int majorIndex=0; majorIndex<=MAX_MAJOR; majorIndex++)
            {
                for (int minorIndex = 0; minorIndex <= MAX_MINOR; minorIndex++)
                {
                    headerWithVer[6] = (byte)majorIndex;
                    headerWithVer[7] = (byte)minorIndex;

                    byte[] decryptedPasswordBytes = new byte[8];

                    for (int i = 0; i < 8; i++)
                    {
                        decryptedPasswordBytes[i] = (byte)(encryptedPasswordBytesWithVer[i] ^ headerWithVer[i]);
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        decryptedPasswordBytes[i] -= (byte)i;
                    }

                    string password = Encoding.Unicode.GetString(decryptedPasswordBytes, 0, 8);
                    passwords.Add(password);
                }
            }

            // Distinct.
            passwords = passwords.Distinct().ToList();

            // Step2. Except abnormal passwords.
            Log.Information($"STEP2. Excepting abnormal passwords...");
            if (!_noExcept)
            {
                passwords = ExceptAbnormalStrings(passwords, _characterSet);
                passwords = ExceptLongStrings(passwords);
            }

            // Step3. Enhance passwords.
            if (_useEnhancer)
            {
                Log.Information($"STEP3. Enhancing passwords(COUNT : {passwords.Count})...");

                List<string> enhancedPasswords = new List<string>();

                RainbowHelper enhancer = new RainbowHelper(_rainbowTablePath);

                foreach (string password in passwords)
                {
                    enhancedPasswords.AddRange(enhancer.Enhance(password));
                }

                passwords = enhancedPasswords;
            }

            result = true;
            return passwords;
        }

        internal override List<string> Break(out bool result)
        {
            List<string> passwords = new List<string>();

            bool breakingResult = false;

            switch (_mode)
            {
                case BreakingMode.BruteForce:
                    passwords = BreakUsingBruteForceMode(out breakingResult);
                    break;
                case BreakingMode.Signature:
                    passwords = BreakUsingSignatureMode(out breakingResult);
                    break;
            }

            result = breakingResult;
            return passwords;
        }
    }
}
