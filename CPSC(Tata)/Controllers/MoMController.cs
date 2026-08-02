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

namespace InputOutput.Controllers
{
    public class MoMController : Controller
    {
        // GET: MoM

        string conn = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        SqlCommand cmdObj;
        DataUtility du = new DataUtility();

        [HandleError()]
     
        public ActionResult Insert_MoM(FormCollection coll)
        {
            Session["sflag"] = 0;
            string s;
            int rowcount;
            s = coll.Get("submitButton");
            if (s == "Submit")
            {
                string tod, date, venue, dealer_code, tml_team, dealer_team;
                string sno, topic, target, resp, plan, tfc, cs;
                string key = Guid.NewGuid().ToString();
                rowcount= Convert.ToInt32(coll.Get("row_count"));
                tod = coll.Get("TOD");date = coll.Get("Date");venue = coll.Get("Venue");dealer_code = coll.Get("Dealer_Code");
                tml_team = coll.Get("Tata_Team");dealer_team = coll.Get("Dealer_Team");
                Insertintoheader(tod, date, venue, dealer_code, tml_team, dealer_team,key);
                for(int i=1;i<=rowcount;i++)
                {
                    sno = coll.Get("Sno" + i); topic = coll.Get("topic" + i);target = coll.Get("target" + i);resp = coll.Get("resp" + i);
                    plan = coll.Get("plan" + i);tfc = coll.Get("tfc" + i);cs = coll.Get("cs" + i);
                    InsertintoData(sno, topic, target,resp, plan, tfc, cs,key);
                }

                Session["sflag"] = 1;
            }
                return View();
        }

        public ActionResult Modify_MoM(FormCollection coll)
        {
            Session["sflag"] = 0;
            string s;
            int rowcount;
            s = coll.Get("submitButton");
            if (s == "Submit")
            {
                string tod, date, venue, dealer_code, tml_team, dealer_team;
                string sno, topic, target, resp, plan, tfc, cs;
                string key = Guid.NewGuid().ToString();
                rowcount = Convert.ToInt32(coll.Get("row_count"));
                tod = coll.Get("TOD"); date = coll.Get("Date"); venue = coll.Get("Venue"); dealer_code = coll.Get("Dealer_Code");
                tml_team = coll.Get("Tata_Team"); dealer_team = coll.Get("Dealer_Team");
                Modifyintoheader(tod, date, venue, dealer_code, tml_team, dealer_team, key);
                for (int i = 1; i <= rowcount; i++)
                {
                    sno = coll.Get("Sno" + i); topic = coll.Get("topic" + i); target = coll.Get("target" + i); resp = coll.Get("resp" + i);
                    plan = coll.Get("plan" + i); tfc = coll.Get("tfc" + i); cs = coll.Get("cs" + i);
                    ModifyintoData(sno, topic, target, resp, plan, tfc, cs, key);
                }

                Session["sflag"] = 1;
            }
            return View();
        }

        public ActionResult GetModifiMoMData(string user, string date, string dealer)
        {
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getModifiMoMData";

            cmdObj.Parameters
               .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
               .Value = user;
            cmdObj.Parameters
               .Add(new SqlParameter("@date", SqlDbType.NVarChar))
               .Value = date;
            cmdObj.Parameters
               .Add(new SqlParameter("@dealer_code", SqlDbType.NVarChar))
               .Value = dealer;
            DataSet dt = new DataSet();
            dt=du.GetDataSetWithProc(cmdObj);
            var sites = "";
            if (dt.Tables[0].Rows.Count > 0)
            {

                DataTable dt1 = new DataTable();
                dt1 = dt.Tables[0];

                DataTable dt2 = new DataTable();
                dt2 = dt.Tables[1];

                var sites1 = DataTableToJSONWithJavaScriptSerializer(dt1);
                var sites2 = DataTableToJSONWithJavaScriptSerializer(dt2);
                sites = sites1 + "^" + sites2;
            }
            else
            {
                sites = "false";
            }
            return Json(sites, JsonRequestBehavior.AllowGet);
        }

        public bool Insertintoheader(string tod,string date,string venue,string dealer_code,string tml_team,string dealer_team,string key)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Insert_MoM_Header";


            cmdObj.Parameters
                .Add(new SqlParameter("@tod", SqlDbType.NVarChar))
                .Value = tod.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@date", SqlDbType.NVarChar))
                .Value = date.Trim();

            cmdObj.Parameters
               .Add(new SqlParameter("@venue", SqlDbType.NVarChar))
               .Value = venue.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@dealer_code", SqlDbType.NVarChar))
                .Value = dealer_code.Trim();

            cmdObj.Parameters
               .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
               .Value = Session["Uid"].ToString().Trim();

            cmdObj.Parameters
                           .Add(new SqlParameter("@tml_team", SqlDbType.NVarChar))
                           .Value = tml_team.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@dealer_team", SqlDbType.NVarChar))
                .Value = dealer_team.Trim();
            cmdObj.Parameters
               .Add(new SqlParameter("@key", SqlDbType.NVarChar))
               .Value = key.Trim();

            var status = du.ExecuteSqlProcedure(cmdObj);


            return status;
        
        }

        public bool InsertintoData(string sno, string topic, string target, string resp, string plan, string tfc,string cs,string key)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Insert_MoM_Data";


            cmdObj.Parameters
                .Add(new SqlParameter("@sno", SqlDbType.NVarChar))
                .Value = sno.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@topic", SqlDbType.NVarChar))
                .Value = topic.Trim();

            cmdObj.Parameters
               .Add(new SqlParameter("@target", SqlDbType.NVarChar))
               .Value = target.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@resp", SqlDbType.NVarChar))
                .Value = resp.Trim();

            cmdObj.Parameters
                           .Add(new SqlParameter("@plan", SqlDbType.NVarChar))
                           .Value = plan.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@tfc", SqlDbType.NVarChar))
                .Value = tfc.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@cs", SqlDbType.NVarChar))
                .Value = cs.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@key", SqlDbType.NVarChar))
                .Value = key.Trim();

            var status = du.ExecuteSqlProcedure(cmdObj);


            return status;

        }

        public bool Modifyintoheader(string tod, string date, string venue, string dealer_code, string tml_team, string dealer_team, string key)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Modify_MoM_Header";


            cmdObj.Parameters
                .Add(new SqlParameter("@tod", SqlDbType.NVarChar))
                .Value = tod.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@date", SqlDbType.NVarChar))
                .Value = date.Trim();

            cmdObj.Parameters
               .Add(new SqlParameter("@venue", SqlDbType.NVarChar))
               .Value = venue.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@dealer_code", SqlDbType.NVarChar))
                .Value = dealer_code.Trim();

            cmdObj.Parameters
               .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
               .Value = Session["Uid"].ToString().Trim();

            cmdObj.Parameters
                           .Add(new SqlParameter("@tml_team", SqlDbType.NVarChar))
                           .Value = tml_team.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@dealer_team", SqlDbType.NVarChar))
                .Value = dealer_team.Trim();
            cmdObj.Parameters
               .Add(new SqlParameter("@key", SqlDbType.NVarChar))
               .Value = key.Trim();

            var status = du.ExecuteSqlProcedure(cmdObj);


            return status;

        }

        public bool ModifyintoData(string sno, string topic, string target, string resp, string plan, string tfc, string cs, string key)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Modify_MoM_Data";


            cmdObj.Parameters
                .Add(new SqlParameter("@sno", SqlDbType.NVarChar))
                .Value = sno.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@topic", SqlDbType.NVarChar))
                .Value = topic.Trim();

            cmdObj.Parameters
               .Add(new SqlParameter("@target", SqlDbType.NVarChar))
               .Value = target.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@resp", SqlDbType.NVarChar))
                .Value = resp.Trim();

            cmdObj.Parameters
                           .Add(new SqlParameter("@plan", SqlDbType.NVarChar))
                           .Value = plan.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@tfc", SqlDbType.NVarChar))
                .Value = tfc.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@cs", SqlDbType.NVarChar))
                .Value = cs.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@key", SqlDbType.NVarChar))
                .Value = key.Trim();

            var status = du.ExecuteSqlProcedure(cmdObj);


            return status;

        }


        public ActionResult GetPrevData(string user,string date,string dealer)
        {
            string opmode = string.Empty;
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "getPrevMoMData";

            cmdObj.Parameters
               .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
               .Value = user;
            cmdObj.Parameters
               .Add(new SqlParameter("@date", SqlDbType.NVarChar))
               .Value = date;
            cmdObj.Parameters
               .Add(new SqlParameter("@dealer_code", SqlDbType.NVarChar))
               .Value = dealer;
            DataTable dt = new DataTable();
            dt = du.GetDataTableWithProc(cmdObj);

            var sites = false;
            if (dt != null)
            {
                if (dt.Rows.Count > 0 )
            {
                sites = true;
            }
            }
            return Json(sites, JsonRequestBehavior.AllowGet);
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
}