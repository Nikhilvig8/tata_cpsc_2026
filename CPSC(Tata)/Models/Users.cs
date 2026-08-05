using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using DataUtilityLayer;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Xml;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Remoting.Contexts;
using System.Net.Http;
using System.Threading.Tasks;

namespace InputOutput.Models
{
    public class Users : DataUtilityLayer.DataUtility
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _tokenUrl;

        string conn = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        SqlCommand cmdObj;
        SqlParameter prmObj;
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember on this computer")]
        public bool RememberMe { get; set; }
        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "IsActive")]
        public string IsActive { get; set; }

        [Display(Name = "Contact")]
        public string Contact { get; set; }

        [Display(Name = "rID")]
        public string rID { get; set; }



        /// <summary>
        /// Checks if user with given password exists in the database
        /// </summary>
        /// <param name="_username">User name</param>
        /// <param name="_password">User password</param>
        /// <returns>True if user exist and password is correct</returns>
        /// bool valid = false;

 

public bool IsValidAD(string _username, string _password)
{
            bool valid = false;

            using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
            {
                valid = context.ValidateCredentials(_username, _password,ContextOptions.Negotiate);
                if (valid == true)
                {
                    using (var cn = new SqlConnection(conn))
                    {
                        string _sql = @"SELECT [username] as Uid ,[UserType],[FST_NAME]+[LAST_NAME] as Username FROM [dbo].[vw_Users] " +
                               @"WHERE [username] = @u AND [Password] = @p";
                        var cmd = new SqlCommand(_sql, cn);
                        cmd.Parameters
                            .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                            .Value = _username;
                        cmd.Parameters
                            .Add(new SqlParameter("@p", SqlDbType.NVarChar))
                            .Value = _password;
                        cn.Open();
                        var reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Session["Type"] = reader["Type"].ToString();
                                Session["UserName"] = reader["Username"].ToString();
                                Session["Uid"] = reader["Uid"].ToString();
                            }
                            reader.Dispose();
                            cmd.Dispose();
                        }
                    }
                }
            }
            return valid;
        }
public new  bool IsValid(string _username, string _password)
        {
            using (var cn = new SqlConnection(conn))
            {
                string _sql = @"ValidateUser @u, @p";
                var cmd = new SqlCommand(_sql, cn);
                cmd.Parameters
                    .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                    .Value = _username;
                cmd.Parameters
                    .Add(new SqlParameter("@p", SqlDbType.NVarChar))
                    .Value = _password;
                cn.Open();
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while(reader.Read())
                    { 
                    Session["Type"] = reader["Type"].ToString();
                    Session["UserName"]= reader["Username"].ToString();
                    Session["Uid"] = reader["Uid"].ToString();
                    Session["Target_Date"] = Target_Date();
                    Session["Actual_Date"] = Actual_Date();
                    }
                    reader.Dispose();
                    cmd.Dispose();
                    return true;
                }
                else
                {
                    reader.Dispose();
                    cmd.Dispose();
                    return false;
                }
            }
        }
        public string Actual_Date()
        {
            using (var connection = new SqlConnection(conn))
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "GetActualMonth";
                command.Parameters
                    .Add(new SqlParameter("@DATE", SqlDbType.Date))
                    .Value = DateTime.Today;

                SqlParameter returnValue = command.Parameters.Add("@Date", SqlDbType.Date);
                returnValue.Direction = ParameterDirection.ReturnValue;

                connection.Open();
                command.ExecuteNonQuery();

                return returnValue.Value.ToString();
            }
        }

        public string Target_Date()
        {
            using (var connection = new SqlConnection(conn))
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "GetTargetMonth";
                command.Parameters
                    .Add(new SqlParameter("@DATE", SqlDbType.Date))
                    .Value = DateTime.Today;

                SqlParameter returnValue = command.Parameters.Add("@Date", SqlDbType.Date);
                returnValue.Direction = ParameterDirection.ReturnValue;

                connection.Open();
                command.ExecuteNonQuery();

                return returnValue.Value.ToString();
            }
        }

        public Users()
        {
            //_httpClient = httpClient;
            _tokenUrl = "https://sso.tatamotors.com/auth/realms/grahak/protocol/openid-connect/token";
        }

        public async Task<bool> IsValidOID1(string username, string password)
        {


            var requestBody = new Dictionary<string, string>
        {
            { "client_id", "cpsc-cv" },
            { "grant_type", "password" },
            { "username", username },
            { "password", password }
        };

            // Create content from the request body
            var content = new FormUrlEncodedContent(requestBody);

            // Make the POST request
            var response = await _httpClient.PostAsync(_tokenUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string IPAddress = string.Empty;
                DateTime start_time = DateTime.Now;
                DateTime end_time = DateTime.Now;

                IPAddress = GetVisitorIPAddress();
                TimeSpan span = end_time.Subtract(start_time);
                string res_time = span.Hours.ToString("00") + ":" + span.Minutes.ToString("00") + ":" + span.Seconds.ToString("00");

                OIDLogs(username, IPAddress, res_time);
                using (var cn = new SqlConnection(conn))
                {
                    string _sql = @"ValidateUser @u, @p";
                    var cmd = new SqlCommand(_sql, cn);
                    cmd.Parameters
                        .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                        .Value = username;
                    cmd.Parameters
                        .Add(new SqlParameter("@p", SqlDbType.NVarChar))
                        .Value = password;
                    cn.Open();
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Session["Type"] = reader["Type"].ToString();
                            Session["UserName"] = reader["Username"].ToString();
                            Session["Uid"] = reader["Uid"].ToString();
                            Session["Target_Date"] = Target_Date();
                            Session["Actual_Date"] = Actual_Date();
                        }
                        reader.Dispose();
                        cmd.Dispose();

                    }
                    else
                    {
                        Session["Type"] = string.Empty;

                        Session["UserName"] = string.Empty;
                        Session["Uid"] = string.Empty;
                        Session["Target_Date"] = Target_Date();
                        Session["Actual_Date"] = Actual_Date();
                        reader.Dispose();
                        cmd.Dispose();

                    }
                }

                return true; // Authentication succeeded
            }
            else
            {
                // Log the response content for debugging
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, Details: {responseContent}");
                return false;
            }
        }

        // --- Browser-based (Authorization Code + PKCE) Keycloak login support ---
        // Keycloak has already authenticated the user by the time we get here; the app never
        // sees their password. So unlike IsValid/IsValidOID1 (which call "ValidateUser @u,@p"),
        // this looks the user's role/profile up by username alone.
        //
        // REQUIRES a stored procedure "GetUserProfileByUsername" (single @Username parameter)
        // returning the same Type/Username/Uid columns as "ValidateUser" for a matching, active
        // local account. If no such proc exists yet, it (or an existing equivalent) needs to be
        // added before the SSO login path below will work end-to-end - this could not be verified
        // without database access.
        public bool LoadUserSessionByUsername(string username)
        {
            using (var cn = new SqlConnection(conn))
            {
                var cmd = new SqlCommand("GetUserProfileByUsername", cn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar)).Value = username;
                cn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Session["Type"] = reader["Type"].ToString();
                            Session["UserName"] = reader["Username"].ToString();
                            Session["Uid"] = reader["Uid"].ToString();
                            Session["Target_Date"] = Target_Date();
                            Session["Actual_Date"] = Actual_Date();
                        }
                        return true;
                    }
                    return false;
                }
            }
        }

        // Exchanges an authorization code for tokens at Keycloak's token endpoint (same endpoint
        // IsValidOID1 already uses for the password grant) and returns the "preferred_username"
        // claim from the resulting ID token, or null on any failure.
        public async Task<string> ExchangeAuthorizationCodeAsync(string clientId, string code, string redirectUri, string codeVerifier)
        {
            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", clientId },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "code_verifier", codeVerifier }
            };

            var content = new FormUrlEncodedContent(requestBody);
            var response = await _httpClient.PostAsync(_tokenUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string body = await response.Content.ReadAsStringAsync();
            var tokenResponse = Newtonsoft.Json.Linq.JObject.Parse(body);
            string idToken = (string)tokenResponse["id_token"];
            if (string.IsNullOrEmpty(idToken))
            {
                return null;
            }

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var parsed = handler.ReadJwtToken(idToken);
            var usernameClaim = parsed.Claims.FirstOrDefault(c => c.Type == "preferred_username");
            return usernameClaim != null ? usernameClaim.Value : null;
        }

        // --- App-level TOTP authenticator MFA support ---
        // REQUIRES two stored procedures ("GetTotpSecret", "SetTotpSecret") and a nullable
        // TotpSecret column on the users table - see the SQL provided alongside this change, since
        // this couldn't be created without database access. Null/missing secret = user hasn't
        // enrolled MFA, login proceeds exactly as before (opt-in, not required for existing users).
        public string GetTotpSecret(string username)
        {
            try
            {
                using (var cn = new SqlConnection(conn))
                {
                    var cmd = new SqlCommand("GetTotpSecret", cn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar)).Value = username;
                    cn.Open();
                    object result = cmd.ExecuteScalar();
                    bool found = result != null && result != DBNull.Value;
                    try
                    {
                        System.IO.File.AppendAllText(
                            System.Web.HttpContext.Current.Server.MapPath("~/Logs/Logs.txt"),
                            $"\r\n[TOTP] GetTotpSecret(username='{username}') -> {(found ? "found a value" : "no row / null value")} - {DateTime.Now}\r\n");
                    }
                    catch { /* logging must never break the login path */ }
                    return found ? result.ToString() : null;
                }
            }
            catch (Exception ex)
            {
                // Missing proc/column, or any DB error: treat as "not enrolled" rather than
                // blocking login entirely - fixing the DB side later just activates MFA for
                // whoever has since enrolled, with zero impact on everyone else in the meantime.
                try
                {
                    System.IO.File.AppendAllText(
                        System.Web.HttpContext.Current.Server.MapPath("~/Logs/Logs.txt"),
                        $"\r\n[TOTP] GetTotpSecret(username='{username}') threw: {ex.Message} - {DateTime.Now}\r\n");
                }
                catch { }
                return null;
            }
        }

        public bool SetTotpSecret(string username, string secret)
        {
            try
            {
                using (var cn = new SqlConnection(conn))
                {
                    var cmd = new SqlCommand("SetTotpSecret", cn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar)).Value = username;
                    cmd.Parameters.Add(new SqlParameter("@TotpSecret", SqlDbType.NVarChar)).Value = secret;
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public  bool IsValidOID(string _username, string _password)
        {
            if (_username.Contains("&") || _password.Contains("&") || _username.Contains("'") || _password.Contains("'") || _username.Contains("\"") || _password.Contains("\"") || _username.Contains("<") || _password.Contains("<") || _username.Contains(">") || _password.Contains(">"))
            {
                _username = _username.Replace("&", "&amp;").Replace("'", "&apos;").Replace("\"", "&quot;").Replace(">", "&gt;").Replace("<", "&lt;");
                _password = _password.Replace("&", "&amp;").Replace("'", "&apos;").Replace("\"", "&quot;").Replace(">", "&gt;").Replace("<", "&lt;");

            }

            DateTime start_time = DateTime.Now;
            string IPAddress = string.Empty;

            IPAddress = GetVisitorIPAddress();

            HttpWebRequest request = CreateWebRequest();
            XmlDocument soapEnvelopeXml = new XmlDocument();

            //Create envolap for soap request
            string envo = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <SOAP:Envelope xmlns:SOAP=""http://schemas.xmlsoap.org/soap/envelope/"">
                <SOAP:Body>
                <OIDAuthenticate xmlns = ""http://schemas.cordys.com/OIDAuthenticationTM"">
                <stringParam>" + _username + @"</stringParam>
               <stringParam1>" + _password + @"</stringParam1>
                </OIDAuthenticate>
                </SOAP:Body>
                </SOAP:Envelope>
                ";



            soapEnvelopeXml.LoadXml(envo);


            try
            {
                using (Stream stream = request.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        string soapResult = rd.ReadToEnd();
                        DateTime end_time = DateTime.Now;
                        TimeSpan span = end_time.Subtract(start_time);
                        string res_time = span.Hours.ToString("00") + ":" + span.Minutes.ToString("00") + ":" + span.Seconds.ToString("00");
                        OIDLogs(_username, IPAddress, res_time);
                        using (var cn = new SqlConnection(conn))
                        {
                            string _sql = @"ValidateUser @u, @p";
                            var cmd = new SqlCommand(_sql, cn);
                            cmd.Parameters
                                .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                                .Value = _username;
                            cmd.Parameters
                                .Add(new SqlParameter("@p", SqlDbType.NVarChar))
                                .Value = _password;
                            cn.Open();
                            var reader = cmd.ExecuteReader();
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Session["Type"] = reader["Type"].ToString();
                                    Session["UserName"] = reader["Username"].ToString();
                                    Session["Uid"] = reader["Uid"].ToString();
                                    Session["Target_Date"] = Target_Date();
                                    Session["Actual_Date"] = Actual_Date();
                                }
                                reader.Dispose();
                                cmd.Dispose();
                               
                            }
                            else
                            {
                                Session["Type"] = string.Empty;
                                
                                Session["UserName"] = string.Empty;
                                Session["Uid"] = string.Empty;
                                Session["Target_Date"] = Target_Date();
                                Session["Actual_Date"] = Actual_Date();
                                reader.Dispose();
                                cmd.Dispose();
                                
                            }
                        }
                        return true;

                        //Console.WriteLine(soapResult);
                        //Console.ReadLine();
                    }
                }
            }
            catch (WebException e)
            {
                DateTime end_time = DateTime.Now;
                TimeSpan span = end_time.Subtract(start_time);
                string res_time = span.Hours.ToString("00") + ":" + span.Minutes.ToString("00") + ":" + span.Seconds.ToString("00");
                OIDLogs(_username, IPAddress, res_time);
                return false;

            }
        }
        /// <summary>
        /// Create a soap webrequest to [Url]
        /// </summary>
        /// <returns></returns>
        public static HttpWebRequest CreateWebRequest()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"https://tmcrmportal.inservices.tatamotors.com/home/TataMotors/com.eibus.web.soap.Gateway.wcp?organization=o=TataMotors,cn=cordys,cn=cbop,o=tatamotors.com");
            //webRequest.UseDefaultCredentials = true;
            //webRequest.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            //webRequest.Credentials = new NetworkCredential("PBELWALKAR", "TML2016");
            webRequest.ClientCertificates.Add(new X509Certificate());
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }
        public bool CreateUser(string _username, string _password,string _Type,String _Email,bool _IsActive,String _Contact)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "CreateUser";

                cmdObj.Parameters
                    .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                    .Value = _username;
                cmdObj.Parameters
                    .Add(new SqlParameter("@p", SqlDbType.NVarChar))
                    .Value = _password;
            cmdObj.Parameters
                  .Add(new SqlParameter("@t", SqlDbType.NVarChar))
                  .Value = _Type;
            cmdObj.Parameters
                .Add(new SqlParameter("@e", SqlDbType.NVarChar))
                .Value = _Email;
            cmdObj.Parameters
              .Add(new SqlParameter("@i", SqlDbType.Bit))
              .Value = _IsActive;
            cmdObj.Parameters
              .Add(new SqlParameter("@c", SqlDbType.NVarChar))
              .Value = _Contact;
            SqlParameter outPutParameter = new SqlParameter("@rID",SqlDbType.NVarChar,100);
            outPutParameter.Direction = System.Data.ParameterDirection.Output;
            outPutParameter.Value = "";
            cmdObj.Parameters.Add(outPutParameter);
            if (ExecuteSqlProcedure(cmdObj))
            {
                rID= outPutParameter.Value.ToString();
                return true;
                    
           }
            else { return false; }
         
               
        }

        public static string GetVisitorIPAddress(bool GetLan = false)
        {
            string visitorIPAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (String.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            if (string.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = HttpContext.Current.Request.UserHostAddress;

            if (string.IsNullOrEmpty(visitorIPAddress) || visitorIPAddress.Trim() == "::1")
            {
                GetLan = true;
                visitorIPAddress = string.Empty;
            }

            if (GetLan)
            {
                if (string.IsNullOrEmpty(visitorIPAddress))
                {
                    //This is for Local(LAN) Connected ID Address
                    string stringHostName = Dns.GetHostName();
                    //Get Ip Host Entry
                    IPHostEntry ipHostEntries = Dns.GetHostEntry(stringHostName);
                    //Get Ip Address From The Ip Host Entry Address List
                    IPAddress[] arrIpAddress = ipHostEntries.AddressList;

                    try
                    {
                        visitorIPAddress = arrIpAddress[arrIpAddress.Length - 2].ToString();
                    }
                    catch
                    {
                        try
                        {
                            visitorIPAddress = arrIpAddress[0].ToString();
                        }
                        catch
                        {
                            try
                            {
                                arrIpAddress = Dns.GetHostAddresses(stringHostName);
                                visitorIPAddress = arrIpAddress[0].ToString();
                            }
                            catch
                            {
                                visitorIPAddress = "127.0.0.1";
                            }
                        }
                    }
                }
            }


            return visitorIPAddress;
        }

        public bool OIDLogs(string uid,string ip,string res_time)
        {

            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Insert_OIDLogs";

            cmdObj.Parameters
                .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
                .Value = uid;
            cmdObj.Parameters
                .Add(new SqlParameter("@res_time", SqlDbType.NVarChar))
                .Value = res_time;
            cmdObj.Parameters
                  .Add(new SqlParameter("@Ip_add", SqlDbType.NVarChar))
                  .Value = ip;
            if (ExecuteSqlProcedure(cmdObj))
            {
                return true;

            }
            else { return false; }

        }
    }
}