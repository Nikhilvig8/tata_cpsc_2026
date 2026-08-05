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
    [HandleError()]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginCheck(FormCollection collection)
        {
            string UserType = string.Empty;
            Users user = new Users();
            user.UserName = collection.Get("username").ToString();
            user.Password = collection.Get("password").ToString();

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
            Session.Clear();

            if (ModelState.IsValid && user.UserName == "IshwarK" && user.Password == "IshwarK8")
            {
                if (true)
                {
                    FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
                    UserType = "Spl";
                    if (UserType != "")
                    {
                        if (UserType == "DL" || UserType=="SCVDL")
                        {
                            Session["Popup"] = "1";
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


                            return RedirectToAction("Dealer_Home", "Login");

                        }
                        else
                        {
                            Session["Popup"] = "1";
                            Session["Type"] = "Spl";
                            Session["Uid"] = "IshwarK";
                            Session["Actual_date"] = DateTime.Now;
                            Session["Target_date"] = DateTime.Now;
                            Session["result"] = "Start";
                            return RedirectToAction("Index", "BulkExcelUpload");
                        }
                    }
                    else
                    {
                        Session["Popup"] = "2";
                        return RedirectToAction("Login", "Login");
                    }
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
                    FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
                    UserType = Session["Type"].ToString();
                    if (UserType != "")
                    {
                        if (UserType == "DL" || UserType == "SCVDL")
                        {
                            Session["Popup"] = "1";
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
                           

                            return RedirectToAction("Dealer_Home", "Login");

                        }
                        else
                        {
                            Session["Popup"] = "1";


                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        Session["Popup"] = "2";
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    //ModelState.AddModelError("", "Login data is incorrect!");

                    Session["Popup"] = "0";
                    return RedirectToAction("Login", "Login");
                }
            }
            else if (ModelState.IsValid)
            {
                if (await user.IsValidOID1(user.UserName, user.Password))
                {
                    LoginThrottle.Reset(user.UserName, clientIp);

                    // App-level TOTP MFA gate: only applies to accounts that have explicitly
                    // enrolled (TotpSecret set - see Users.GetTotpSecret). Password is correct at
                    // this point, but the FormsAuth cookie is deliberately NOT set yet - login only
                    // completes once VerifyMfaCode confirms the authenticator code too.
                    string totpSecret = user.GetTotpSecret(user.UserName);
                    if (!string.IsNullOrEmpty(totpSecret))
                    {
                        Session["PendingMfaUser"] = user.UserName;
                        Session["PendingMfaRememberMe"] = user.RememberMe;
                        return RedirectToAction("VerifyMfa", "Login");
                    }

                    FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
                    UserType = Session["Type"].ToString();
                    if (UserType != "")
                    {
                        if (UserType == "DL" || UserType == "SCVDL")
                        {
                            Session["Popup"] = "1";
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


                            return RedirectToAction("Dealer_Home", "Login");

                        }
                        else
                        {
                            Session["Popup"] = "1";


                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        Session["Popup"] = "2";
                        return RedirectToAction("Login", "Login");
                    }
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
            FormsAuthentication.SetAuthCookie(pendingUser, rememberMe);
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

            string accountLabel = (Session["UserName"] as string) ?? Session["Uid"].ToString();
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
        public ActionResult ConfirmMfaSetup(FormCollection collection)
        {
            if (Session["Uid"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            string pendingSecret = Session["PendingTotpSecret"] as string;
            string submittedCode = collection.Get("code");

            if (string.IsNullOrEmpty(pendingSecret) || !TotpHelper.ValidateCode(pendingSecret, submittedCode))
            {
                ViewBag.SetupError = true;
                ViewBag.ManualEntryCode = string.IsNullOrEmpty(pendingSecret) ? null : TotpHelper.FormatForDisplay(pendingSecret);
                return View("SetupMfa");
            }

            // Must match the identifier LoginCheck's MFA gate looks the secret up with
            // (user.UserName, the literal username typed at login - i.e. the "Username" DB
            // column, not "Uid"). Session["UserName"] is populated from that same column
            // wherever Session["Uid"] is, so this is a like-for-like swap, not a new field.
            string username = (Session["UserName"] as string) ?? Session["Uid"].ToString();
            bool saved = new Users().SetTotpSecret(username, pendingSecret);
            Session.Remove("PendingTotpSecret");

            ViewBag.SetupComplete = saved;
            return View("SetupMfaResult");
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

            FormsAuthentication.SetAuthCookie(username, false);
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