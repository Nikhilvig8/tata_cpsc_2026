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
using DataUtilityLayer;
using System.Configuration;
using ClosedXML.Excel;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using Execution;


namespace InputOutput.Controllers
{
    // No local [HandleError()] - see LoginController.cs for why (it silently blocks the global
    // LoggingHandleErrorAttribute from logging this controller's exceptions).
    public class HomeController : Controller
    {
        string conn = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        SqlCommand cmdObj;

        //[Execution_Logs]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AdminIndex()
        {
            return View();
        }

        public ActionResult Data_Complition_Target()
        {
            return View();
        }

        public ActionResult Data_Complition_Actual()
        {
            return View();
        }

        public ActionResult Data_Complition_Sales_Target()
        {
            return View();
        }

        public ActionResult Data_Complition_Sales_Actual()
        {
            return View();
        }

        public ActionResult Data_Complition_AfterSales_Target()
        {
            return View();
        }

        public ActionResult Data_Complition_AfterSales_Actual()
        {
            return View();
        }

        public ActionResult CurrentStatus()
        {
            return View();
        }

        public ActionResult CurrentStatus_target()
        {
            return View();
        }

        public ActionResult CurrentStatus_actual()
        {
            return View();
        }

        public ActionResult PandingInputTSM_CSMWise_Target()
        {
            return View();
        }

        public ActionResult PandingInputTSM_CSMWise_Actual()
        {
            return View();
        }

        public ActionResult DelayedData_Target()
        {
            return View();
        }

        public ActionResult DelayedData_Actual()
        {
            return View();
        }
        public ActionResult Unlock_Target()
        {
            return View();
        }
        public ActionResult Unlock_Actual()
        {
            return View();
        }
        public ActionResult Update_Unlock_Target_Notifi(string fromwhom, string Dealer_code, string remark, string month, string Noti_id, string flag, string Unid)
        {
            DataUtility du = new DataUtility();
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_Unlock_Target_Notifi";

            cmdObj.Parameters
                .Add(new SqlParameter("@fromwhom", SqlDbType.NVarChar))
                .Value = fromwhom.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@dealer_code", SqlDbType.NVarChar))
                .Value = Dealer_code.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@remark", SqlDbType.NVarChar))
                .Value = remark.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@month", SqlDbType.NVarChar))
                  .Value = month.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@notifi_id", SqlDbType.NVarChar))
                  .Value = Noti_id.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@flag", SqlDbType.NVarChar))
                .Value = flag.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                  .Value = Session["Uid"].ToString().Trim();
            cmdObj.Parameters
              .Add(new SqlParameter("@Un_id", SqlDbType.NVarChar))
              .Value = Unid.Trim();
            if (du.ExecuteSqlProcedure(cmdObj))
            {

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else { return Json(false, JsonRequestBehavior.AllowGet); }
        }

        public ActionResult Update_Unlock_Actual_Notifi(string fromwhom, string Dealer_code, string remark, string month, string Noti_id, string flag, string Unid)
        {
            DataUtility du = new DataUtility();
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_Unlock_Actual_Notifi";

            cmdObj.Parameters
                .Add(new SqlParameter("@fromwhom", SqlDbType.NVarChar))
                .Value = fromwhom.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@dealer_code", SqlDbType.NVarChar))
                .Value = Dealer_code.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@remark", SqlDbType.NVarChar))
                .Value = remark.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@month", SqlDbType.NVarChar))
                  .Value = month.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@notifi_id", SqlDbType.NVarChar))
                  .Value = Noti_id.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@flag", SqlDbType.NVarChar))
                .Value = flag.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                  .Value = Session["Uid"].ToString().Trim();
            cmdObj.Parameters
              .Add(new SqlParameter("@Un_id", SqlDbType.NVarChar))
              .Value = Unid.Trim();
            if (du.ExecuteSqlProcedure(cmdObj))
            {

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else { return Json(false, JsonRequestBehavior.AllowGet); }
        }

        public ActionResult Insert_Unlock_Target_Notifi(string SPM_SSM, string Dealer_code, string Division_id, string LOB, string remark, string month)
        {
            DataUtility du = new DataUtility();
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Insert_Unlock_Target_Notifi";

            cmdObj.Parameters
                .Add(new SqlParameter("@SPM_SSM", SqlDbType.NVarChar))
                .Value = SPM_SSM.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = Dealer_code.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = LOB.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                  .Value = Division_id.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                  .Value = Session["Uid"].ToString().Trim();

            cmdObj.Parameters
                .Add(new SqlParameter("@flag", SqlDbType.NVarChar))
                .Value = "UnlockTarget";
            cmdObj.Parameters
                 .Add(new SqlParameter("@type", SqlDbType.NVarChar))
                 .Value = Session["Type"].ToString().Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@remark", SqlDbType.NVarChar))
                .Value = remark;

            if (du.ExecuteSqlProcedure(cmdObj))
            {

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else { return Json(false, JsonRequestBehavior.AllowGet); }

        }

        public ActionResult Insert_Unlock_Actual_Notifi(string TSM_CSM, string Dealer_code, string Division_id, string LOB, string remark, string month)
        {
            DataUtility du = new DataUtility();
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Insert_Unlock_Actual_Notifi";

            cmdObj.Parameters
                .Add(new SqlParameter("@TSM_CSM", SqlDbType.NVarChar))
                .Value = TSM_CSM.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = Dealer_code.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = LOB.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                  .Value = Division_id.Trim();
            cmdObj.Parameters
                  .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                  .Value = Session["Uid"].ToString().Trim();

            cmdObj.Parameters
                .Add(new SqlParameter("@flag", SqlDbType.NVarChar))
                .Value = "UnlockActual";
            cmdObj.Parameters
                 .Add(new SqlParameter("@type", SqlDbType.NVarChar))
                 .Value = Session["Type"].ToString().Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@remark", SqlDbType.NVarChar))
                .Value = remark;

            if (du.ExecuteSqlProcedure(cmdObj))
            {

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else { return Json(false, JsonRequestBehavior.AllowGet); }

        }

        public ActionResult Unlock_Target_Req(string Uid, string fromwhom, string Flag, string dealer, string month, string LOB, string o, string dealer_name, string LOB_name, string o_name, string monthdesc, string Notification_id, string remarks_out, string msg, string emp_name, FormCollection coll)
        {

            return View();
        }

        public ActionResult Unlock_Actual_Req(string Uid, string fromwhom, string Flag, string dealer, string month, string LOB, string o, string dealer_name, string LOB_name, string o_name, string monthdesc, string Notification_id, string remarks_out, string msg, string emp_name, FormCollection coll)
        {

            return View();
        }

        public ActionResult Dashboard(string dash_type_Tml)
        {
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
            Session["dash_type_tml"] = dash_type_Tml;

            return RedirectToAction("TDashboard", "Login");

        }

    
        public ActionResult AMWiseKPI()
        {

            return RedirectToAction("Index", "KPI");
        }

    public ActionResult DTToExcel(string Proc, string flag, string filename)
    {

        DataTable dt = new DataTable("Report");
        string CS1 = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        string _sql2 = string.Empty;
        _sql2 = Proc;
        using (SqlConnection cn = new SqlConnection(CS1))
        {

            //var cmd = new SqlCommand(_sql, cn);
            var daCampus = new SqlDataAdapter(_sql2, cn);
            daCampus.SelectCommand.CommandType = CommandType.StoredProcedure;
            daCampus.SelectCommand.Parameters.AddWithValue("@FW", Session["Uid"].ToString());
            daCampus.SelectCommand.Parameters.AddWithValue("@ForActual_Target", flag);
            daCampus.Fill(dt);
                
                //Remove Unwanted columns
                List<string> listtoRemove = new List<string> { "LOGIN", "KPI_ID" };
                for (int i = dt.Columns.Count - 1; i >= 0; i--)
                {
                    DataColumn dc = dt.Columns[i];
                    if (listtoRemove.Contains(dc.ColumnName.ToUpper()))
                    {
                        dt.Columns.Remove(dc);
                    }
                }
            }

        //DataSet data = JsonConvert.DeserializeObject<DataSet>(dt);
        //your datatable
        JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

        using (XLWorkbook wb = new XLWorkbook())
        {
            wb.Worksheets.Add(dt);

            string path = AppDomain.CurrentDomain.BaseDirectory + "Reports";
            filename = path + "\\" + filename;
            wb.SaveAs(filename);

        }
        return Json(true, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    [DeleteFileAttribute] //Action Filter, it will auto delete the file after download, 
                          
    public ActionResult Download(string file)
    {
        //get the temp folder and file path in server
        string fullPath = Path.Combine(Server.MapPath("~/Reports"), file);

        //return the file for download, this is an Excel 
        //so I set the file content type to "application/vnd.ms-excel"
        return File(fullPath, "application/vnd.ms-excel", file);
    }
}
public class DeleteFileAttribute : ActionFilterAttribute
{
    public override void OnResultExecuted(ResultExecutedContext filterContext)
    {
        filterContext.HttpContext.Response.Flush();

        //convert the current filter context to file and get the file path
        string filePath = (filterContext.Result as FilePathResult).FileName;

        //delete the file after download
        System.IO.File.Delete(filePath);
    }
}
}