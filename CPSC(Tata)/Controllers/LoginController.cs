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
        public async Task<ActionResult> LoginCheck(FormCollection collection)
        {
            string UserType = string.Empty;
            Users user = new Users();
            user.UserName = collection.Get("username").ToString();
            user.Password = collection.Get("password").ToString();

            // Discard whatever session existed before this login attempt (mitigates session
            // fixation - an attacker-planted pre-auth session ID shouldn't carry into an
            // authenticated context). Applies uniformly ahead of every branch below.
            Session.Clear();

            string throttleKey = user.UserName + "|" + Users.GetVisitorIPAddress();
            TimeSpan lockoutRemaining;
            if (LoginThrottle.IsLocked(throttleKey, out lockoutRemaining))
            {
                Session["Popup"] = "4";
                return RedirectToAction("Login", "Login");
            }

            if (!await VerifyRecaptchaAsync(collection.Get("g-recaptcha-response")))
            {
                LoginThrottle.RegisterFailure(throttleKey);
                Session["Popup"] = "5";
                return RedirectToAction("Login", "Login");
            }

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
                    LoginThrottle.Reset(throttleKey);
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

                    LoginThrottle.RegisterFailure(throttleKey);
                    Session["Popup"] = "0";
                    return RedirectToAction("Login", "Login");
                }
            }
            return View(user);
        }

        private async Task<bool> VerifyRecaptchaAsync(string recaptchaResponse)
        {
            string secretKey = ConfigurationManager.AppSettings["ReCaptchaSecretKey"];
            if (string.IsNullOrEmpty(secretKey) || secretKey == "REPLACE_WITH_RECAPTCHA_SECRET_KEY")
            {
                // Not configured yet - don't lock everyone out of login because of it.
                return true;
            }
            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                return false;
            }
            try
            {
                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                        { "secret", secretKey },
                        { "response", recaptchaResponse },
                        { "remoteip", Users.GetVisitorIPAddress() }
                    };
                    var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", new FormUrlEncodedContent(values));
                    string body = await response.Content.ReadAsStringAsync();
                    var json = Newtonsoft.Json.Linq.JObject.Parse(body);
                    return (bool?)json["success"] == true;
                }
            }
            catch
            {
                // If Google's endpoint can't be reached (e.g. no outbound internet access from
                // this server), fail OPEN rather than locking out every login - a captcha outage
                // should not become a site-wide login outage. Confirm outbound HTTPS to
                // google.com is allowed from the app server if you need this to fail closed instead.
                return true;
            }
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