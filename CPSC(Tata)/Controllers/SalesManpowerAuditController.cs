using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;
using System.Xml;

namespace InputOutput.Controllers
{
    public class SalesManpowerAuditController : Controller
    {

        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["constr"].ConnectionString);
        // GET: SalesManpowerAudit
        public ActionResult SalesManpowerAudit()
        {


            GetSalesManpowerAuditDealer();
            return View();
        }
        [HttpPost]
        public ActionResult SalesManpowerAuditDetails(String DealerCode)
        {
            try
            {
                String pos = Session["Type"].ToString();
                var model = new List<DealerDetails>();
                SqlCommand cmd = new SqlCommand("GetSalesManpowerAudit", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DealerCode", DealerCode);
                cmd.Parameters.AddWithValue("@Uid", Session["Uid"].ToString());
                cmd.Parameters.AddWithValue("@PositionType", Session["Type"].ToString());
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                List<string> TotalRowCount = new List<string>();

                while (rdr.Read())
                {
                    var dataEntry = new DealerDetails();
                    dataEntry.EmployeeLogin = rdr["Employee Login"].ToString();
                    dataEntry.LOB = rdr["LOB"].ToString();
                    dataEntry.PositionType = rdr["Position Type"].ToString();
                    dataEntry.EmployeeFirstName = rdr["Employee First Name"].ToString();
                    dataEntry.EmployeeMiddleName = rdr["Employee Middle Name"].ToString();
                    dataEntry.EmployeeLastName = rdr["Employee Last Name"].ToString();
                    dataEntry.TotalRowCount = rdr["TotalRowCount"].ToString();
                    dataEntry.UserStatus = rdr["User Status"].ToString();
                    dataEntry.IsActive = rdr["IsActive"].ToString();
                    model.Add(dataEntry);
                }
                con.Close();

                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                return Json(serializer.Serialize(model), JsonRequestBehavior.AllowGet);
                //return View(model);

            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                return View();
            }
        }

        [HttpPost]
        public ActionResult SalesManpowerAuditForDisable(string json)
        {
            XmlDocument XmlDoc;
            XmlDoc = (XmlDocument)JsonConvert.DeserializeXmlNode("{\"SalesManpowerAuditDisable\":" + json + "}", "SalesManpowerAudit");
            UpdateSalesManpowerAuditForDisable(XmlDoc.InnerXml);
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return Json(serializer.Serialize("Record has been submitted Successfully"), JsonRequestBehavior.AllowGet);
        }
        public void UpdateSalesManpowerAuditForDisable(string dataEntry)
        {
            SqlCommand cmd = new SqlCommand("UpdateSalesManpowerAuditForDisable", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@input", dataEntry);
            cmd.Parameters.AddWithValue("@Uid", Session["Uid"].ToString());
            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
        }
        public class DealerDetails
        {
            public string UserStatus { get; set; }
            public string IsActive { get; set; }
            public string EmployeeLogin { get; set; }
            public string LOB { get; set; }
            public string PositionType { get; set; }
            public string EmployeeFirstName { get; set; }
            public string EmployeeMiddleName { get; set; }
            public string EmployeeLastName { get; set; }
            public string DealerCode { get; set; }
            public string DealerName { get; set; }
            public string TotalRowCount { get; set; }

        }

        public ActionResult GetSalesManpowerAuditDealer()
        {
            string a = Session["Uid"].ToString();
            DealerDetails DE = new DealerDetails();
            DataSet ds = new DataSet();
            SqlCommand cmd = new SqlCommand("GetSalesManpowerAuditDealer", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@uid", Session["Uid"].ToString());
            cmd.Parameters.AddWithValue("@PositionType", Session["Type"].ToString());
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            List<DealerDetails> dataEntries = new List<DealerDetails>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DealerDetails uobj = new DealerDetails();
                uobj.DealerCode = ds.Tables[0].Rows[i]["Dealer Code"].ToString();
                uobj.DealerName = ds.Tables[0].Rows[i]["Dealer"].ToString() + "-" + uobj.DealerCode;
                //DealerList(ds.Tables[0].Rows[0]["Distributor_Code"].ToString());
                dataEntries.Add(uobj);
            }
            ViewBag.Dealer = dataEntries;
            con.Close();
            return View();

        }
    }
}