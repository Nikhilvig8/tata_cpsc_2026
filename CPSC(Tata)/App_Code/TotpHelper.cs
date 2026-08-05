using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace InputOutput
{
    // Self-hosted TOTP two-factor authentication (RFC 6238 / RFC 4226 HOTP base). No external
    // service, no NuGet package, no cost - compatible with Google Authenticator, Microsoft
    // Authenticator, Authy, or any other standard authenticator app, since they all implement the
    // same open standard.
    public static class TotpHelper
    {
        private const int SecretByteLength = 20; // 160 bits, matches Google Authenticator's default
        private const int CodeDigits = 6;
        private const int StepSeconds = 30;
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string GenerateSecret()
        {
            var bytes = new byte[SecretByteLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Base32Encode(bytes);
        }

        // Formats a Base32 secret in 4-character groups for easier manual typing into an
        // authenticator app's "enter a setup key" option, e.g. "ABCD EFGH IJKL ...".
        public static string FormatForDisplay(string base32Secret)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < base32Secret.Length; i += 4)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(base32Secret.Substring(i, Math.Min(4, base32Secret.Length - i)));
            }
            return sb.ToString();
        }

        // Standard otpauth:// URI - not used today (no QR code rendering yet, manual entry only),
        // but kept so a QR code can be added later (e.g. via the free QRCoder NuGet package)
        // without having to redo the provisioning-URI logic.
        public static string GetProvisioningUri(string issuer, string account, string base32Secret)
        {
            string label = Uri.EscapeDataString(issuer + ":" + account);
            return $"otpauth://totp/{label}?secret={base32Secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits={CodeDigits}&period={StepSeconds}";
        }

        // Accepts the code if it matches the current 30-second step or one step either side
        // (+/-30s), to tolerate normal clock drift between the phone and the server.
        public static bool ValidateCode(string base32Secret, string submittedCode, int driftSteps = 1)
        {
            if (string.IsNullOrWhiteSpace(base32Secret) || string.IsNullOrWhiteSpace(submittedCode))
            {
                return false;
            }

            submittedCode = submittedCode.Trim();
            if (submittedCode.Length != CodeDigits)
            {
                return false;
            }

            byte[] secretBytes = Base32Decode(base32Secret);
            long currentStep = ToUnixTimeSeconds(DateTime.UtcNow) / StepSeconds;

            for (int drift = -driftSteps; drift <= driftSteps; drift++)
            {
                string expected = ComputeCode(secretBytes, currentStep + drift);
                if (FixedTimeEquals(expected, submittedCode))
                {
                    return true;
                }
            }
            return false;
        }

        private static string ComputeCode(byte[] secretBytes, long counter)
        {
            byte[] counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes);

            byte[] hash;
            using (var hmac = new HMACSHA1(secretBytes))
            {
                hash = hmac.ComputeHash(counterBytes);
            }

            int offset = hash[hash.Length - 1] & 0x0F;
            int binaryCode = ((hash[offset] & 0x7F) << 24)
                            | ((hash[offset + 1] & 0xFF) << 16)
                            | ((hash[offset + 2] & 0xFF) << 8)
                            | (hash[offset + 3] & 0xFF);

            int code = binaryCode % 1000000;
            return code.ToString("D6");
        }

        // Constant-time comparison so verifying a code doesn't leak timing information about how
        // many leading digits matched.
        private static bool FixedTimeEquals(string a, string b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }

        private static long ToUnixTimeSeconds(DateTime utcTime)
        {
            return (long)(utcTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        private static string Base32Encode(byte[] data)
        {
            var sb = new StringBuilder();
            int bitBuffer = 0, bitsInBuffer = 0;
            foreach (byte b in data)
            {
                bitBuffer = (bitBuffer << 8) | b;
                bitsInBuffer += 8;
                while (bitsInBuffer >= 5)
                {
                    int index = (bitBuffer >> (bitsInBuffer - 5)) & 0x1F;
                    sb.Append(Base32Alphabet[index]);
                    bitsInBuffer -= 5;
                }
            }
            if (bitsInBuffer > 0)
            {
                int index = (bitBuffer << (5 - bitsInBuffer)) & 0x1F;
                sb.Append(Base32Alphabet[index]);
            }
            return sb.ToString();
        }

        private static byte[] Base32Decode(string base32)
        {
            base32 = base32.Trim().TrimEnd('=').ToUpperInvariant().Replace(" ", "");
            var bytes = new List<byte>();
            int bitBuffer = 0, bitsInBuffer = 0;
            foreach (char c in base32)
            {
                int index = Base32Alphabet.IndexOf(c);
                if (index < 0) continue; // tolerate stray formatting characters
                bitBuffer = (bitBuffer << 5) | index;
                bitsInBuffer += 5;
                if (bitsInBuffer >= 8)
                {
                    bytes.Add((byte)((bitBuffer >> (bitsInBuffer - 8)) & 0xFF));
                    bitsInBuffer -= 8;
                }
            }
            return bytes.ToArray();
        }
    }
}
