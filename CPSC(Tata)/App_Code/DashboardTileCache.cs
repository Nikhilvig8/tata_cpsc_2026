using System;
using System.Data;
using System.Runtime.Caching;

namespace InputOutput
{
    // Performance: the home dashboard's top tiles come from stored procedure getBoxValue, which takes
    // ~11 s on production and runs on every visit to /Home/Index. Its result depends only on the
    // user id, so it is kept in memory per user for a few minutes: only the first visit in that
    // window waits for the database, repeat visits render immediately.
    //
    // Trade-off: tile numbers can be up to CacheDuration old. Call Invalidate(uid) after a change
    // that must show immediately.
    public static class DashboardTileCache
    {
        private const string KeyPrefix = "dashboard_boxvalue_";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        // Returns a private copy of the cached table (callers may modify it); calls load() on a miss.
        // A null or failed load is never cached.
        public static DataTable GetBoxValues(string uid, Func<DataTable> load)
        {
            string key = KeyPrefix + (uid ?? string.Empty).Trim().ToLowerInvariant();

            var cached = MemoryCache.Default.Get(key) as DataTable;
            if (cached != null)
            {
                return cached.Copy();
            }

            DataTable fresh = load();
            if (fresh != null)
            {
                MemoryCache.Default.Set(key, fresh.Copy(), DateTimeOffset.UtcNow.Add(CacheDuration));
            }
            return fresh;
        }

        public static void Invalidate(string uid)
        {
            MemoryCache.Default.Remove(KeyPrefix + (uid ?? string.Empty).Trim().ToLowerInvariant());
        }
    }
}
