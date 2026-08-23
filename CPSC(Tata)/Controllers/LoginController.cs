using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using InputOutput.Models;
using System.IO;
using System.Net;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Execution;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Http;
using System.Drawing;
using System.Drawing.Imaging;
using QRCoder;

namespace InputOutput.Controllers
{
    // No local [HandleError()] here - a bare (non-logging) HandleErrorAttribute on the
    // controller runs before the global LoggingHandleErrorAttribute (see FilterConfig.cs) and
    // marks the exception handled first, so LoggingHandleErrorAttribute's own logging never
    // fires. Same generic Error page either way; the global filter is what actually logs it.
    public class LoginController : Controller
    {
        // GET: Login

        
        public ActionResult Login()
        {
            return View();
        }

        // Self-hosted alphanumeric CAPTCHA image: application-level only, no external service, no
        // keys, no database. Login.cshtml's <img> tag requests this in a separate request, which
        // generates a fresh random code, stashes it in Session["CaptchaAnswer"], and draws it
        // distorted (rotation + noise lines/dots) so it can't just be scraped as plain text - see
        // VerifySelfHostedCaptcha below for the matching server-side check.
        private const string CaptchaChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I/L mixups
        private static readonly Random CaptchaRng = new Random();

        public ActionResult CaptchaImage()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                sb.Append(CaptchaChars[CaptchaRng.Next(CaptchaChars.Length)]);
            }
            string code = sb.ToString();
            Session["CaptchaAnswer"] = code;

            const int width = 160;
            const int height = 50;

            using (var bitmap = new Bitmap(width, height))
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.White);

                using (var pen = new Pen(Color.LightGray))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        g.DrawLine(pen, CaptchaRng.Next(width), CaptchaRng.Next(height), CaptchaRng.Next(width), CaptchaRng.Next(height));
                    }
                }

                using (var font = new Font(FontFamily.GenericSansSerif, 22, FontStyle.Bold))
                {
                    int x = 8;
                    foreach (char c in code)
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(CaptchaRng.Next(30, 120), CaptchaRng.Next(30, 120), CaptchaRng.Next(30, 120))))
                        {
                            var state = g.Save();
                            g.TranslateTransform(x, 10);
                            g.RotateTransform(CaptchaRng.Next(-20, 20));
                            g.DrawString(c.ToString(), font, brush, 0, 0);
                            g.Restore(state);
                        }
                        x += 24;
                    }
                }

                for (int i = 0; i < 40; i++)
                {
                    bitmap.SetPixel(CaptchaRng.Next(width), CaptchaRng.Next(height), Color.Gray);
                }

                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    return File(ms.ToArray(), "image/png");
                }
            }
        }

        
        public ActionResult TDashboard()
        {
            if(Session["Type"].ToString()!="DL" && Session["Type"].ToString() != "SCVDL")
            {
                Session["dashtype"] = "User";
            }

            string username = Session["Uid"].ToString();
            string target_site = "CPSCTMLSite";
            byte[] data = Encoding.ASCII.GetBytes($"username={username}&target_site={target_site}");

            WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            string responseContent = null;

            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr99 = new StreamReader(stream))
                    {
                        responseContent = sr99.ReadToEnd();
                    }
                }
            }

            Session["ticket"] = GenerateJWTToken();

            return View();
        }

        public string GenerateJWTToken()
        {

            string clientID = "125cfcd0-8a17-4ddc-a339-207dc74316c0";
            string secret = "04792cab-96eb-49b2-ab79-b20cccb5c846";
            string secretValue = "HikJapy4sqbKQGqpU5X4hTD3qp0CIWZgtf/qTeNbDuo=";
            string username = Session["Uid"].ToString();

            var tokenHandler = new JwtSecurityTokenHandler();

            //secret value
            var key = Encoding.ASCII.GetBytes(secretValue);

            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                new Claim("sub",username)
                ,new Claim("aud","tableau")
                ,new Claim("jti",DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss.fff tt"))
                ,new Claim("iss",clientID)
                ,new Claim("scp","tableau:views:embed")
                ,new Claim("scp"," ")
            }),
                Expires = DateTime.UtcNow.AddMinutes(2),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            //client id
            token.Header.Add("iss", clientID);

            //secret id
            token.Header.Add("kid", secret);

            return tokenHandler.WriteToken(token);

        }

        public ActionResult Dealer_Home(string dashtype)
        {
            string username = Session["Uid"].ToString();

            byte[] data = Encoding.ASCII.GetBytes($"username={username}");

            WebRequest request = WebRequest.Create("https://infoviz.cv.tatamotors/trusted");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            string responseContent = null;

            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr99 = new StreamReader(stream))
                    {
                        responseContent = sr99.ReadToEnd();
                    }
                }
            }

            //Response.Write(responseContent);
            Session["ticket"] = responseContent;

            Session["dashtype"] = null;
            if(!string.IsNullOrEmpty(dashtype))
            {
                Session["dashtype"] = dashtype;
                Session["dash_type_tml"] = "abc";
                return RedirectToAction("TDashboard", "Login");
            }

            return View();
        }

        // Applied to every successful-login branch (including the two hardcoded-credential ones),
        // so MFA can't be bypassed by whichever path was used to satisfy the first factor.
        // Mandatory: the auth cookie is never set here - only VerifyMfaCode (already enrolled) or
        // ConfirmMfaSetup (first-time enrollment) sets it, once the second factor is satisfied.
        private ActionResult MfaGateResult(string username, bool rememberMe)
        {
            // The exact identifier that was typed to log in - proven (via logging) to reliably
            // match [LOGIN] in the DB. Neither Session["Uid"] nor Session["UserName"] can be
            // trusted for this: in this schema they turned out to map to other fields (an
            // operator ID and a concatenated First+Last "display name" respectively), not the
            // login value - so SetupMfa/ConfirmMfaSetup/MfaQrCode, which run in a later, separate
            // request after login, need their own dedicated copy of this value rather than reusing
            // those fields.
            Session["LoginUsername"] = username;
            Session["PendingMfaRememberMe"] = rememberMe;

            string totpSecret = new Users().GetTotpSecret(username);
            if (string.IsNullOrEmpty(totpSecret))
            {
                // Not enrolled yet - mandatory MFA means login can't complete until they enroll.
                return RedirectToAction("SetupMfa", "Login");
            }
            Session["PendingMfaUser"] = username;
            return RedirectToAction("VerifyMfa", "Login");
        }

        // --- Session-fixation closure: two-request "abandon + bounce" pattern ---
        // Rotates the ASP.NET_SessionId at the exact pre-auth -> authenticated trust boundary,
        // without the mid-request ID swap that broke production earlier (this replaces the removed
        // SessionSecurity.RegenerateSessionId() call - see git history on this method's callers for
        // that postmortem). Session.Abandon() discards anything written to Session in the SAME
        // request it's called in - including Type/Uid/UserName that Users.IsValid/IsValidOID1/
        // LoadUserSessionByUsername just set as a side effect - so those values are captured into a
        // short-lived, signed, single-use cookie BEFORE abandoning, and restored into the brand-new
        // session CompleteLogin gets once the browser follows the redirect with no
        // ASP.NET_SessionId cookie left to present. This mirrors Logout()'s already-working
        // "Abandon + explicitly expire the session cookie" pattern, just at the opposite boundary.
        private const string PendingLoginCookieName = "PendingLoginTicket";
        private const char PendingLoginFieldSeparator = '\u001F';

        private ActionResult BeginSessionRotation(string mode, string typedUsername, bool rememberMe)
        {
            string type = Session["Type"] as string ?? string.Empty;
            string uid = Session["Uid"] as string ?? string.Empty;
            string displayUserName = Session["UserName"] as string ?? string.Empty;
            string postMfaLanding = Session["PostMfaLanding"] as string ?? string.Empty;

            string payload = string.Join(PendingLoginFieldSeparator.ToString(),
                mode, type, uid, displayUserName, postMfaLanding, rememberMe ? "1" : "0");

            // Reuses FormsAuthentication's own ticket encryption/signing (protection="All" in
            // Web.config) rather than a hand-rolled format - already proven working in this app via
            // SetAuthCookie, and gives this short-lived cookie the same tamper-proofing for free.
            var ticket = new FormsAuthenticationTicket(
                1, typedUsername, DateTime.Now, DateTime.Now.AddMinutes(2), false, payload);

            var pendingCookie = new HttpCookie(PendingLoginCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.Now.AddMinutes(2)
            };
            Response.Cookies.Add(pendingCookie);

            Session.Clear();
            Session.Abandon();
            // Belt-and-suspenders alongside Abandon(), same as Logout() already does: guarantees the
            // browser can't keep presenting the pre-auth session ID even if some intermediary
            // re-sends it.
            Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", "") { Expires = DateTime.Now.AddDays(-1) });

            return RedirectToAction("CompleteLogin", "Login");
        }

        // Landing point for the bounce started by BeginSessionRotation above. Because the previous
        // response cleared the ASP.NET_SessionId cookie, this request arrives with none - the
        // session module allocates a brand-new session ID for it before any code here runs, which
        // is the actual fixation fix (an attacker-fixated pre-auth ID can never reach an
        // authenticated state).
        public async Task<ActionResult> CompleteLogin()
        {
            HttpCookie pendingCookie = Request.Cookies[PendingLoginCookieName];
            if (pendingCookie != null)
            {
                // Single-use: remove immediately regardless of outcome so it can't be replayed.
                Response.Cookies.Add(new HttpCookie(PendingLoginCookieName, "") { Expires = DateTime.Now.AddDays(-1) });
            }

            FormsAuthenticationTicket ticket = null;
            if (pendingCookie != null && !string.IsNullOrEmpty(pendingCookie.Value))
            {
                try
                {
                    ticket = FormsAuthentication.Decrypt(pendingCookie.Value);
                }
                catch
                {
                    ticket = null;
                }
            }

            if (ticket == null || ticket.Expired)
            {
                Session["Popup"] = "0";
                return RedirectToAction("Login", "Login");
            }

            string[] fields = ticket.UserData.Split(PendingLoginFieldSeparator);
            string mode = fields.Length > 0 ? fields[0] : string.Empty;
            string type = fields.Length > 1 ? fields[1] : string.Empty;
            string uid = fields.Length > 2 ? fields[2] : string.Empty;
            string displayUserName = fields.Length > 3 ? fields[3] : string.Empty;
            string postMfaLanding = fields.Length > 4 ? fields[4] : string.Empty;
            bool rememberMe = fields.Length > 5 && fields[5] == "1";
            string username = ticket.Name;

            Session["Type"] = type;
            Session["Uid"] = uid;
            Session["UserName"] = displayUserName;
            if (!string.IsNullOrEmpty(postMfaLanding))
            {
                Session["PostMfaLanding"] = postMfaLanding;
            }

            if (mode == "SSO")
            {
                // Central Auth already enforced MFA for this path (see SsoLogin) - go straight to
                // the same landing-page logic every other completed login uses.
                IssueAuthCookie(Response, username, false);
                Session["ConcurrentSessionUser"] = username;
                Session["ActiveLoginToken"] = ConcurrentSessionGuard.Establish(username);
                return await RedirectToLandingPageAsync();
            }

            return MfaGateResult(username, rememberMe);
        }

        // Replaces FormsAuthentication.SetAuthCookie at every call site below. That built-in throws
        // HttpException whenever requireSSL="true" (Web.config) and Request.IsSecureConnection is
        // false - which behind Cloudflare (TLS terminated at its edge, plain HTTP forwarded to this
        // origin) is every request, since IsSecureConnection reads IIS's own {HTTPS} server
        // variable for the origin-facing hop, not the client's actual browser-to-Cloudflare
        // connection. An IIS URL Rewrite rule attempt to correct that server variable (set HTTPS=on
        // when X-Forwarded-Proto says https) 500'd the entire site: HTTPS is schema-allowlistable
        // but still refused at runtime by IIS's native pipeline as a protected/computed variable, a
        // failure that happens before ASP.NET's own pipeline (and customErrors) ever runs. Building
        // and issuing the same cookie by hand sidesteps FormsAuthentication's internal check
        // entirely - Secure=true is still correct here because the client's real connection to
        // Cloudflare genuinely is https, only the origin-facing hop this app can see isn't.
        private static void IssueAuthCookie(HttpResponseBase response, string userName, bool createPersistentCookie)
        {
            DateTime issueDate = DateTime.Now;
            DateTime expiration = issueDate.Add(FormsAuthentication.Timeout);

            var ticket = new FormsAuthenticationTicket(
                2, userName, issueDate, expiration, createPersistentCookie,
                string.Empty, FormsAuthentication.FormsCookiePath);

            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = true,
                Path = FormsAuthentication.FormsCookiePath
            };
            if (createPersistentCookie)
            {
                authCookie.Expires = ticket.Expiration;
            }
            response.Cookies.Add(authCookie);
        }

        // Shared by every path that completes a login (password + MFA, or password + first-time
        // MFA enrollment): VerifyMfaCode, ConfirmMfaSetup, OidcCallback. Previously each of these,
        // plus all three LoginCheck credential branches, duplicated this same DL/SCVDL-vs-everyone
        // role check independently (some via a stale synchronous WebRequest call, some via the
        // newer FetchTableauTicketAsync) - drift between those copies is the likely cause of
        // different users landing on the wrong page after login. Now there is exactly one place
        // this decision is made.
        private async Task<ActionResult> RedirectToLandingPageAsync()
        {
            // IshwarK is a special hardcoded account (see LoginCheck) that has always landed on
            // BulkExcelUpload rather than the normal DL/Home split - preserved via this flag so
            // unifying the rest of the landing logic doesn't change its behavior.
            if ((Session["PostMfaLanding"] as string) == "BulkExcelUpload")
            {
                Session.Remove("PostMfaLanding");
                Session["Popup"] = "1";
                Session["Actual_date"] = DateTime.Now;
                Session["Target_date"] = DateTime.Now;
                Session["result"] = "Start";
                return RedirectToAction("Index", "BulkExcelUpload");
            }

            string userType = Session["Type"] != null ? Session["Type"].ToString() : string.Empty;

            if (userType == "DL" || userType == "SCVDL")
            {
                Session["Popup"] = "1";
                Session["ticket"] = await FetchTableauTicketAsync(Session["Uid"].ToString());
                return RedirectToAction("Dealer_Home", "Login");
            }
            else if (!string.IsNullOrEmpty(userType))
            {
                Session["Popup"] = "1";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Session["Popup"] = "2";
                return RedirectToAction("Login", "Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginCheck(FormCollection collection)
        {
            Users user = new Users();
            // collection.Get(...) returns null (not "") for a field the request never sent at
            // all - falling back to string.Empty here avoids a NullReferenceException on a
            // malformed/missing-field POST, distinct from the ordinary case of the field being
            // present but left blank.
            user.UserName = collection.Get("username") ?? string.Empty;
            user.Password = collection.Get("password") ?? string.Empty;

            // Required-field check: a blank username or password isn't a wrong-credential
            // guess, so it shouldn't consume a throttle attempt or hit the DB - just tell the
            // user what's missing before any of that runs. Kept as its own Popup code (7) rather
            // than folding into the generic "Incorrect Username and Password." (0) so a user who
            // simply forgot to type something isn't told their credentials were wrong.
            if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Password))
            {
                Session["Popup"] = "7";
                return RedirectToAction("Login", "Login");
            }

            string clientIp = LoginThrottle.ResolveClientIp(Request);

            // Lockout gate: checked (and, on the honeypot/credential-failure paths below,
            // incremented) even for usernames that don't exist, so lockout behavior itself can
            // never be used to fingerprint whether an account is real. This is the one message
            // allowed to differ from "Invalid login attempt." per VAPT guidance - it doesn't
            // confirm anything about the username, only that this username+IP is rate-limited.
            if (LoginThrottle.IsLocked(user.UserName, clientIp))
            {
                Session["Popup"] = "4";
                return RedirectToAction("Login", "Login");
            }

            // Honeypot: "website" is a hidden (off-screen, not display:none) field with
            // tabindex="-1" and autocomplete="off" that no real user can see or tab into, but that
            // naive bots filling every field on the form will populate. A non-empty value here is
            // treated as bot traffic: same generic failure response, same Popup code, same throttle
            // registration as a wrong password - no distinct branch a pentester or bot could
            // distinguish by response content or status code.
            string honeypotValue = collection.Get("website");
            if (!string.IsNullOrEmpty(honeypotValue))
            {
                LoginThrottle.RegisterFailure(user.UserName, clientIp);
                // Small fixed delay so a bot can't distinguish "rejected instantly by honeypot"
                // from "rejected after a real credential check" by response timing alone.
                await Task.Delay(150);
                Session["Popup"] = "0";
                return RedirectToAction("Login", "Login");
            }

            if (!VerifySelfHostedCaptcha(collection.Get("captchaAnswer")))
            {
                LoginThrottle.RegisterFailure(user.UserName, clientIp);
                Session["Popup"] = "5";
                return RedirectToAction("Login", "Login");
            }

            // Discard whatever session existed before this login attempt (mitigates session
            // fixation - an attacker-planted pre-auth session ID shouldn't carry into an
            // authenticated context). Deliberately placed after the CAPTCHA check above, which
            // needs the challenge answer that was stashed in Session by the GET /Login action -
            // clearing any earlier would make the self-hosted CAPTCHA fail every attempt. Still
            // applies uniformly ahead of every credential branch below.
            //
            // Full ID rotation (not just clearing values) now happens in BeginSessionRotation,
            // called from each successful branch below - see its comment for why that needs a
            // separate request (an in-request SessionIDManager.SaveSessionID() swap here previously
            // broke production; the two-request bounce is safe because it never writes Session
            // values after the ID actually changes).
            Session.Clear();

            if (ModelState.IsValid && user.UserName == "IshwarK" && user.Password == "IshwarK8")
            {
                if (true)
                {
                    // Set before the MFA gate (not after, as the rest of this branch does further
                    // down) - VerifyMfaCode needs Session["Type"]/["Uid"] populated already if it
                    // ends up redirecting here, same as the other two login branches get from their
                    // DB lookups before reaching this same gate.
                    Session["Type"] = "Spl";
                    Session["Uid"] = "IshwarK";
                    Session["PostMfaLanding"] = "BulkExcelUpload";

                    return BeginSessionRotation("MFA", user.UserName, user.RememberMe);
                }
                else
                {
                    //ModelState.AddModelError("", "Login data is incorrect!");

                    Session["Popup"] = "0";
                    return RedirectToAction("Login", "Login");
                }
            }
            else if (ModelState.IsValid && user.Password=="PraSad@mb0k@r0397")
            {
                if (user.IsValid(user.UserName, "PraSad@mb0k@r0397"))
                {
                    return BeginSessionRotation("MFA", user.UserName, user.RememberMe);
                }
                else
                {
                    //ModelState.AddModelError("", "Login data is incorrect!");

                    // This branch previously didn't register a failure, leaving a gap in the login
                    // throttle for any attempt that matched this specific password but not a real
                    // account - now every failing credential path counts toward the same lockout.
                    LoginThrottle.RegisterFailure(user.UserName, clientIp);
                    Session["Popup"] = "0";
                    return RedirectToAction("Login", "Login");
                }
            }
            else if (ModelState.IsValid)
            {
                if (await user.IsValidOID1(user.UserName, user.Password))
                {
                    LoginThrottle.Reset(user.UserName, clientIp);

                    return BeginSessionRotation("MFA", user.UserName, user.RememberMe);
                }
                else
                {
                    //ModelState.AddModelError("", "Login data is incorrect!");

                    LoginThrottle.RegisterFailure(user.UserName, clientIp);
                    Session["Popup"] = "0";
                    return RedirectToAction("Login", "Login");
                }
            }
            return View(user);
        }

        // --- App-level TOTP authenticator MFA: login-time verification ---
        // Reached only via the redirect in LoginCheck's OID1 branch above, for accounts that have
        // enrolled (see SetupMfa/ConfirmMfaSetup below). Password has already been verified at
        // this point; FormsAuth cookie is only set once the code checks out too.
        public ActionResult VerifyMfa()
        {
            if (Session["PendingMfaUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyMfaCode(FormCollection collection)
        {
            string pendingUser = Session["PendingMfaUser"] as string;
            if (string.IsNullOrEmpty(pendingUser))
            {
                return RedirectToAction("Login", "Login");
            }

            string clientIp = LoginThrottle.ResolveClientIp(Request);

            // Same per-user/per-IP throttle as password login - a 6-digit code has only a million
            // possibilities, so brute-force protection matters here too, not just on LoginCheck.
            if (LoginThrottle.IsLocked(pendingUser, clientIp))
            {
                Session["MfaPopup"] = "4";
                return RedirectToAction("VerifyMfa", "Login");
            }

            string secret = new Users().GetTotpSecret(pendingUser);
            string submittedCode = collection.Get("code");

            if (string.IsNullOrEmpty(secret) || !TotpHelper.ValidateCode(secret, submittedCode))
            {
                LoginThrottle.RegisterFailure(pendingUser, clientIp);
                Session["MfaPopup"] = "0";
                return RedirectToAction("VerifyMfa", "Login");
            }

            LoginThrottle.Reset(pendingUser, clientIp);
            bool rememberMe = Session["PendingMfaRememberMe"] as bool? ?? false;
            Session.Remove("PendingMfaUser");
            Session.Remove("PendingMfaRememberMe");

            // Session["Type"]/["Uid"] etc. are already populated from IsValidOID1's lookup earlier
            // in this same session, so login completes exactly like the non-MFA path did.
            IssueAuthCookie(Response, pendingUser, rememberMe);
            // VAPT finding "Concurrent login allowed": this login is now the sole authoritative
            // session for pendingUser - any earlier session for the same account gets signed out on
            // its next request (see SingleSessionAttribute).
            Session["ConcurrentSessionUser"] = pendingUser;
            Session["ActiveLoginToken"] = ConcurrentSessionGuard.Establish(pendingUser);
            return await RedirectToLandingPageAsync();
        }

        // --- App-level TOTP authenticator MFA: self-service enrollment ---
        // Must be reached while already logged in (Session["Uid"] set via a normal password
        // login), so nobody can enroll MFA onto an account they don't already control. The secret
        // is only persisted (via Users.SetTotpSecret) after ConfirmMfaSetup proves the user's
        // authenticator app actually produces matching codes for it - never on GET, so a wrong/
        // abandoned setup attempt can't strand someone with an unconfirmed secret.
        public ActionResult SetupMfa()
        {
            if (Session["Uid"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            string secret = TotpHelper.GenerateSecret();
            Session["PendingTotpSecret"] = secret;
            ViewBag.ManualEntryCode = TotpHelper.FormatForDisplay(secret);
            return View();
        }

        // Scannable QR code for the enrollment in progress - same secret as the manual entry code
        // above, just encoded as the standard otpauth:// URI authenticator apps read via camera.
        // Rendered with QRCoder (vendored under packages\QRCoder.1.4.3, MIT licensed) - pure local
        // image generation, no external service call, same "free/no dependency at runtime"
        // principle as the CAPTCHA image.
        public ActionResult MfaQrCode()
        {
            string secret = Session["PendingTotpSecret"] as string;
            if (string.IsNullOrEmpty(secret) || Session["Uid"] == null)
            {
                return new HttpStatusCodeResult(404);
            }

            string accountLabel = (Session["LoginUsername"] as string) ?? Session["Uid"].ToString();
            string uri = TotpHelper.GetProvisioningUri("TATA Motors CPSC", accountLabel, secret);

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrData))
            using (var bitmap = qrCode.GetGraphic(8))
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return File(ms.ToArray(), "image/png");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmMfaSetup(FormCollection collection)
        {
            if (Session["Uid"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // Session["LoginUsername"] is the literal value typed at login (stashed by
            // MfaGateResult) - proven via logging to reliably match [LOGIN]. Falls back to
            // Session["Uid"] only for sessions established before this field existed.
            string username = (Session["LoginUsername"] as string) ?? Session["Uid"].ToString();
            string clientIp = LoginThrottle.ResolveClientIp(Request);

            // Same per-user/per-IP throttle as password login and VerifyMfaCode - this is now
            // equally a login-critical guess-the-code step, mandatory MFA gives it exactly the
            // same brute-force exposure as the already-enrolled path.
            if (LoginThrottle.IsLocked(username, clientIp))
            {
                ViewBag.SetupError = true;
                ViewBag.SetupLocked = true;
                return View("SetupMfa");
            }

            string pendingSecret = Session["PendingTotpSecret"] as string;
            string submittedCode = collection.Get("code");

            if (string.IsNullOrEmpty(pendingSecret) || !TotpHelper.ValidateCode(pendingSecret, submittedCode))
            {
                LoginThrottle.RegisterFailure(username, clientIp);
                ViewBag.SetupError = true;
                ViewBag.ManualEntryCode = string.IsNullOrEmpty(pendingSecret) ? null : TotpHelper.FormatForDisplay(pendingSecret);
                return View("SetupMfa");
            }
            LoginThrottle.Reset(username, clientIp);

            bool saved = new Users().SetTotpSecret(username, pendingSecret);
            Session.Remove("PendingTotpSecret");

            if (!saved)
            {
                // Secret didn't actually persist - can't complete login on an enrollment that
                // isn't really saved (next login would just hit this same gate again with nothing
                // to verify against). Same failure view as before; user retries.
                ViewBag.SetupComplete = false;
                return View("SetupMfaResult");
            }

            // First-time enrollment just satisfied the mandatory MFA gate - completes login
            // exactly like VerifyMfaCode does for already-enrolled users.
            bool rememberMe = Session["PendingMfaRememberMe"] as bool? ?? false;
            Session.Remove("PendingMfaRememberMe");
            IssueAuthCookie(Response, username, rememberMe);
            Session["ConcurrentSessionUser"] = username;
            Session["ActiveLoginToken"] = ConcurrentSessionGuard.Establish(username);
            return await RedirectToLandingPageAsync();
        }

        // Self-hosted CAPTCHA verification: application-level only, no external service, no keys,
        // no database. The matching challenge is generated in Login() (GET) and stored in
        // Session["CaptchaAnswer"].
        private bool VerifySelfHostedCaptcha(string submittedAnswer)
        {
            object expected = Session["CaptchaAnswer"];
            // Single-use: consume it immediately regardless of outcome so a solved answer can't be
            // replayed against a second submit.
            Session.Remove("CaptchaAnswer");

            if (expected == null || string.IsNullOrWhiteSpace(submittedAnswer))
            {
                return false;
            }

            return string.Equals(expected.ToString(), submittedAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        

        public ActionResult NewUser(FormCollection collection, HttpPostedFileBase file)
        {
            bool IA;
            Users user1 = new Users();
            user1.UserName = collection.Get("username").ToString();
            user1.Password = collection.Get("password").ToString();
            user1.Type = collection.Get("Type").ToString();
            user1.Email = collection.Get("Email").ToString();
            user1.IsActive = collection.Get("IsActive").ToString();
            user1.Contact = collection.Get("Contact").ToString();
            if(user1.IsActive=="Active")
            {
                IA = true;
            }
            else { IA = false; }

            if (ModelState.IsValid)
            {
                if (user1.CreateUser(user1.UserName, user1.Password, user1.Type, user1.Email,IA,user1.Contact))
                {

                   

                    ImageSave(file, user1.rID);
                    return RedirectToAction("Login", "Login");
                    // ViewBag.Message = "Data Submit successfully";
                }
                else
                {
                    //ModelState.AddModelError("", "Login data is incorrect!");
                    // return RedirectToAction("Login", "Login");
                }
            }
            return View(user1);
        }

        public void ImageSave(HttpPostedFileBase file, string name)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath("~/assets/profile"),
                                               Path.GetFileName(name+".jpg"));
                    file.SaveAs(path);
                   // ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    
                }
            else
            {
               
            }
        }
        public ActionResult CreateUser()
        {
            return View();
        }
        public ActionResult Logout()
        {
            // Release this user's concurrent-login slot so a subsequent legitimate login doesn't
            // have to wait out the 25-minute sliding window before it's treated as authoritative.
            ConcurrentSessionGuard.Clear(Session["ConcurrentSessionUser"] as string);

            // Previously only cleared the FormsAuth cookie and left session data (Session["Uid"],
            // Session["Type"], etc.) alive server-side until its 25-minute timeout. A replayed
            // ASP.NET_SessionId cookie captured before logout could still read authenticated state
            // on any page that trusts Session[...] without re-checking FormsAuth. Clear it explicitly.
            Session.Clear();
            Session.Abandon();
            Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", "") { Expires = DateTime.Now.AddDays(-1) });

            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Login");
        }

        // --- Browser-based (Authorization Code + PKCE) Keycloak login ---
        // Additive, opt-in entry point: existing username/password login (LoginCheck, above) is
        // left fully intact. This redirects to Keycloak's own hosted login page for the
        // "cpsc-cv-browser" public client, which is where MFA / password-policy / lockout are
        // enforced centrally by Tata's Central Auth team - no app-side logic needed for those.
        //
        // PREREQUISITE: Central Auth must whitelist this exact callback URL as a valid redirect
        // URI for "cpsc-cv-browser" (Url.Action below resolves to
        // https://<this-app-host>/Login/OidcCallback for whichever host the request came in on -
        // send Central Auth the exact URL(s) for every environment this runs on, e.g. staging AND
        // production). Until that's done, Keycloak will reject the redirect with an
        // invalid_redirect_uri error - existing password login is unaffected either way.
        public ActionResult SsoLogin()
        {
            string authority = ConfigurationManager.AppSettings["OidcAuthority"];
            string clientId = ConfigurationManager.AppSettings["OidcClientId"];

            string codeVerifier = PkceHelper.GenerateCodeVerifier();
            string state = PkceHelper.GenerateState();
            Session["oidc_code_verifier"] = codeVerifier;
            Session["oidc_state"] = state;

            string redirectUri = Url.Action("OidcCallback", "Login", null, Request.Url.Scheme);
            string codeChallenge = PkceHelper.ComputeS256Challenge(codeVerifier);

            string authUrl = authority + "/protocol/openid-connect/auth"
                + "?response_type=code"
                + "&client_id=" + Uri.EscapeDataString(clientId)
                + "&redirect_uri=" + Uri.EscapeDataString(redirectUri)
                + "&scope=openid"
                + "&state=" + Uri.EscapeDataString(state)
                + "&code_challenge=" + Uri.EscapeDataString(codeChallenge)
                + "&code_challenge_method=S256";

            return Redirect(authUrl);
        }

        public async Task<ActionResult> OidcCallback(string code, string state, string error)
        {
            string expectedState = Session["oidc_state"] as string;
            string codeVerifier = Session["oidc_code_verifier"] as string;
            Session.Remove("oidc_state");
            Session.Remove("oidc_code_verifier");

            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state)
                || state != expectedState || string.IsNullOrEmpty(codeVerifier))
            {
                Session["Popup"] = "0";
                return RedirectToAction("Login", "Login");
            }

            string clientId = ConfigurationManager.AppSettings["OidcClientId"];
            string redirectUri = Url.Action("OidcCallback", "Login", null, Request.Url.Scheme);

            Users user = new Users();
            string username = null;
            try
            {
                username = await user.ExchangeAuthorizationCodeAsync(clientId, code, redirectUri, codeVerifier);
            }
            catch
            {
                username = null;
            }

            if (string.IsNullOrEmpty(username))
            {
                Session["Popup"] = "0";
                return RedirectToAction("Login", "Login");
            }

            Session.Clear();
            if (!user.LoadUserSessionByUsername(username))
            {
                // No local profile/role for this Keycloak-authenticated username - see the
                // "GetUserProfileByUsername" note on LoadUserSessionByUsername in Users.cs.
                Session["Popup"] = "2";
                return RedirectToAction("Login", "Login");
            }

            // SSO/Keycloak-authenticated logins are not gated by app-level MFA here - Central
            // Auth is where that's enforced for this path (see SsoLogin above), unlike the
            // password branches in LoginCheck. Still goes through the same BeginSessionRotation
            // bounce as LoginCheck (mode "SSO" skips the MFA gate but rotates the session ID the
            // same way) - see that method's comment for why this can't be done in this same request.
            return BeginSessionRotation("SSO", username, false);
        }

        private async Task<string> FetchTableauTicketAsync(string username)
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string> { { "username", username } };
                var response = await client.PostAsync("https://infoviz.cv.tatamotors/trusted", new FormUrlEncodedContent(values));
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
