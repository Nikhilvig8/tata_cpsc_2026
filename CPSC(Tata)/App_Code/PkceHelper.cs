using System;
using System.Security.Cryptography;
using System.Text;

namespace InputOutput
{
    // RFC 7636 PKCE helpers for the Keycloak Authorization Code flow (public client "cpsc-cv-browser"
    // has no client secret, so PKCE is what proves the token-exchange request came from the same
    // party that started the redirect, instead of a secret).
    public static class PkceHelper
    {
        public static string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Base64UrlEncode(bytes);
        }

        public static string ComputeS256Challenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
                return Base64UrlEncode(hash);
            }
        }

        public static string GenerateState()
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
