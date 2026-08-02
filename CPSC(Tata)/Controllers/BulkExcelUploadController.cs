using ClosedXML.Excel;
//using Hangfire;
//using InputOutput.Models;
//using NPOI.HSSF.UserModel;
//using NPOI.SS.UserModel;
//using NPOI.XSSF.UserModel;
using Hangfire;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Script.Serialization;

//using TATA_IBCPSC.Models;

namespace InputOutput.Controllers
{
    public class BulkExcelUploadController : Controller
    {
        public static string GetFinancialYear(DateTime curDate)
        {
            int CurrentYear = curDate.Year;
            int PreviousYear = (curDate.Year - 1);
            int NextYear = (curDate.Year + 1);
            string PreYear = PreviousYear.ToString();
            string NexYear = NextYear.ToString();
            string CurYear = CurrentYear.ToString();
            string FinYear = string.Empty;
            if (curDate.Month > 4)
            {
                FinYear = CurYear + "-" + NexYear;
            }
            else
            {
                FinYear = PreYear + "-" + CurYear;
            }
            return FinYear;
        }
        public static string GetFinancialStartYear(DateTime curDate)
        {
            int CurrentYear = curDate.Year;
            int PreviousYear = (curDate.Year - 1);
            int NextYear = (curDate.Year + 1);
            string PreYear = PreviousYear.ToString();
            string NexYear = NextYear.ToString();
            string CurYear = CurrentYear.ToString();
            string FinYear = string.Empty;
            if (curDate.Month > 4)
            {
                FinYear = CurYear;
            }
            else
            {
                FinYear = PreYear;
            }
            return FinYear;
        }
        public static string GetFinancialEndYear(DateTime curDate)
        {
            int CurrentYear = curDate.Year;
            int PreviousYear = (curDate.Year - 1);
            int NextYear = (curDate.Year + 1);
            string PreYear = PreviousYear.ToString();
            string NexYear = NextYear.ToString();
            string CurYear = CurrentYear.ToString();
            string FinYear = string.Empty;
            if (curDate.Month > 4)
            {
                FinYear = NexYear;
            }
            else
            {
                FinYear = CurYear;
            }
            return FinYear;
        }


        public ActionResult Index()
        {
            if (Session["Type"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            ViewBag.StartDate = GetFinancialStartYear(DateTime.Now);
            ViewBag.EndDate = GetFinancialEndYear(DateTime.Now);

            string UserType = Session["Type"].ToString();


            List<string> List = new List<string> { "CEI", "Sales Satisfaction", "Retail 2", "Tata OK", "AMC Policy", "Workshop Revenue Report", "Activity Detailed Report", "CPSC Wrong Number", "DP", "DSAdmn" };
            //string[] List = new string[] { "Activities Done And Open","CEI","Sales Satisfaction" };
            //List = new List<string>(List).ToArray();

            ViewBag.List = List;
            return View();
        }

        public ActionResult DownloadExcelEPPlus(string filename)
        {
            // Step 1: Get data from the stored procedure
            DataTable dt = new DataTable("Report");
            string connectionString = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string storedProcedure = "SpGetExcelData";
                using (SqlDataAdapter adapter = new SqlDataAdapter(storedProcedure, connection))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@FW", Session["Uid"].ToString());
                    adapter.SelectCommand.Parameters.AddWithValue("@fileName", filename);
                    adapter.Fill(dt);
                }
            }

            // Step 2: Generate the Excel file
            var stream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Required for non-commercial use of EPPlus
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");
                worksheet.Cells.LoadFromDataTable(dt, true);
                package.Save(); // Save the Excel package to the stream
            }

            // Step 3: Return the Excel file as a downloadable response
            stream.Position = 0; // Reset stream position to the beginning
            string downloadFileName = $"{filename}-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", downloadFileName);
        }

        //public ActionResult DownloadExcelEPPlus(string filename)
        //{
        //    dynamic list = null;

        //    //if (filename == "DP" || filename=="DSAdmn")
        //    //{
        //    //    DataTable dt1 = new DataTable("Report");
        //    //    string CS11 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
        //    //    string _sql21 = string.Empty;
        //    //    _sql21 = "getDPDSAdmn";
        //    //    using (SqlConnection cn = new SqlConnection(CS11))
        //    //    {

        //    //        //var cmd = new SqlCommand(_sql, cn);
        //    //        var daCampus = new SqlDataAdapter(_sql21, cn);
        //    //        daCampus.SelectCommand.CommandType = CommandType.StoredProcedure;
        //    //        daCampus.SelectCommand.Parameters.AddWithValue("@FW", Session["Uid"].ToString());
        //    //        daCampus.SelectCommand.Parameters.AddWithValue("@fileName", filename);
        //    //        daCampus.Fill(dt1);


        //    //    }

        //    //    JavaScriptSerializer jsSerializer1 = new JavaScriptSerializer();

        //    //    using (XLWorkbook wb = new XLWorkbook())
        //    //    {
        //    //        wb.Worksheets.Add(dt1);

        //    //        string path = AppDomain.CurrentDomain.BaseDirectory + "Reports";
        //    //        filename = path + "\\" + filename + ".xlsx";
        //    //        wb.SaveAs(filename);

        //    //    }




        //    //    string fullPath1 = Path.Combine(Server.MapPath("~/Reports"), filename);

        //    //    //return the file for download, this is an Excel 
        //    //    //so I set the file content type to "application/vnd.ms-excel"
        //    //    return File(fullPath1, "application/vnd.ms-excel", filename);
        //    //}
        //    //else if (filename == "User")
        //    //{
        //    //    list = entity.proc_tblemployeemaster().Where(m => m.LoginId != Session["LoginId"].ToString()).ToList();
        //    //}
        //    //else if (filename == "Dealer")
        //    //{
        //    //    list = entity.tbl_Distributor_Dealer_Master.ToList();
        //    //}
        //    //else if (filename == "F2_b_EVR_CFR")
        //    //{
        //    //    list = entity.vw_F2_b_EVR_CFR_Format.ToList();
        //    //}
        //    //else if (filename == "F2_b_Fund_Balance_Vehicles")
        //    //{
        //    //    list = entity.vw_F2_b_Fund_Balance_Vehicles_Format.ToList();
        //    //}
        //    //else if (filename == "F3_a_Shipment_Tgt_Spares")
        //    //{
        //    //    list = entity.vw_F3_a_Shipment_Tgt_Spares_Format.ToList();
        //    //}
        //    //else if (filename == "F3_b_FundBalance_SpareParts")
        //    //{
        //    //    list = entity.vw_F3_b_FundBalance_SpareParts_Format.ToList();
        //    //}
        //    //else if (filename == "F4_a_Durafit_Tyre_TMGO")
        //    //{
        //    //    list = entity.vw_F4_a_Durafit_Tyre_TMGO_Format.ToList();
        //    //}
        //    //else if (filename == "C1_SSI")
        //    //{
        //    //    list = entity.vw_C1_SSI_Format.ToList();
        //    //}
        //    //else if (filename == "C2_CSI")
        //    //{
        //    //    list = entity.vw_C2_CSI_Format.ToList();

        //    //}
        //    //else if (filename == "P3_Actual")
        //    //{
        //    //    list = entity.vw_P3_Actual_Format.ToList();

        //    //}
        //    //else if (filename == "F3_b_OG_Target_Actual")
        //    //{
        //    //    list = entity.vw_F3_b_OG_Target_Actual_Format.ToList();

        //    //}
        //    //else if (filename == "P1_Actual")
        //    //{
        //    //    list = entity.vw_P1_Actual_Format.ToList();

        //    //}

        //    ////Yearly
        //    //else if (filename == "F1_Target")
        //    //{
        //    //    // list = entity.vw_F1_Target_Format.ToList();
        //    //    string conString = "";
        //    //    conString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //    //    using (SqlConnection conn = new SqlConnection(conString))
        //    //    using (SqlCommand cmd = new SqlCommand("Proc_F1_Target_Format", conn))
        //    //    {


        //    //        SqlDataAdapter adapt = new SqlDataAdapter(cmd);
        //    //        adapt.SelectCommand.CommandType = CommandType.StoredProcedure;


        //    //        DataTable dt = new DataTable();
        //    //        adapt.Fill(dt);
        //    //        var ds = dt;
        //    //        var stream1 = new MemoryStream();
        //    //        //required using OfficeOpenXml;
        //    //        // If you use EPPlus in a noncommercial context
        //    //        // according to the Polyform Noncommercial license:
        //    //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    //        using (var package = new ExcelPackage(stream1))
        //    //        {
        //    //            var workSheet = package.Workbook.Worksheets.Add(filename);
        //    //            workSheet.Cells.LoadFromDataTable(dt, true);
        //    //            package.Save();
        //    //        }
        //    //        stream1.Position = 0;
        //    //        string excelName1 = $"{filename}-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
        //    //        return File(stream1, "application/octet-stream", excelName1);




        //    //    }




        //    //}
        //    //else if (filename == "C3_CurrentYear")
        //    //{
        //    //    list = entity.vw_C3_CurrentYear_Format.ToList();

        //    //}
        //    //else if (filename == "C3_LastYearExit")
        //    //{
        //    //    list = entity.vw_C3_LastYearExit_Format.ToList();


        //    //}
        //    ////Semi
        //    //else if (filename == "F2_a_Actual")
        //    //{
        //    //    list = entity.vw_F2_a_Actual_Format.ToList();

        //    //}

        //    //var stream = new MemoryStream();
        //    ////required using OfficeOpenXml;
        //    //// If you use EPPlus in a noncommercial context
        //    //// according to the Polyform Noncommercial license:
        //    //OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    //using (var package = new ExcelPackage(stream))
        //    //{
        //    //    var workSheet = package.Workbook.Worksheets.Add(filename);
        //    //    workSheet.Cells.LoadFromCollection(list, true);
        //    //    package.Save();
        //    //}
        //    //stream.Position = 0;
        //    //string excelName = $"{filename}-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
        //    DataTable dt = new DataTable("Report");
        //    string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
        //    string _sql2 = string.Empty;
        //    _sql2 = "SpGetExcelData";
        //    using (SqlConnection cn = new SqlConnection(CS1))
        //    {

        //        //var cmd = new SqlCommand(_sql, cn);
        //        var daCampus = new SqlDataAdapter(_sql2, cn);
        //        daCampus.SelectCommand.CommandType = CommandType.StoredProcedure;
        //        daCampus.SelectCommand.Parameters.AddWithValue("@FW", Session["Uid"].ToString());
        //        daCampus.SelectCommand.Parameters.AddWithValue("@fileName", filename);
        //        daCampus.Fill(dt);


        //    }

        //    JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

        //    using (XLWorkbook wb = new XLWorkbook())
        //    {
        //        wb.Worksheets.Add(dt);

        //        string path = AppDomain.CurrentDomain.BaseDirectory + "Reports";
        //        filename = path + "\\" + filename + ".xlsx";
        //        wb.SaveAs(filename);

        //    }




        //    string fullPath = Path.Combine(Server.MapPath("~/Reports"), filename);

        //    //return the file for download, this is an Excel 
        //    //so I set the file content type to "application/vnd.ms-excel"
        //    return File(fullPath, "application/vnd.ms-excel", filename);

        //}

        //private readonly string storageDirectory = "D:\\";

        //[HttpPost]
        //public ActionResult Index(HttpPostedFileBase file)
        //{
        //    if (Session["Type"] != null && file != null && file.ContentLength > 0)
        //    {
        //        string filename = file.FileName.ToString();

        //        string filePath = Path.Combine(storageDirectory, Path.GetFileName(file.FileName));

        //        //string filePath = @"D:\Sales Satisfaction Index Score TBSS Calling (76).csv";

        //        if (filename.Contains("Sales Satisfaction"))
        //        {
        //            filename = "Sales Satisfaction";
        //        }
        //        else if (filename.Contains("Activities Done And Open"))
        //        {
        //            filename = "Activities Done And Open";
        //        }
        //        else if (filename.Contains("Retail 2"))
        //        {
        //            filename = "Retail 2";
        //        }
        //        else if (filename.Contains("CEI"))
        //        {
        //            filename = "CEI";
        //        }
        //        else if (filename.Contains("Tata OK"))
        //        {
        //            filename = "Tata OK";
        //        }
        //        else if (filename.Contains("AMC policy"))
        //        {
        //            filename = "AMC Policy";
        //        }
        //        else if (filename.Contains("Workshop Revenue Report"))
        //        {
        //            filename = "Workshop Revenue Report";
        //        }
        //        else if (filename.Contains("Activity Detailed Report"))
        //        {
        //            filename = "Activity Detailed Report";
        //        }
        //        else if (filename.Contains("CPSC Wrong number"))
        //        {
        //            filename = "CPSC Wrong Number";
        //        }
        //        else if (filename.Contains("DP"))
        //        {
        //            filename = "DP";
        //        }
        //        else if (filename.Contains("DSAdmn"))
        //        {
        //            filename = "DSAdmn";
        //        }

        //        List<string> List = new List<string> { "CEI", "Sales Satisfaction", "Retail 2", "Tata OK", "AMC Policy", "Workshop Revenue Report", "Activity Detailed Report", "CPSC Wrong Number", "DP", "DSAdmn" };
        //        ViewBag.List = List;
        //        //DataTable dt = new DataTable();
        //        DataTable dt_Orignal = new DataTable();
        //        if (ModelState.IsValid)
        //        {

        //            if (file.ContentLength > 0)
        //            {
        //                if (file.FileName.EndsWith(".csv") || file.FileName.EndsWith(".xls"))
        //                {
        //                    //WorkSheet.FirstRow().Delete();//if you want to remove ist row
        //                    dt_Orignal = ExceltoDatatable(filePath);

        //                    string err_msg = validatedatatable(dt_Orignal, filename);

        //                    if (err_msg == "True" && filename == "Activities Done And Open")
        //                    {
        //                        dt_Orignal = ExceltoDatatable(filePath);
        //                        err_msg = UpdateBulkupload(dt_Orignal, "SpInsertDataActiviesDoneAndOpen", filename);
        //                        if (err_msg != "succ")
        //                        {
        //                            Session["result"] = err_msg;
        //                        }
        //                        else
        //                        {
        //                            Session["result"] = "succ";
        //                        }
        //                    }
        //                    else if (err_msg == "True" && filename == "Sales Satisfaction")
        //                    {

        //                        dt_Orignal = ExceltoDatatable(filePath);
        //                        err_msg = UpdateBulkupload(dt_Orignal, "SpInsertSalesSatisfaction", filename);
        //                        if (err_msg != "succ")
        //                        {
        //                            Session["result"] = err_msg;
        //                        }
        //                        else
        //                        {
        //                            Session["result"] = "succ";
        //                        }
        //                    }
        //                    else if (err_msg == "True" && filename == "Retail 2")
        //                    {

        //                        dt_Orignal = ExceltoDatatable(filePath);
        //                        err_msg = UpdateBulkupload(dt_Orignal, "SpInsertRetail2", filename);
        //                        if (err_msg != "succ")
        //                        {
        //                            Session["result"] = err_msg;
        //                        }
        //                        else
        //                        {
        //                            Session["result"] = "succ";
        //                        }
        //                    }
        //                    else if (err_msg == "True" && filename == "Tata OK")
        //                    {

        //                        dt_Orignal = ExceltoDatatable(filePath);
        //                        err_msg = UpdateBulkupload(dt_Orignal, "SpInsertTataOk", filename);
        //                        if (err_msg != "succ")
        //                        {
        //                            Session["result"] = err_msg;
        //                        }
        //                        else
        //                        {
        //                            Session["result"] = "succ";
        //                        }
        //                    }
        //                    else if (err_msg == "True" && filename == "AMC Policy")
        //                    {

        //                        dt_Orignal = ExceltoDatatable(filePath);
        //                        err_msg = UpdateBulkupload(dt_Orignal, "SpInsertAMCPolicy", filename);
        //                        if (err_msg != "succ")
        //                        {
        //                            Session["result"] = err_msg;
        //                        }
        //                        else
        //                        {
        //                            Session["result"] = "succ";
        //                        }
        //                    }
        //                    else if (err_msg == "True" && filename == "CEI")
        //                    {

        //                        dt_Orignal = ExceltoDatatable(filePath);
        //                        err_msg = UpdateBulkupload(dt_Orignal, "SpInsertCEI", filename);
        //                        if (err_msg != "succ")
        //                        {
        //                            Session["result"] = err_msg;
        //                        }
        //                        else
        //                        {
        //                            Session["result"] = "succ";
        //                        }
        //                    }
        //                    else if (err_msg == "True" && filename == "Workshop Revenue Report")
        //                    {

        //                        dt_Orignal = ExceltoDatatable(filePath);
        //                        err_msg = UpdateBulkupload(dt_Orignal, "SpInsertWorkshopRevenueReport", filename);
        //                        if (err_msg != "succ")
        //                        {
        //                            Session["result"] = err_msg;
        //                        }
        //                        else
        //                        {
        //                            Session["result"] = "succ";
        //                        }
        //                    }
        //                    else if (err_msg == "True" && filename == "Activity Detailed Report")
        //                    {

        //                        dt_Orignal = ExceltoDatatable(filePath);
        //                        err_msg = UpdateBulkupload(dt_Orignal, "SpInsertActivityDetailedReport", filename);
        //                        if (err_msg != "succ")
        //                        {
        //                            Session["result"] = err_msg;
        //                        }
        //                        else
        //                        {
        //                            Session["result"] = "succ";
        //                        }
        //                    }
        //                    else if (err_msg == "True" && filename == "CPSC Wrong Number")
        //                    {

        //                        dt_Orignal = ExceltoDatatable(filePath);
        //                        err_msg = UpdateBulkupload(dt_Orignal, "SpInsertCPSCWrongNumber", filename);
        //                        if (err_msg != "succ")
        //                        {
        //                            Session["result"] = err_msg;
        //                        }
        //                        else
        //                        {
        //                            Session["result"] = "succ";
        //                        }
        //                    }
        //                    else if (err_msg == "True" && filename == "DP")
        //                    {

        //                        dt_Orignal = ExceltoDatatable(filePath);
        //                        err_msg = UpdateBulkupload(dt_Orignal, "SpInsertDP", filename);
        //                        if (err_msg != "succ")
        //                        {
        //                            Session["result"] = err_msg;
        //                        }
        //                        else
        //                        {
        //                            Session["result"] = "succ";
        //                        }
        //                    }
        //                    else if (err_msg == "True" && filename == "DSAdmn")
        //                    {

        //                        dt_Orignal = ExceltoDatatable(filePath);
        //                        err_msg = UpdateBulkupload(dt_Orignal, "SpInsertDSAdmn", filename);
        //                        if (err_msg != "succ")
        //                        {
        //                            Session["result"] = err_msg;
        //                        }
        //                        else
        //                        {
        //                            Session["result"] = "succ";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ModelState.AddModelError(String.Empty, err_msg);
        //                        return View();
        //                    }


        //                }
        //                else
        //                {
        //                    ModelState.AddModelError(String.Empty, "Only .csv and .xls files are allowed");
        //                    return View();
        //                }
        //            }
        //            else
        //            {
        //                ModelState.AddModelError(String.Empty, "Not a valid file");
        //                return View();
        //            }
        //        }

        //        return View();
        //    }
        //    else
        //    {
        //        Session["Popup"] = "0";
        //        return RedirectToAction("Login", "Login");
        //    }

        //}

        private readonly string storageDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (Session["Type"] != null && file != null && file.ContentLength > 0)
            {
                string filename = file.FileName;
                string filePath = Path.Combine(storageDirectory, Path.GetFileName(filename));

                try
                {
                    // Save the uploaded file to the specified path
                    file.SaveAs(filePath);

                    // Now the file is saved, you can proceed with your logic
                    DataTable dt_Orignal = new DataTable();

                    if (filename.Contains("Sales Satisfaction"))
                    {
                        filename = "Sales Satisfaction";
                    }
                    else if (filename.Contains("Activities Done And Open"))
                    {
                        filename = "Activities Done And Open";
                    }
                    else if (filename.Contains("Retail 2"))
                    {
                        filename = "Retail 2";
                    }
                    else if (filename.Contains("CEI"))
                    {
                        filename = "CEI";
                    }
                    else if (filename.Contains("Tata OK"))
                    {
                        filename = "Tata OK";
                    }
                    else if (filename.Contains("AMC policy"))
                    {
                        filename = "AMC Policy";
                    }
                    else if (filename.Contains("Workshop Revenue Report"))
                    {
                        filename = "Workshop Revenue Report";
                    }
                    else if (filename.Contains("Activity Detailed Report"))
                    {
                        filename = "Activity Detailed Report";
                    }
                    else if (filename.Contains("CPSC Wrong number"))
                    {
                        filename = "CPSC Wrong Number";
                    }
                    else if (filename.Contains("DP"))
                    {
                        filename = "DP";
                    }
                    else if (filename.Contains("DSAdmn"))
                    {
                        filename = "DSAdmn";
                    }

                    // ... Handle other cases here

                    List<string> List = new List<string> { "CEI", "Sales Satisfaction", "Retail 2", "Tata OK", "AMC Policy", "Workshop Revenue Report", "Activity Detailed Report", "CPSC Wrong Number", "DP", "DSAdmn" };
                    ViewBag.List = List;
                    // Call ExceltoDatatable with the saved file path
                    dt_Orignal = ExceltoDatatable(filePath);

                    // Perform your validation and bulk upload operations
                    string err_msg = validatedatatable(dt_Orignal, filename);

                    if (err_msg == "True")
                    {
                        err_msg = UpdateBulkupload(dt_Orignal, $"SpInsert{filename.Replace(" ", string.Empty)}", filename);
                        if (err_msg != "succ")
                        {
                            Session["result"] = err_msg;
                        }
                        else
                        {
                            Session["result"] = "succ";
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, err_msg);
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                    return View();
                }

                return View();
            }
            else
            {
                Session["Popup"] = "0";
                return RedirectToAction("Login", "Login");
            }
        }

        public DataTable ExceltoDatatable(string csvFilePath)
        {
            DataTable csvData = new DataTable();
            using (StreamReader sr = new StreamReader(csvFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    csvData.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = csvData.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    csvData.Rows.Add(dr);
                }
            }
            return csvData;
        }

        public string validatedatatable(DataTable datatab, string filename)
        {
            if (filename.Contains("Activities Done And Open"))
            {
                string msg = string.Empty;

                //DataTable datatab2 = GetDataForValidation("getActivitiesDoneAndOpen", "Target");

                //foreach (DataRow row in datatab2.Rows)
                //{

                //    //test for DataLocked
                //    if (row[datatab2.Columns.Count - 1].ToString() == "True")
                //    {
                //        msg = "Data is locked for entry so please contact with CPSC Team.";

                //        return msg;
                //    }


                //}
                foreach (DataColumn dc in datatab.Columns)
                {
                    if (dc.ColumnName == "Month" || dc.ColumnName == "Region" || dc.ColumnName == "Dealer code" || dc.ColumnName == "Dealer" || dc.ColumnName == "Done" || dc.ColumnName == "Done & Open")
                    {
                        msg = "True";
                        //return msg;
                    }
                    else
                    {
                        msg = "Column names are not matched.Please check the column names or download the data format and match the columns";
                        return msg;
                    }
                }
                int i = 1;

                foreach (DataRow row in datatab.Rows)
                {
                    foreach (DataColumn col in datatab.Columns)
                    {
                        //test for null here
                        if (row[col] == DBNull.Value || string.IsNullOrEmpty(row[col].ToString()) || row[col].ToString() == string.Empty)
                        {
                            msg = "Please Check Row " + i + " found Empty or Null value in Column " + col.ColumnName.ToString() + ".";

                            return msg;
                        }

                    }

                    i++;
                }



                //check for non numeric value in data column

                //DataTable dt_CheckNonNumric = CheckNonNumericValues(datatab, "Target_Value");

                //if (dt_CheckNonNumric.Rows.Count > 0)
                //{
                //    msg = "Found Non Numeric Value,Only Numeric values are allow in Target/Actual column.Please check the data once and retry.";

                //    return msg;
                //}

                //DataTable dtresult = GetDataDifference(datatab, datatab2);

                //if (dtresult.Rows.Count > 0)
                //{
                //    msg = "Data are not match with dump.Please check the data or download the data format to match.";

                //    return msg;
                //}



                return msg;
            }
            else if (filename == "Sales Satisfaction")
            {
                string msg = string.Empty;

                foreach (DataColumn dc in datatab.Columns)
                {
                    if (dc.ColumnName == "Month" || dc.ColumnName == "Dealer Name" || dc.ColumnName == "Dealer Code" || dc.ColumnName == "Region" || dc.ColumnName == "Final Score")
                    {
                        msg = "True";
                        //return msg;
                    }
                    else
                    {
                        msg = "Column names are not matched.Please check the column names or download the data format and match the columns";
                        return msg;
                    }
                }
                int i = 1;

                foreach (DataRow row in datatab.Rows)
                {
                    foreach (DataColumn col in datatab.Columns)
                    {
                        int s = row[2].ToString().Length;
                        //test for null here
                        if (row[col] == DBNull.Value || string.IsNullOrEmpty(row[col].ToString()) || row[col].ToString() == string.Empty)
                        {
                            msg = "Please Check Row " + i + " found Empty or Null value in Column " + col.ColumnName.ToString() + ".";

                            return msg;
                        }
                        if (row[2].ToString().Length != 7)
                        {
                            msg = "Dealer Code contain only 7 characters.";

                            return msg;
                        }

                    }

                    i++;
                }

                return msg;
            }
            else if (filename == "CEI")
            {
                string msg = string.Empty;

                foreach (DataColumn dc in datatab.Columns)
                {
                    if (dc.ColumnName == "Month" || dc.ColumnName == "Dealer Name" || dc.ColumnName == "Division" || dc.ColumnName == "Dealer Code" || dc.ColumnName == "Region" || dc.ColumnName == "Satisfied%")
                    {
                        msg = "True";
                        //return msg;
                    }
                    else
                    {
                        msg = "Column names are not matched.Please check the column names or download the data format and match the columns";
                        return msg;
                    }
                }
                int i = 1;

                foreach (DataRow row in datatab.Rows)
                {
                    foreach (DataColumn col in datatab.Columns)
                    {
                        //test for null here
                        if (row[col] == DBNull.Value || string.IsNullOrEmpty(row[col].ToString()) || row[col].ToString() == string.Empty)
                        {
                            msg = "Please Check Row " + i + " found Empty or Null value in Column " + col.ColumnName.ToString() + ".";

                            return msg;
                        }
                        if (row[3].ToString().Length != 7)
                        {
                            msg = "Dealer Code contain only 7 characters.";

                            return msg;
                        }
                        if (row[4].ToString().Length > 5)
                        {
                            msg = "Please mentioned Region properly.";

                            return msg;
                        }

                    }

                    i++;
                }

                return msg;
            }
            else if (filename == "Retail 2")
            {
                string msg = string.Empty;

                foreach (DataColumn dc in datatab.Columns)
                {
                    if (dc.ColumnName == "Month" || dc.ColumnName == "Dealer" || dc.ColumnName == "Dealer Code" || dc.ColumnName == "Region" || dc.ColumnName == "LOB" || dc.ColumnName == "Retail")
                    {
                        msg = "True";
                        //return msg;
                    }
                    else
                    {
                        msg = "Column names are not matched.Please check the column names or download the data format and match the columns";
                        return msg;
                    }
                }
                int i = 1;

                foreach (DataRow row in datatab.Rows)
                {
                    foreach (DataColumn col in datatab.Columns)
                    {
                        //test for null here
                        if (row[col] == DBNull.Value || string.IsNullOrEmpty(row[col].ToString()) || row[col].ToString() == string.Empty)
                        {
                            msg = "Please Check Row " + i + " found Empty or Null value in Column " + col.ColumnName.ToString() + ".";

                            return msg;
                        }
                        if (row[2].ToString().Length != 7)
                        {
                            msg = "Dealer Code contain only 7 characters.";

                            return msg;
                        }
                        if (row[3].ToString().Length > 5)
                        {
                            msg = "Please mentioned Region properly.";

                            return msg;
                        }

                    }

                    i++;
                }

                return msg;
            }
            else if (filename == "Tata OK")
            {
                string msg = string.Empty;

                foreach (DataColumn dc in datatab.Columns)
                {
                    if (dc.ColumnName == "Month" || dc.ColumnName == "Dealer" || dc.ColumnName == "Dealer Code" || dc.ColumnName == "Region" || dc.ColumnName == "LOB" || dc.ColumnName == "Assured / Non-Assured" || dc.ColumnName == "Resale Quantity")
                    {
                        msg = "True";
                        //return msg;
                    }
                    else
                    {
                        msg = "Column names are not matched.Please check the column names or download the data format and match the columns";
                        return msg;
                    }
                }
                int i = 1;

                foreach (DataRow row in datatab.Rows)
                {
                    foreach (DataColumn col in datatab.Columns)
                    {
                        //test for null here
                        if (row[col] == DBNull.Value || string.IsNullOrEmpty(row[col].ToString()) || row[col].ToString() == string.Empty)
                        {
                            msg = "Please Check Row " + i + " found Empty or Null value in Column " + col.ColumnName.ToString() + ".";

                            return msg;
                        }
                        if (row[2].ToString().Length != 7)
                        {
                            msg = "Dealer Code contain only 7 characters.";

                            return msg;
                        }

                    }

                    i++;
                }

                return msg;
            }
            else if (filename == "AMC Policy")
            {
                string msg = string.Empty;

                foreach (DataColumn dc in datatab.Columns)
                {
                    if (dc.ColumnName == "Month" || dc.ColumnName == "Dealer Name" || dc.ColumnName == "Dealer Code" || dc.ColumnName == "LOB" || dc.ColumnName == "Sum of vehicles retailed with AMC Policy")
                    {
                        msg = "True";
                        //return msg;
                    }
                    else
                    {
                        msg = "Column names are not matched.Please check the column names or download the data format and match the columns";
                        return msg;
                    }
                }
                int i = 1;

                foreach (DataRow row in datatab.Rows)
                {
                    foreach (DataColumn col in datatab.Columns)
                    {
                        //test for null here
                        if (row[col] == DBNull.Value || string.IsNullOrEmpty(row[col].ToString()) || row[col].ToString() == string.Empty)
                        {
                            msg = "Please Check Row " + i + " found Empty or Null value in Column " + col.ColumnName.ToString() + ".";

                            return msg;
                        }
                        if (row[2].ToString().Length != 7)
                        {
                            msg = "Dealer Code contain only 7 characters.";

                            return msg;
                        }
                        int value;
                        if (int.TryParse(row[4].ToString(), out value) != true)
                        {
                            msg = "(Sum of vehicles retailed with AMC Policy) column only get integer data.";

                            return msg;
                        }

                    }

                    i++;
                }

                return msg;
            }
            else if (filename == "Workshop Revenue Report")
            {
                string msg = string.Empty;

                foreach (DataColumn dc in datatab.Columns)
                {
                    if (dc.ColumnName == "TM Fiscal Year" || dc.ColumnName == "Month" || dc.ColumnName == "Region" || dc.ColumnName == "State" || dc.ColumnName == "Dealer Code" || dc.ColumnName == "Dealer" || dc.ColumnName == "Labour Revenue" || dc.ColumnName == "Lubs Revenue" || dc.ColumnName == "Spares Revenue" || dc.ColumnName == "Total Workshop Revenue" || dc.ColumnName == "Spares Revenue - OTC")
                    {
                        msg = "True";
                        //return msg;
                    }
                    else
                    {
                        msg = "Column names are not matched.Please check the column names or download the data format and match the columns";
                        return msg;
                    }
                }
                int i = 1;

                foreach (DataRow row in datatab.Rows)
                {
                    foreach (DataColumn col in datatab.Columns)
                    {
                        //test for null here
                        if (row[col] == DBNull.Value || string.IsNullOrEmpty(row[col].ToString()) || row[col].ToString() == string.Empty)
                        {
                            msg = "Please Check Row " + i + " found Empty or Null value in Column " + col.ColumnName.ToString() + ".";

                            return msg;
                        }
                        if (row[4].ToString().Length != 7 || row[0].ToString().Length != 7)
                        {
                            msg = "Columns contain only 7 characters.";

                            return msg;
                        }
                        double value1, value2, value3, value4, value5;
                        if (double.TryParse(row[6].ToString(), out value1) != true || double.TryParse(row[7].ToString(), out value2) != true || double.TryParse(row[8].ToString(), out value3) != true || double.TryParse(row[9].ToString(), out value4) != true || double.TryParse(row[10].ToString(), out value5) != true)
                        {
                            msg = "columns only get integer data.";

                            return msg;
                        }

                    }

                    i++;
                }

                return msg;
            }
            else if (filename == "CPSC Wrong Number")
            {
                string msg = string.Empty;

                foreach (DataColumn dc in datatab.Columns)
                {
                    if (dc.ColumnName == "Month" || dc.ColumnName == "Dealer Name" || dc.ColumnName == "Dealer Code" || dc.ColumnName == "Region" || dc.ColumnName == "Wrong no CSR" || dc.ColumnName == "# Of CSR" || dc.ColumnName == "Count CSR#")
                    {
                        msg = "True";
                        //return msg;
                    }
                    else
                    {
                        msg = "Column names are not matched.Please check the column names or download the data format and match the columns";
                        return msg;
                    }
                }
                int i = 1;

                foreach (DataRow row in datatab.Rows)
                {
                    foreach (DataColumn col in datatab.Columns)
                    {
                        //test for null here
                        if (row[2] == DBNull.Value || string.IsNullOrEmpty(row[2].ToString()) || row[2].ToString() == string.Empty)
                        {
                            msg = "Please Check Row " + i + " found Empty or Null value in Column " + col.ColumnName.ToString() + ".";

                            return msg;
                        }
                        if (row[2].ToString().Length != 7)
                        {
                            msg = "Dealer Code contain only 7 characters.";

                            return msg;
                        }

                    }

                    i++;
                }

                return msg;
            }
            else if (filename == "DP")
            {
                string msg = string.Empty;

                foreach (DataColumn dc in datatab.Columns)
                {
                    if (dc.ColumnName == "Dealer Code" || dc.ColumnName == "Employee Position Type" || dc.ColumnName == "Employee ID" || dc.ColumnName == "Employee sub-type" || dc.ColumnName == "Employee Login" || dc.ColumnName == "User Status" || dc.ColumnName == "Employee First Name" || dc.ColumnName == "Employee Last Name" || dc.ColumnName == "Employee Email ID" || dc.ColumnName == "Cell Phone" || dc.ColumnName == "Dealer" || dc.ColumnName == "Area" || dc.ColumnName == "Region")
                    {
                        msg = "True";
                        //return msg;
                    }
                    else
                    {
                        msg = "Column names are not matched.Please check the column names or download the data format and match the columns";
                        return msg;
                    }
                }
                int i = 1;

                foreach (DataRow row in datatab.Rows)
                {
                    foreach (DataColumn col in datatab.Columns)
                    {
                        //test for null here
                        if (row[0] == DBNull.Value || string.IsNullOrEmpty(row[0].ToString()) || row[0].ToString() == string.Empty)
                        {
                            msg = "Please Check Row " + i + " found Empty or Null value in Column " + col.ColumnName.ToString() + ".";

                            return msg;
                        }

                    }

                    i++;
                }

                return msg;
            }
            else if (filename == "DSAdmn")
            {
                string msg = string.Empty;

                foreach (DataColumn dc in datatab.Columns)
                {
                    if (dc.ColumnName == "Dealer Code" || dc.ColumnName == "Employee Position Type" || dc.ColumnName == "Employee Login" || dc.ColumnName == "User Status" || dc.ColumnName == "Employee First Name" || dc.ColumnName == "Employee Last Name" || dc.ColumnName == "Employee Email ID" || dc.ColumnName == "Cell Phone" || dc.ColumnName == "Employee ID" || dc.ColumnName == "Employee sub-type" || dc.ColumnName == "Dealer" || dc.ColumnName == "Area" || dc.ColumnName == "Region")
                    {
                        msg = "True";
                        //return msg;
                    }
                    else
                    {
                        msg = "Column names are not matched.Please check the column names or download the data format and match the columns";
                        return msg;
                    }
                }
                int i = 1;

                foreach (DataRow row in datatab.Rows)
                {
                    foreach (DataColumn col in datatab.Columns)
                    {
                        //test for null here
                        if (row[0] == DBNull.Value || string.IsNullOrEmpty(row[0].ToString()) || row[0].ToString() == string.Empty)
                        {
                            msg = "Please Check Row " + i + " found Empty or Null value in Column " + col.ColumnName.ToString() + ".";

                            return msg;
                        }

                    }

                    i++;
                }

                return msg;
            }
            else //if (filename == "Activity Detailed Report")
            {
                string msg = string.Empty;

                foreach (DataColumn dc in datatab.Columns)
                {
                    if (dc.ColumnName == "Month" || dc.ColumnName == "Region" || dc.ColumnName == "Dealer Code" || dc.ColumnName == "Dealer" || dc.ColumnName == "Handling Division" || dc.ColumnName == "Activity No" || dc.ColumnName == "Activty Type" || dc.ColumnName == "Activty SubType" || dc.ColumnName == "First Name" || dc.ColumnName == "Last Name" || dc.ColumnName == "Phone No Cell" || dc.ColumnName == "LOB" || dc.ColumnName == "PPL" || dc.ColumnName == "PL" || dc.ColumnName == "Chassis No" || dc.ColumnName == "Sale date" || dc.ColumnName == "Activity Created Date" || dc.ColumnName == "Activty Status" || dc.ColumnName == "Call Status" || dc.ColumnName == "FBV Indicator" || dc.ColumnName == "Appointment Date" || dc.ColumnName == "Appointment Assigned To" || dc.ColumnName == "Activity Completion Date" || dc.ColumnName == "CRE ID" || dc.ColumnName == "VOC")
                    {
                        msg = "True";
                        //return msg;
                    }
                    else
                    {
                        msg = "Column names are not matched.Please check the column names or download the data format and match the columns";
                        return msg;
                    }
                }
                int i = 1;

                foreach (DataRow row in datatab.Rows)
                {
                    foreach (DataColumn col in datatab.Columns)
                    {
                        //test for null here
                        if (row[2] == DBNull.Value || string.IsNullOrEmpty(row[2].ToString()) || row[2].ToString() == string.Empty)
                        {
                            msg = "Please Check Row " + i + " found Empty or Null value in Column " + col.ColumnName.ToString() + ".";

                            return msg;
                        }

                    }
                    if (row[2].ToString().Length != 7)
                    {
                        msg = "Dealer Code contain only 7 characters.";

                        return msg;
                    }

                    i++;
                }

                return msg;
            }

            //else
            //{
            //    string msg = string.Empty;

            //    foreach (DataColumn dc in datatab.Columns)
            //    {
            //        if (dc.ColumnName == "Month" || dc.ColumnName == "Dealer Name" || dc.ColumnName == "Dealer code" || dc.ColumnName == "Division" || dc.ColumnName == "Region" || dc.ColumnName == "Satisfied%")
            //        {
            //            msg = "True";
            //            //return msg;
            //        }
            //        else
            //        {
            //            msg = "Column names are not matched.Please check the column names or download the data format and match the columns";
            //            return msg;
            //        }
            //    }
            //    int i = 1;

            //    foreach (DataRow row in datatab.Rows)
            //    {
            //        foreach (DataColumn col in datatab.Columns)
            //        {
            //            //test for null here
            //            if (row[col] == DBNull.Value || string.IsNullOrEmpty(row[col].ToString()) || row[col].ToString() == string.Empty)
            //            {
            //                msg = "Please Check Row " + i + " found Empty or Null value in Column " + col.ColumnName.ToString() + ".";

            //                return msg;
            //            }

            //        }

            //        i++;
            //    }

            //    return msg;
            //}
        }

        public string UpdateBulkupload(DataTable dt, string Proc, string filename)
        {
            if (filename == "Activities Done And Open")
            {
                //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
                string flag = string.Empty;
                string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {
                    try
                    {
                        var cmd = new SqlCommand(_sql2, cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@Uid";
                        param.Value = Session["Uid"].ToString();
                        cmd.Parameters.Add(param);

                        SqlParameter param1 = new SqlParameter();
                        param1.ParameterName = "@TypeActivitiesDoneAndOpen";
                        param1.Value = dt;
                        cmd.Parameters.Add(param1);

                        cn.Open();
                        int count = cmd.ExecuteNonQuery();
                        cn.Close();
                        cn.Dispose();
                        if (count > 0)
                        {
                            flag = "succ";
                        }
                        else
                        {
                            flag = "False";
                        }
                    }
                    catch (Exception e)
                    {
                        flag = e.Message;
                    }

                }
                return flag;
            }
            else if (filename == "Sales Satisfaction")
            {
                //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
                string flag = string.Empty;
                string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {
                    try
                    {
                        var cmd = new SqlCommand(_sql2, cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@Uid";
                        param.Value = Session["Uid"].ToString();
                        cmd.Parameters.Add(param);

                        SqlParameter param1 = new SqlParameter();
                        param1.ParameterName = "@TypeSalesSatisfaction";
                        param1.Value = dt;
                        cmd.Parameters.Add(param1);

                        cn.Open();
                        int count = cmd.ExecuteNonQuery();
                        cn.Close();
                        cn.Dispose();
                        if (count > 0)
                        {
                            flag = "succ";
                        }
                        else
                        {
                            flag = "False";
                        }
                    }
                    catch (Exception e)
                    {
                        flag = e.Message;
                    }

                }
                return flag;
            }
            else if (filename == "CEI")
            {
                //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
                string flag = string.Empty;
                string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {
                    try
                    {
                        var cmd = new SqlCommand(_sql2, cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@Uid";
                        param.Value = Session["Uid"].ToString();
                        cmd.Parameters.Add(param);

                        SqlParameter param1 = new SqlParameter();
                        param1.ParameterName = "@TypeCEI";
                        param1.Value = dt;
                        cmd.Parameters.Add(param1);

                        cn.Open();
                        int count = cmd.ExecuteNonQuery();
                        cn.Close();
                        cn.Dispose();
                        if (count > 0)
                        {
                            flag = "succ";
                        }
                        else
                        {
                            flag = "False";
                        }
                    }
                    catch (Exception e)
                    {
                        flag = e.Message;
                    }

                }
                return flag;
            }
            else if (filename == "Retail 2")
            {
                //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
                string flag = string.Empty;
                string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {
                    try
                    {
                        var cmd = new SqlCommand(_sql2, cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@Uid";
                        param.Value = Session["Uid"].ToString();
                        cmd.Parameters.Add(param);

                        SqlParameter param1 = new SqlParameter();
                        param1.ParameterName = "@TypeRetail2";
                        param1.Value = dt;
                        cmd.Parameters.Add(param1);

                        cn.Open();
                        int count = cmd.ExecuteNonQuery();
                        cn.Close();
                        cn.Dispose();
                        if (count > 0)
                        {
                            flag = "succ";
                        }
                        else
                        {
                            flag = "False";
                        }
                    }
                    catch (Exception e)
                    {
                        flag = e.Message;
                    }

                }
                return flag;
            }
            else if (filename == "Tata OK")
            {
                //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
                string flag = string.Empty;
                string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {
                    try
                    {
                        var cmd = new SqlCommand(_sql2, cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@Uid";
                        param.Value = Session["Uid"].ToString();
                        cmd.Parameters.Add(param);

                        SqlParameter param1 = new SqlParameter();
                        param1.ParameterName = "@TypeTataOk";
                        param1.Value = dt;
                        cmd.Parameters.Add(param1);

                        cn.Open();
                        int count = cmd.ExecuteNonQuery();
                        cn.Close();
                        cn.Dispose();
                        if (count > 0)
                        {
                            flag = "succ";
                        }
                        else
                        {
                            flag = "False";
                        }
                    }
                    catch (Exception e)
                    {
                        flag = e.Message;
                    }

                }
                return flag;
            }
            else if (filename == "AMC Policy")
            {
                //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
                string flag = string.Empty;
                string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {
                    try
                    {
                        var cmd = new SqlCommand(_sql2, cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@Uid";
                        param.Value = Session["Uid"].ToString();
                        cmd.Parameters.Add(param);

                        SqlParameter param1 = new SqlParameter();
                        param1.ParameterName = "@TypeAMCPolicy";
                        param1.Value = dt;
                        cmd.Parameters.Add(param1);

                        cn.Open();
                        int count = cmd.ExecuteNonQuery();
                        cn.Close();
                        cn.Dispose();
                        if (count > 0)
                        {
                            flag = "succ";
                        }
                        else
                        {
                            flag = "False";
                        }
                    }
                    catch (Exception e)
                    {
                        flag = e.Message;
                    }

                }
                return flag;
            }
            else if (filename == "Workshop Revenue Report")
            {
                //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
                string flag = string.Empty;
                string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {
                    try
                    {
                        var cmd = new SqlCommand(_sql2, cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@Uid";
                        param.Value = Session["Uid"].ToString();
                        cmd.Parameters.Add(param);

                        SqlParameter param1 = new SqlParameter();
                        param1.ParameterName = "@TypeWorkshopRevenueReport";
                        param1.Value = dt;
                        cmd.Parameters.Add(param1);

                        cn.Open();
                        int count = cmd.ExecuteNonQuery();
                        cn.Close();
                        cn.Dispose();
                        if (count > 0)
                        {
                            flag = "succ";
                        }
                        else
                        {
                            flag = "False";
                        }
                    }
                    catch (Exception e)
                    {
                        flag = e.Message;
                    }

                }
                return flag;
            }
            else if (filename == "Activity Detailed Report")
            {
                //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
                string flag = string.Empty;
                string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {
                    try
                    {
                        var cmd = new SqlCommand(_sql2, cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@Uid";
                        param.Value = Session["Uid"].ToString();
                        cmd.Parameters.Add(param);

                        SqlParameter param1 = new SqlParameter();
                        param1.ParameterName = "@TypeActivityDetailedReport";
                        param1.Value = dt;
                        cmd.Parameters.Add(param1);

                        cn.Open();
                        int count = cmd.ExecuteNonQuery();
                        cn.Close();
                        cn.Dispose();
                        if (count > 0)
                        {
                            flag = "succ";
                        }
                        else
                        {
                            flag = "False";
                        }
                    }
                    catch (Exception e)
                    {
                        flag = e.Message;
                    }

                }
                return flag;
            }
            else if (filename == "DP")
            {
                //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
                string flag = string.Empty;
                string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {
                    try
                    {
                        var cmd = new SqlCommand(_sql2, cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@Uid";
                        param.Value = Session["Uid"].ToString();
                        cmd.Parameters.Add(param);

                        SqlParameter param1 = new SqlParameter();
                        param1.ParameterName = "@TypeDP";
                        param1.Value = dt;
                        cmd.Parameters.Add(param1);

                        cn.Open();
                        int count = cmd.ExecuteNonQuery();
                        cn.Close();
                        cn.Dispose();
                        if (count > 0)
                        {
                            flag = "succ";
                        }
                        else
                        {
                            flag = "False";
                        }
                    }
                    catch (Exception e)
                    {
                        flag = e.Message;
                    }

                }
                return flag;
            }
            else if (filename == "DSAdmn")
            {
                //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
                string flag = string.Empty;
                string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {
                    try
                    {
                        var cmd = new SqlCommand(_sql2, cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@Uid";
                        param.Value = Session["Uid"].ToString();
                        cmd.Parameters.Add(param);

                        SqlParameter param1 = new SqlParameter();
                        param1.ParameterName = "@TypeDSAdmn";
                        param1.Value = dt;
                        cmd.Parameters.Add(param1);

                        cn.Open();
                        int count = cmd.ExecuteNonQuery();
                        cn.Close();
                        cn.Dispose();
                        if (count > 0)
                        {
                            flag = "succ";
                        }
                        else
                        {
                            flag = "False";
                        }
                    }
                    catch (Exception e)
                    {
                        flag = e.Message;
                    }

                }
                return flag;
            }
            else //if (filename == "CPSC Wrong Number")
            {
                //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
                string flag = string.Empty;
                string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
                string _sql2 = string.Empty;
                _sql2 = Proc;
                using (SqlConnection cn = new SqlConnection(CS1))
                {
                    try
                    {

                        var cmd = new SqlCommand(_sql2, cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@Uid";

                        param.Value = Session["Uid"].ToString();
                        cmd.Parameters.Add(param);

                        SqlParameter param1 = new SqlParameter();
                        param1.ParameterName = "@TypeCPSCWrongNumber";
                        param1.Value = dt;
                        cmd.Parameters.Add(param1);

                        cn.Open();
                        int count = cmd.ExecuteNonQuery();
                        cn.Close();
                        cn.Dispose();
                        if (count > 0)
                        {
                            flag = "succ";
                        }
                        else
                        {
                            flag = "False";
                        }

                    }
                    catch (Exception e)
                    {
                        flag = e.Message;
                    }

                }
                return flag;
            }
            //else
            //{
            //    //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
            //    string flag = string.Empty;
            //    string CS1 = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
            //    string _sql2 = string.Empty;
            //    _sql2 = Proc;
            //    using (SqlConnection cn = new SqlConnection(CS1))
            //    {

            //        var cmd = new SqlCommand(_sql2, cn);
            //        cmd.CommandType = CommandType.StoredProcedure;

            //        SqlParameter param = new SqlParameter();
            //        param.ParameterName = "@Uid";
            //        param.Value = Session["Uid"].ToString();
            //        cmd.Parameters.Add(param);

            //        SqlParameter param1 = new SqlParameter();
            //        param1.ParameterName = "@TypeSalesSatisfaction";
            //        param1.Value = dt;
            //        cmd.Parameters.Add(param1);

            //        cn.Open();
            //        int count = cmd.ExecuteNonQuery();
            //        cn.Close();
            //        cn.Dispose();
            //        if (count > 0)
            //        {
            //            flag = "succ";
            //        }
            //        else
            //        {
            //            flag = "False";
            //        }

            //    }
            //    return flag;
            //}
        }

        [HttpPost]

        public ActionResult Importexcel1()
        {
            if (Session["Uid"] == null)
            {
                return this.Content("Session Expired Login Again");
            }


            int cout = 0;

            HttpPostedFileBase file = Request.Files[0];
            string sFileExtension = Path.GetExtension(file.FileName).ToLower();
            if (sFileExtension != ".xlsx")
            {
                return this.Content("Please select only excel file xlsx format");

            }
            DateTime now = DateTime.Today;
            string filename = Request.Form["filename"].ToString();
            string CurrentMonth = now.ToString("MMM");
            string CurrnetYear = now.ToString("yyyy");
            string PreviousMonth = now.AddMonths(-1).ToString("MMM");
            string PreviousYear = now.AddMonths(-1).ToString("yyyy");
            string Timespan = Guid.NewGuid().ToString();

            string fullPath = "";
            int distributorcount = 0;
            try
            {
                //distributorcount = entity.tbl_Country_Distributor_Master.Where(x => x.Status == "Active").Count();
                //if(distributorcount==0)
                //{

                //}
                //else
                //{
                //    distributorcount += 1;
                //}
            }
            catch (Exception)
            {
                distributorcount = 100;
            }


            // string folderName = now.ToString("yyyy");
            string message = "";
            FileStream stream = null;

            // string path = Server.MapPath("~/App_Data/"+ folderName);





            if (file.ContentLength > 0)
            {


                string path = HostingEnvironment.MapPath("/");
                path = Path.Combine(path, "App_Data\\" + CurrnetYear + "\\" + CurrentMonth + "");
                fullPath = Path.Combine(path, "" + filename + "_" + "_" + Timespan + ".xlsx");


                if (filename == "Activities Done And Open")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Month", "Region", "Dealer code", "Dealer", "Done", "Done & Open" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 6; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                //    for (int colNumber = 0; colNumber < 6; colNumber++)
                                //    {
                                //        ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                //        if (colNumber == 0 || colNumber == 1 || colNumber == 3 || colNumber == 8)
                                //        {
                                //            if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                //            {
                                //                System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                //            }
                                //        }
                                //        if (colNumber == 2 || colNumber == 9 || colNumber == 7)
                                //        {
                                //            if (IsNumeric(cell.ToString()))
                                //            {
                                //                if (Convert.ToDouble(cell.ToString()) >= 0)
                                //                {
                                //                    //positive
                                //                }
                                //                else
                                //                {
                                //                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                //                }
                                //            }
                                //            else
                                //            {
                                //                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                //            }
                                //        }
                                //        if (colNumber == 4 || colNumber == 5)
                                //        {
                                //            if (colNumber == 4)
                                //            {
                                //                if (cell.ToString() == PreviousMonth)
                                //                {
                                //                    //positive
                                //                }
                                //                else
                                //                {
                                //                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Month Data");
                                //                }

                                //            }
                                //            else if (colNumber == 5)
                                //            {
                                //                if (cell.ToString() == PreviousYear)
                                //                {
                                //                    //positive
                                //                }
                                //                else
                                //                {
                                //                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                //                }

                                //            }
                                //        }




                                //    }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }
                else if (filename == "P1_Actual")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Month", "Year", "Sub Para", "Metric", "Wt", "Evaluation", "Actual" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 11; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 11; colNumber++)
                                {
                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3 || colNumber == 7 || colNumber == 10)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath);
                                            return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 6)
                                    {
                                        string[] printer1 = { "P1 a)", "P1 b)", "P1 c)", "P1 d)", "P1 e)" };
                                        if (Array.Exists(printer1, element => element == cell.ToString()))
                                        {
                                            continue;
                                        }
                                        else
                                        {

                                            System.IO.File.Delete(fullPath); return this.Content("Please " + cell.ToString() + "Check File Sub para sequence according to the format or not");
                                        }



                                    }
                                    if (colNumber == 2 || colNumber == 8)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) > 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 4 || colNumber == 5)
                                    {
                                        if (colNumber == 4)
                                        {
                                            if (cell.ToString() == PreviousMonth)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Month Data");
                                            }

                                        }
                                        else if (colNumber == 5)
                                        {
                                            if (cell.ToString() == PreviousYear)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                            }

                                        }
                                    }




                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }
                else if (filename == "F1_Target")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Category", "LOB", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb", "Mar" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 18; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {
                                    if (printer[j].Contains(cell.ToString().Remove(cell.ToString().Length - 3, 3)))
                                    {
                                        continue;
                                    }

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 18; colNumber++)
                                {
                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3 || colNumber == 4 || colNumber == 5)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + "  Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 2)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) > 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    //if (colNumber == 4)
                                    //{

                                    //    if (cell.ToString() == PreviousYear)
                                    //    {
                                    //        //positive
                                    //    }
                                    //    else
                                    //    {
                                    //        System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                    //    }


                                    //}
                                    if (colNumber == 6 || colNumber == 7 || colNumber == 8 || colNumber == 9 || colNumber == 10 || colNumber == 11 || colNumber == 12 || colNumber == 13 || colNumber == 14 || colNumber == 15 || colNumber == 16 || colNumber == 17)
                                    {
                                        if (cell.ToString() == "")
                                        {
                                            continue;
                                        }
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) >= 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }


                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }

                else if (filename == "F2_a_Actual")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Month", "Year", "Terms of Payment", "Discription" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 8; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= distributorcount; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 8; colNumber++)
                                {
                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);

                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3 || colNumber == 6 || colNumber == 7)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 2)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) > 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 4 || colNumber == 5)
                                    {
                                        if (colNumber == 4)
                                        {
                                            if (cell.ToString() == PreviousMonth)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Month Data");
                                            }

                                        }
                                        else if (colNumber == 5)
                                        {
                                            if (cell.ToString() == PreviousYear)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                            }

                                        }
                                    }

                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }
                else if (filename == "F2_b_EVR_CFR")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Month", "Year", "PI Qty", "TOTAL CFR" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 8; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= distributorcount; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 8; colNumber++)
                                {

                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 2 || colNumber == 6 || colNumber == 7)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) >= 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 4 || colNumber == 5)
                                    {
                                        if (colNumber == 4)
                                        {
                                            if (cell.ToString() == PreviousMonth)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Month Data");
                                            }

                                        }
                                        else if (colNumber == 5)
                                        {
                                            if (cell.ToString() == PreviousYear)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                            }

                                        }
                                    }

                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }
                else if (filename == "F2_b_Fund_Balance_Vehicles")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Month", "Year", "Vehicle  Funds avilable" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 7; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= distributorcount; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 7; colNumber++)
                                {

                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 2)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) > 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 6)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                        else if (IsNumeric(cell.ToString()))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 4 || colNumber == 5)
                                    {
                                        if (colNumber == 4)
                                        {
                                            if (cell.ToString() == PreviousMonth)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Month Data");
                                            }

                                        }
                                        else if (colNumber == 5)
                                        {
                                            if (cell.ToString() == PreviousYear)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                            }

                                        }
                                    }
                                }



                            }








                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }
                else if (filename == "F3_a_Shipment_Tgt_Spares")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Customer Code", "Month", "Year", "Billing  Target", "Sales in Crs" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 9; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= distributorcount; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 9; colNumber++)
                                {

                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 2 || colNumber == 7 || colNumber == 8)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) >= 0)
                                            {

                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 5 || colNumber == 6)
                                    {
                                        if (colNumber == 5)
                                        {
                                            if (cell.ToString() == PreviousMonth)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Month Data");
                                            }

                                        }
                                        else if (colNumber == 6)
                                        {
                                            if (cell.ToString() == PreviousYear)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                            }

                                        }
                                    }

                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }

                else if (filename == "F3_b_OG_Target_Actual")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Customer Code", "Month", "Year", "OG Target", "OG Actual" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 9; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= distributorcount; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 9; colNumber++)
                                {
                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + "  Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 2 || colNumber == 7 || colNumber == 8)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) >= 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 5)
                                    {

                                        if (cell.ToString() == PreviousMonth)
                                        {
                                            //positive
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Month Data");
                                        }


                                    }
                                    if (colNumber == 6)
                                    {

                                        if (cell.ToString() == PreviousYear)
                                        {
                                            //positive
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                        }


                                    }



                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }

                else if (filename == "F3_b_FundBalance_SpareParts")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Customer Code", "Month", "Year", "Spare Parts Funds avilable" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 8; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= distributorcount; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 8; colNumber++)
                                {

                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 2)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToInt32(cell.ToString()) > 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 7)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            //if (Convert.ToDouble(cell.ToString()) > 0)
                                            //{
                                            //    //positive
                                            //}
                                            //else
                                            //{
                                            //    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            //}
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 5 || colNumber == 6)
                                    {
                                        if (colNumber == 5)
                                        {
                                            if (cell.ToString() == PreviousMonth)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Month Data");
                                            }

                                        }
                                        else if (colNumber == 6)
                                        {
                                            if (cell.ToString() == PreviousYear)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                            }

                                        }
                                    }

                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }

                else if (filename == "F4_a_Durafit_Tyre_TMGO")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Customer Code", "Month", "Year", "Durafit", "TYRE", "TMGO" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 10; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= distributorcount; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 10; colNumber++)
                                {

                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 2 || colNumber == 7 || colNumber == 8 || colNumber == 9)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) >= 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 5 || colNumber == 6)
                                    {
                                        if (colNumber == 5)
                                        {
                                            if (cell.ToString() == PreviousMonth)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Month Data");
                                            }

                                        }
                                        else if (colNumber == 6)
                                        {
                                            if (cell.ToString() == PreviousYear)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                            }

                                        }
                                    }

                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }
                else if (filename == "C1_SSI")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Month", "Year", "C1" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 7; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= distributorcount; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 7; colNumber++)
                                {

                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 2 || colNumber == 6)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) >= 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 4 || colNumber == 5)
                                    {
                                        if (colNumber == 4)
                                        {
                                            if (cell.ToString() == PreviousMonth)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Month Data");
                                            }

                                        }
                                        else if (colNumber == 5)
                                        {
                                            if (cell.ToString() == PreviousYear)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                            }

                                        }
                                    }

                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }
                else if (filename == "C2_CSI")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Month", "Year", "C2" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 7; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= distributorcount; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 7; colNumber++)
                                {

                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 2 || colNumber == 6)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) >= 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 4 || colNumber == 5)
                                    {
                                        if (colNumber == 4)
                                        {
                                            if (cell.ToString() == PreviousMonth)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Month Data");
                                            }

                                        }
                                        else if (colNumber == 5)
                                        {
                                            if (cell.ToString() == PreviousYear)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                            }

                                        }
                                    }

                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }

                else if (filename == "C3_LastYearExit")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";









                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "March Exit", "M HCV", "I LCV", "SCV Pu W", "Buses" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 9; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= distributorcount; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 9; colNumber++)
                                {

                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 5 || colNumber == 6 || colNumber == 7 || colNumber == 8)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            continue;
                                        }
                                        else if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) >= 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 2)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) > 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 4)
                                    {
                                        if (colNumber == 4)
                                        {
                                            if (cell.ToString() == PreviousYear)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                            }

                                        }

                                    }

                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }
                else if (filename == "C3_CurrentYear")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";









                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Current Year Target", "M HCV", "I LCV", "SCV Pu W", "Buses" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {

                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 9; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= distributorcount; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 9; colNumber++)
                                {

                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 5 || colNumber == 6 || colNumber == 7 || colNumber == 8)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            continue;
                                        }
                                        else if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) >= 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 2)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToDouble(cell.ToString()) > 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 4)
                                    {
                                        if (colNumber == 4)
                                        {
                                            if (cell.ToString() == PreviousYear)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                            }

                                        }

                                    }

                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }
                else if (filename == "P3_Actual")
                {

                    sFileExtension = Path.GetExtension(file.FileName).ToLower();

                    ISheet sheet;
                    // path = path + "\\"+Section+"\\" + "\\"+Parameter+"\\" + "\\"+SubParameter+"\\" + "\\"+Type+"\\";










                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    // string fullPath = Path.Combine(path);
                    // fullPath +=""+ SubParameter + ""+ "_"+CurrnetYear+"_" + CurrentMonth+"_"+Type+"" + sFileExtension;
                    int retry = 0;
                    retry_possibility:
                    retry++;
                    try
                    {
                        using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
                        {

                            file.InputStream.CopyTo(stream);


                            stream.Position = 0;

                            if (sFileExtension == ".xls")

                            {

                                HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                            }

                            else

                            {

                                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                                hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                                sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                            }














                            IRow headerRow = sheet.GetRow(0);
                            string[] printer = { "Region", "Country", "Distributor Code", "Distributor Name", "Month", "Year", "SEGMENT", "EVR QTY", "EVR DATE" };
                            int cellCount = headerRow.LastCellNum;
                            if (printer.Length != cellCount)
                            {
                                System.IO.File.Delete(fullPath); return this.Content("Extra columns is there please follow format");
                            }


                            for (int j = 0; j < 9; j++)
                            {

                                NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);

                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                {

                                    System.IO.File.Delete(fullPath); return this.Content((j + 1) + " Columns has no Heading");

                                }



                                if (printer[j].Contains(cell.ToString()))
                                {
                                    continue;
                                }
                                else
                                {

                                    System.IO.File.Delete(fullPath); return this.Content("Please Check File order sequence according to the format or not");
                                }







                            }



                            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                            {




                                IRow row = sheet.GetRow(i);




                                if (row == null)
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Row");

                                }

                                if (row.Cells.All(d => d.CellType == NPOI.SS.UserModel.CellType.Blank))
                                {
                                    System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row contain empty or blank Cells");

                                }
                                for (int colNumber = 0; colNumber < 9; colNumber++)
                                {

                                    ICell cell = row.GetCell(colNumber, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                                    if (colNumber == 0 || colNumber == 1 || colNumber == 3 || colNumber == 6 || colNumber == 8)
                                    {
                                        if (cell.ToString() == "" || cell.ToString() == null || cell.ToString() == String.Empty)
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content((i + 1) + " Row " + (colNumber + 1) + " Column has Empty or blank");
                                        }
                                    }
                                    if (colNumber == 2 || colNumber == 7)
                                    {
                                        if (IsNumeric(cell.ToString()))
                                        {
                                            if (Convert.ToInt32(cell.ToString()) >= 0)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Should be greater than 0");
                                            }
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain  number Only");
                                        }
                                    }
                                    if (colNumber == 4 || colNumber == 5)
                                    {
                                        if (colNumber == 4)
                                        {
                                            if (cell.ToString() == PreviousMonth)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Month Data");
                                            }

                                        }
                                        else if (colNumber == 5)
                                        {
                                            if (cell.ToString() == PreviousYear)
                                            {
                                                //positive
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(fullPath); return this.Content("Please Check " + (i + 1) + " Row " + (colNumber + 1) + " Column contain Current Year Data");
                                            }

                                        }
                                    }

                                }





                            }


                            cout++;
                            if (cout > 0)
                            {
                                BackgroundJob.Enqueue(() => excelexporttosql(filename, CurrentMonth, CurrnetYear, Timespan, Session["E_Mail"].ToString()));
                            }
                            cout--;
                        }
                    }
                    catch (IOException ex)
                    {
                        if (retry <= 3)
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream = null;
                            }
                            goto retry_possibility;
                        }
                        else
                        {
                            System.IO.File.Delete(fullPath); return this.Content(ex.Message);
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        System.IO.File.Delete(fullPath); return this.Content("Out Of Range columns is there");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(fullPath); return this.Content(ex.Message.ToString());
                    }

                }


                else
                {
                    System.IO.File.Delete(fullPath); return this.Content("Please check File Name is correct or not");
                }
            }
            else
            {
                return this.Content("I think File is blank please check");
            }

            //entity.Sp_Log_Insert_FileUpload_Record(Session["LoginId"].ToString(), filename, fullPath);
            return this.Content("File has been Successfully submit");
        }

        //public ActionResult Importexcel1()
        //{
        //    UploadFile UploadFile = new UploadFile();
        //    DataTable dt_Orignal = new DataTable();
        //    if (ModelState.IsValid)
        //    {

        //        if (UploadFile.ExcelFile.ContentLength > 0)
        //        {
        //            if (UploadFile.ExcelFile.FileName.EndsWith(".xlsx") || UploadFile.ExcelFile.FileName.EndsWith(".xls"))
        //            {
        //                XLWorkbook Workbook;
        //                try
        //                {
        //                    Workbook = new XLWorkbook(UploadFile.ExcelFile.InputStream);
        //                }
        //                catch (Exception ex)
        //                {
        //                    ModelState.AddModelError(String.Empty, $"Check your file. {ex.Message}");
        //                    return View();
        //                }
        //                IXLWorksheet WorkSheet = null;

        //                try//incase if the sheet you are looking for is not found
        //                {
        //                    WorkSheet = Workbook.Worksheet(1);

        //                }
        //                catch
        //                {
        //                    ModelState.AddModelError(String.Empty, "Sheet not found!");
        //                    return View();
        //                }
        //                //WorkSheet.FirstRow().Delete();//if you want to remove ist row
        //                dt_Orignal = ExceltoDatatable(WorkSheet);

        //                string err_msg = validatedatatable(dt_Orignal);

        //                if (err_msg == "True")
        //                {
        //                    dt_Orignal = ExceltoDatatable(WorkSheet);
        //                    err_msg = UpdateBulkupload(dt_Orignal, "SpInsertDataActiviesDoneAndOpen");
        //                    if (err_msg != "succ")
        //                    {
        //                        Session["result"] = err_msg;
        //                    }
        //                }
        //                else
        //                {
        //                    ModelState.AddModelError(String.Empty, err_msg);
        //                    return View();
        //                }
        //            }
        //            else
        //            {
        //                ModelState.AddModelError(String.Empty, "Only .xlsx and .xls files are allowed");
        //                return View();
        //            }
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(String.Empty, "Not a valid file");
        //            return View();
        //        }
        //    }

        //    return View();
        //}

        public bool IsNumeric(string value)
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(value), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
        private object getCellValue(ICell cell)
        {
            object cValue = string.Empty;
            switch (cell.CellType)
            {
                case (NPOI.SS.UserModel.CellType.Unknown):
                    cValue = cell.ToString();
                    break;
                case (NPOI.SS.UserModel.CellType.Blank):
                    cValue = cell.ToString();
                    break;
                case (NPOI.SS.UserModel.CellType.Formula):

                    try
                    {
                        cValue = cell.StringCellValue;
                    }
                    catch (Exception ex)
                    {
                        cValue = cell.NumericCellValue;
                    }
                    //var k = cell.ToString();

                    //cValue = cell.ToString();
                    //if (k == cValue)
                    //{
                    //    cValue = cell.StringCellValue;
                    //}
                    //if (k == cValue)
                    //{
                    //    cValue = cell.NumericCellValue;
                    //}

                    break;
                case NPOI.SS.UserModel.CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        DateTime date = cell.DateCellValue;
                        //ICellStyle style = cell.CellStyle;
                        // Excel uses lowercase m for month whereas .Net uses uppercase
                        //string format = style.GetDataFormatString().Replace('m', 'M');
                        cValue = date.ToString();
                    }
                    else
                    {
                        cValue = cell.NumericCellValue.ToString("0.########");

                    }

                    break;
                case NPOI.SS.UserModel.CellType.String:
                    cValue = cell.StringCellValue;
                    break;
                case NPOI.SS.UserModel.CellType.Boolean:
                    cValue = cell.BooleanCellValue;
                    break;
                case NPOI.SS.UserModel.CellType.Error:
                    cValue = cell.ErrorCellValue;
                    break;
                default:
                    cValue = string.Empty;
                    break;
            }
            return cValue;
        }
        public void excelexporttosql(string filename, string CurrentMonth, string CurrnetYear, string Timespan, string toemail)
        {


            string path = HostingEnvironment.MapPath("/");
            path = Path.Combine(path, "App_Data\\" + CurrnetYear + "\\" + CurrentMonth + "");
            string fullPath = Path.Combine(path, "" + filename + "_" + "_" + Timespan + ".xlsx");
            string TableName = "Tbl_Concurrent_Activities done and Open";
            string conString = "";
            conString = ConfigurationManager.ConnectionStrings["constrOLAP"].ConnectionString;
            int retry = 0;
            retry_possibility:
            retry++;

            try
            {
                string tablename = String.Empty;

                tablename = TableName;/* Path.GetFileNameWithoutExtension(fullPath);*/


                DataTable dt = new DataTable();

                dt = GetDataTableFromExcel(fullPath, CurrentMonth, CurrnetYear);
                //if (filename == "F1_Target")
                //{
                //    dt.Columns.Add(new DataColumn("Year") { DefaultValue = GetFinancialYear(DateTime.Now) });
                //    dt.Columns["Year"].SetOrdinal(dt.Columns.IndexOf("Category"));
                //}
                SqlConnection SQLConnection = new SqlConnection();
                SqlConnection SQLConnection1 = new SqlConnection();

                SqlCommand SQLCmd = new SqlCommand();
                SqlCommand SQLCmd1 = new SqlCommand();

                //string tableDDL = "";
                //tableDDL += "IF Not EXISTS (SELECT * FROM sys.objects WHERE object_id = ";
                //tableDDL += "OBJECT_ID(N'[dbo].[" + tablename + "]') AND type in (N'U'))";
                ////tableDDL += "Drop Table [dbo].[" + tablename + "]";

                //tableDDL += "Create table [" + tablename + "]";
                //tableDDL += "(";

                //for (int i = 0; i < dt.Columns.Count; i++)
                //{
                //    if (i != dt.Columns.Count - 1)
                //        tableDDL += "[" + dt.Columns[i].ColumnName + "] " + "NVarchar(max)" + ",";
                //    else
                //        tableDDL += "[" + dt.Columns[i].ColumnName + "] " + "NVarchar(max)";
                //}
                ////tableDDL += ",[ID] " + "int primary key identity(1,1)";
                //tableDDL += ")";
                ////Insert the Data read from the Excel file to Database Table.

                ////Create Connection to SQL Server Database 

                //SQLConnection.ConnectionString = conString;
                //SQLConnection.Open();
                //SQLCmd = new SqlCommand(tableDDL, SQLConnection);
                //SQLCmd.ExecuteNonQuery();

                //// Load the data from DataTable to SQL Server Table.
                //SqlBulkCopy blk = new SqlBulkCopy(SQLConnection);
                //blk.DestinationTableName = "[" + tablename + "]";
                //blk.WriteToServer(dt);
                //SQLConnection.Close();

                //if (tablename == "C3_CurrentYear" || tablename == "F2_b_Fund_Balance_Vehicles" || tablename == "F3_b_FundBalance_SpareParts")
                //{
                if (filename == "F3_a_Shipment_Tgt_Spares"
                    || filename == "F3_b_FundBalance_SpareParts"
                    || filename == "F3_b_OG_Target_Actual"
                    || filename == "F4_a_Durafit_Tyre_TMGO"
                    )
                {

                    dt.Columns["Customer Code"].SetOrdinal(dt.Columns.IndexOf("Current_Year"));

                }
                if (filename == "F1_Target")
                {

                    dt.Columns.Add(new DataColumn("Year") { DefaultValue = GetFinancialYear(DateTime.Now) });
                    dt.Columns["Year"].SetOrdinal(dt.Columns.IndexOf("Category"));

                    string tableDDL = ""; string tableDDL1 = "";
                    int columncount = dt.Columns.Count;
                    tableDDL = "";
                    tableDDL += "DECLARE @COLUMNS NVARCHAR(max)";
                    tableDDL += "SELECT @Columns = SubString (Replace(( SELECT Top " + columncount + " ', ' + QUOTENAME(Column_name ) from INFORMATION_SCHEMA.columns WHERE Table_name ='" + tablename + "' ";
                    tableDDL += "FOR XML PATH ( '' )),'&amp;','&'), 3, 100000)";
                    tableDDL += "declare @insertquery nvarchar(max)";
                    tableDDL += "declare @insertquery1 nvarchar(max)";
                    tableDDL += "declare @insertquery2 nvarchar(max)";
                    tableDDL += "declare @insertquery3 nvarchar(max)";
                    tableDDL += "set @insertquery = N' delete from " + TableName + " where  cast(Current_Year + ''-'' + Current_Month +''-01'' as datetime) between(select[Current_Date] from tbl_ListOfFile_Master where filename = ''" + TableName + "'') and (select Next_Date from tbl_ListOfFile_Master where filename = ''" + TableName + "'')  ';";
                    //tableDDL += "set  @insertquery1 = '  ''" + CurrentMonth + "'' in(' + @COLUMNS + ')';";
                    //tableDDL += "set @insertquery2 = ' and ''" + CurrnetYear + "'' in(' + @COLUMNS + ')';";

                    //tableDDL += "';";
                    tableDDL += "Exec (@insertquery)";


                    SQLConnection.ConnectionString = conString;
                    SQLConnection.Open();
                    SQLCmd = new SqlCommand(tableDDL, SQLConnection);
                    SQLCmd.ExecuteNonQuery();
                    SQLConnection.Close();




                    List<string> charsToRemove = new List<string>() { "@", "\'", "\"", ",", "\r", "\n" };
                    int rowcount = dt.Rows.Count;
                    int limit = 999;
                    int skip = 0;




                    while (limit <= rowcount)
                    {
                        tableDDL1 = "";
                        tableDDL1 += "DECLARE @COLUMNS NVARCHAR(max)";
                        tableDDL1 += "SELECT @Columns = SubString (Replace(( SELECT Top " + columncount + " ', ' + QUOTENAME(Column_name ) from INFORMATION_SCHEMA.columns WHERE Table_name ='" + tablename + "' ";
                        tableDDL1 += "FOR XML PATH ( '' )),'&amp;','&'), 3, 100000)";
                        tableDDL1 += "declare @insertquery nvarchar(max)";
                        tableDDL1 += "set @insertquery = ' insert into [dbo].[" + tablename + "](' + @COLUMNS + ')";
                        tableDDL1 += "values ";
                        //dt.Rows.Cast<System.Data.DataRow>().Take(rowcount).Skip(skip);
                        //.Skip(skip).Take(rowcount)
                        foreach (DataRow row in dt.Rows.Cast<System.Data.DataRow>().Skip(skip).Take(limit))
                        {
                            tableDDL1 += "(";

                            foreach (var item in row.ItemArray)
                            {
                                tableDDL1 += " Ltrim(RTRIM(''" + Filter1(item.ToString(), charsToRemove) + "'')) " + ", ";
                                //  tableDDL1 += "''" + Filter1(item.ToString(), charsToRemove) + "''" + ",";

                            }
                            tableDDL1 = tableDDL1.Substring(0, tableDDL1.Length - 1);
                            //tableDDL1 = " TRIM(''" + CurrentMonth + "'')" + ",";
                            //tableDDL1 = " TRIM(''" + CurrnetYear + "'')";
                            tableDDL1 += "),";

                        }

                        string output = tableDDL1.Substring(0, tableDDL1.Length - 1);
                        tableDDL1 = output;
                        tableDDL1 += "';";
                        tableDDL1 += "Exec (@insertquery)";
                        rowcount -= limit;
                        skip += limit;
                        SQLConnection.ConnectionString = conString;
                        SQLConnection.Open();
                        SQLCmd = new SqlCommand(tableDDL1, SQLConnection);
                        SQLCmd.ExecuteNonQuery();
                        SQLConnection.Close();
                    }

                    if (rowcount > 0 && rowcount < 1000)
                    {
                        tableDDL1 = "";
                        tableDDL1 += "DECLARE @COLUMNS NVARCHAR(max)";
                        tableDDL1 += "SELECT @Columns = SubString (Replace(( SELECT Top " + columncount + " ', ' + QUOTENAME(Column_name ) from INFORMATION_SCHEMA.columns WHERE Table_name ='" + tablename + "' ";
                        tableDDL1 += "FOR XML PATH ( '' )),'&amp;','&'), 3, 100000)";
                        tableDDL1 += "declare @insertquery nvarchar(max)";
                        tableDDL1 += "set @insertquery = ' insert into [dbo].[" + tablename + "](' + @COLUMNS + ')";
                        tableDDL1 += "values ";
                        //dt.Rows.Cast<System.Data.DataRow>().Take(rowcount).Skip(skip);
                        foreach (DataRow row in dt.Rows.Cast<System.Data.DataRow>().Skip(skip).Take(rowcount))
                        {
                            tableDDL1 += "(";

                            foreach (var item in row.ItemArray)
                            {
                                tableDDL1 += " Ltrim(RTRIM(''" + Filter1(item.ToString(), charsToRemove) + "'')) " + ", ";
                                // tableDDL1 += "''" + Filter1(item.ToString(), charsToRemove) + "''" + ",";

                            }
                            tableDDL1 = tableDDL1.Substring(0, tableDDL1.Length - 1);
                            //tableDDL1 = " TRIM(''" + CurrentMonth + "'')" + ",";
                            //tableDDL1 = " TRIM(''" + CurrnetYear + "'')";
                            tableDDL1 += "),";

                        }

                        string output = tableDDL1.Substring(0, tableDDL1.Length - 1);
                        tableDDL1 = output;
                        tableDDL1 += "';";
                        tableDDL1 += "Exec (@insertquery)";
                        rowcount -= limit;
                        skip += limit;
                        SQLConnection.ConnectionString = conString;
                        SQLConnection.Open();
                        SQLCmd = new SqlCommand(tableDDL1, SQLConnection);
                        SQLCmd.ExecuteNonQuery();
                        SQLConnection.Close();
                    }
                }
                else
                {
                    string tableDDL = ""; string tableDDL1 = "";
                    int columncount = dt.Columns.Count;
                    tableDDL = "";
                    tableDDL += "DECLARE @COLUMNS NVARCHAR(max)";
                    tableDDL += "SELECT @Columns = SubString (Replace(( SELECT Top " + columncount + " ', ' + QUOTENAME(Column_name ) from INFORMATION_SCHEMA.columns WHERE Table_name ='" + tablename + "' ";
                    tableDDL += "FOR XML PATH ( '' )),'&amp;','&'), 3, 100000)";
                    tableDDL += "declare @insertquery nvarchar(max)";
                    tableDDL += "declare @insertquery1 nvarchar(max)";
                    tableDDL += "declare @insertquery2 nvarchar(max)";
                    tableDDL += "declare @insertquery3 nvarchar(max)";
                    tableDDL += "set @insertquery = N' delete from " + TableName + " where  cast(Current_Year + ''-'' + Current_Month +''-01'' as datetime) between(select[Current_Date] from tbl_ListOfFile_Master where filename = ''" + TableName + "'') and (select Next_Date from tbl_ListOfFile_Master where filename = ''" + TableName + "'')  ';";
                    //tableDDL += "set  @insertquery1 = '  ''" + CurrentMonth + "'' in(' + @COLUMNS + ')';";
                    //tableDDL += "set @insertquery2 = ' and ''" + CurrnetYear + "'' in(' + @COLUMNS + ')';";

                    //tableDDL += "';";
                    tableDDL += "Exec (@insertquery)";


                    SQLConnection.ConnectionString = conString;
                    SQLConnection.Open();
                    SQLCmd = new SqlCommand(tableDDL, SQLConnection);
                    SQLCmd.ExecuteNonQuery();
                    SQLConnection.Close();




                    List<string> charsToRemove = new List<string>() { "@", "\'", "\"", ",", "\r", "\n" };
                    int rowcount = dt.Rows.Count;
                    int limit = 999;
                    int skip = 0;




                    while (limit <= rowcount)
                    {
                        tableDDL1 = "";
                        tableDDL1 += "DECLARE @COLUMNS NVARCHAR(max)";
                        tableDDL1 += "SELECT @Columns = SubString (Replace(( SELECT Top " + columncount + " ', ' + QUOTENAME(Column_name ) from INFORMATION_SCHEMA.columns WHERE Table_name ='" + tablename + "' ";
                        tableDDL1 += "FOR XML PATH ( '' )),'&amp;','&'), 3, 100000)";
                        tableDDL1 += "declare @insertquery nvarchar(max)";
                        tableDDL1 += "set @insertquery = ' insert into [dbo].[" + tablename + "](' + @COLUMNS + ')";
                        tableDDL1 += "values ";
                        //dt.Rows.Cast<System.Data.DataRow>().Take(rowcount).Skip(skip);
                        //.Skip(skip).Take(rowcount)
                        foreach (DataRow row in dt.Rows.Cast<System.Data.DataRow>().Skip(skip).Take(limit))
                        {
                            tableDDL1 += "(";

                            foreach (var item in row.ItemArray)
                            {
                                //tableDDL1 += " Ltrim(RTRIM(''" + Filter1(item.ToString(), charsToRemove) + "'')) " + ", ";
                                tableDDL1 += "''" + Filter1(item.ToString(), charsToRemove) + "'' " + ", ";
                                //  tableDDL1 += "''" + Filter1(item.ToString(), charsToRemove) + "''" + ",";

                            }
                            tableDDL1 = tableDDL1.Substring(0, tableDDL1.Length - 2);
                            //tableDDL1 = " TRIM(''" + CurrentMonth + "'')" + ",";
                            //tableDDL1 = " TRIM(''" + CurrnetYear + "'')";
                            tableDDL1 += "),";

                        }

                        string output = tableDDL1.Substring(0, tableDDL1.Length - 1);
                        tableDDL1 = output;
                        tableDDL1 += "';";
                        tableDDL1 += "Exec (@insertquery)";
                        rowcount -= limit;
                        skip += limit;
                        SQLConnection.ConnectionString = conString;
                        SQLConnection.Open();
                        SQLCmd = new SqlCommand(tableDDL1, SQLConnection);
                        SQLCmd.ExecuteNonQuery();
                        SQLConnection.Close();
                    }

                    if (rowcount > 0 && rowcount < 1000)
                    {
                        tableDDL1 = "";
                        tableDDL1 += "DECLARE @COLUMNS NVARCHAR(max)";
                        tableDDL1 += "SELECT @Columns = SubString (Replace(( SELECT Top " + columncount + " ', ' + QUOTENAME(Column_name ) from INFORMATION_SCHEMA.columns WHERE Table_name ='" + tablename + "' ";
                        tableDDL1 += "FOR XML PATH ( '' )),'&amp;','&'), 3, 100000)";
                        tableDDL1 += "declare @insertquery nvarchar(max)";
                        tableDDL1 += "set @insertquery = ' insert into [dbo].[" + tablename + "](' + @COLUMNS + ')";
                        tableDDL1 += "values ";
                        //dt.Rows.Cast<System.Data.DataRow>().Take(rowcount).Skip(skip);
                        foreach (DataRow row in dt.Rows.Cast<System.Data.DataRow>().Skip(skip).Take(rowcount))
                        {
                            tableDDL1 += " ( ";

                            foreach (var item in row.ItemArray)
                            {
                                tableDDL1 += "''" + Filter1(item.ToString(), charsToRemove) + "'' " + ", ";
                                //   tableDDL1 += " ''" + Filter1(item.ToString(), charsToRemove) + "''" + ", ";

                            }
                            tableDDL1 = tableDDL1.Substring(0, tableDDL1.Length - 2);
                            //tableDDL1 = " TRIM(''" + CurrentMonth + "'')" + ",";
                            //tableDDL1 = " TRIM(''" + CurrnetYear + "'')";
                            tableDDL1 += "), ";

                        }

                        string output = tableDDL1.Substring(0, tableDDL1.Length - 2);
                        tableDDL1 = output;
                        tableDDL1 += "';";
                        tableDDL1 += "Exec (@insertquery)";
                        rowcount -= limit;
                        skip += limit;
                        SQLConnection.ConnectionString = conString;
                        SQLConnection.Open();
                        SQLCmd = new SqlCommand(tableDDL1, SQLConnection);
                        SQLCmd.ExecuteNonQuery();
                        SQLConnection.Close();
                    }
                }

                // }


                //else
                //{
                //    string tableDDL = ""; string tableDDL1 = "";
                //    int columncount = dt.Columns.Count;
                //    tableDDL = "";
                //    tableDDL += "DECLARE @COLUMNS NVARCHAR(max)";
                //    tableDDL += "SELECT @Columns = SubString (Replace(( SELECT Top " + columncount + " ', ' + QUOTENAME(Column_name ) from INFORMATION_SCHEMA.columns WHERE Table_name ='" + tablename + "' ";
                //    tableDDL += "FOR XML PATH ( '' )),'&amp;','&'), 3, 100000)";
                //    tableDDL += "declare @insertquery nvarchar(max)";
                //    tableDDL += "declare @insertquery1 nvarchar(max)";
                //    tableDDL += "declare @insertquery2 nvarchar(max)";
                //    tableDDL += "declare @insertquery3 nvarchar(max)";
                //    tableDDL += "set @insertquery = N' delete from " + TableName + " where  cast(Current_Year + ''-'' + Current_Month + ''-''+FORMAT (getdate(), ''dd'') as date) between(select[Current_Date] from tbl_ListOfFile_Master where filename = ''" + TableName + "'') and (select Next_Date from tbl_ListOfFile_Master where filename = ''" + TableName + "'')  ';";
                //    //tableDDL += "set  @insertquery1 = '  ''" + CurrentMonth + "'' in(' + @COLUMNS + ')';";
                //    //tableDDL += "set @insertquery2 = ' and ''" + CurrnetYear + "'' in(' + @COLUMNS + ')';";

                //    //tableDDL += "';";
                //    tableDDL += "Exec (@insertquery)";


                //    SQLConnection.ConnectionString = conString;
                //    SQLConnection.Open();
                //    SQLCmd = new SqlCommand(tableDDL, SQLConnection);
                //    SQLCmd.ExecuteNonQuery();
                //    SQLConnection.Close();




                //    List<string> charsToRemove = new List<string>() { "@", "\'", "\"", ",", "-", "\r", "\n" };
                //    int rowcount = dt.Rows.Count;
                //    int limit = 999;
                //    int skip = 0;




                //    while (limit <= rowcount)
                //    {
                //        tableDDL1 = "";
                //        tableDDL1 += "DECLARE @COLUMNS NVARCHAR(max)";
                //        tableDDL1 += "SELECT @Columns = SubString (Replace(( SELECT Top " + columncount + " ', ' + QUOTENAME(Column_name ) from INFORMATION_SCHEMA.columns WHERE Table_name ='" + tablename + "' ";
                //        tableDDL1 += "FOR XML PATH ( '' )),'&amp;','&'), 3, 100000)";
                //        tableDDL1 += "declare @insertquery nvarchar(max)";
                //        tableDDL1 += "declare @insertquery1 nvarchar(max)";
                //        tableDDL1 += "declare @insertquery2 nvarchar(max)";
                //        tableDDL1 += "declare @insertquery3 nvarchar(max)";
                //        //tableDDL1 += "set @insertquery = N' delete from " + tablename + " where  ';";
                //        //tableDDL1 += "set  @insertquery1 = '  ''" + CurrentMonth + "'' in(' + @COLUMNS + ')';";
                //        //tableDDL1 += "set @insertquery2 = ' and ''" + CurrnetYear + "'' in(' + @COLUMNS + ')';";
                //        tableDDL1 += " set @insertquery3 = ' insert into [dbo].[" + tablename + "] (' + @COLUMNS + ')";
                //        tableDDL1 += " values ";
                //        /*dt.Rows.Cast<System.Data.DataRow>().Take(limit).Skip(skip);*/
                //        foreach (DataRow row in dt.Rows.Cast<System.Data.DataRow>().Skip(skip).Take(limit))
                //        {
                //            tableDDL1 += "(";

                //            foreach (var item in row.ItemArray)
                //            {

                //                tableDDL1 += " ''" + Filter1(item.ToString(), charsToRemove) + "'' " + ",";

                //            }
                //            tableDDL1 = tableDDL1.Substring(0, tableDDL1.Length - 1);
                //            //tableDDL1 = "TRIM(''" + CurrentMonth + "'')" + ",";
                //            //tableDDL1 = "TRIM(''" + CurrnetYear + "'')";
                //            tableDDL1 += "),";

                //        }

                //        string output = tableDDL1.Substring(0, tableDDL1.Length - 1);
                //        tableDDL1 = output;
                //        tableDDL1 += "';";
                //        tableDDL1 += "Exec (@insertquery3)";

                //        rowcount -= limit;
                //        skip += limit;
                //        SQLConnection.ConnectionString = conString;
                //        SQLConnection.Open();

                //        SQLCmd = new SqlCommand(tableDDL1, SQLConnection);
                //        SQLCmd.ExecuteNonQuery();
                //        SQLConnection.Close();
                //    }

                //    if (rowcount > 0 && rowcount < 1000)
                //    {
                //        tableDDL1 = "";
                //        tableDDL1 += "DECLARE @COLUMNS NVARCHAR(max)";
                //        tableDDL1 += "SELECT @Columns = SubString (Replace(( SELECT Top " + columncount + " ', ' + QUOTENAME(Column_name ) from INFORMATION_SCHEMA.columns WHERE Table_name ='" + tablename + "'  ";
                //        tableDDL1 += "FOR XML PATH ( '' )),'&amp;','&'), 3, 100000)";
                //        tableDDL1 += "declare @insertquery nvarchar(max)";
                //        tableDDL1 += "set @insertquery = ' insert into [dbo].[" + tablename + "](' + @COLUMNS + ')";
                //        tableDDL1 += "values ";
                //        //dt.Rows.Cast<System.Data.DataRow>().Take(rowcount).Skip(skip);
                //        foreach (DataRow row in dt.Rows.Cast<System.Data.DataRow>().Skip(skip).Take(rowcount))
                //        {
                //            tableDDL1 += "(";

                //            foreach (var item in row.ItemArray)
                //            {

                //                tableDDL1 += "TRIM(''" + Filter1(item.ToString(), charsToRemove) + "'')" + ",";

                //            }
                //            tableDDL1 = tableDDL1.Substring(0, tableDDL1.Length - 1);
                //            //tableDDL1 = " TRIM(''" + CurrentMonth + "'')" + ",";
                //            //tableDDL1 = " TRIM(''" + CurrnetYear + "'')";
                //            tableDDL1 += "),";

                //        }

                //        string output = tableDDL1.Substring(0, tableDDL1.Length - 1);
                //        tableDDL1 = output;
                //        tableDDL1 += "';";
                //        tableDDL1 += "Exec (@insertquery)";
                //        rowcount -= limit;
                //        skip += limit;
                //        SQLConnection.ConnectionString = conString;
                //        SQLConnection.Open();
                //        SQLCmd = new SqlCommand(tableDDL1, SQLConnection);
                //        SQLCmd.ExecuteNonQuery();
                //        SQLConnection.Close();
                //    }

                //}












































            }
            catch (Exception ex)
            {
                if (retry <= 3)
                {

                    goto retry_possibility;
                }
                else
                {
                    uploadsendmail(toemail, filename, DateTime.Now);
                }

            }




        }
        private DataTable GetDataTableFromExcel(string fullPath, string month, string year)
        {
            FileStream stream = null;
            ISheet sheet;
            string sFileExtension = Path.GetExtension(fullPath).ToLower();
            using (stream = new FileStream(fullPath, System.IO.FileMode.OpenOrCreate))
            {




                stream.Position = 0;

                if (sFileExtension == ".xls")

                {

                    HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                    hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                    sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  

                }

                else

                {

                    XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                    hssfwb.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;
                    sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   

                }
            }
            DataTable dt = new DataTable(sheet.SheetName);

            // write header row
            IRow headerRow = sheet.GetRow(0);
            int j = 2;
            int co = 0;
            foreach (ICell headerCell in headerRow)
            {
                //if (headerCell.ToString().ToLower() == "country")
                //{
                //    if (co == 2)
                //    {
                //        dt.Columns.Add("country1");
                //    }
                //    else
                //    {

                //    }
                //}
                //else
                //{

                try
                {
                    DataColumnCollection columns = dt.Columns;
                    if (columns.Contains(headerCell.ToString().Replace("\n", " ").ToLower()))
                    {
                        dt.Columns.Add(headerCell.ToString().Replace("\n", " ") + (j++));
                    }
                    else
                    {
                        dt.Columns.Add(headerCell.ToString().Replace("\n", " "));
                    }

                }
                catch (Exception ex)
                {
                    dt.Columns.Add(headerCell.ToString().Replace("\n", " ") + (j++));
                }
                // }




            }
            dt.Columns.Add("Current_Month").DefaultValue = month;
            dt.Columns.Add("Current_Year").DefaultValue = year;


            // write the rest
            int rowIndex = 0;
            foreach (IRow row in sheet)
            {
                // skip header row
                if (rowIndex++ == 0) continue;

                // add row into datatable
                var cells = new List<ICell>();


                for (int i = 0; i < headerRow.Cells.Count; i++)
                {
                    cells.Add(row.GetCell(i, MissingCellPolicy.CREATE_NULL_AS_BLANK));
                }

                dt.Rows.Add(cells.Select(c => getCellValue(c).ToString()).ToArray());





            }


            return dt;
        }

        public string Filter1(string str, List<string> charsToRemove)
        {
            foreach (string c in charsToRemove)
            {
                str = str.Replace(c.ToString(), String.Empty);
            }

            return str;
        }




        public void uploadsendmail(string tomail, string filename, DateTime current)
        {
            try
            {
                using (var client = new SmtpClient("smtp.sendgrid.net", 25))
                {




                    MailMessage mail = new MailMessage();

                    string Sender = "apikey";
                    string Password = "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";
                    string EmailFrom = "ibcpsc@tatamotors.com";
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(Sender, Password);
                    mail.From = new MailAddress(EmailFrom, "IBCPSC");
                    mail.To.Add(new MailAddress("nikhil.vig@teamcomputers.com"));
                    mail.To.Add(new MailAddress("ankushr.team@tatamotors.com"));
                    // mail.CC.Add(new MailAddress("babita@teamcomputers.com"));
                    mail.Subject = "Reminder for DataEntry";
                    mail.Body = "Dear Stakeholder,<br/><br/>File is not Uploaded on the server.<br/> Email ID : " + tomail + " <br/> File Name : " + filename + "<br/>  Uploaded on : " + current + "";
                    mail.IsBodyHtml = true;

                    client.Send(mail);









                }
            }
            catch (Exception ex)
            {

            }


        }



    }







}