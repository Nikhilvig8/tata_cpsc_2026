using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using CPSC.Models;
using Newtonsoft.Json;
using System.Xml;



namespace CPAS.Controllers
{
    public class ActionPlanController : Controller
    {

        //DataRepository dataRepository = new DataRepository();

        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["constr"].ConnectionString);

        // GET: EnterActionPlan
        //[SessionExpireAttribute]
        public ActionResult EnterActionPlan()
        {
            string LoginID = Session["Uid"].ToString();
            string DealerCode = "";
            string Date = "";
            var Model = new List<ActionPlanModel>();

            SqlCommand cmd = new SqlCommand("GetActionPlanData '" + LoginID + "', '" + DealerCode + "','" + Date + "'", con);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

           // ViewBag.DealerDetails = DealerDetails();
            ViewBag.Status = Status();

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; dt.Rows.Count > i; i++)
                {
                    ActionPlanModel actionPlanModel = new ActionPlanModel();

                    actionPlanModel.AID = dt.Rows[i]["AID"].ToString();
                    actionPlanModel.Parameter = dt.Rows[i]["Parameter"].ToString();
                    actionPlanModel.MetricName = dt.Rows[i]["MetricName"].ToString();
                    actionPlanModel.MaxPoint = dt.Rows[i]["MaxPoint"].ToString();
                    actionPlanModel.ParaScoreRYG = dt.Rows[i]["ParaScoreRYG"].ToString();
                    //actionPlanModel.PointWeightage = dt.Rows[i]["PointWeightage"].ToString();

                    Model.Add(actionPlanModel);
                }

            }




            return View(Model);
        }
        public DataTable DealerDetails()
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand("GetDealerbyLogin", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LoginId", "10098");//Session["LoginId"].ToString());
                da.SelectCommand = cmd;
                da.Fill(dt);
                ViewBag.DealerDetail = ToSelectList(dt, "Dealer_Name");


            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return dt;
        }
        public DataTable Status()
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand("GetActionPlanStatus", con);
                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                da.Fill(dt);
                ViewBag.ActionPlans = ToSelectList(dt, "StatusName");


            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return dt;
        }
        public ActionResult UpdateActionPlan()
        {
            string LoginID = Session["Uid"].ToString();
            string DealerCode = "";
            string Date = "";
            var Model = new List<ActionPlanModel>();

            SqlCommand cmd = new SqlCommand("GetActionPlanDataUpdate '" + LoginID + "', '" + DealerCode + "','" + Date + "'", con);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            ViewBag.DealerDetails = DealerDetails();
            ViewBag.Status = Status();

            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["Submited"].ToString() == "1")
                {
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; dt.Rows.Count > i; i++)
                        {
                            ActionPlanModel actionPlanModel = new ActionPlanModel();

                            actionPlanModel.AID = dt.Rows[i]["AID"].ToString();
                            actionPlanModel.Parameter = dt.Rows[i]["Parameter"].ToString();
                            actionPlanModel.MetricName = dt.Rows[i]["MetricName"].ToString();
                            actionPlanModel.MaxPoint = dt.Rows[i]["MaxPoint"].ToString();
                            actionPlanModel.ParaScoreRYG = dt.Rows[i]["ParaScoreRYG"].ToString();
                            actionPlanModel.CurrentStatus = dt.Rows[i]["CurrentStatus"].ToString();
                            actionPlanModel.Observation = dt.Rows[i]["Observation"].ToString();
                            actionPlanModel.Remarks = dt.Rows[i]["Remarks"].ToString();

                            actionPlanModel.ActionPlan = dt.Rows[i]["ActionPlan"].ToString();
                            actionPlanModel.Responsibility = dt.Rows[i]["Responsibility"].ToString();
                            actionPlanModel.Timeline = dt.Rows[i]["Timeline"].ToString();

                            actionPlanModel.DataType = dt.Rows[i]["DataType"].ToString();
                            actionPlanModel.Submited = dt.Rows[i]["Submited"].ToString();
                            actionPlanModel.Score_Dealer = dt.Rows[i]["Score_Dealer"].ToString();

                            Model.Add(actionPlanModel);
                        }

                    }
                }

            }
            return View(Model);

        }
        public List<SelectListItem> ToSelectList(DataTable table, string valueField)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            List<string> dublicate = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                if (!dublicate.Contains(row[valueField].ToString()))
                {
                    dublicate.Add(row[valueField].ToString());
                    list.Add(new SelectListItem()
                    {
                        Value = row[valueField].ToString(),
                        Text = row[valueField].ToString()
                    });
                }
            }
            return list;
        }
        public ActionResult BindActionPlanDetails(string DealerCode, string date)
        {
            string LoginID = Session["Uid"].ToString();
            string Dealer = DealerCode;
            var Model = new List<ActionPlanModel>();

            SqlCommand cmd = new SqlCommand("GetActionPlanData '" + LoginID + "', '" + Dealer + "','" + date + "'", con);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            ViewBag.DealerDetails = DealerDetails();
            ViewBag.Status = Status();

            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["Submited"].ToString() == "1")
                {
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; dt.Rows.Count > i; i++)
                        {
                            ActionPlanModel actionPlanModel = new ActionPlanModel();

                            actionPlanModel.AID = dt.Rows[i]["AID"].ToString();
                            actionPlanModel.Parameter = dt.Rows[i]["Parameter"].ToString();
                            actionPlanModel.MetricName = dt.Rows[i]["MetricName"].ToString();
                            actionPlanModel.MaxPoint = dt.Rows[i]["MaxPoint"].ToString();
                            actionPlanModel.ParaScoreRYG = dt.Rows[i]["ParaScoreRYG"].ToString();
                            actionPlanModel.CurrentStatus = dt.Rows[i]["CurrentStatus"].ToString();
                            actionPlanModel.Observation = dt.Rows[i]["Observation"].ToString();

                            actionPlanModel.ActionPlan = dt.Rows[i]["ActionPlan"].ToString();
                            actionPlanModel.Responsibility = dt.Rows[i]["Responsibility"].ToString();
                            actionPlanModel.Timeline = dt.Rows[i]["Timeline"].ToString();

                            actionPlanModel.DataType = dt.Rows[i]["DataType"].ToString();
                            actionPlanModel.Submited = dt.Rows[i]["Submited"].ToString();
                            actionPlanModel.Score_Dealer = dt.Rows[i]["Score_Dealer"].ToString();

                            Model.Add(actionPlanModel);
                        }

                    }
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; dt.Rows.Count > i; i++)
                        {
                            ActionPlanModel actionPlanModel = new ActionPlanModel();
                            actionPlanModel.AID = dt.Rows[i]["AID"].ToString();
                            actionPlanModel.Parameter = dt.Rows[i]["Parameter"].ToString();
                            actionPlanModel.MetricName = dt.Rows[i]["MetricName"].ToString();
                            actionPlanModel.MaxPoint = dt.Rows[i]["MaxPoint"].ToString();
                            actionPlanModel.ParaScoreRYG = dt.Rows[i]["ParaScoreRYG"].ToString();
                            actionPlanModel.Submited = dt.Rows[i]["Submited"].ToString();
                            actionPlanModel.Score_Dealer = dt.Rows[i]["Score_Dealer"].ToString();

                            Model.Add(actionPlanModel);
                        }

                    }
                }
            }


            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return Json(serializer.Serialize(Model), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult InsertActionPlan(string json)
        {
            XmlDocument XmlDoc;
            XmlDoc = (XmlDocument)JsonConvert.DeserializeXmlNode("{\"Details\":" + json + "}", "ActionPlan");
            //string QualXml = XmlDoc.InnerXml.ToString();
            InsertActionPlanDetails(XmlDoc.InnerXml);
            TempData["Message"] = "Record has been Insert Successfully";
            return RedirectToAction("EnterActionPlan", "ActionPlan");
        }
        public void InsertActionPlanDetails(string dataEntry)
        {
            SqlCommand cmd = new SqlCommand("InsertActionPlanData", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DataEntry", dataEntry);
            cmd.Parameters.AddWithValue("@CreatedBy", Session["Uid"].ToString());

            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
        }
        public ActionResult BindActionPlanDetailsForUpdate(string DealerCode, string Date)
        {
            string LoginID = Session["Uid"].ToString();
            var Model = new List<ActionPlanModel>();

            SqlCommand cmd = new SqlCommand("GetActionPlanDataUpdate '" + LoginID + "', '" + DealerCode + "','" + Date + "'", con);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            ViewBag.DealerDetails = DealerDetails();
            ViewBag.Status = Status();

            if (dt.Rows.Count > 0)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; dt.Rows.Count > i; i++)
                    {
                        ActionPlanModel actionPlanModel = new ActionPlanModel();

                        actionPlanModel.AID = dt.Rows[i]["AID"].ToString();
                        actionPlanModel.Parameter = dt.Rows[i]["Parameter"].ToString();
                        actionPlanModel.MetricName = dt.Rows[i]["MetricName"].ToString();
                        actionPlanModel.MaxPoint = dt.Rows[i]["MaxPoint"].ToString();
                        actionPlanModel.ParaScoreRYG = dt.Rows[i]["ParaScoreRYG"].ToString();
                        actionPlanModel.CurrentStatus = dt.Rows[i]["CurrentStatus"].ToString();
                        actionPlanModel.Observation = dt.Rows[i]["Observation"].ToString();
                        actionPlanModel.Remarks = dt.Rows[i]["Remarks"].ToString();

                        actionPlanModel.ActionPlan = dt.Rows[i]["ActionPlan"].ToString();
                        actionPlanModel.Responsibility = dt.Rows[i]["Responsibility"].ToString();
                        actionPlanModel.Timeline = dt.Rows[i]["Timeline"].ToString();

                        actionPlanModel.DataType = dt.Rows[i]["DataType"].ToString();
                        actionPlanModel.Submited = dt.Rows[i]["Submited"].ToString();
                        actionPlanModel.Score_Dealer = dt.Rows[i]["Score_Dealer"].ToString();

                        Model.Add(actionPlanModel);
                    }

                }

            }
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return Json(serializer.Serialize(Model), JsonRequestBehavior.AllowGet);

        }
        public ActionResult PostUpdateActionPlan(string json)
        {
            XmlDocument XmlDoc;
            XmlDoc = (XmlDocument)JsonConvert.DeserializeXmlNode("{\"Details\":" + json + "}", "ActionPlan");
            //string QualXml = XmlDoc.InnerXml.ToString();
            UpdateActionPlanDetails(XmlDoc.InnerXml);
            TempData["Message"] = "Record has been update successfully";
            return RedirectToAction("EnterActionPlan", "ActionPlan");
        }
        public void UpdateActionPlanDetails(string dataEntry)
        {
            SqlCommand cmd = new SqlCommand("UpdateActionPlanData", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DataEntry", dataEntry);
            cmd.Parameters.AddWithValue("@CreatedBy", Session["Uid"].ToString());

            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
        }

    }


}