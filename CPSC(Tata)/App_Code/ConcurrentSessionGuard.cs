using System;
using System.Runtime.Caching;

namespace InputOutput
{
    // VAPT finding "Concurrent login allowed": at most one active session per username. A second
    // successful login (any path - password+MFA, first-time MFA enrollment, SSO) supersedes
    // whatever session was active before it; the superseded session's next request is signed out
    // rather than left silently valid alongside the new one.
    //
    // Same in-process MemoryCache pattern as LoginThrottle.cs, with the same tradeoff: single-server
    // only (a web farm would need a shared store instead), and the cache is wiped on every app-pool
    // recycle. A recycle is handled by self-healing (see Touch) rather than force-logging everyone
    // out - IIS recycles routinely (idle timeout, scheduled) and that must not look like "signed in
    // elsewhere" to every real user.
    public static class ConcurrentSessionGuard
    {
        private const string KeyPrefix = "activesession_u_";
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(25); // matches sessionState timeout
        private static readonly object SyncRoot = new object();

        private static CacheItemPolicy Policy()
        {
            return new CacheItemPolicy { SlidingExpiration = Window };
        }

        // Called once, immediately after FormsAuthentication.SetAuthCookie, on every path that
        // completes a login. Returns a fresh per-login token; the caller stashes it in
        // Session["ActiveLoginToken"] - this session is now the sole authoritative one for the user,
        // and any earlier session (which has a different, now-stale token) will be signed out on its
        // next request.
        public static string Establish(string username)
        {
            string token = Guid.NewGuid().ToString("N");
            try
            {
                lock (SyncRoot)
                {
                    MemoryCache.Default.Set(KeyPrefix + Normalize(username), token, Policy());
                }
            }
            catch
            {
                // A cache write failure must not block login - worst case this degrades to
                // "concurrent login not enforced this request", not a broken login.
            }
            return token;
        }

        // Called on every request from an already-logged-in session (see SingleSessionAttribute).
        // True = still the authoritative session for this user (window refreshed); false = a newer
        // login elsewhere has superseded it.
        public static bool Touch(string username, string sessionToken)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(sessionToken))
            {
                return true; // nothing to compare against
            }

            try
            {
                string key = KeyPrefix + Normalize(username);
                lock (SyncRoot)
                {
                    var current = MemoryCache.Default.Get(key) as string;

                    if (current == null)
                    {
                        // Cache-only reset (app-pool recycle): self-heal by re-adopting this
                        // session rather than rejecting a legitimately still-logged-in user.
                        MemoryCache.Default.Set(key, sessionToken, Policy());
                        return true;
                    }

                    if (current == sessionToken)
                    {
                        MemoryCache.Default.Set(key, sessionToken, Policy()); // refresh sliding window
                        return true;
                    }

                    return false;
                }
            }
            catch
            {
                return true; // fail open - a cache error must never itself log a real user out
            }
        }

        public static void Clear(string username)
        {
            if (string.IsNullOrEmpty(username)) return;
            try { MemoryCache.Default.Remove(KeyPrefix + Normalize(username)); } catch { }
        }

        private static string Normalize(string username)
        {
            return (username ?? string.Empty).Trim().ToLowerInvariant();
        }
    }
}
