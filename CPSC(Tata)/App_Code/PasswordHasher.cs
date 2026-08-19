using System;
using System.Security.Cryptography;
using System.Text;

namespace InputOutput
{
    // VAPT finding "Password in plain text". PBKDF2-HMACSHA256, per-password random salt, output
    // is a single self-describing string so a validator can tell a hash apart from a legacy
    // plaintext value on sight (plaintext passwords never contain "$", the field separator here):
    //
    //   PBKDF2$<iterations>$<base64 salt>$<base64 hash>
    //
    // NOT yet wired into the login/create-user path - see the note on Users.cs' ValidateUser/
    // CreateUser calls. Those go through SQL stored procedures ("ValidateUser", "CreateUser") that
    // read/write a plaintext [Password] column directly in the database; switching them over
    // requires either rewriting those procedures or adding new ones, plus a migration for existing
    // rows, none of which can be done or verified without direct database access. This class is the
    // app-side half of that fix, ready to call once the DB side exists:
    //
    //   1. Add a migration that either reuses [Password] (storing the PBKDF2 string there instead
    //      of plaintext - simplest if that column is wide enough, e.g. nvarchar(200)+) or adds a new
    //      nullable [PasswordHash] column alongside it (safer if you're not sure of the column
    //      width - same pattern already used for [TotpSecret], see Users.GetTotpSecret).
    //   2. New/updated procedure to fetch the stored value by username so the app can call
    //      PasswordHasher.Verify() itself, instead of the database doing a plaintext "=" compare.
    //   3. Lazy migration on successful login: while a row still holds a legacy plaintext value,
    //      verify with a constant-time string compare, then immediately call PasswordHasher.Hash()
    //      on the password just typed and write it back - existing users get migrated the moment
    //      they next log in, with no bulk-update downtime and no forced password reset.
    //   4. CreateUser (new accounts) can switch to PasswordHasher.Hash() immediately; it needs no
    //      legacy branch since there's no old plaintext row to migrate.
    public static class PasswordHasher
    {
        private const int SaltSize = 16;   // bytes
        private const int HashSize = 32;   // bytes (SHA-256 output size)
        private const int Iterations = 100000;
        private const string Prefix = "PBKDF2";

        public static string Hash(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = Derive(password, salt, Iterations);

            return string.Join("$", Prefix, Iterations.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        // True if candidatePassword matches a value previously produced by Hash(). Returns false
        // (never throws) for anything that isn't in this format - callers use IsHashed() first to
        // decide whether a stored value is even meant to be checked this way, vs. compared as
        // legacy plaintext.
        public static bool Verify(string candidatePassword, string storedValue)
        {
            if (candidatePassword == null || string.IsNullOrEmpty(storedValue)) return false;

            string[] parts = storedValue.Split('$');
            if (parts.Length != 4 || parts[0] != Prefix) return false;

            int iterations;
            if (!int.TryParse(parts[1], out iterations)) return false;

            byte[] salt, expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedHash = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] actualHash = Derive(candidatePassword, salt, iterations);
            return FixedTimeEquals(actualHash, expectedHash);
        }

        // Lets a caller branch between "run Verify()" (new-format hash) and "legacy plaintext
        // compare, then migrate" without duplicating the format check in Users.cs.
        public static bool IsHashed(string storedValue)
        {
            return !string.IsNullOrEmpty(storedValue) && storedValue.StartsWith(Prefix + "$", StringComparison.Ordinal);
        }

        // Rfc2898DeriveBytes's SHA-256 overload doesn't exist until .NET Framework 4.7.2 (this app
        // targets 4.6, per Web.config's targetFramework and packages.config's net452 packages), and
        // its parameterless constructor is SHA-1-only - so PBKDF2-HMACSHA256 is implemented by hand
        // here per RFC 2898 (section 5.2), rather than silently downgrading to PBKDF2-SHA1.
        private static byte[] Derive(string password, byte[] salt, int iterations)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(password)))
            {
                int hLen = hmac.HashSize / 8;
                int blockCount = (HashSize + hLen - 1) / hLen;
                byte[] output = new byte[blockCount * hLen];
                byte[] block = new byte[salt.Length + 4];
                Buffer.BlockCopy(salt, 0, block, 0, salt.Length);

                for (int i = 1; i <= blockCount; i++)
                {
                    block[salt.Length] = (byte)(i >> 24);
                    block[salt.Length + 1] = (byte)(i >> 16);
                    block[salt.Length + 2] = (byte)(i >> 8);
                    block[salt.Length + 3] = (byte)i;

                    byte[] u = hmac.ComputeHash(block);
                    byte[] t = (byte[])u.Clone();
                    for (int j = 1; j < iterations; j++)
                    {
                        u = hmac.ComputeHash(u);
                        for (int k = 0; k < t.Length; k++) t[k] ^= u[k];
                    }
                    Buffer.BlockCopy(t, 0, output, (i - 1) * hLen, hLen);
                }

                byte[] result = new byte[HashSize];
                Buffer.BlockCopy(output, 0, result, 0, HashSize);
                return result;
            }
        }

        // Constant-time comparison so a timing side-channel can't leak how many leading bytes of a
        // guessed hash were correct.
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
