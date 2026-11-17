using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Web;

namespace InputOutput
{
    // VAPT-hardened brute-force / bot lockout for the login endpoint.
    //
    // Tracks failures on TWO independent counters and locks if EITHER trips:
    //  - per-username: catches a targeted attack against one account from many IPs.
    //  - per-IP:        catches one source spraying many different usernames (this is what a
    //                    single combined "username+IP" key would miss).
    // Both counters are tracked (and can lock) even for usernames that don't exist in the system -
    // deliberately, so lockout behavior can never be used to fingerprint whether an account is real.
    //
    // MemoryCache's own sliding expiration IS the 15-minute rolling window (no separate sweep/timer
    // needed): each failure refreshes the window, so a slow, patient attacker never fully resets it
    // as long as they keep trying periodically.
    public static class LoginThrottle
    {
        // Per-username limit protects each account. The per-IP limit only has to catch one source
        // spraying many usernames; it is much higher because a whole office shares one public IP
        // (corporate NAT), and 5 mistakes there would otherwise lock out every colleague.
        // Override with appSettings LoginMaxAttemptsPerIp if the security team requires a value.
        private const int MaxAttemptsPerUser = 5;
        private const int DefaultMaxAttemptsPerIp = 50;
        private static readonly TimeSpan WindowLength = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan LockoutLength = TimeSpan.FromMinutes(15);

        private const string UserCounterPrefix = "loginfail_u_";
        private const string IpCounterPrefix = "loginfail_ip_";
        private const string UserLockPrefix = "loginlock_u_";
        private const string IpLockPrefix = "loginlock_ip_";

        // Single process-wide lock around the read-modify-write increment. Login traffic volume
        // is low enough that this simple approach is safe and sufficient (no need for per-key
        // locking) - this is what makes "check count, then increment, then maybe set lock" atomic
        // under concurrent requests instead of racy.
        private static readonly object SyncRoot = new object();

        private class Counter
        {
            public int Count;
        }

        // Fail CLOSED: if the cache check itself throws for any reason, treat the request as
        // locked out rather than silently letting an unlimited number of attempts through.
        public static bool IsLocked(string username, string ip)
        {
            try
            {
                var cache = MemoryCache.Default;
                return cache.Contains(UserLockPrefix + NormalizeUser(username))
                    || cache.Contains(IpLockPrefix + NormalizeIp(ip));
            }
            catch
            {
                return true;
            }
        }

        public static void RegisterFailure(string username, string ip)
        {
            try
            {
                lock (SyncRoot)
                {
                    string normUser = NormalizeUser(username);
                    string normIp = NormalizeIp(ip);
                    IncrementAndMaybeLock(UserCounterPrefix + normUser, UserLockPrefix + normUser, MaxAttemptsPerUser);
                    IncrementAndMaybeLock(IpCounterPrefix + normIp, IpLockPrefix + normIp, MaxAttemptsPerIp());
                }
            }
            catch
            {
                // A failure to record an attempt must not itself break the login response.
                // IsLocked's fail-closed behavior above is the real backstop against abuse.
            }
        }

        public static void Reset(string username, string ip)
        {
            try
            {
                var cache = MemoryCache.Default;
                string normUser = NormalizeUser(username);
                string normIp = NormalizeIp(ip);
                cache.Remove(UserCounterPrefix + normUser);
                cache.Remove(IpCounterPrefix + normIp);
                cache.Remove(UserLockPrefix + normUser);
                cache.Remove(IpLockPrefix + normIp);
            }
            catch
            {
                // Non-fatal: a stale counter just expires naturally on its own window.
            }
        }

        // Admin unlock (Login/UnlockUser). Clears the username's counter and lock; the IP side is
        // left alone so an admin can't accidentally lift a lock on an attacking source.
        public static bool UnlockUser(string username)
        {
            var cache = MemoryCache.Default;
            string normUser = NormalizeUser(username);
            bool wasLocked = cache.Contains(UserLockPrefix + normUser);
            cache.Remove(UserCounterPrefix + normUser);
            cache.Remove(UserLockPrefix + normUser);
            return wasLocked;
        }

        // Admin "clear all lockouts": every username and IP counter/lock. Returns locks removed.
        public static int ClearAll()
        {
            var cache = MemoryCache.Default;
            int locks = 0;
            foreach (var key in cache.Select(kv => kv.Key).ToList())
            {
                bool isLock = key.StartsWith(UserLockPrefix, StringComparison.Ordinal) || key.StartsWith(IpLockPrefix, StringComparison.Ordinal);
                bool isCounter = key.StartsWith(UserCounterPrefix, StringComparison.Ordinal) || key.StartsWith(IpCounterPrefix, StringComparison.Ordinal);
                if (isLock || isCounter)
                {
                    cache.Remove(key);
                    if (isLock) locks++;
                }
            }
            return locks;
        }

        private static int MaxAttemptsPerIp()
        {
            int value;
            return int.TryParse(ConfigurationManager.AppSettings["LoginMaxAttemptsPerIp"], out value) && value > 0
                ? value
                : DefaultMaxAttemptsPerIp;
        }

        private static void IncrementAndMaybeLock(string counterKey, string lockKey, int maxAttempts)
        {
            var cache = MemoryCache.Default;
            var entry = cache.Get(counterKey) as Counter;
            if (entry == null)
            {
                entry = new Counter();
            }

            entry.Count++;
            cache.Set(counterKey, entry, new CacheItemPolicy { SlidingExpiration = WindowLength });

            if (entry.Count >= maxAttempts)
            {
                cache.Set(lockKey, true, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.Add(LockoutLength) });
            }
        }

        private static string NormalizeUser(string username)
        {
            return (username ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string NormalizeIp(string ip)
        {
            return string.IsNullOrWhiteSpace(ip) ? "unknown" : ip.Trim();
        }

        // Correct, deliberately conservative client IP resolution for lockout purposes.
        // X-Forwarded-For is trivially spoofable by anyone sending a direct request, so it is
        // IGNORED by default - only trusted if TrustProxyForwardedHeader is explicitly set to
        // "true" in Web.config, which you should only do once you've confirmed there really is a
        // reverse proxy/load balancer in front that overwrites (not appends to) that header for
        // external traffic. Until then this falls back to Request.UserHostAddress, the actual TCP
        // peer address IIS itself saw, which cannot be spoofed by the client.
        public static string ResolveClientIp(HttpRequestBase request)
        {
            // Behind Cloudflare the TCP peer is a Cloudflare edge server shared by many users, so
            // per-IP lockouts hit everyone routed through that edge. Cloudflare passes the real
            // client in CF-Connecting-IP; it is trusted ONLY when the connection itself comes from
            // Cloudflare's published ranges, so a direct request can't spoof it.
            string peer = request.UserHostAddress;
            if (IsCloudflare(peer))
            {
                string cf = request.Headers["CF-Connecting-IP"];
                IPAddress parsed;
                if (!string.IsNullOrWhiteSpace(cf) && IPAddress.TryParse(cf.Trim(), out parsed))
                {
                    return parsed.ToString();
                }
            }

            bool trustForwardedHeader = string.Equals(
                ConfigurationManager.AppSettings["TrustProxyForwardedHeader"], "true",
                StringComparison.OrdinalIgnoreCase);

            if (trustForwardedHeader)
            {
                string forwarded = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!string.IsNullOrWhiteSpace(forwarded))
                {
                    // Left-most entry is the original client as seen by the nearest trusted hop.
                    return forwarded.Split(',')[0].Trim();
                }
            }

            return request.UserHostAddress;
        }

        // https://www.cloudflare.com/ips-v4 and /ips-v6 (fetched 2026-11-17). If Cloudflare adds
        // ranges, requests from new edges simply fall back to the peer address (safe, not open).
        private static readonly string[] CloudflareRanges =
        {
            "173.245.48.0/20", "103.21.244.0/22", "103.22.200.0/22", "103.31.4.0/22",
            "141.101.64.0/18", "108.162.192.0/18", "190.93.240.0/20", "188.114.96.0/20",
            "197.234.240.0/22", "198.41.128.0/17", "162.158.0.0/15", "104.16.0.0/13",
            "104.24.0.0/14", "172.64.0.0/13", "131.0.72.0/22",
            "2400:cb00::/32", "2606:4700::/32", "2803:f800::/32", "2405:b500::/32",
            "2405:8100::/32", "2a06:98c0::/29", "2c0f:f248::/32"
        };

        private static bool IsCloudflare(string address)
        {
            IPAddress ip;
            if (string.IsNullOrWhiteSpace(address) || !IPAddress.TryParse(address.Trim(), out ip)) return false;
            if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
            byte[] bytes = ip.GetAddressBytes();
            foreach (string range in CloudflareRanges)
            {
                string[] parts = range.Split('/');
                byte[] net = IPAddress.Parse(parts[0]).GetAddressBytes();
                if (net.Length != bytes.Length) continue;
                int bits = int.Parse(parts[1]);
                bool match = true;
                for (int i = 0; i < net.Length && bits > 0; i++, bits -= 8)
                {
                    int mask = bits >= 8 ? 0xFF : (0xFF << (8 - bits)) & 0xFF;
                    if ((bytes[i] & mask) != (net[i] & mask)) { match = false; break; }
                }
                if (match) return true;
            }
            return false;
        }
    }
}
