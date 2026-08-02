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

namespace InputOutput.Controllers
{
    public class ChangeReqController : Controller
    {
        string conn = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        SqlCommand cmdObj;
        DataUtility du = new DataUtility();
        // GET: ChangeReq target

        public ActionResult Change_Target()
        {
            return View();
        }

        public ActionResult Insert_ChangeReq_Target_Notifi(string SPM_SSM, string Dealer_code, string Division_id, string LOB, string remark, string month,string year)
        {
            DataUtility du = new DataUtility();
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Insert_ChangeReq_Target_Notifi";

            cmdObj.Parameters
                .Add(new SqlParameter("@SPM_SSM", SqlDbType.NVarChar))
                .Value = SPM_SSM.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@y", SqlDbType.NVarChar))
                .Value = year.Trim();
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
                .Value = "ChangeTarget";
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


        public ActionResult Change_Target_Req(string Uid, string fromwhom, string Flag, string dealer, string month, string LOB, string o, string dealer_name, string LOB_name, string o_name, string monthdesc, string Notification_id, string remarks_out, string msg, string emp_name, string binding_key,string year, FormCollection coll)
        {

            return View();
        }

        public ActionResult Update_Change_Target_Notifi(string fromwhom, string Dealer_code, string remark, string month, string Noti_id, string flag, string Unid,string binding_key,string year)
        {
            DataUtility du = new DataUtility();
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_Change_Target_Notifi";

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
                  .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                  .Value = year.Trim();
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
            cmdObj.Parameters
              .Add(new SqlParameter("@Binding_key", SqlDbType.NVarChar))
              .Value = binding_key.Trim();
            if (du.ExecuteSqlProcedure(cmdObj))
            {

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else { return Json(false, JsonRequestBehavior.AllowGet); }
        }

        //Get: ChangeReq actual

        public ActionResult Change_Actual()
        {
            return View();
        }

        public ActionResult Insert_ChangeReq_Actual_Notifi(string SPM_SSM, string Dealer_code, string Division_id, string LOB, string remark, string month,string year)
        {
            DataUtility du = new DataUtility();
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Insert_ChangeReq_Actual_Notifi";

            cmdObj.Parameters
                .Add(new SqlParameter("@SPM_SSM", SqlDbType.NVarChar))
                .Value = SPM_SSM.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@y", SqlDbType.NVarChar))
                .Value = year.Trim();

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
                .Value = "ChangeActual";
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

        public ActionResult Change_Actual_Req(string Uid, string fromwhom, string Flag, string dealer, string month, string LOB, string o, string dealer_name, string LOB_name, string o_name, string monthdesc, string Notification_id, string remarks_out, string msg, string emp_name, string binding_key,string year ,FormCollection coll)
        {

            return View();
        }
        public ActionResult Update_Change_Actual_Notifi(string fromwhom, string Dealer_code, string remark, string month, string Noti_id, string flag, string Unid, string binding_key,string year)
        {
            DataUtility du = new DataUtility();
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_Change_Actual_Notifi";

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
                  .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                  .Value = year.Trim();
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
            cmdObj.Parameters
              .Add(new SqlParameter("@Binding_key", SqlDbType.NVarChar))
              .Value = binding_key.Trim();
            if (du.ExecuteSqlProcedure(cmdObj))
            {

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else { return Json(false, JsonRequestBehavior.AllowGet); }
        }

        public ActionResult Update_Hasbeenseenflag_ChangeReq(string notifi_id, string fromwhom,string binding_key)
        {

            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_Hasbeenseen_ChangeReq";


            cmdObj.Parameters
                .Add(new SqlParameter("@Notification_id", SqlDbType.NVarChar))
                .Value = notifi_id.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@FromWhom", SqlDbType.NVarChar))
                .Value = fromwhom.Trim();
            cmdObj.Parameters
            .Add(new SqlParameter("@Binding_key", SqlDbType.NVarChar))
            .Value = binding_key.Trim();

            var status = du.ExecuteSqlProcedure(cmdObj);


            return Json(status, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangeReq_Logs()
        {
            return View();
        }

    }


}