using System;
using System.Configuration;
using System.Text;

namespace InputOutput
{
    // Checkmarx "Use_Of_Hardcoded_Password" (LoginController). The special-purpose logins that used
    // to compare against string literals now read their expected values from appSettings, which
    // Web.config pulls from the git-ignored secrets.config (template: secrets.config.example).
    //
    // Fail closed: a key that is missing or blank never matches, so an environment without these
    // settings simply has the special logins switched off.
    public static class ConfiguredLogin
    {
        public static string Setting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? null : value;
        }

        // Constant-time comparison of a submitted value against the configured one.
        public static bool Matches(string key, string supplied)
        {
            string expected = Setting(key);
            if (expected == null || supplied == null) return false;

            byte[] a = Encoding.UTF8.GetBytes(expected);
            byte[] b = Encoding.UTF8.GetBytes(supplied);
            int diff = a.Length ^ b.Length;
            int len = Math.Min(a.Length, b.Length);
            for (int i = 0; i < len; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
