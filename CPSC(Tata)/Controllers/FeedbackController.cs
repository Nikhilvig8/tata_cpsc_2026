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
    public class FeedbackController : Controller
    {

        string conn = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        SqlCommand cmdObj;
        DataUtility du = new DataUtility();

        [HandleError()]
        public ActionResult Insert_Feedback(FormCollection coll)
        {
            Session["sflag"] = 0;
            string s;
            int rowcount;
            s = coll.Get("submitButton");
            if (s == "Submit")
            {
                string Dash,Feed;
              
                rowcount = Convert.ToInt32(coll.Get("row_count"));
                for (int i = 0; i < rowcount; i++)
                {
                    Dash = coll.Get("dash"+i);
                    Feed = coll.Get("feed" + i);
                    InsertintoData(Dash, Feed);
                }
                Session["sflag"] = 1;
            }
            return View();
        }

        public bool InsertintoData(string dash, string feed)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Insert_Feedback_Data";

            cmdObj.Parameters
              .Add(new SqlParameter("@uid", SqlDbType.NVarChar))
              .Value = Session["Uid"].ToString().Trim();
            cmdObj.Parameters
               .Add(new SqlParameter("@dash", SqlDbType.NVarChar))
               .Value = dash.Trim();
            cmdObj.Parameters
                .Add(new SqlParameter("@feed", SqlDbType.NVarChar))
                .Value = feed.Trim();

           var status = du.ExecuteSqlProcedure(cmdObj);

            return status;

        }

        public ActionResult Insert_Feedback_status()
        {
            return View();
        }

    }
}