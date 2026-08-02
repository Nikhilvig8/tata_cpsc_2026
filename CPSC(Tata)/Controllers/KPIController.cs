using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using InputOutput.Models;
using System.Data.SqlClient;
using System.Configuration;
using Execution;


namespace InputOutput.Controllers
{
    [HandleError()]
    public class KPIController : Controller
    {
        // GET: KPI

        string cs = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        public ActionResult Index()
        {
        
return View();
        }

        public ActionResult AMIndex()
        {
            return View();
        }

        public ActionResult Div_List_With_No_Data()
        {
            return View();
        }
        public ActionResult DealerList()
        {
            return View();
        }
        public ActionResult DealerScorecard()
        {
            return View();
        }
        public ActionResult AMTarget(FormCollection coll)
        {
            string SheetFlag = coll.Get("SheetFlag");
            string s;
            int count;
            bool flag;
            List<InputOutput.Models.Transaction> datatable = new List<Transaction>();
            Transaction t1 = new Transaction();
            TransactionViewModel tvm = new TransactionViewModel();
            DataTable dt;
            t1.Month = coll.Get("Month");
            t1.year = coll.Get("year");
            string out1 = string.Empty;
            out1 = coll.Get("OUT");
            var piece = out1.Split('/');

            t1.Out=piece[0] ;
            t1.Dealer = piece[1];
            t1.LOB = coll.Get("LOB");
            t1.TSM_id = coll.Get("User_id");
            s = coll.Get("submitButton");
           if (s == "Confirm")
            {
                count = Convert.ToInt32(coll.Get("Count"));
                for (int i = 1;i<= count;i++)
                {
                    t1.KPI = coll.Get("KPI"+i);
                    t1.Target = coll.Get("Target"+i);
                    flag = t1.Update_Data(t1.Dealer, t1.Month, t1.LOB, t1.KPI, t1.Target,t1.Out);
                }
                t1.notification_Insert(t1.Month, t1.LOB, t1.Dealer,t1.Out,"Target");
                t1.updateTarget_UserId( t1.Month, t1.LOB, t1.Dealer, t1.Out);



                if (!string.IsNullOrEmpty(t1.Month) && !string.IsNullOrEmpty(t1.LOB))
                {

                    dt = t1.Transation_Data(t1.Dealer, t1.Month,t1.LOB,t1.Out,SheetFlag,t1.year);

                    foreach (DataRow row in dt.Rows)
                    {
                        Transaction t = new Transaction();
                        t.premise_location = Convert.ToString(row["Premise_Location"]);
                        t.Dealer_Level = Convert.ToString(row["Dealer_Level"]);
                        t.Division_Level = Convert.ToString(row["Division_Level"]);
                        t.LOB_Level = Convert.ToString(row["LOB_Level"]);
                        t.Sorting = Convert.ToString(row["Sorting"]);
                        t.KPI = Convert.ToString(row["KPI_Name"]);
                        t.Target = Convert.ToString(row["Target_Value"]);
                        t.Actual = Convert.ToString(row["Actual_Value"]);
                        t.Last_Month_Target = Convert.ToString(row["Last_M_T"]);
                        t.Last_Month_Actual = Convert.ToString(row["Last_M_A"]);
                        t.Last_12_Month_Avg = Convert.ToString(row["Last_12_M_Avg"]);
                        t.Last_12_Month_Min = Convert.ToString(row["Last_12_M_Min"]);
                        t.Last_12_Month_Max = Convert.ToString(row["Last_12_M_Max"]);
                        t.SingelEntry_Tooltip_Target= Convert.ToString(row["SingleEntry_ToolTip_Target"]);
                        t.SingelEntry_Tooltip_Actual = Convert.ToString(row["SingleEntry_ToolTip_Actual"]);
                        t.Input_Type = Convert.ToString(row["Input_Type"]);
                        t.Input_Pattern = Convert.ToString(row["Input_Pattern"]);
                        t.Input_OnKey = Convert.ToString(row["Input_OnKey"]);
                        t.Input_Popup = Convert.ToString(row["Input_Popup"]);
                        t.KPI_Description = Convert.ToString(row["KPI_Description"]);
                        t.IsTargetRequired = Convert.ToInt32(row["IsTargetRequired"]);
                        t.IsActualRequired = Convert.ToInt32(row["IsActualRequired"]);
                        tvm.Trans_list.Add(t);
                    }
                    tvm.Trans = t1;

                   
                }
                return RedirectToAction("AMIndex","KPI");
            }
            else
            {
                if (!string.IsNullOrEmpty(t1.Month) && !string.IsNullOrEmpty(t1.LOB) && t1.LOB !="LOB")
                {

                    dt = t1.Transation_Data(t1.Dealer, t1.Month, t1.LOB,t1.Out,SheetFlag,t1.year);

                    foreach (DataRow row in dt.Rows)
                    {
                        Transaction t = new Transaction();
                        t.premise_location = Convert.ToString(row["Premise_Location"]);
                        t.Dealer_Level = Convert.ToString(row["Dealer_Level"]);
                        t.Division_Level = Convert.ToString(row["Division_Level"]);
                        t.LOB_Level = Convert.ToString(row["LOB_Level"]);
                        t.Sorting = Convert.ToString(row["Sorting"]);
                        t.KPI = Convert.ToString(row["KPI_Name"]);
                        t.Target = Convert.ToString(row["Target_Value"]);
                        t.Actual = Convert.ToString(row["Actual_Value"]);
                        t.Last_Month_Target = Convert.ToString(row["Last_M_T"]);
                        t.Last_Month_Actual = Convert.ToString(row["Last_M_A"]);
                        t.Last_12_Month_Avg = Convert.ToString(row["Last_12_M_Avg"]);
                        t.Last_12_Month_Min = Convert.ToString(row["Last_12_M_Min"]);
                        t.Last_12_Month_Max = Convert.ToString(row["Last_12_M_Max"]);
                        t.SingelEntry_Tooltip_Target = Convert.ToString(row["SingleEntry_ToolTip_Target"]);
                        t.SingelEntry_Tooltip_Actual = Convert.ToString(row["SingleEntry_ToolTip_Actual"]);
                        t.Input_Type = Convert.ToString(row["Input_Type"]);
                        t.Input_Pattern = Convert.ToString(row["Input_Pattern"]);
                        t.Input_OnKey = Convert.ToString(row["Input_OnKey"]);
                        t.Input_Popup = Convert.ToString(row["Input_Popup"]);
                        t.KPI_Description = Convert.ToString(row["KPI_Description"]);
                        t.IsTargetRequired = Convert.ToInt32(row["IsTargetRequired"]);
                        t.IsActualRequired = Convert.ToInt32(row["IsActualRequired"]);

                        tvm.Trans_list.Add(t);
                    }
                    tvm.Trans = t1;

                    return View(tvm);
                }
                else
                {
                   

                    return RedirectToAction("AMIndex","KPI");

                }
            }
        }
        public ActionResult AMActualIndex()
        {
            return View();
        }

        public ActionResult Dialog(FormCollection coll)
        {
            bool flag;
            int count;
            string s = string.Empty;
            count=Convert.ToInt32(coll.Get("count"));
            Transaction t1 = new Transaction();
            s=  coll.Get("submitButton");
            if (s == "Submit") { 
            for (int i = 0; i < count; i++)
            {
                flag = t1.updatehyg(coll.Get("no" + i), coll.Get("des" + i), coll.Get("Select_Value" + i));
            }
            }

            return View();
        }
        public ActionResult Hygiene()
        {
            return View();
        }

        public ActionResult DSE()
        {
            return View();
        }

        public ActionResult DSM()
        {
            return View();
        }

        public ActionResult DataReports()
        {
            return View();
        }
        public ActionResult DataReportsDO()
        {
            return View();
        }
        public ActionResult DataReportsDS()
        {
            return View();
        }
        public ActionResult DataReportsPA()
        {
            return View();
        }
        public ActionResult DataReports_actual()
        {
            return View();
        }
        public ActionResult DataReportsDO_actual()
        {
            return View();
        }
        public ActionResult DataReportsDS_actual()
        {
            return View();
        }
        public ActionResult DataReportsPA_actual()
        {
            return View();
        }

        //[Execution_Logs]
        public ActionResult BulkDataSheet( FormCollection coll)
        {
            Session["result"] = string.Empty;
            int Columncount;
            int Rowcount;
            string s;
            string div_prev=string.Empty, lob_prev=string.Empty;
            string year = string.Empty;
            
            List<InputOutput.Models.Transaction> datatable = new List<Transaction>();
            Transaction t1 = new Transaction();
            TransactionViewModel tvm = new TransactionViewModel();
            //DataTable dt;
            bool flag;
            t1.Month = coll.Get("Month");
            year = coll.Get("year");
            t1.Dealer = coll.Get("Dealer");

            
            s = coll.Get("submitButton");
            if (s == "Submit")
            {
                Rowcount = Convert.ToInt32(coll.Get("row_count"));
                Columncount = Convert.ToInt32(coll.Get("column_count"));
                string Notification_id = Guid.NewGuid().ToString();
                for (int i = 0; i < Rowcount; i++)
                {
                    for (int j = 0; j < Columncount; j++)
                    {
                        if (j == 2) { t1.Out = coll.Get("Division_Id" + i + j); }
                        else if (j == 4) { t1.LOB = coll.Get("LOB_ID" + i + j); }
                        else if (j == 0 || j == 1 || j == 3 || j == 5 || j == 6 || j == 7 || j == 8) { }
                        else
                        {
                            t1.Actual = coll.Get("actual" + i + j);
                            t1.KPI = coll.Get("kpi_id" + i + j);
                            if(t1.Actual=="")
                            {
                                t1.Actual = null;
                            }
                            flag = t1.Update_BULKActualData(t1.Dealer, year,t1.Month, t1.LOB, t1.KPI, t1.Actual, t1.Out, Notification_id);
                        }
                        //t1.LOB = coll.Get("LOB_ID"+i+j);
                        //t1.Out = coll.Get("Division_Id" + i + j);
                        //t1.Target = coll.Get("target" + i + j);
                        //t1.KPI = coll.Get("kpi_id"+ j);


                    }
                   
                    if (div_prev != t1.Out || lob_prev != t1.LOB)
                    {
                        t1.Bulknotification_Insert(t1.Month, t1.LOB, t1.Dealer, t1.Out, "BulkActual", Notification_id,year);
                        t1.Email(t1.Dealer, "BulkActual", Notification_id);
                        div_prev = t1.Out;
                        lob_prev = t1.LOB;
                        Notification_id = Guid.NewGuid().ToString();
                    }
                }
            }
            //t1.Bulknotification_Insert(t1.Month, "%", t1.Dealer, "%", "BulkActual",Notification_id);
            
            t1.updateActualBulk_UserId(t1.Month, t1.Dealer,year);
            return View();
        }

        public ActionResult BulkDataSheet_Quarterly(FormCollection coll)
        {
            Session["result"] = string.Empty;
            int Columncount;
            int Rowcount;
            string s;
            //string Notification_id = Guid.NewGuid().ToString();
            string div_prev = string.Empty, lob_prev = string.Empty;
            List<InputOutput.Models.Transaction> datatable = new List<Transaction>();
            Transaction t1 = new Transaction();
            TransactionViewModel tvm = new TransactionViewModel();
            //DataTable dt;
            bool flag;
            t1.Month = coll.Get("Month");
            string year= coll.Get("year");
            t1.Dealer = coll.Get("Dealer");


            s = coll.Get("submitButton");
            if (s == "Submit")
            {
                Rowcount = Convert.ToInt32(coll.Get("row_count"));
                Columncount = Convert.ToInt32(coll.Get("column_count"));
                string Notification_id = Guid.NewGuid().ToString();
                for (int i = 0; i < Rowcount; i++)
                {
                    for (int j = 0; j < Columncount; j++)
                    {
                        if (j == 2) { t1.Out = coll.Get("Division_Id" + i + j); }
                        else if (j == 4) { t1.LOB = coll.Get("LOB_ID" + i + j); }
                        else if (j == 0 || j == 1 || j == 3 || j == 5 || j == 6 || j == 7 || j == 8) { }
                        else
                        {
                            t1.Actual = coll.Get("actual" + i + j);
                            t1.KPI = coll.Get("kpi_id" + i + j);
                            if (t1.Actual == "")
                            {
                                t1.Actual = null;
                            }
                            flag = t1.Update_BULKActualData(t1.Dealer, year,t1.Month, t1.LOB, t1.KPI, t1.Actual, t1.Out,Notification_id);
                        }
                        //t1.LOB = coll.Get("LOB_ID"+i+j);
                        //t1.Out = coll.Get("Division_Id" + i + j);
                        //t1.Target = coll.Get("target" + i + j);
                        //t1.KPI = coll.Get("kpi_id"+ j);


                    }
                    //flag = t1.Update_BULKActualData(t1.Dealer, t1.Month, t1.LOB, t1.KPI, t1.Actual, t1.Out, Notification_id);
                    if (div_prev != t1.Out || lob_prev != t1.LOB)
                    {
                        t1.Bulknotification_Insert(t1.Month, t1.LOB, t1.Dealer, t1.Out, "BulkActual", Notification_id,year);
                        t1.Email(t1.Dealer, "BulkActual", Notification_id);
                        div_prev = t1.Out;
                        lob_prev = t1.LOB;
                        Notification_id = Guid.NewGuid().ToString();
                    }

                }
            }
            //t1.Bulknotification_Insert(t1.Month, "%", t1.Dealer, "%", "BulkActual",Notification_id);
           // t1.Email(t1.Dealer, "BulkActual", Notification_id);
            t1.updateActualBulk_UserId(t1.Month, t1.Dealer,year);
            return View();
        }

        public ActionResult BulkDataSheetManpowerTarget(FormCollection coll)
        {
            Session["result"] = string.Empty;
            int Columncount;
            int Rowcount;
            string s;
            //string notification_id = Guid.NewGuid().ToString();
            string div_prev = string.Empty, lob_prev = string.Empty;
            List<InputOutput.Models.Transaction> datatable = new List<Transaction>();
            Transaction t1 = new Transaction();
            TransactionViewModel tvm = new TransactionViewModel();
            //DataTable dt;
            bool flag;
            t1.Month = coll.Get("Month");
            string year = coll.Get("year");
            t1.Dealer = coll.Get("Dealer");


            s = coll.Get("submitButton");
            if (s == "Submit")
            {
                Rowcount = Convert.ToInt32(coll.Get("row_count"));
                Columncount = Convert.ToInt32(coll.Get("column_count"));
                string Notification_id = Guid.NewGuid().ToString();
                for (int i = 0; i < Rowcount; i++)
                {
                    for (int j = 0; j < Columncount; j++)
                    {
                        if (j == 2) { t1.Out = coll.Get("Division_Id" + i + j); }
                        else if (j == 4) { t1.LOB = coll.Get("LOB_ID" + i + j); }
                        else if (j == 0 || j == 1 || j == 3 || j == 5 || j == 6 || j == 7 || j == 8) { }
                        else
                        {
                            t1.Target = coll.Get("target" + i + j);
                            t1.KPI = coll.Get("kpi_id" + i + j);
                            if (t1.Target == "")
                            {
                                t1.Target = null;
                            }
                            flag = t1.Update_BULKManpowerData(t1.Dealer, t1.Month, t1.LOB, t1.KPI, t1.Target, t1.Out, Notification_id, year);
                        }
                        //t1.LOB = coll.Get("LOB_ID"+i+j);
                        //t1.Out = coll.Get("Division_Id" + i + j);
                        //t1.Target = coll.Get("target" + i + j);
                        //t1.KPI = coll.Get("kpi_id"+ j);


                    }

                    if (div_prev != t1.Out || lob_prev != t1.LOB)
                    {
                        t1.Bulknotification_Insert(t1.Month, t1.LOB, t1.Dealer, t1.Out, "BulkTarget", Notification_id, year);
                        t1.Email(t1.Dealer, "BulkTarget", Notification_id);
                        div_prev = t1.Out;
                        lob_prev = t1.LOB;
                        Notification_id = Guid.NewGuid().ToString();
                    }
                }

                //t1.Bulknotification_Insert(t1.Month, "%", t1.Dealer, "%", "BulkTarget",notification_id);
                //t1.Email(t1.Dealer, "BulkTarget", notification_id);
                t1.updateTargetBulk_UserId(t1.Month, t1.Dealer, year);
            }
            return View();
        }

        //[Execution_Logs]
        public ActionResult BulkDataSheetTarget( FormCollection coll)
        {
            Session["result"] = string.Empty;
            int Columncount;
            int Rowcount;
            string s;
            //string notification_id = Guid.NewGuid().ToString();
            string div_prev = string.Empty, lob_prev = string.Empty;
            List<InputOutput.Models.Transaction> datatable = new List<Transaction>();
            Transaction t1 = new Transaction();
            TransactionViewModel tvm = new TransactionViewModel();
            //DataTable dt;
            bool flag;
            t1.Month = coll.Get("Month");
            string year = coll.Get("year");
            t1.Dealer = coll.Get("Dealer");
            
           
            s = coll.Get("submitButton");
            if (s == "Submit")
            {
                Rowcount = Convert.ToInt32(coll.Get("row_count"));
                Columncount = Convert.ToInt32(coll.Get("column_count"));
                string Notification_id = Guid.NewGuid().ToString();
                for (int i = 0; i < Rowcount; i++)
                {
                    for (int j = 0; j < Columncount; j++)
                    {
                        if (j == 2) { t1.Out = coll.Get("Division_Id" + i + j); }
                        else if (j == 4) { t1.LOB = coll.Get("LOB_ID" + i + j); }
                        else if (j == 0 || j == 1 || j == 3 || j == 5 || j == 6 || j == 7 || j == 8) { }
                        else 
                        {
                            t1.Target = coll.Get("target" + i + j);
                            t1.KPI = coll.Get("kpi_id" + i + j);
                            if (t1.Target == "")
                            {
                                t1.Target = null;
                            }
                            flag = t1.Update_BULKData(t1.Dealer, t1.Month, t1.LOB, t1.KPI, t1.Target, t1.Out, Notification_id,year);
                        }
                        //t1.LOB = coll.Get("LOB_ID"+i+j);
                        //t1.Out = coll.Get("Division_Id" + i + j);
                        //t1.Target = coll.Get("target" + i + j);
                        //t1.KPI = coll.Get("kpi_id"+ j);

                        
                    }
                    
                    if (div_prev != t1.Out || lob_prev != t1.LOB)
                    {
                        t1.Bulknotification_Insert(t1.Month, t1.LOB, t1.Dealer, t1.Out, "BulkTarget", Notification_id,year);
                        t1.Email(t1.Dealer, "BulkTarget", Notification_id);
                        div_prev = t1.Out;
                        lob_prev = t1.LOB;
                        Notification_id = Guid.NewGuid().ToString();
                    }
                }

                //t1.Bulknotification_Insert(t1.Month, "%", t1.Dealer, "%", "BulkTarget",notification_id);
                //t1.Email(t1.Dealer, "BulkTarget", notification_id);
                t1.updateTargetBulk_UserId(t1.Month,t1.Dealer,year);
            }          return View();
        }


        public ActionResult BulkDataSheetTarget_Quarterly(FormCollection coll)
        {
            Session["result"] = string.Empty;
            int Columncount;
            int Rowcount;
            string s;
            string div_prev = string.Empty, lob_prev = string.Empty;
            //string notification_id = Guid.NewGuid().ToString();
            List<InputOutput.Models.Transaction> datatable = new List<Transaction>();
            Transaction t1 = new Transaction();
            TransactionViewModel tvm = new TransactionViewModel();
            //DataTable dt;
            bool flag;

            t1.Month = coll.Get("Month");
            t1.Dealer = coll.Get("Dealer");
            string year = coll.Get("year");

            s = coll.Get("submitButton");
            if (s == "Submit")
            {
                Rowcount = Convert.ToInt32(coll.Get("row_count"));
                Columncount = Convert.ToInt32(coll.Get("column_count"));
                string Notification_id = Guid.NewGuid().ToString();
                for (int i = 0; i < Rowcount; i++)
                {
                    for (int j = 0; j < Columncount; j++)
                    {
                        if (j == 2) { t1.Out = coll.Get("Division_Id" + i + j); }
                        else if (j == 4) { t1.LOB = coll.Get("LOB_ID" + i + j); }
                        else if (j == 0 || j == 1 || j == 3 || j == 5 || j == 6 || j == 7 || j == 8) { }
                        else
                        {
                            t1.Target = coll.Get("target" + i + j);
                            t1.KPI = coll.Get("kpi_id" + i + j);
                            if (t1.Target == "")
                            {
                                t1.Target = null;
                            }
                            flag = t1.Update_BULKData(t1.Dealer, t1.Month, t1.LOB, t1.KPI, t1.Target, t1.Out,Notification_id,year);
                        }
                        //t1.LOB = coll.Get("LOB_ID"+i+j);
                        //t1.Out = coll.Get("Division_Id" + i + j);
                        //t1.Target = coll.Get("target" + i + j);
                        //t1.KPI = coll.Get("kpi_id"+ j);


                    }

                    if (div_prev != t1.Out || lob_prev != t1.LOB)
                    {
                        t1.Bulknotification_Insert(t1.Month, t1.LOB, t1.Dealer, t1.Out, "BulkTarget", Notification_id,year);
                        t1.Email(t1.Dealer, "BulkTarget", Notification_id);
                        div_prev = t1.Out;
                        lob_prev = t1.LOB;
                        Notification_id = Guid.NewGuid().ToString();
                    }
                }

                //t1.Bulknotification_Insert(t1.Month, "%", t1.Dealer, "%", "BulkTarget",notification_id);
                //t1.Email(t1.Dealer, "BulkTarget", notification_id);
                t1.updateTargetBulk_UserId(t1.Month, t1.Dealer,year);
            }
            return View();
        }
        public ActionResult AMActual(FormCollection coll)
        {
            string SheetFlag = coll.Get("SheetFlag");
            string s;
            int count;
            bool flag;
            List<InputOutput.Models.Transaction> datatable = new List<Transaction>();
            Transaction t1 = new Transaction();
            TransactionViewModel tvm = new TransactionViewModel();
            DataTable dt;
            t1.Month = coll.Get("Month");
            t1.year = coll.Get("year");
            string out1 = string.Empty;
            out1 = coll.Get("OUT");
            var piece = out1.Split('/');

            t1.Out = piece[0];
            t1.Dealer = piece[1];
            t1.LOB = coll.Get("LOB");
            t1.TSM_id = coll.Get("User_id");
            s = coll.Get("submitButton");
            if (s == "Confirm")
            {
                count = Convert.ToInt32(coll.Get("Count"));
                for (int i = 1; i <= count; i++)
                {
                    t1.KPI = coll.Get("KPI" + i);
                    t1.Actual = coll.Get("Actual" + i);
                    flag = t1.Update_ActualData(t1.Dealer, t1.Month, t1.LOB, t1.KPI, t1.Actual, t1.Out);
                }
                t1.notification_Insert(t1.Month, t1.LOB, t1.Dealer, t1.Out, "Actual");
                t1.updateActual_UserId(t1.Month, t1.LOB, t1.Dealer, t1.Out);



                if (!string.IsNullOrEmpty(t1.Month) && !string.IsNullOrEmpty(t1.LOB))
                {

                    dt = t1.Transation_Data(t1.Dealer, t1.Month, t1.LOB, t1.Out,SheetFlag,t1.year);

                    foreach (DataRow row in dt.Rows)
                    {
                        Transaction t = new Transaction();
                        t.premise_location = Convert.ToString(row["Premise_Location"]);
                        t.Dealer_Level = Convert.ToString(row["Dealer_Level"]);
                        t.Division_Level = Convert.ToString(row["Division_Level"]);
                        t.LOB_Level = Convert.ToString(row["LOB_Level"]);
                        t.Sorting = Convert.ToString(row["Sorting"]);
                        t.KPI = Convert.ToString(row["KPI_Name"]);
                        t.Target = Convert.ToString(row["Target_Value"]);
                        t.Actual = Convert.ToString(row["Actual_Value"]);
                        t.Last_Month_Target = Convert.ToString(row["Last_M_T"]);
                        t.Last_Month_Actual = Convert.ToString(row["Last_M_A"]);
                        t.Last_12_Month_Avg = Convert.ToString(row["Last_12_M_Avg_For_Actual"]);
                        t.Last_12_Month_Min = Convert.ToString(row["Last_12_M_Min_For_Actual"]);
                        t.Last_12_Month_Max = Convert.ToString(row["Last_12_M_Max_For_Actual"]);
                        t.SingelEntry_Tooltip_Target = Convert.ToString(row["SingleEntry_ToolTip_Target"]);
                        t.SingelEntry_Tooltip_Actual = Convert.ToString(row["SingleEntry_ToolTip_Actual"]);
                        t.Input_Type = Convert.ToString(row["Input_Type"]);
                        t.Input_Pattern = Convert.ToString(row["Input_Pattern"]);
                        t.Input_OnKey = Convert.ToString(row["Input_OnKey"]);
                        t.Input_Popup = Convert.ToString(row["Input_Popup"]);
                        t.KPI_Description = Convert.ToString(row["KPI_Description"]);
                        t.IsTargetRequired = Convert.ToInt32(row["IsTargetRequired"]);
                        t.IsActualRequired = Convert.ToInt32(row["IsActualRequired"]);


                        tvm.Trans_list.Add(t);
                    }
                    tvm.Trans = t1;


                }
                return RedirectToAction("AMActualIndex", "KPI"); 
            }
            else
            {
                if (!string.IsNullOrEmpty(t1.Month) && !string.IsNullOrEmpty(t1.LOB) && t1.LOB != "LOB")
                {

                    dt = t1.Transation_Data(t1.Dealer, t1.Month, t1.LOB, t1.Out,SheetFlag,t1.year);

                    foreach (DataRow row in dt.Rows)
                    {
                        Transaction t = new Transaction();
                        t.premise_location = Convert.ToString(row["Premise_Location"]);
                        t.Dealer_Level = Convert.ToString(row["Dealer_Level"]);
                        t.Division_Level = Convert.ToString(row["Division_Level"]);
                        t.LOB_Level = Convert.ToString(row["LOB_Level"]);
                        t.Sorting = Convert.ToString(row["Sorting"]);
                        t.KPI = Convert.ToString(row["KPI_Name"]);
                        t.Target = Convert.ToString(row["Target_Value"]);
                        t.Actual = Convert.ToString(row["Actual_Value"]);
                        t.Last_Month_Target = Convert.ToString(row["Last_M_T"]);
                        t.Last_Month_Actual = Convert.ToString(row["Last_M_A"]);
                        t.Last_12_Month_Avg = Convert.ToString(row["Last_12_M_Avg_For_Actual"]);
                        t.Last_12_Month_Min = Convert.ToString(row["Last_12_M_Min_For_Actual"]);
                        t.Last_12_Month_Max = Convert.ToString(row["Last_12_M_Max_For_Actual"]);
                        t.SingelEntry_Tooltip_Target = Convert.ToString(row["SingleEntry_ToolTip_Target"]);
                        t.SingelEntry_Tooltip_Actual = Convert.ToString(row["SingleEntry_ToolTip_Actual"]);
                        t.Input_Type = Convert.ToString(row["Input_Type"]);
                        t.Input_Pattern = Convert.ToString(row["Input_Pattern"]);
                        t.Input_OnKey = Convert.ToString(row["Input_OnKey"]);
                        t.Input_Popup = Convert.ToString(row["Input_Popup"]);
                        t.KPI_Description = Convert.ToString(row["KPI_Description"]);
                        t.IsTargetRequired = Convert.ToInt32(row["IsTargetRequired"]);
                        t.IsActualRequired = Convert.ToInt32(row["IsActualRequired"]);

                        tvm.Trans_list.Add(t);
                    }
                    tvm.Trans = t1;

                    return View(tvm);
                }
                else
                {


                    return RedirectToAction("AMActualIndex", "KPI");

                }
            }
        }

        public ActionResult Notification(string Uid,string fromwhom,string Flag,string dealer,string month,string LOB, string o,string dealer_name, string LOB_name,string o_name,string monthdesc ,FormCollection coll)
        {

            string s;
       
            bool flag;
            List<InputOutput.Models.Transaction> datatable = new List<Transaction>();
            Transaction t1 = new Transaction();
            TransactionViewModel tvm = new TransactionViewModel();
            DataTable dt;
           
            s = coll.Get("submitButton");
            if (s == "Approved")
            {
                t1.Unique_Id = coll.Get("Unique_id");
                t1.Flag = coll.Get("Flag");
                string Notifocation_id = coll.Get("Notification_id");
                t1.Month = coll.Get("Month");
                t1.year = coll.Get("year");
                t1.Dealer = coll.Get("Dealer_Id");
                t1.Out = coll.Get("Division_Id");
                t1.LOB = coll.Get("LOB_Id");
                t1.monthdesc = coll.Get("Month_Name");
                t1.Dealer_Name = coll.Get("Dealer_Name");
                t1.Out_Name = coll.Get("Division_Name");
                t1.LOB_Name = coll.Get("LOB_Name");
                string remarks= coll.Get("remarks");

                flag = t1.Update_Flag(t1.Unique_Id,t1.Flag, t1.Out, t1.LOB,"accept",fromwhom,remarks, Notifocation_id);
                using (var cn = new SqlConnection(cs))
                {
                    string _sql = @"select Approved_Status from tblTarget_Actual_Approvels " + @"WHERE Unique_Id = @u and [Type]=@f and [LOB_Id]=@LOB";
                    var cmd = new SqlCommand(_sql, cn);
                    cmd.Parameters
                        .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                        .Value = t1.Unique_Id;
                    cmd.Parameters
                        .Add(new SqlParameter("@LOB", SqlDbType.NVarChar))
                        .Value = t1.LOB;
                    cmd.Parameters
                        .Add(new SqlParameter("@f", SqlDbType.NVarChar))
                        .Value = t1.Flag;

                    cn.Open();
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                           
                                t1.TAFlag = reader["Approved_Status"].ToString();
                           
                        }
                        reader.Dispose();
                        cmd.Dispose();
                    
                    }
                    else
                    {
                        reader.Dispose();
                        cmd.Dispose();
                     
                    }
                }

                if (!string.IsNullOrEmpty(t1.Unique_Id) && !string.IsNullOrEmpty(t1.Flag))
                {


                    dt = t1.Approval_Transation_Data(fromwhom, t1.Dealer, t1.Month, t1.LOB, t1.Out, Flag,t1.year);


                    foreach (DataRow row in dt.Rows)
                    {
                        Transaction t = new Transaction();

                        t.KPI = Convert.ToString(row["KPI_Name"]);
                        t.Dealer_Level = Convert.ToString(row["Dealer_Level"]);
                        t.Division_Level = Convert.ToString(row["Division_Level"]);
                        t.LOB_Level = Convert.ToString(row["LOB_Level"]);
                        t.Target = Convert.ToString(row["Target_Value"]);
                        t.Actual = Convert.ToString(row["Actual_Value"]);
                        t.Last_Month_Target = Convert.ToString(row["Last_M_T"]);
                        t.Last_Month_Actual = Convert.ToString(row["Last_M_A"]);
                        t.Last_12_Month_Avg = Convert.ToString(row["Last_12_M_Avg"]);
                        t.Last_12_Month_Min = Convert.ToString(row["Last_12_M_Min"]);
                        t.Last_12_Month_Max = Convert.ToString(row["Last_12_M_Max"]);
                        t.SingelEntry_Tooltip_Target = Convert.ToString(row["SingleEntry_ToolTip_Target"]);
                        t.SingelEntry_Tooltip_Actual = Convert.ToString(row["SingleEntry_ToolTip_Actual"]);
                        t.IsActualRequired= Convert.ToInt32(row["IsActualRequired"]);
                        t.IsTargetRequired= Convert.ToInt32(row["IsTargetRequired"]);
                        tvm.Trans_list.Add(t);
                     
                    }

                    
                    tvm.Trans = t1;


                }
                return RedirectToAction("ShowAllNotification", "KPI", new { Uid = Session["Uid"].ToString() });
            }
            else
            {
                using (var cn = new SqlConnection(cs))
                {
                    string _sql = @"select Approved_Status from tblTarget_Actual_Approvels " + @"WHERE Unique_Id = @u and [Type]=@f and [LOB_id]=@LOB ";
                    var cmd = new SqlCommand(_sql, cn);
                    cmd.Parameters
                        .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                        .Value = Uid;
                    cmd.Parameters
                        .Add(new SqlParameter("@LOB", SqlDbType.NVarChar))
                        .Value = LOB;
                    cmd.Parameters
                        .Add(new SqlParameter("@f", SqlDbType.NVarChar))
                        .Value = Flag;

                    cn.Open();
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            t1.TAFlag = reader["Approved_Status"].ToString();

                        }
                        reader.Dispose();
                        cmd.Dispose();

                    }
                    else
                    {
                        reader.Dispose();
                        cmd.Dispose();

                    }
                }
                if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(Flag))
                {
                    t1.Unique_Id = Uid;
                    t1.Flag = Flag;
                    t1.Month = month;
                    t1.Dealer = dealer;
                    t1.Out = o;
                    t1.LOB = LOB;
                    t1.Dealer_Name = dealer_name;
                    t1.Out_Name = o_name;
                    t1.LOB_Name = LOB_name;
                    t1.monthdesc = monthdesc;

                    //dt = t1.Approved_Data(Uid);
                    dt = t1.Approval_Transation_Data(fromwhom, dealer, month, LOB, o,Flag,t1.year);
                    foreach (DataRow row in dt.Rows)
                    {
                        Transaction t = new Transaction();
                        t.KPI = Convert.ToString(row["KPI_Name"]);
                        t.Dealer_Level = Convert.ToString(row["Dealer_Level"]);
                        t.Division_Level = Convert.ToString(row["Division_Level"]);
                        t.LOB_Level = Convert.ToString(row["LOB_Level"]);
                        t.Target = Convert.ToString(row["Target_Value"]);
                        t.Actual = Convert.ToString(row["Actual_Value"]);
                        t.Last_Month_Target = Convert.ToString(row["Last_M_T"]);
                        t.Last_Month_Actual = Convert.ToString(row["Last_M_A"]);
                        t.Last_12_Month_Avg = Convert.ToString(row["Last_12_M_Avg"]);
                        t.Last_12_Month_Min = Convert.ToString(row["Last_12_M_Min"]);
                        t.Last_12_Month_Max = Convert.ToString(row["Last_12_M_Max"]);
                        t.SingelEntry_Tooltip_Target = Convert.ToString(row["SingleEntry_ToolTip_Target"]);
                        t.SingelEntry_Tooltip_Actual = Convert.ToString(row["SingleEntry_ToolTip_Actual"]);
                        t.IsActualRequired = Convert.ToInt32(row["IsActualRequired"]);
                        t.IsTargetRequired = Convert.ToInt32(row["IsTargetRequired"]);

                        tvm.Trans_list.Add(t);
                       
                    }
                    tvm.Trans = t1;

                    return View(tvm);
                }
                else
                {


                    return RedirectToAction("AMIndex", "KPI");

                }
            }
          
        }



        public ActionResult AllNotificationSubmission(FormCollection coll)
        {

            string s,s1;

            bool flag;
            DataTable dt;

            Transaction t1 = new Transaction();

            s = coll.Get("submitButton");
            s1 = coll.Get("dissubmitButton");

            string Notifocation_id;
            string chk;
            int Rowcount = Convert.ToInt32(coll.Get("loopcount"));

            if (s == "Approve" && s1 ==null)
            {

                for (int i = 1; i < Rowcount; i++)
                {
                    Notifocation_id = coll.Get("N_id" + i);
                    chk = coll.Get("chk" + i);

                    if(chk == null)
                    {
                        chk = "off";
                    }

                    if(!string.IsNullOrEmpty(chk))
                    { 
                    if (chk.ToString() == "on")
                    {
                        t1.Update_AllCheckedNotification(Notifocation_id, s);                      
                    }

                }
                }
            }

            else
            {
                              
                for (int i = 1; i < Rowcount; i++)
                {
                    Notifocation_id = coll.Get("N_id" + i);
                    chk = coll.Get("chk" + i);
                    
                    if (chk == null)
                    {
                        chk = "off";
                    }

                    if (!string.IsNullOrEmpty(chk))
                    {
                        if (chk.ToString() == "on")
                        {

                            t1.Update_AllCheckedNotification(Notifocation_id, s1);
                           
                        }
                    }
                }

            }          


            return RedirectToAction("ShowAllNotification", "KPI", new { Uid = Session["Uid"].ToString() });
        }

        //[Execution_Logs]
        public ActionResult BULKNotification(string Uid, string fromwhom, string Flag, string dealer, string month, string LOB, string o, string dealer_name, string LOB_name, string o_name, string monthdesc,string Notification_id,string remarks_out,string year,FormCollection coll)
        {

            string s;
            string s1,s2;
            int Columncount;
            int Rowcount;
            string notification_id = "NULL";
            string remarks = string.Empty;
            bool flag;
            List<InputOutput.Models.Transaction> datatable = new List<Transaction>();
            Transaction t1 = new Transaction();
            TransactionViewModel tvm = new TransactionViewModel();
            DataTable dt;

            s = coll.Get("submitButton");
            s1 = coll.Get("dissubmitButton");
            s2 = coll.Get("submitRejection");
            if (s == "Approve")
            {
                t1.Unique_Id = coll.Get("Unique_id");
                t1.Flag = coll.Get("Flag");

                t1.Month = coll.Get("Month");
                t1.year = coll.Get("year");
                t1.Dealer = coll.Get("Dealer");
                t1.Out = coll.Get("Out");
                t1.LOB = coll.Get("LOB");
                remarks= coll.Get("remarks");
                string Notifocation_id= coll.Get("Notification_id");
                string fromwhom1 = coll.Get("fromwhom");
                string flag_pass;
                 
                if (t1.Flag == "Target")
                {
                    flag_pass = "BulkTarget";
                }
                else
                {
                    flag_pass = "BulkActual";
                }
                // submit data action start

                Rowcount = Convert.ToInt32(coll.Get("row_count"));
                Columncount = Convert.ToInt32(coll.Get("column_count"));
                
                string flag_pass1, f;
                if (t1.Flag == "Target" || t1.Flag == "BulkTarget")
                {
                    flag_pass1 = "target";

                }
                else
                {
                    flag_pass1 = "actual";

                }


                for (int i = 0; i < Rowcount; i++)
                {
                    for (int j = 0; j < Columncount; j++)
                    {
                        if (j == 2) { t1.Out = coll.Get("Division_Id" + i + j); }
                        else if (j == 4) { t1.LOB = coll.Get("LOB_ID" + i + j); }
                        else if (j == 0 || j == 1 || j == 3 || j == 5 || j == 6 || j == 7 || j == 8) { }
                        else
                        {
                            t1.Actual = coll.Get(flag_pass1 + i + j);
                            t1.KPI = coll.Get("kpi_id" + i + j);
                            if (t1.Actual == "")
                            {
                                t1.Actual = null;
                            }

                            if (flag_pass1 == "target")
                            {
                                flag = t1.Update_BULKData(t1.Dealer, t1.Month, t1.LOB, t1.KPI, t1.Target, t1.Out,notification_id,t1.year);
                            }
                            else
                            {
                                flag = t1.Update_BULKActualData(t1.Dealer,t1.year,t1.Month, t1.LOB, t1.KPI, t1.Actual, t1.Out,notification_id);
                            }


                        }
                        //t1.LOB = coll.Get("LOB_ID"+i+j);
                        //t1.Out = coll.Get("Division_Id" + i + j);
                        //t1.Target = coll.Get("target" + i + j);
                        //t1.KPI = coll.Get("kpi_id"+ j);


                    }


                }

                // submit data action end

               


                flag = t1.Update_Flag(t1.Unique_Id, flag_pass, t1.Out, t1.LOB, "accept",fromwhom1,remarks, Notifocation_id);
                using (var cn = new SqlConnection(cs))
                {

                    string _sql = @"select Approved_Status from tblTarget_Actual_Approvels " + @"WHERE Unique_Id = @u and [Type]=@f and [LOB_Id]=@LOB and Notification_id=@N_id";
                    var cmd = new SqlCommand(_sql, cn);
                    cmd.Parameters
                        .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                        .Value = t1.Unique_Id;
                    cmd.Parameters
                        .Add(new SqlParameter("@LOB", SqlDbType.NVarChar))
                        .Value = t1.LOB;
                    cmd.Parameters
                        .Add(new SqlParameter("@f", SqlDbType.NVarChar))
                         .Value = flag_pass;
                    cmd.Parameters
                        .Add(new SqlParameter("@N_id", SqlDbType.NVarChar))
                         .Value = Notification_id;


                    cn.Open();
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            t1.TAFlag = reader["Approved_Status"].ToString();

                        }
                        reader.Dispose();
                        cmd.Dispose();

                    }
                    else
                    {
                        reader.Dispose();
                        cmd.Dispose();

                    }
                }

                if (!string.IsNullOrEmpty(t1.Unique_Id) && !string.IsNullOrEmpty(t1.Flag))
                {


                    dt = t1.Approval_Transation_Data(fromwhom1, t1.Dealer, t1.Month, t1.LOB, t1.Out, Flag,t1.year);


                    foreach (DataRow row in dt.Rows)
                    {
                        Transaction t = new Transaction();

                        t.KPI = Convert.ToString(row["KPI_Name"]);
                        t.Dealer_Level = Convert.ToString(row["Dealer_Level"]);
                        t.Division_Level = Convert.ToString(row["Division_Level"]);
                        t.LOB_Level = Convert.ToString(row["LOB_Level"]);
                        t.Target = Convert.ToString(row["Target_Value"]);
                        t.Actual = Convert.ToString(row["Actual_Value"]);
                        t.Last_Month_Target = Convert.ToString(row["Last_M_T"]);
                        t.Last_Month_Actual = Convert.ToString(row["Last_M_A"]);
                        t.Last_12_Month_Avg = Convert.ToString(row["Last_12_M_Avg"]);
                        t.Last_12_Month_Min = Convert.ToString(row["Last_12_M_Min"]);
                        t.Last_12_Month_Max = Convert.ToString(row["Last_12_M_Max"]);
                        t.SingelEntry_Tooltip_Target = Convert.ToString(row["SingleEntry_ToolTip_Target"]);
                        t.SingelEntry_Tooltip_Actual = Convert.ToString(row["SingleEntry_ToolTip_Actual"]);
                        t.IsActualRequired = Convert.ToInt32(row["IsActualRequired"]);
                        t.IsTargetRequired = Convert.ToInt32(row["IsTargetRequired"]);

                        tvm.Trans_list.Add(t);

                    }


                    tvm.Trans = t1;


                }
                return RedirectToAction("ShowAllNotification", "KPI", new { Uid = Session["Uid"].ToString() });
            }
            else if (s1 == "Reject")
            {
                t1.Unique_Id = coll.Get("Unique_id");
                t1.Flag = coll.Get("Flag");

                t1.Month = coll.Get("Month");
                t1.year = coll.Get("year");
                t1.Dealer = coll.Get("Dealer");
                t1.Out = coll.Get("Out");
                t1.LOB = coll.Get("LOB");
                remarks = coll.Get("remarks");
                string Notifocation_id = coll.Get("Notification_id");
                string fromwhom1 = coll.Get("fromwhom");
                string flag_pass;
                if (t1.Flag == "Target")
                {
                    flag_pass = "BulkTarget";
                }
                else
                {
                    flag_pass = "BulkActual";
                }
               


                flag = t1.Update_Flag(t1.Unique_Id, flag_pass, t1.Out, t1.LOB, "reject",fromwhom1,remarks, Notifocation_id);


                if (!string.IsNullOrEmpty(t1.Unique_Id) && !string.IsNullOrEmpty(t1.Flag))
                {


                    dt = t1.Approval_Transation_Data(fromwhom1, t1.Dealer, t1.Month, t1.LOB, t1.Out, Flag,t1.year);


                    foreach (DataRow row in dt.Rows)
                    {
                        Transaction t = new Transaction();

                        t.KPI = Convert.ToString(row["KPI_Name"]);
                        t.Dealer_Level = Convert.ToString(row["Dealer_Level"]);
                        t.Division_Level = Convert.ToString(row["Division_Level"]);
                        t.LOB_Level = Convert.ToString(row["LOB_Level"]);
                        t.Target = Convert.ToString(row["Target_Value"]);
                        t.Actual = Convert.ToString(row["Actual_Value"]);
                        t.Last_Month_Target = Convert.ToString(row["Last_M_T"]);
                        t.Last_Month_Actual = Convert.ToString(row["Last_M_A"]);
                        t.Last_12_Month_Avg = Convert.ToString(row["Last_12_M_Avg"]);
                        t.Last_12_Month_Min = Convert.ToString(row["Last_12_M_Min"]);
                        t.Last_12_Month_Max = Convert.ToString(row["Last_12_M_Max"]);
                        t.SingelEntry_Tooltip_Target = Convert.ToString(row["SingleEntry_ToolTip_Target"]);
                        t.SingelEntry_Tooltip_Actual = Convert.ToString(row["SingleEntry_ToolTip_Actual"]);
                        t.IsActualRequired = Convert.ToInt32(row["IsActualRequired"]);
                        t.IsTargetRequired = Convert.ToInt32(row["IsTargetRequired"]);

                        tvm.Trans_list.Add(t);

                    }


                    tvm.Trans = t1;


                }
                return RedirectToAction("ShowAllNotification", "KPI", new { Uid = Session["Uid"].ToString() });

            }
            else if (s2 == "Submit")
            {
                Rowcount = Convert.ToInt32(coll.Get("row_count"));
                Columncount = Convert.ToInt32(coll.Get("column_count"));
                t1.Unique_Id = coll.Get("Unique_id");
                t1.Flag = coll.Get("Flag");

                t1.Month = coll.Get("Month");
                 t1.year = coll.Get("year");
                t1.Dealer = coll.Get("Dealer");
                string fromwhom1 = coll.Get("fromwhom");
                remarks = coll.Get("remarks");
                string Notifocation_id = coll.Get("Notification_id");
                string flag_pass,f;
                if (t1.Flag == "Target" || t1.Flag== "BulkTarget")
                {
                    flag_pass = "target"; 
                    
                }
                else
                {
                    flag_pass = "actual";

                }


                for (int i = 0; i < Rowcount; i++)
                {
                    for (int j = 0; j < Columncount; j++)
                    {
                        if (j == 2) { t1.Out = coll.Get("Division_Id" + i + j); }
                        else if (j == 4) { t1.LOB = coll.Get("LOB_ID" + i + j); }
                        else if (j == 0 || j == 1 || j == 3 || j == 5 || j == 6 || j == 7 || j == 8) { }
                        else
                        {
                            t1.Actual = coll.Get(flag_pass + i + j);
                            t1.KPI = coll.Get("kpi_id" + i + j);
                            if (t1.Actual == "")
                            {
                                t1.Actual = null;
                            }

                            if (flag_pass == "target")
                            {
                                flag = t1.Update_BULKData(t1.Dealer, t1.Month, t1.LOB, t1.KPI, t1.Target, t1.Out,notification_id,t1.year);
                            }
                            else {
                                flag = t1.Update_BULKActualData(t1.Dealer,t1.year,t1.Month, t1.LOB, t1.KPI, t1.Actual, t1.Out,notification_id);
                            }

                             
                        }
                        //t1.LOB = coll.Get("LOB_ID"+i+j);
                        //t1.Out = coll.Get("Division_Id" + i + j);
                        //t1.Target = coll.Get("target" + i + j);
                        //t1.KPI = coll.Get("kpi_id"+ j);


                    }


                }
                t1.Update_Flag(t1.Unique_Id, "Bulk"+t1.Flag,t1.Out,t1.LOB, "Rejectaccept", fromwhom1,remarks, Notifocation_id);
                
                // t1.Bulknotification_Insert(t1.Month, "%", t1.Dealer, "%", "BulkActual");
                t1.updateActualBulk_UserId(t1.Month, t1.Dealer,t1.year);
                return RedirectToAction("ShowAllNotification", "KPI", new { Uid = Session["Uid"].ToString() });
            }
            else
            {
                using (var cn = new SqlConnection(cs))
                {

                    string flag_pass;
                    if (t1.Flag == "Target")
                    {
                        flag_pass = "BulkTarget";
                    }
                    else
                    {
                        flag_pass = "BulkActual";
                    }
                    string _sql = @"select Approved_Status from tblTarget_Actual_Approvels " + @"WHERE Unique_Id = @u and [Type]=@f and [LOB_id]=@LOB and Notification_id=@N_id";
                    var cmd = new SqlCommand(_sql, cn);
                    cmd.Parameters
                        .Add(new SqlParameter("@u", SqlDbType.NVarChar))
                        .Value = Uid;
                    cmd.Parameters
                        .Add(new SqlParameter("@LOB", SqlDbType.NVarChar))
                        .Value = LOB;
                    cmd.Parameters
                        .Add(new SqlParameter("@f", SqlDbType.NVarChar))
                        .Value = flag_pass;
                    cmd.Parameters
                        .Add(new SqlParameter("@N_id", SqlDbType.NVarChar))
                        .Value = Notification_id;

                    cn.Open();
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            t1.TAFlag = reader["Approved_Status"].ToString();

                        }
                        reader.Dispose();
                        cmd.Dispose();

                    }
                    else
                    {
                        reader.Dispose();
                        cmd.Dispose();

                    }
                }
                if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(Flag))
                {
                    t1.Unique_Id = Uid;
                    t1.Flag = Flag;
                    t1.Month = month;
                    t1.year = year;
                    t1.Dealer = dealer;
                    t1.Out = o;
                    t1.LOB = LOB;
                    t1.Dealer_Name = dealer_name;
                    t1.Out_Name = o_name;
                    t1.LOB_Name = LOB_name;
                    t1.monthdesc = monthdesc;
                    t1.Remarks = remarks_out;

                    //dt = t1.Approved_Data(Uid);
                    tvm.Trans = t1;

                    return View(tvm);
                }
                else
                {


                    return RedirectToAction("AMIndex", "KPI");

                }
            }

        }

        public ActionResult ShowAllNotification(string uid)
        {

            return View();
        }
    }
}