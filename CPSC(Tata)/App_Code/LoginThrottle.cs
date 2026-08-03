using System;
using System.Configuration;
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
        private const int MaxAttempts = 5;
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
                    IncrementAndMaybeLock(UserCounterPrefix + normUser, UserLockPrefix + normUser);
                    IncrementAndMaybeLock(IpCounterPrefix + normIp, IpLockPrefix + normIp);
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

        private static void IncrementAndMaybeLock(string counterKey, string lockKey)
        {
            var cache = MemoryCache.Default;
            var entry = cache.Get(counterKey) as Counter;
            if (entry == null)
            {
                entry = new Counter();
            }

            entry.Count++;
            cache.Set(counterKey, entry, new CacheItemPolicy { SlidingExpiration = WindowLength });

            if (entry.Count >= MaxAttempts)
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
    }
}
