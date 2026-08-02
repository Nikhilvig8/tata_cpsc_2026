using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace InputOutput.Controllers
{
    public class DataFillingController : Controller
    {
        string conn = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

        // GET: DataFilling
        public ActionResult DataFilling()
        {
            IsLocked();
            DealerState();
            return View();
        }


        public class DealerList
        {
            public string Region { get; set; }
            public string DealerState { get; set; }
            public string TIV { get; set; }
            public string TMV { get; set; }
            public string MS { get; set; }
            public string YTDTIV { get; set; }
            public string YTDTMV { get; set; }
            public string YTDMS { get; set; }
            public string IsLocked { get; set; }
            public string StateId { get; set; }
            public string LOB { get; set; }
        }

        public ActionResult DealerRegion()
        {
            DealerList DE = new DealerList();
            DataSet ds = new DataSet();
            SqlConnection con = new SqlConnection(conn);
            SqlCommand cmd = new SqlCommand("get_SHRegion", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@LoginId", Session["Uid"].ToString());
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            List<SelectListItem> dataEntries = new List<SelectListItem>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DealerList uobj = new DealerList();
                uobj.Region = ds.Tables[0].Rows[i]["Region"].ToString();
                dataEntries.Add(new SelectListItem { Value = uobj.Region, Text = uobj.Region });
            }
            ViewBag.DealerRegion = dataEntries;
            con.Close();
            return View();

        }

        public ActionResult DealerState()
        {
            DealerList DE = new DealerList();
            DataSet ds = new DataSet();
            SqlConnection con = new SqlConnection(conn);
            SqlCommand cmd = new SqlCommand("get_SHRegionState", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@LoginId", Session["Uid"].ToString());
            // cmd.Parameters.AddWithValue("@Region", DealerRegion).ToString();
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            List<SelectListItem> dataEntries = new List<SelectListItem>();
            //List<SelectListItem> dataEntriesIsLocked = new List<SelectListItem>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DealerList uobj = new DealerList();
                uobj.StateId = ds.Tables[0].Rows[i]["StateId"].ToString();
                uobj.DealerState = ds.Tables[0].Rows[i]["DealerState"].ToString();
                dataEntries.Add(new SelectListItem { Value = uobj.StateId, Text = uobj.DealerState });
                //dataEntriesIsLocked.Add(new SelectListItem { Value = uobj.IsLocked});
            }
            //var IsLocked = dataEntriesIsLocked[0];
            //ViewBag.IsLocked = dataEntriesIsLocked[0];
            ViewBag.DealerState = dataEntries;
            con.Close();
            return Json(ViewBag.DealerState, JsonRequestBehavior.AllowGet);

        }

        public ActionResult DataFillingInsert(string json)
        {
            XmlDocument XmlDoc;
            XmlDoc = (XmlDocument)JsonConvert.DeserializeXmlNode("{\"DataFilling\":" + json + "}", "StateWise");
            //string QualXml = XmlDoc.InnerXml.ToString();
            DataFillingInsertDetails(XmlDoc.InnerXml);
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return Json(serializer.Serialize("Record has been submitted Successfully"), JsonRequestBehavior.AllowGet);
        }
        public void DataFillingInsertDetails(string dataEntry)
        {
            SqlConnection con = new SqlConnection(conn);
            SqlCommand cmd = new SqlCommand("StatewiseDataFillingInsert", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@input", dataEntry);
            cmd.Parameters.AddWithValue("@Login", Session["Uid"].ToString());
            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
        }

        public ActionResult DataFillingDetails(string StateId)
        {
            string a = Session["Uid"].ToString();
            DateTime A_d = Convert.ToDateTime(Session["Actual_Date"].ToString());
            string Actual_Month = A_d.Month.ToString();
            string Actual_Year = A_d.Year.ToString();
            string Actual_Month_Des = A_d.Year.ToString() + "-" + A_d.ToString("MMMM");
            var model = new List<DealerList>();
            SqlConnection con = new SqlConnection(conn);
            SqlCommand cmd = new SqlCommand("GetStatewiseDataFilling", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Login", Session["Uid"].ToString());
            //cmd.Parameters.AddWithValue("@Region", Region);
            cmd.Parameters.AddWithValue("@State", StateId);
            cmd.Parameters.AddWithValue("@ForMonth", Actual_Month);
            cmd.Parameters.AddWithValue("@ForYear", Actual_Year);
            con.Open();
            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                DealerList dealerList = new DealerList();
                //dealerList.Region = rdr["Region"].ToString();
                dealerList.DealerState = rdr["State"].ToString();
                dealerList.LOB = rdr["LOB"].ToString();
                dealerList.TIV = rdr["TIV"].ToString();
                dealerList.TMV = rdr["TMV"].ToString();
                dealerList.MS = rdr["MS"].ToString();
                dealerList.YTDTIV = rdr["YTDTIV"].ToString();
                dealerList.YTDTMV = rdr["YTDTMV"].ToString();
                dealerList.YTDMS = rdr["YTDMS"].ToString();
                model.Add(dealerList);
            }
            con.Close();
            TempData["Message"] = "";
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return Json(serializer.Serialize(model), JsonRequestBehavior.AllowGet);
        }

        public bool IsLocked()
        {
            string Login = Session["Uid"].ToString();
            string conn = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            SqlConnection con = new SqlConnection(conn);
            SqlCommand cmd = new SqlCommand("ValidateStateHead", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Login", Login);
            con.Open();
            var reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Session["IsLocked"] = reader["IsLocked"].ToString();
                }
                ViewBag.IsLocked = Session["IsLocked"].ToString();
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
}