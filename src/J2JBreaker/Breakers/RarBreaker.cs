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
    internal class RarBreaker : Breaker
    {
        #region ::Constants:

        /// <summary>
        /// The RAR Header(v1.50 onwards).
        /// </summary>
        private readonly byte[] HEADER_RAR_V150 = new byte[6] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07 };

        /// <summary>
        /// The RAR Header(v5.00 onwards).
        /// </summary>
        private readonly byte[] HEADER_RAR_V500 = new byte[8] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 };

        #endregion

        internal RarBreaker(FileStream fileStream) : base(fileStream)
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

            // Brute-force with v1.50 header.
            byte[] buffer1 = new byte[6];
            _fileStream.Position = 0;
            _fileStream.Read(buffer1, 0, 6);

            passwords.AddRange(helper.BruteForce(HEADER_RAR_V150, buffer1));

            // Brute-force with v5.00 header.
            byte[] buffer2 = new byte[8];
            _fileStream.Position = 0;
            _fileStream.Read(buffer2, 0, 8);

            passwords.AddRange(helper.BruteForce(HEADER_RAR_V500, buffer2));

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

            // Read bytes.
            byte[] encryptedPasswordBytesV150 = new byte[6];
            _fileStream.Position = 0;
            _fileStream.Read(encryptedPasswordBytesV150, 0, 6);

            byte[] encryptedPasswordBytesV500 = new byte[8];
            _fileStream.Position = 0;
            _fileStream.Read(encryptedPasswordBytesV500, 0, 8);

            // Add a V1.50 password.
            byte[] decryptedPasswordBytesV150 = new byte[6];

            for (int i = 0; i < 6; i++)
            {
                decryptedPasswordBytesV150[i] = (byte)(encryptedPasswordBytesV150[i] ^ HEADER_RAR_V150[i]);
            }

            for (int i = 0; i < 6; i++)
            {
                decryptedPasswordBytesV150[i] -= (byte)i;
            }

            string passwordV150 = Encoding.Unicode.GetString(decryptedPasswordBytesV150, 0, 6);
            passwords.Add(passwordV150);

            // Add a V5.00 password.
            byte[] decryptedPasswordBytesV500 = new byte[8];

            for (int i = 0; i < 8; i++)
            {
                decryptedPasswordBytesV500[i] = (byte)(encryptedPasswordBytesV500[i] ^ HEADER_RAR_V500[i]);
            }

            for (int i = 0; i < 8; i++)
            {
                decryptedPasswordBytesV500[i] -= (byte)i;
            }

            string passwordV500 = Encoding.Unicode.GetString(decryptedPasswordBytesV500, 0, 8);
            passwords.Add(passwordV500);

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
