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
    internal class ZipBreaker : Breaker
    {
        #region ::Constants:

        /// <summary>
        /// The ZIP Local Header(PK..).
        /// </summary>
        private readonly byte[] HEADER_ZIP = new byte[4] { 0x50, 0x4B, 0x03, 0x04 };

        #endregion

        internal ZipBreaker(FileStream fileStream) : base(fileStream)
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

            byte[] buffer = new byte[4];
            _fileStream.Position = 0;
            _fileStream.Read(buffer, 0, 4);

            passwords = helper.BruteForce(HEADER_ZIP, buffer);

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

            // Step1. Find a another ZIP local header.
            Log.Information("STEP1. Finding another ZIP local headers...");

            List<long> headerPositions = _fileStream.FindAll(0, HEADER_ZIP);

            if (headerPositions.Count == 0)
            {
                Log.Fatal("No another local headers exist.");
                result = false;
                return new List<string>();
            }

            // Step2. Find a another ZIP local header.
            Log.Information($"STEP2. Loading headers(COUNT : {headerPositions.Count})...");

            List<byte[]> headers = new List<byte[]>();

            foreach (long pos in headerPositions)
            {
                byte[] buffer = new byte[14];
                _fileStream.Position = pos;
                _fileStream.Read(buffer, 0, 14);

                bool errorFlag = false;

                foreach (byte[] header in headers)
                {
                    if (header.SequenceEqual(buffer))
                    {
                        errorFlag = true;
                    }
                }

                if (!errorFlag)
                {
                    headers.Add(buffer);
                }
            }

            // Step3. Analyze headers.
            Log.Information($"STEP3. Analyzing headers(COUNT : {headers.Count})...");

            byte[] encryptedPasswordBytes = new byte[14];
            _fileStream.Position = 0;
            _fileStream.Read(encryptedPasswordBytes, 0, 14);

            foreach (byte[] header in headers)
            {
                byte[] decryptedPasswordBytes = new byte[14];

                for (int i = 0; i < 14; i++)
                {
                    decryptedPasswordBytes[i] = (byte)(encryptedPasswordBytes[i] ^ header[i]);
                }

                for (int i = 0; i < 14; i++)
                {
                    decryptedPasswordBytes[i] -= (byte)i;
                }

                string password = Encoding.Unicode.GetString(decryptedPasswordBytes, 0, 14);
                passwords.Add(password);
            }

            // Step4. Except abnormal passwords.
            Log.Information($"STEP4. Excepting abnormal passwords...");
            if (!_noExcept)
            {
                passwords = ExceptAbnormalStrings(passwords, _characterSet);
                passwords = ExceptLongStrings(passwords);
            }

            // Step5. Enhance passwords.
            if (_useEnhancer)
            {
                Log.Information($"STEP5. Enhancing passwords(COUNT : {passwords.Count})...");

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
