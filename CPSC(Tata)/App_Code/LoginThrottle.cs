using System;
using System.Collections.Concurrent;

namespace InputOutput
{
    // In-process login throttling: after MaxAttempts failures for the same key (username+IP)
    // within WindowMinutes, further attempts are blocked for LockoutMinutes.
    // Deliberately simple/self-contained (no new package references) to keep this change low-risk
    // on a legacy single-instance deployment. If the app is ever scaled out behind a load balancer
    // across multiple instances, this in-memory store won't be shared across them and a DB/shared
    // cache-backed version would be needed instead.
    public static class LoginThrottle
    {
        private const int MaxAttempts = 5;
        private static readonly TimeSpan WindowLength = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan LockoutLength = TimeSpan.FromMinutes(15);

        private class Record
        {
            public int Count;
            public DateTime WindowStart;
            public DateTime? LockedUntil;
        }

        private static readonly ConcurrentDictionary<string, Record> Attempts =
            new ConcurrentDictionary<string, Record>(StringComparer.OrdinalIgnoreCase);

        public static bool IsLocked(string key, out TimeSpan retryAfter)
        {
            retryAfter = TimeSpan.Zero;
            if (string.IsNullOrEmpty(key)) return false;

            Record record;
            if (Attempts.TryGetValue(key, out record) && record.LockedUntil.HasValue)
            {
                var now = DateTime.UtcNow;
                if (record.LockedUntil.Value > now)
                {
                    retryAfter = record.LockedUntil.Value - now;
                    return true;
                }
            }
            return false;
        }

        public static void RegisterFailure(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            var now = DateTime.UtcNow;

            Attempts.AddOrUpdate(key,
                _ => new Record { Count = 1, WindowStart = now },
                (_, existing) =>
                {
                    lock (existing)
                    {
                        if (now - existing.WindowStart > WindowLength)
                        {
                            existing.Count = 1;
                            existing.WindowStart = now;
                            existing.LockedUntil = null;
                        }
                        else
                        {
                            existing.Count++;
                            if (existing.Count >= MaxAttempts)
                            {
                                existing.LockedUntil = now + LockoutLength;
                            }
                        }
                        return existing;
                    }
                });
        }

        public static void Reset(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            Record removed;
            Attempts.TryRemove(key, out removed);
        }
    }
}
