using System.Web;
using System.Web.SessionState;

namespace InputOutput
{
    // DEPRECATED / UNUSED - do not call this. RegenerateSessionId's in-request SessionIDManager
    // swap broke every production login (SaveSessionID() changes the outgoing cookie but doesn't
    // reliably relocate where SessionStateModule persists THAT request's own Session[...] writes,
    // so everything set afterward silently vanished). Session-fixation closure now lives in
    // LoginController.BeginSessionRotation/CompleteLogin, which rotates the ID via a safe
    // two-request abandon-then-redirect bounce instead. Kept only for its history; not referenced
    // anywhere in the app.
    //
    // VAPT finding "Session Fixation": Session.Clear() (already called pre-auth in LoginCheck and
    // OidcCallback) only empties the CURRENT session's stored values - it does not change the
    // ASP.NET_SessionId itself. An attacker who plants a known session ID on a victim (e.g. via a
    // crafted link containing a pre-set ASP.NET_SessionId) still holds a valid cookie for whatever
    // that same ID ends up authenticated as, because Clear() alone never rotates it.
    //
    // This swaps in a brand-new session ID for the CURRENT request via SessionIDManager, the same
    // primitive ASP.NET's own SessionStateModule uses. Session's in-memory collection for this
    // request is already loaded before this call, so anything already in Session (or added to it
    // afterwards, later in the same request) is preserved - the module just persists it under the
    // NEW id at end-of-request (mode="InProc", per Web.config) and the Set-Cookie header carries
    // only that new id back to the caller. The old id is left with nothing tied to the freshly
    // started session.
    //
    // Called right after Session.Clear() at every pre-auth entry point (LoginCheck, OidcCallback) -
    // before any credential is checked - so no session ID that ever crossed the authentication
    // boundary can be one an attacker chose in advance.
    public static class SessionSecurity
    {
        public static void RegenerateSessionId(HttpContext context)
        {
            if (context == null || context.Session == null)
            {
                return;
            }

            var manager = new SessionIDManager();
            string newId = manager.CreateSessionID(context);
            bool redirected, cookieAdded;
            manager.SaveSessionID(context, newId, out redirected, out cookieAdded);
        }
    }
}
