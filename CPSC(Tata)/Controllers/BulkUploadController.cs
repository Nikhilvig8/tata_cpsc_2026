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
using InputOutput.Models;
using System.Text.RegularExpressions;

namespace InputOutput.Controllers
{
    public class BulkUploadController : Controller
    {
        // GET: BulkUpload
        public ActionResult BulkUploadTargetIndex()
        {
            UploadFile UploadFile = new UploadFile();
            return View(UploadFile);
            //return View();
        }


        public ActionResult BulkUploadActualIndex()
        {
            UploadFile UploadFile = new UploadFile();
            return View(UploadFile);
            //return View();
        }


        public ActionResult SamiriddhiBulkUpload()
        {
            UploadFile UploadFile = new UploadFile();
            return View(UploadFile);
            //return View();
        }


        public ActionResult DTToExcel(string Proc, string flag, string filename)
        {
            //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
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
                List<string> listtoRemove = new List<string> { "LOGIN", "KPI_ID", "ISLOCKED_TARGET", "ISLOCKED_ACTUAL" };
                for (int i = dt.Columns.Count - 1; i >= 0; i--)
                {
                    DataColumn dc = dt.Columns[i];
                    if (listtoRemove.Contains(dc.ColumnName.ToUpper()))
                    {
                        dt.Columns.Remove(dc);
                    }
                }
            }

            ////DataSet data = JsonConvert.DeserializeObject<DataSet>(dt);
            ////your datatable
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


        [HttpPost]
        public ActionResult BulkUploadTargetIndex(UploadFile UploadFile)
        {
            //DataTable dt = new DataTable();
           DataTable dt_Orignal = new DataTable();
            if (ModelState.IsValid)
            {

                if (UploadFile.ExcelFile.ContentLength > 0)
                {
                    if (UploadFile.ExcelFile.FileName.EndsWith(".xlsx") || UploadFile.ExcelFile.FileName.EndsWith(".xls"))
                    {
                        XLWorkbook Workbook;
                        try
                        {
                            Workbook = new XLWorkbook(UploadFile.ExcelFile.InputStream);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError(String.Empty, $"Check your file. {ex.Message}");
                            return View();
                        }
                        IXLWorksheet WorkSheet = null;

                        try//incase if the sheet you are looking for is not found
                        {
                            WorkSheet = Workbook.Worksheet(1);

                        }
                        catch
                        {
                            ModelState.AddModelError(String.Empty, "Sheet not found!");
                            return View();
                        }
                        //WorkSheet.FirstRow().Delete();//if you want to remove ist row
                        dt_Orignal = ExceltoDatatable(WorkSheet);
                        
                        string err_msg = validatedatatable(dt_Orignal);
                        
                        if (err_msg == "True")
                        {
                            dt_Orignal = ExceltoDatatable(WorkSheet);
                            err_msg = UpdateBulkupload(dt_Orignal, "BulkUploadTarget_Update");
                            if(err_msg=="succ")
                            { 
                            Session["result"] = err_msg;
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(String.Empty, err_msg);
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Only .xlsx and .xls files are allowed");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Not a valid file");
                    return View();
                }
            }

            return View();
        }

        public DataTable ExceltoDatatable(IXLWorksheet _WorkSheet)
        {
            DataTable _dt = new DataTable();
            bool firstRow = true;
            foreach (var row in _WorkSheet.RowsUsed())
            {
                //do something here

                if (firstRow)
                {
                    foreach (IXLCell cell in row.Cells())
                    {
                        _dt.Columns.Add(cell.Value.ToString());
                    }
                    firstRow = false;
                }
                else
                {
                    //Add rows to DataTable.
                    _dt.Rows.Add();
                    int i = 0;

                    foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))
                    {
                        _dt.Rows[_dt.Rows.Count - 1][i] = cell.Value.ToString();
                        i++;
                    }
                }

            }

            return _dt;
        }

        public string validatedatatable(DataTable datatab)
        {
            string msg = string.Empty;

            DataTable datatab2 = GetDataForValidation("getDataDump1", "Target");

            foreach (DataRow row in datatab2.Rows)
            {

                //test for DataLocked
                if (row[datatab2.Columns.Count - 1].ToString() == "True")
                {
                    msg = "Data is locked for entry so please contact with CPSC Team.";

                    return msg;
                }


            }
            foreach (DataColumn dc in datatab.Columns)
            {
                if (dc.ColumnName == "ForYear" || dc.ColumnName == "Month" || dc.ColumnName == "Region" || dc.ColumnName == "State" || dc.ColumnName == "Dealer_Code" || dc.ColumnName == "Dealer_Name" || dc.ColumnName == "Division_Name" || dc.ColumnName == "LOB" || dc.ColumnName == "KPI_Name" || dc.ColumnName == "Target_Value")
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

            DataTable dt_CheckNonNumric = CheckNonNumericValues(datatab,"Target_Value");

            if (dt_CheckNonNumric.Rows.Count > 0)
            {
                msg = "Found Non Numeric Value,Only Numeric values are allow in Target/Actual column.Please check the data once and retry.";

                return msg;
            }

            DataTable dtresult = GetDataDifference(datatab, datatab2);

            if(dtresult.Rows.Count>0)
            {
                msg = "Data are not match with dump.Please check the data or download the data format to match.";

                return msg;
            }
           
           

            return msg;
        }

        private DataTable CheckNonNumericValues(DataTable dtnonNum,String Column_Name)
        {
            var numbersonly = dtnonNum.AsEnumerable()
    .Where(x => Regex.IsMatch(x.Field<string>(Column_Name), @"^[0-9]\d*(\.\d+)?$"))
    .ToList();
            //see result 1

            //get non-numbers
            var nonnumbersonly = dtnonNum.AsEnumerable()
                .Except(numbersonly)
                .ToList();

            DataTable dt_return = new DataTable();

            foreach (DataColumn c in dtnonNum.Columns)
            {

                dt_return.Columns.Add(new DataColumn(c.ColumnName, typeof(string)));


            }

            //Bind the data from both user data sets and add it to the datatable


            foreach (var dataRow in nonnumbersonly)

            {

                DataRow dr = dt_return.NewRow();

                dr[0] = dataRow[0];

                dr[1] = dataRow[1];

                dr[2] = dataRow[2];

                dr[3] = dataRow[3];

                dr[4] = dataRow[4];

                dr[5] = dataRow[5];
                dr[6] = dataRow[6];

                dr[7] = dataRow[7];
                dr[8] = dataRow[8];

                dr[9] = dataRow[9];


                dt_return.Rows.Add(dr);

            }

            return dt_return;
        }

        private DataTable CheckNonNumericValuesSamiriddhi(DataTable dtnonNum, String Column_Name)
        {
            var numbersonly = dtnonNum.AsEnumerable()
    .Where(x => Regex.IsMatch(x.Field<string>(Column_Name), @"^[0-9]\d*(\.\d+)?$"))
    .ToList();
            //see result 1

            //get non-numbers
            var nonnumbersonly = dtnonNum.AsEnumerable()
                .Except(numbersonly)
                .ToList();

            DataTable dt_return = new DataTable();

            foreach (DataColumn c in dtnonNum.Columns)
            {

                dt_return.Columns.Add(new DataColumn(c.ColumnName, typeof(string)));


            }

            //Bind the data from both user data sets and add it to the datatable


            foreach (var dataRow in nonnumbersonly)

            {

                DataRow dr = dt_return.NewRow();

                dr[0] = dataRow[0];

                dr[1] = dataRow[1];

                dr[2] = dataRow[2];

                dr[3] = dataRow[3];

                dr[4] = dataRow[4];


                dt_return.Rows.Add(dr);

            }

            return dt_return;
        }

        private DataTable GetDataDifference(DataTable dt1, DataTable dt2)

        {
            string result = string.Empty; 
            List<string> listtoRemove = new List<string> { "LOGIN", "KPI_ID", "TARGET_VALUE", "ACTUAL_VALUE","REGION","STATE","DEALER_NAME", "ISLOCKED_TARGET", "ISLOCKED_ACTUAL" };
            for (int i = dt1.Columns.Count - 1; i >= 0; i--)
            {
                DataColumn dc = dt1.Columns[i];
                if (listtoRemove.Contains(dc.ColumnName.ToUpper()))
                {
                    dt1.Columns.Remove(dc);
                }
            }

            List<string> listtoRemove1 = new List<string> { "LOGIN", "KPI_ID", "TARGET_VALUE", "ACTUAL_VALUE","REGION", "STATE", "DEALER_NAME", "ISLOCKED_TARGET", "ISLOCKED_ACTUAL" };
            for (int i = dt2.Columns.Count - 1; i >= 0; i--)
            {
                DataColumn dc = dt2.Columns[i];
                if (listtoRemove1.Contains(dc.ColumnName.ToUpper()))
                {
                    dt2.Columns.Remove(dc);
                }
            }
            //Query first dataset

            IEnumerable<DataRow> query1 = from userData in dt1.AsEnumerable()

                                          select userData;



            //Query second dataset

            IEnumerable<DataRow> query2 = from userData in dt2.AsEnumerable()

                                          select userData;



            //Create data tables and get the data from above queries

            DataTable userData1 = query1.CopyToDataTable();

            DataTable userData2 = query2.CopyToDataTable();



            //Now use Except operator to find the data in first set and not in second

            var userDataFirstSet = userData1.AsEnumerable().Except(userData2.AsEnumerable(),

                                                                       DataRowComparer.Default);

            //Find data in second and not in first

            var userDataSecondSet = userData2.AsEnumerable().Except(userData1.AsEnumerable(),

                                                                             DataRowComparer.Default);

            //Create a new data table and add new columns

            DataTable dtAll = new DataTable();

            foreach(DataColumn c in dt1.Columns)
            { 

            dtAll.Columns.Add(new DataColumn(c.ColumnName, typeof(string)));

            
            }

            //Bind the data from both user data sets and add it to the datatable


            foreach (var dataRow in userDataFirstSet)

            {

                DataRow dr = dtAll.NewRow();

                dr[0] = dataRow[0];

                dr[1] = dataRow[1];

                dr[2] = dataRow[2];

                dr[3] = dataRow[3];

                dr[4] = dataRow[4];

                dr[5] = dataRow[5];


                dtAll.Rows.Add(dr);

            }



            foreach (var dataRow in userDataSecondSet)

            {

                DataRow dr = dtAll.NewRow();

                dr[0] = dataRow[0];

                dr[1] = dataRow[1];

                dr[2] = dataRow[2];

                dr[3] = dataRow[3];

                dr[4] = dataRow[4];

                dr[5] = dataRow[5];

                dtAll.Rows.Add(dr);


            }
            return dtAll;
        }

        private DataTable GetSamiriddhiDataDifference(DataTable dt1, DataTable dt2)

        {
            string result = string.Empty;

            //Query first dataset

            IEnumerable<DataRow> query1 = from userData in dt1.AsEnumerable()

                                          select userData;



            //Query second dataset

            IEnumerable<DataRow> query2 = from userData in dt2.AsEnumerable()

                                          select userData;



            //Create data tables and get the data from above queries

            DataTable userData1 = query1.CopyToDataTable();
            userData1.Columns.RemoveAt(4);

            DataTable userData2 = query2.CopyToDataTable();

            //Now use Except operator to find the data in first set and not in second

            var userDataFirstSet = userData1.AsEnumerable().Except(userData2.AsEnumerable(),

                                                                       DataRowComparer.Default);

            //Find data in second and not in first

            var userDataSecondSet = userData2.AsEnumerable().Except(userData1.AsEnumerable(),

                                                                             DataRowComparer.Default);

            //Create a new data table and add new columns

            DataTable dtAll = new DataTable();

            foreach (DataColumn c in dt1.Columns)
            {

                dtAll.Columns.Add(new DataColumn(c.ColumnName, typeof(string)));


            }

            //Bind the data from both user data sets and add it to the datatable


            foreach (var dataRow in userDataFirstSet)

            {

                DataRow dr = dtAll.NewRow();

                dr[0] = dataRow[0];

                dr[1] = dataRow[1];

                dr[2] = dataRow[2];

                dr[3] = dataRow[3];

                dr[4] = dataRow[4];

                dtAll.Rows.Add(dr);

            }



            foreach (var dataRow in userDataSecondSet)

            {

                DataRow dr = dtAll.NewRow();

                dr[0] = dataRow[0];

                dr[1] = dataRow[1];

                dr[2] = dataRow[2];

                dr[3] = dataRow[3];

                dr[4] = dataRow[4];

                dtAll.Rows.Add(dr);


            }
            return dtAll;
        }

        public DataTable GetDataForValidation(string Proc, string flag)
        {
            //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
            DataTable dt = new DataTable();
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
                List<string> listtoRemove = new List<string> { "LOGIN", "KPI_ID", "TARGET_VALUE", "ACTUAL_VALUE" };
                for (int i = dt.Columns.Count - 1; i >= 0; i--)
                {
                    DataColumn dc = dt.Columns[i];
                    if (listtoRemove.Contains(dc.ColumnName.ToUpper()))
                    {
                        dt.Columns.Remove(dc);
                    }
                }
            }
            return dt;
        }

        public DataTable GetSamiriddhiDataForValidation(string Proc, string flag)
        {
            //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
            DataTable dt = new DataTable();
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
            }
            return dt;
        }

        //final update tahe excel into DB
        public string UpdateBulkupload(DataTable dt,string Proc)
        {
            //filename = filename + DateTime.Now.ToShortDateString()+".xlsx";
            string flag = string.Empty;
            string CS1 = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string _sql2 = string.Empty;
            _sql2 = Proc;
            using (SqlConnection cn = new SqlConnection(CS1))
            {

                var cmd = new SqlCommand(_sql2, cn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@Uid";
                param.Value = Session["Uid"].ToString();
                cmd.Parameters.Add(param);

                SqlParameter param1 = new SqlParameter();
                param1.ParameterName = "@DataDumpType";
                param1.Value = dt;
                cmd.Parameters.Add(param1);

                cn.Open();
                int count= cmd.ExecuteNonQuery();
                cn.Close();
                cn.Dispose();
                if(count>0)
                {
                    flag = "succ";
                }
                else
                {
                    flag = "False";
                }

            }
            return flag;
        }

        [HttpPost]
        public ActionResult BulkUploadActualIndex(UploadFile UploadFile)
        {
            //DataTable dt = new DataTable();
            DataTable dt_Orignal = new DataTable();
            if (ModelState.IsValid)
            {

                if (UploadFile.ExcelFile.ContentLength > 0)
                {
                    if (UploadFile.ExcelFile.FileName.EndsWith(".xlsx") || UploadFile.ExcelFile.FileName.EndsWith(".xls"))
                    {
                        XLWorkbook Workbook;
                        try
                        {
                            Workbook = new XLWorkbook(UploadFile.ExcelFile.InputStream);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError(String.Empty, $"Check your file. {ex.Message}");
                            return View();
                        }
                        IXLWorksheet WorkSheet = null;

                        try//incase if the sheet you are looking for is not found
                        {
                            WorkSheet = Workbook.Worksheet(1);

                        }
                        catch
                        {
                            ModelState.AddModelError(String.Empty, "Sheet not found!");
                            return View();
                        }
                        //WorkSheet.FirstRow().Delete();//if you want to remove ist row
                        dt_Orignal = ExceltoDatatable(WorkSheet);

                        string err_msg = validatedatatable_actual(dt_Orignal);

                        if (err_msg == "True")
                        {
                            dt_Orignal = ExceltoDatatable(WorkSheet);
                            err_msg = UpdateBulkupload(dt_Orignal, "BulkUploadActual_Update");
                            if (err_msg == "succ")
                            {
                                Session["result"] = err_msg;
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(String.Empty, err_msg);
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Only .xlsx and .xls files are allowed");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Not a valid file");
                    return View();
                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult SamiriddhiBulkUpload(UploadFile UploadFile)
        {
            //DataTable dt = new DataTable();
            DataTable dt_Orignal = new DataTable();
            if (ModelState.IsValid)
            {

                if (UploadFile.ExcelFile.ContentLength > 0)
                {
                    if (UploadFile.ExcelFile.FileName.EndsWith(".xlsx") || UploadFile.ExcelFile.FileName.EndsWith(".xls"))
                    {
                        XLWorkbook Workbook;
                        try
                        {
                            Workbook = new XLWorkbook(UploadFile.ExcelFile.InputStream);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError(String.Empty, $"Check your file. {ex.Message}");
                            return View();
                        }
                        IXLWorksheet WorkSheet = null;

                        try//incase if the sheet you are looking for is not found
                        {
                            WorkSheet = Workbook.Worksheet(1);

                        }
                        catch
                        {
                            ModelState.AddModelError(String.Empty, "Sheet not found!");
                            return View();
                        }
                        //WorkSheet.FirstRow().Delete();//if you want to remove ist row
                        dt_Orignal = ExceltoDatatable(WorkSheet);

                        string err_msg = validatedatatable_Samiriddhi(dt_Orignal);

                        if (err_msg == "True")
                        {
                            dt_Orignal = ExceltoDatatable(WorkSheet);
                            err_msg = UpdateBulkupload(dt_Orignal, "SamiriddhiBulkUploadUpdate");
                            if (err_msg == "succ")
                            {
                                Session["result"] = err_msg;
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(String.Empty, err_msg);
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Only .xlsx and .xls files are allowed");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Not a valid file");
                    return View();
                }
            }

            return View();
        }

        public string validatedatatable_actual(DataTable datatab)
        {
            string msg = string.Empty;

            DataTable datatab2 = GetDataForValidation("getDataDump1", "Actual");

            foreach (DataRow row in datatab2.Rows)
            {

                //test for DataLocked
                if (row[datatab2.Columns.Count - 1].ToString() == "True")
                {
                    msg = "Data is locked for entry so please contact with CPSC Team.";

                    return msg;
                }


            }
            foreach (DataColumn dc in datatab.Columns)
            {
                if (dc.ColumnName == "ForYear" || dc.ColumnName == "Month" || dc.ColumnName == "Region" || dc.ColumnName == "State" || dc.ColumnName == "Dealer_Code" || dc.ColumnName == "Dealer_Name" || dc.ColumnName == "Division_Name" || dc.ColumnName == "LOB" || dc.ColumnName == "KPI_Name" || dc.ColumnName == "Actual_Value")
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

            DataTable dt_CheckNonNumric = CheckNonNumericValues(datatab, "Actual_Value");

            if (dt_CheckNonNumric.Rows.Count > 0)
            {
                msg = "Found Non Numeric Value,Only Numeric values are allow in Target/Actual column.Please check the data once and retry.";

                return msg;
            }

            DataTable dtresult = GetDataDifference(datatab, datatab2);

            if (dtresult.Rows.Count > 0)
            {
                msg = "Data are not match with dump.Please check the data or download the data format to match.";

                return msg;
            }



            return msg;
        }

        public string validatedatatable_Samiriddhi(DataTable datatab)
        {
            string msg = string.Empty;

            DataTable datatab2 = GetSamiriddhiDataForValidation("getDataForTest", "Actual");

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
                if (dc.ColumnName == "Year Month" || dc.ColumnName == "Dealer_Name" || dc.ColumnName == "Dealer_code" || dc.ColumnName == "Region" || dc.ColumnName == "DWM Audit")
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

            DataTable dt_CheckNonNumric = CheckNonNumericValuesSamiriddhi(datatab, "DWM Audit");

            if (dt_CheckNonNumric.Rows.Count > 0)
            {
                msg = "Found Non Numeric Value,Only Numeric values are allow in Target/Actual column.Please check the data once and retry.";

                return msg;
            }

            DataTable dtresult = GetSamiriddhiDataDifference(datatab, datatab2);

            if (dtresult.Rows.Count > 0)
            {
                msg = "Data are not match with dump.Please check the data or download the data format to match.";

                return msg;
            }



            return msg;
        }
    }
}