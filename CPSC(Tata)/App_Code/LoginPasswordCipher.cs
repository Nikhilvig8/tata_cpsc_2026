using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace InputOutput
{
    // VAPT "Client-Side AES Encryption Key/IV Exposed in HTML" / "Password Transmitted Without
    // Client-Side Encryption": the login password is encrypted in the browser with RSA-OAEP
    // (Web Crypto API) before the form is submitted.
    //
    // - Asymmetric: only the RSA PUBLIC key is ever sent to the browser. The private key is
    //   generated in memory on the server, never written to disk, never sent anywhere. Capturing
    //   the page source plus the POST body (proxy log, browser cache, MITM behind a TLS-terminating
    //   proxy) is therefore no longer enough to recover the password.
    // - Key rotation: a fresh 2048-bit key pair is generated every RotationInterval (and on every
    //   app-pool restart). The previous key is kept for one more interval so a login page opened
    //   just before a rotation still works.
    // - Replay protection: the browser encrypts "<nonce>\n<password>", where the nonce is a random
    //   single-use value issued with the login page and stored in Session. TryDecrypt consumes the
    //   nonce, so a captured ciphertext cannot be re-submitted later as a password equivalent.
    // - No plaintext fallback: if the value does not decrypt (old browser, tampered or replayed
    //   value, expired session) the login is rejected - the raw submitted value is NEVER used as a
    //   password.
    //
    // RSACryptoServiceProvider + OAEP(SHA-1) is used (rather than RSACng/OAEP-SHA256) so the same
    // file compiles against both .NET 4.5.2 (CPSC) and 4.8 (TASS). OAEP with SHA-1 remains secure
    // for encryption; it is the Web Crypto "RSA-OAEP" + hash "SHA-1" combination.
    public static class LoginPasswordCipher
    {
        private const string NonceSessionKey = "LoginPwNonce";
        private const int KeySizeBits = 2048;
        private static readonly TimeSpan RotationInterval = TimeSpan.FromHours(12);

        private static readonly object Sync = new object();
        private static RSACryptoServiceProvider _current;
        private static RSACryptoServiceProvider _previous;
        private static DateTime _currentCreatedUtc;

        public sealed class LoginPageKey
        {
            public string ModulusB64Url { get; set; }
            public string ExponentB64Url { get; set; }
            public string Nonce { get; set; }
        }

        // Called by GET /Login: returns the public key (JWK n/e) and a fresh single-use nonce,
        // which is also stored in Session for LoginCheck to verify.
        public static LoginPageKey PrepareLoginPage(HttpSessionStateBase session)
        {
            RSAParameters pub;
            lock (Sync)
            {
                EnsureCurrentKey();
                pub = _current.ExportParameters(false);
            }

            string nonce = Base64Url(RandomBytes(16));
            session[NonceSessionKey] = nonce;

            return new LoginPageKey
            {
                ModulusB64Url = Base64Url(pub.Modulus),
                ExponentB64Url = Base64Url(pub.Exponent),
                Nonce = nonce
            };
        }

        // Returns the decrypted password, or null on ANY failure (missing/used nonce, malformed
        // base64, wrong key, nonce mismatch). Never throws. Always consumes the session nonce.
        public static string TryDecrypt(string cipherTextBase64, HttpSessionStateBase session)
        {
            string expectedNonce = session[NonceSessionKey] as string;
            session.Remove(NonceSessionKey);

            if (string.IsNullOrEmpty(expectedNonce) || string.IsNullOrEmpty(cipherTextBase64)) return null;

            byte[] cipherBytes;
            try
            {
                cipherBytes = Convert.FromBase64String(cipherTextBase64);
            }
            catch (FormatException)
            {
                return null;
            }

            byte[] plainBytes = null;
            lock (Sync)
            {
                plainBytes = TryDecryptWith(_current, cipherBytes) ?? TryDecryptWith(_previous, cipherBytes);
            }
            if (plainBytes == null) return null;

            string plain;
            try
            {
                plain = new UTF8Encoding(false, true).GetString(plainBytes);
            }
            catch (ArgumentException)
            {
                return null;
            }
            finally
            {
                Array.Clear(plainBytes, 0, plainBytes.Length);
            }

            int sep = plain.IndexOf('\n');
            if (sep <= 0) return null;
            if (!FixedTimeEquals(plain.Substring(0, sep), expectedNonce)) return null;

            return plain.Substring(sep + 1);
        }

        private static byte[] TryDecryptWith(RSACryptoServiceProvider rsa, byte[] cipherBytes)
        {
            if (rsa == null) return null;
            try
            {
                return rsa.Decrypt(cipherBytes, true); // true = OAEP padding (SHA-1)
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        // Caller must hold Sync.
        private static void EnsureCurrentKey()
        {
            if (_current != null && DateTime.UtcNow - _currentCreatedUtc < RotationInterval) return;

            // UseMachineKeyStore: IIS app pools usually run without a loaded user profile, where
            // the default (user) key store is unavailable and key generation throws - which
            // surfaced as HTTP 500 on the login page.
            var csp = new CspParameters { Flags = CspProviderFlags.UseMachineKeyStore };
            var fresh = new RSACryptoServiceProvider(KeySizeBits, csp) { PersistKeyInCsp = false };
            if (_previous != null) _previous.Dispose();
            _previous = _current;
            _current = fresh;
            _currentCreatedUtc = DateTime.UtcNow;
        }

        private static byte[] RandomBytes(int count)
        {
            var bytes = new byte[count];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return bytes;
        }

        private static string Base64Url(byte[] data)
        {
            return Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
