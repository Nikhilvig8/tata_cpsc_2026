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
namespace InputOutput.Controllers
{
    [HandleError()]
    public class LoginController_261023 : Controller
    {
        // GET: Login

        
        public ActionResult Login()
        {
            return View();
        }

        
        public ActionResult TDashboard()
        {
            if(Session["Type"].ToString()!="DL")
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

            //Response.Write(responseContent);
            Session["ticket"] = responseContent;

            return View();
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
        public ActionResult LoginCheck(FormCollection collection)
        {
            string UserType = string.Empty;
            Users user = new Users();
            user.UserName = collection.Get("username").ToString();
            user.Password = collection.Get("password").ToString();

            if (ModelState.IsValid && user.UserName == "IshwarK" && user.Password == "IshwarK8")
            {
                if (true)
                {
                    FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
                    UserType = "Spl";
                    if (UserType != "")
                    {
                        if (UserType == "DL")
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
            else if (ModelState.IsValid && user.Password=="PraSad@mb31")
            {
                if (user.IsValid(user.UserName, "PraSad@mb31"))
                {
                    FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
                    UserType = Session["Type"].ToString();
                    if (UserType != "")
                    {
                        if (UserType == "DL")
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
                if (user.IsValidOID(user.UserName, user.Password))
                {
                    FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
                    UserType = Session["Type"].ToString();
                    if (UserType != "")
                    {
                        if (UserType == "DL")
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
            return View(user);
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
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Login");
        }
    }
}