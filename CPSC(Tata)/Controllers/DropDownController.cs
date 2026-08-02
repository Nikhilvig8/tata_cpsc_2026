using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using DataUtilityLayer;
using System.Configuration;
using System.Web.Script.Serialization;
using InputOutput.Models;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Reflection;

namespace InputOutput.Controllers
{
    public class DropDownController : Controller
    {
        // GET: DropDown

        string conn = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        SqlCommand cmdObj;
        DataUtility du = new DataUtility();
        public ActionResult GetDealer(String user,String Month)
        {
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getDataDealer";
           
            cmdObj.Parameters
                .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
                .Value = user;

            cmdObj.Parameters
                .Add(new SqlParameter("@month", SqlDbType.NVarChar))
                .Value = Month;
           
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);

            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSPM_SSM()
        {
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getSPM_SSM";
            cmdObj.Parameters
               .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
               .Value = Session["Uid"].ToString();
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);

            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTSM_CSM()
        {
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getTSM_CSM";
            cmdObj.Parameters
               .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
               .Value = Session["Uid"].ToString();
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);

            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDivision(String user,String Dealer)
        {
            int opmode;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "procGetDivision";
            if(Session["Type"].ToString()=="AM" || Session["Type"].ToString() == "TSM" || Session["Type"].ToString() == "SLM")
            {
                opmode = 1;
            }
            else
            {

                opmode = 2;
            }
            cmdObj.Parameters
                .Add(new SqlParameter("@OpMOde", SqlDbType.Int))
                .Value = opmode;
            if (Session["Type"].ToString() == "AM" || Session["Type"].ToString() == "ASM" || Session["Type"].ToString() =="SLM" || Session["Type"].ToString() =="SSM" )
            {
                cmdObj.Parameters
                    .Add(new SqlParameter("@AM_ASM_Code", SqlDbType.NVarChar))
                    .Value = user;
            }
            else
            {
                cmdObj.Parameters
                    .Add(new SqlParameter("@TSM_CSM_Code", SqlDbType.NVarChar))
                    .Value = user;
            }
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_Code", SqlDbType.NVarChar))
                .Value = Dealer;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);

            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetScorecard(String user, int month, int year)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getDealer_ScoreCard";
            
            cmdObj.Parameters
                .Add(new SqlParameter("@ForYear", SqlDbType.NVarChar))
                .Value = year;

            cmdObj.Parameters
                .Add(new SqlParameter("@ForMonth", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@ForUserId", SqlDbType.NVarChar))
                .Value = user;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDataEntryPoint(int month_as, int year_as, string user_id_as, string sheet_as)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getFirstDataEntryPoint";

            cmdObj.Parameters
                .Add(new SqlParameter("@ForMonth", SqlDbType.NVarChar))
                .Value = month_as;
            cmdObj.Parameters
                .Add(new SqlParameter("@ForYear", SqlDbType.NVarChar))
                .Value = year_as;
            cmdObj.Parameters
                .Add(new SqlParameter("@UserId", SqlDbType.NVarChar))
                .Value = user_id_as;
            cmdObj.Parameters
                .Add(new SqlParameter("@Target_Actual", SqlDbType.NVarChar))
                .Value = sheet_as;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDSEList(int month, int year, string dealer, string division, string lob)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getDSEList";

            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@del", SqlDbType.NVarChar))
                .Value = dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@div", SqlDbType.NVarChar))
                .Value = division;
            cmdObj.Parameters
                .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                .Value = lob;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }


        //----CI Audit Popup Begin Code --//


        public ActionResult GetCIAuditList(int month, int year, string dealer, string division, string lob)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getCIAuditList";

            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@del", SqlDbType.NVarChar))
                .Value = dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@div", SqlDbType.NVarChar))
                .Value = division;
            cmdObj.Parameters
                .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                .Value = lob;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TempCIAuditListData(string month, string year, string dealer_code, string division_id, string lob_id, string KPI, string Serial_No, string Desc, string Selection,string Weightage)
        {


            cmdObj = new SqlCommand();
            cmdObj.CommandText = "UpdateTempCIAuditList";

            cmdObj.Parameters
                .Add(new SqlParameter("@User_id", SqlDbType.NVarChar))
                .Value = Session["Uid"].ToString();
            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Month", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_code", SqlDbType.NVarChar))
                .Value = dealer_code;
            cmdObj.Parameters
                .Add(new SqlParameter("@Division_id", SqlDbType.NVarChar))
                .Value = division_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@LOB_id", SqlDbType.NVarChar))
                .Value = lob_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@KPI", SqlDbType.NVarChar))
                .Value = KPI;
            cmdObj.Parameters
                .Add(new SqlParameter("@Serial_No", SqlDbType.NVarChar))
                .Value = Serial_No;
            cmdObj.Parameters
               .Add(new SqlParameter("@Description", SqlDbType.NVarChar))
               .Value = Desc;
            cmdObj.Parameters
               .Add(new SqlParameter("@Selection", SqlDbType.NVarChar))
               .Value = Selection;

            cmdObj.Parameters
               .Add(new SqlParameter("@Weightage", SqlDbType.NVarChar))
               .Value = Weightage;

            var status = du.ExecuteSqlProcedure(cmdObj);


            return Json(status, JsonRequestBehavior.AllowGet);
        }

        //----CI Audit Popup End Code --//

        //----DWM Audit Popup Begin Code --//



        public ActionResult GetDWMAuditList(int month, int year, string dealer, string division, string lob)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getDWMAuditList";

            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@del", SqlDbType.NVarChar))
                .Value = dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@div", SqlDbType.NVarChar))
                .Value = division;
            cmdObj.Parameters
                .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                .Value = lob;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult TempDWMAuditListData(string month, string year, string dealer_code, string division_id, string lob_id, string KPI, List<Bvalues_dwm> select_jdata)
        {

            DataTable dt;

            List<Bvalues_dwm> jdata = select_jdata;

            if (jdata != null)
            {
                dt = ToDataTable(jdata);
                //dt = JsonStringToDataTable(select_jdata);
            }
            else
            {
                dt = null;
            }

            cmdObj = new SqlCommand();
            cmdObj.CommandText = "UpdateTempDWMAuditList";

            cmdObj.Parameters
                .Add(new SqlParameter("@User_id", SqlDbType.NVarChar))
                .Value = Session["Uid"].ToString();
            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Month", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_code", SqlDbType.NVarChar))
                .Value = dealer_code;
            cmdObj.Parameters
                .Add(new SqlParameter("@Division_id", SqlDbType.NVarChar))
                .Value = division_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@LOB_id", SqlDbType.NVarChar))
                .Value = lob_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@KPI", SqlDbType.NVarChar))
                .Value = KPI;
            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@data";
            param1.Value = dt;
            cmdObj.Parameters
                .Add(param1);

            var status = du.ExecuteSqlProcedure(cmdObj);


            return Json(status, JsonRequestBehavior.AllowGet);
        }

        //----DWM Audit Popup End Code --//


        //----Safety Audit Popup Begin Code --//
        public ActionResult GetSafetyAuditList(int month, int year, string dealer, string division, string lob)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getSafetyAuditList";

            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@del", SqlDbType.NVarChar))
                .Value = dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@div", SqlDbType.NVarChar))
                .Value = division;
            cmdObj.Parameters
                .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                .Value = lob;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult TempSafetyAuditListData(string month, string year, string dealer_code, string division_id, string lob_id, string KPI, List<Bvalues_saf> select_jdata)
        {

            DataTable dt;

            List<Bvalues_saf> jdata = select_jdata;

            if (jdata != null)
            {
                dt = ToDataTable(jdata);
                //dt = JsonStringToDataTable(select_jdata);
            }
            else
            {
                dt = null;
            }


            cmdObj = new SqlCommand();
            cmdObj.CommandText = "UpdateTempSafetyAuditList";

            cmdObj.Parameters
                .Add(new SqlParameter("@User_id", SqlDbType.NVarChar))
                .Value = Session["Uid"].ToString();
            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Month", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_code", SqlDbType.NVarChar))
                .Value = dealer_code;
            cmdObj.Parameters
                .Add(new SqlParameter("@Division_id", SqlDbType.NVarChar))
                .Value = division_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@LOB_id", SqlDbType.NVarChar))
                .Value = lob_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@KPI", SqlDbType.NVarChar))
                .Value = KPI;
            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@data";
            param1.Value = dt;
            cmdObj.Parameters
                .Add(param1);
            
            var status = du.ExecuteSqlProcedure(cmdObj);


            return Json(status, JsonRequestBehavior.AllowGet);
        }

        //----Safety Audit Popup End Code --//


        public ActionResult GetHyList(int month, int year, string dealer, string division, string lob)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getHygienList";

            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@del", SqlDbType.NVarChar))
                .Value = dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@div", SqlDbType.NVarChar))
                .Value = division;
            cmdObj.Parameters
                .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                .Value = lob;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TempHyListData(string month, string year, string dealer_code, string division_id, string lob_id, string KPI, string Serial_No, string Desc, string Selection)
        {


            cmdObj = new SqlCommand();
            cmdObj.CommandText = "UpdateTempHyList";

            cmdObj.Parameters
                .Add(new SqlParameter("@User_id", SqlDbType.NVarChar))
                .Value = Session["Uid"].ToString();
            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Month", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_code", SqlDbType.NVarChar))
                .Value = dealer_code;
            cmdObj.Parameters
                .Add(new SqlParameter("@Division_id", SqlDbType.NVarChar))
                .Value = division_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@LOB_id", SqlDbType.NVarChar))
                .Value = lob_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@KPI", SqlDbType.NVarChar))
                .Value = KPI;
            cmdObj.Parameters
                .Add(new SqlParameter("@Serial_No", SqlDbType.NVarChar))
                .Value = Serial_No;
            cmdObj.Parameters
               .Add(new SqlParameter("@Description", SqlDbType.NVarChar))
               .Value = Desc;
            cmdObj.Parameters
               .Add(new SqlParameter("@Selection", SqlDbType.NVarChar))
               .Value = Selection;


            var status = du.ExecuteSqlProcedure(cmdObj);


            return Json(status, JsonRequestBehavior.AllowGet);
        }


        
        [HttpPost]
        public ActionResult TempDSEData(string month, string year, string dealer_code, string division_id, string lob_id,string KPI,List <Bvalues_dse> select_jdata)
          {
            DataTable dt;

            List <Bvalues_dse> jdata = select_jdata;

            if (jdata!=null)
            {
                dt = ToDataTable(jdata);
                //dt = JsonStringToDataTable(select_jdata);
            }
            else
            {
                dt = null;
            }

            cmdObj = new SqlCommand();
            cmdObj.CommandText = "UpdateTempDSE";
            cmdObj.Parameters
                .Add(new SqlParameter("@User_id", SqlDbType.NVarChar))
                .Value = Session["Uid"].ToString();
            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Month", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_code", SqlDbType.NVarChar))
                .Value = dealer_code;
            cmdObj.Parameters
                .Add(new SqlParameter("@Division_id", SqlDbType.NVarChar))
                .Value = division_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@LOB_id", SqlDbType.NVarChar))
                .Value = lob_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@KPI", SqlDbType.NVarChar))
                .Value = KPI;
            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@data";
            param1.Value = dt;
            cmdObj.Parameters
                .Add(param1);
               
           


            var status = du.ExecuteSqlProcedure(cmdObj);

           
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDSMList(int month, int year, string dealer, string division, string lob)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getDSMList";

            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@del", SqlDbType.NVarChar))
                .Value = dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@div", SqlDbType.NVarChar))
                .Value = division;
            cmdObj.Parameters
                .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                .Value = lob;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult TempDSMData(string month, string year, string dealer_code, string division_id, string lob_id, string KPI, List<Bvalues_dsm>  select_jdata)
        {

            DataTable dt;

            List<Bvalues_dsm> jdata = select_jdata;

            if (jdata != null)
            {
                dt = ToDataTable(jdata);
                //dt = JsonStringToDataTable(select_jdata);
            }
            else
            {
                dt = null;
            }

            cmdObj = new SqlCommand();
            cmdObj.CommandText = "UpdateTempDSM";

            cmdObj.Parameters
                .Add(new SqlParameter("@User_id", SqlDbType.NVarChar))
                .Value = Session["Uid"].ToString();
            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Month", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_code", SqlDbType.NVarChar))
                .Value = dealer_code;
            cmdObj.Parameters
                .Add(new SqlParameter("@Division_id", SqlDbType.NVarChar))
                .Value = division_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@LOB_id", SqlDbType.NVarChar))
                .Value = lob_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@KPI", SqlDbType.NVarChar))
                .Value = KPI;
            SqlParameter param1 = new SqlParameter();
            param1.ParameterName = "@data";
            param1.Value = dt;
            cmdObj.Parameters
                .Add(param1);


            var status = du.ExecuteSqlProcedure(cmdObj);


            return Json(status, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetSalesActivityList(int month, int year, string dealer, string division, string lob)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getSaleActivityList";

            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year.ToString();
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = month.ToString();
            cmdObj.Parameters
                .Add(new SqlParameter("@del", SqlDbType.NVarChar))
                .Value = dealer.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@div", SqlDbType.NVarChar))
                .Value = division.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                .Value = lob.Trim();
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult autoSubmitData(string dealer, string month, string lob, string kpi, string actual, string division,string year)
        {
            string notification_id = "NULL";
            Transaction t1 = new Transaction();
            var status = t1.Update_BULKActualData(dealer,year,month, lob, kpi, actual, division,notification_id);
            return Json(status, JsonRequestBehavior.AllowGet);
        }
        public ActionResult autoSubmitDataTarget(string dealer, string month, string lob, string kpi, string target, string division,string year)
        {
            string notification_id = "NULL";
            Transaction t1 = new Transaction();
            var status = t1.Update_BULKData(dealer, month, lob, kpi, target, division,notification_id,year);
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TempActivityData(string month, string year, string dealer_code, string division_id, string lob_id, string KPI, string DSE_name, string DSE_Activity, string Selection)
        {


            cmdObj = new SqlCommand();
            cmdObj.CommandText = "UpdateTempActivity";

            cmdObj.Parameters
                .Add(new SqlParameter("@User_id", SqlDbType.NVarChar))
                .Value = Session["Uid"].ToString();
            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Month", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_code", SqlDbType.NVarChar))
                .Value = dealer_code;
            cmdObj.Parameters
                .Add(new SqlParameter("@Division_id", SqlDbType.NVarChar))
                .Value = division_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@LOB_id", SqlDbType.NVarChar))
                .Value = lob_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@KPI", SqlDbType.NVarChar))
                .Value = KPI;
            cmdObj.Parameters
                .Add(new SqlParameter("@DSE_name", SqlDbType.NVarChar))
                .Value = DSE_name;
            cmdObj.Parameters
               .Add(new SqlParameter("@DSE_Activity", SqlDbType.NVarChar))
               .Value = DSE_Activity;
            cmdObj.Parameters
               .Add(new SqlParameter("@Selection", SqlDbType.NVarChar))
               .Value = Selection;


            var status = du.ExecuteSqlProcedure(cmdObj);


            return Json(status, JsonRequestBehavior.AllowGet);
        }

     

        public ActionResult GetBulkData(String user, int month, string dealer,int year)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getBulkEntryActual";

            cmdObj.Parameters
               .Add(new SqlParameter("@ForYear", SqlDbType.NVarChar))
               .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_Code", SqlDbType.NVarChar))
                .Value = dealer;

            cmdObj.Parameters
                .Add(new SqlParameter("@ForMonth", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@User_Id", SqlDbType.NVarChar))
                .Value = user;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("Column_Name");
            dt1.Columns.Add("Count");
            for(int i=0;i<dt.Columns.Count;i++)
            {
                DataRow row = dt1.NewRow();
                row["Column_Name"] = dt.Columns[i].ColumnName.ToString();
                row["Count"] = dt.Columns.Count.ToString();
                dt1.Rows.Add(row);
            }

            var sites1 = DataTableToJSONWithJavaScriptSerializer(dt);
            var Column_Name= DataTableToJSONWithJavaScriptSerializer(dt1);
            var sites = sites1 + "^" + Column_Name;
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBulkData_Notifi(String user, int month, string dealer,string div,string LOB,string year)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getBulkEntryActual_Notifi";

            cmdObj.Parameters
              .Add(new SqlParameter("@ForYear", SqlDbType.NVarChar))
              .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_Code", SqlDbType.NVarChar))
                .Value = dealer;
            cmdObj.Parameters
               .Add(new SqlParameter("@Division_Id", SqlDbType.NVarChar))
               .Value = div;
            cmdObj.Parameters
                .Add(new SqlParameter("@ForMonth", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@User_Id", SqlDbType.NVarChar))
                .Value = user;
            cmdObj.Parameters
               .Add(new SqlParameter("@LOB_Id", SqlDbType.NVarChar))
               .Value = LOB;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("Column_Name");
            dt1.Columns.Add("Count");
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DataRow row = dt1.NewRow();
                row["Column_Name"] = dt.Columns[i].ColumnName.ToString();
                row["Count"] = dt.Columns.Count.ToString();
                dt1.Rows.Add(row);
            }

            var sites1 = DataTableToJSONWithJavaScriptSerializer(dt);
            var Column_Name = DataTableToJSONWithJavaScriptSerializer(dt1);
            var sites = sites1 + "^" + Column_Name;
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBulkDataQuarterly(String user, int month, string dealer,int year)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getBulkEntryActual_Quarterly";

            cmdObj.Parameters
               .Add(new SqlParameter("@ForYear", SqlDbType.NVarChar))
               .Value = year;

            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_Code", SqlDbType.NVarChar))
                .Value = dealer;

            cmdObj.Parameters
                .Add(new SqlParameter("@ForMonth", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@User_Id", SqlDbType.NVarChar))
                .Value = user;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("Column_Name");
            dt1.Columns.Add("Count");
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DataRow row = dt1.NewRow();
                row["Column_Name"] = dt.Columns[i].ColumnName.ToString();
                row["Count"] = dt.Columns.Count.ToString();
                dt1.Rows.Add(row);
            }

            var sites1 = DataTableToJSONWithJavaScriptSerializer(dt);
            var Column_Name = DataTableToJSONWithJavaScriptSerializer(dt1);
            var sites = sites1 + "^" + Column_Name;
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBulkDataTargetQuarterly(String user, int month, string dealer,int year)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getBulkEntryTarget_Quarterly";

            cmdObj.Parameters
                .Add(new SqlParameter("@ForYear", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_Code", SqlDbType.NVarChar))
                .Value = dealer;

            cmdObj.Parameters
                .Add(new SqlParameter("@ForMonth", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@User_Id", SqlDbType.NVarChar))
                .Value = user;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("Column_Name");
            dt1.Columns.Add("Count");
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DataRow row = dt1.NewRow();
                row["Column_Name"] = dt.Columns[i].ColumnName.ToString();
                row["Count"] = dt.Columns.Count.ToString();
                dt1.Rows.Add(row);
            }

            var sites1 = DataTableToJSONWithJavaScriptSerializer(dt);
            var Column_Name = DataTableToJSONWithJavaScriptSerializer(dt1);
            var sites = sites1 + "^" + Column_Name;
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBulkDataManpowerTarget(String user, int month, string dealer, int year)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getBulkEntryManpowerTarget";

            cmdObj.Parameters
              .Add(new SqlParameter("@ForYear", SqlDbType.NVarChar))
              .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_Code", SqlDbType.NVarChar))
                .Value = dealer;

            cmdObj.Parameters
                .Add(new SqlParameter("@ForMonth", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@User_Id", SqlDbType.NVarChar))
                .Value = user;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("Column_Name");
            dt1.Columns.Add("Count");
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DataRow row = dt1.NewRow();
                row["Column_Name"] = dt.Columns[i].ColumnName.ToString();
                row["Count"] = dt.Columns.Count.ToString();
                dt1.Rows.Add(row);
            }

            var sites1 = DataTableToJSONWithJavaScriptSerializer(dt);
            var Column_Name = DataTableToJSONWithJavaScriptSerializer(dt1);
            var sites = sites1 + "^" + Column_Name;
            return Json(sites, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetBulkDataTarget(String user, int month, string dealer,int year)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getBulkEntryTarget";

            cmdObj.Parameters
              .Add(new SqlParameter("@ForYear", SqlDbType.NVarChar))
              .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_Code", SqlDbType.NVarChar))
                .Value = dealer;

            cmdObj.Parameters
                .Add(new SqlParameter("@ForMonth", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@User_Id", SqlDbType.NVarChar))
                .Value = user;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("Column_Name");
            dt1.Columns.Add("Count");
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DataRow row = dt1.NewRow();
                row["Column_Name"] = dt.Columns[i].ColumnName.ToString();
                row["Count"] = dt.Columns.Count.ToString();
                dt1.Rows.Add(row);
            }

            var sites1 = DataTableToJSONWithJavaScriptSerializer(dt);
            var Column_Name = DataTableToJSONWithJavaScriptSerializer(dt1);
            var sites = sites1 + "^" + Column_Name;
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBulkDataTarget_Notifi(String user, int month, string dealer,string div,string LOB,string year)
        {
            //Transaction t1 = new Transaction();
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getBulkEntryTarget_Notifi";

            cmdObj.Parameters
              .Add(new SqlParameter("@ForYear", SqlDbType.NVarChar))
              .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_Code", SqlDbType.NVarChar))
                .Value = dealer;
            cmdObj.Parameters
               .Add(new SqlParameter("@Division_Id", SqlDbType.NVarChar))
               .Value = div;
            cmdObj.Parameters
                .Add(new SqlParameter("@ForMonth", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@User_Id", SqlDbType.NVarChar))
                .Value = user;
            cmdObj.Parameters
               .Add(new SqlParameter("@LOB_Id", SqlDbType.NVarChar))
               .Value = LOB;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);
            //t1.Month = Convert.ToString(month);
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("Column_Name");
            dt1.Columns.Add("Count");
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DataRow row = dt1.NewRow();
                row["Column_Name"] = dt.Columns[i].ColumnName.ToString();
                row["Count"] = dt.Columns.Count.ToString();
                dt1.Rows.Add(row);
            }

            var sites1 = DataTableToJSONWithJavaScriptSerializer(dt);
            var Column_Name = DataTableToJSONWithJavaScriptSerializer(dt1);
            var sites = sites1 + "^" + Column_Name;
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLOB(String d_id1, String d_id2)
        {
            string d_id3 = "";
            if (d_id2 == "") { d_id3 = ""; } else { d_id3 = "&" + d_id2; }
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "procGetDivision_LOB";
            cmdObj.Parameters
              .Add(new SqlParameter("@Login_id", SqlDbType.NVarChar))
              .Value = Session["Uid"].ToString() ;
            cmdObj.Parameters
              .Add(new SqlParameter("@Division_Id", SqlDbType.NVarChar))
              .Value = d_id1 + "" + d_id3;

            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);

            var sites = DataTableToJSONWithJavaScriptSerializer(dt);
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UnlockEditing(string flag,string Dealer_Code,string Division_Id,string LOB_ID)
        {

            cmdObj = new SqlCommand();
            cmdObj.CommandText = "procUnLockEditing";

          
            cmdObj.Parameters
                .Add(new SqlParameter("@flag", SqlDbType.NVarChar))
                .Value = flag.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_code", SqlDbType.NVarChar))
                .Value = Dealer_Code.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@Division_id", SqlDbType.NVarChar))
                .Value = Division_Id.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@LOB_id", SqlDbType.NVarChar))
                .Value = LOB_ID.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
                .Value = Session["Uid"].ToString();


            var status = du.ExecuteSqlProcedure(cmdObj);


            return Json(status, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_Hasbeenseenflag(string notifi_id,string fromwhom)
        {

            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_Hasbeenseen_Target";


            cmdObj.Parameters
                .Add(new SqlParameter("@Notification_id", SqlDbType.NVarChar))
                .Value = notifi_id.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@FromWhom", SqlDbType.NVarChar))
                .Value = fromwhom.Trim();


            var status = du.ExecuteSqlProcedure(cmdObj);


            return Json(status, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Tempdataremove(string month, string year, string dealer_code, string division_id, string lob_id, string KPI,string procname)
        {


            cmdObj = new SqlCommand();
            cmdObj.CommandText = procname;

            cmdObj.Parameters
                .Add(new SqlParameter("@User_id", SqlDbType.NVarChar))
                .Value = Session["Uid"].ToString();
            cmdObj.Parameters
                .Add(new SqlParameter("@year", SqlDbType.NVarChar))
                .Value = year;
            cmdObj.Parameters
                .Add(new SqlParameter("@Month", SqlDbType.NVarChar))
                .Value = month;
            cmdObj.Parameters
                .Add(new SqlParameter("@Dealer_code", SqlDbType.NVarChar))
                .Value = dealer_code;
            cmdObj.Parameters
                .Add(new SqlParameter("@Division_id", SqlDbType.NVarChar))
                .Value = division_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@LOB_id", SqlDbType.NVarChar))
                .Value = lob_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@KPI", SqlDbType.NVarChar))
                .Value = KPI;
            
            var status = du.ExecuteSqlProcedure(cmdObj);


            return Json(status, JsonRequestBehavior.AllowGet);
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        public static string Base64Decode(string base64EncodedData)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
        }
        public DataTable JsonStringToDataTable(string jsonString)
        {
            DataTable dt = new DataTable();
            string[] jsonStringArray = Regex.Split(jsonString.Replace("[", "").Replace("]", ""), "},{");

            List<string> ColumnsName = new List<string>();
            foreach (string jSA in jsonStringArray)
            {
                string[] jsonStringData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                foreach (string ColumnsNameData in jsonStringData)
                {
                    try
                    {
                        int idx = ColumnsNameData.IndexOf(":");
                        string ColumnsNameString = ColumnsNameData.Substring(0, idx - 1).Replace("\"", "");
                        if (!ColumnsName.Contains(ColumnsNameString))
                        {
                            ColumnsName.Add(ColumnsNameString);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error Parsing Column Name : {0}", ColumnsNameData));
                    }
                }
                break;
            }
            foreach (string AddColumnName in ColumnsName)
            {
                dt.Columns.Add(AddColumnName);
            }
            foreach (string jSA in jsonStringArray)
            {
                string[] RowData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                DataRow nr = dt.NewRow();
                foreach (string rowData in RowData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string RowColumns = rowData.Substring(0, idx - 1).Replace("\"", "");
                        string RowDataString = rowData.Substring(idx + 1).Replace("\"", "");
                        nr[RowColumns] = RowDataString;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                dt.Rows.Add(nr);
            }
            return dt;
        }
        public string DataTableToJSONWithJavaScriptSerializer(DataTable table)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    childRow.Add(col.ColumnName, row[col]);
                }
                parentRow.Add(childRow);
            }
            return jsSerializer.Serialize(parentRow);
        }
    }

   

    public class Bvalues_dse
    {
        public string dse_id { set; get; }
        public string dse_name { set; get; }
        public string Selection { set; get; }
    }
    public class Bvalues_dsm
    {
        public string dsm_id { set; get; }
        public string dsm_name { set; get; }
        public string Selection { set; get; }
    }
    public class Bvalues_dwm
    {
        public string Serial_No { set; get; }
        public string Desc { set; get; }
        public string Weightage { set; get; }
        public string Selection { set; get; }
    }
    public class Bvalues_saf
    {
        public string Serial_No { set; get; }
        public string Desc { set; get; }
        public string Weightage { set; get; }
        public string Selection { set; get; }
    }
}