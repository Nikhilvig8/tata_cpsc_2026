using System;
using System.IO;
using System.Text.RegularExpressions;

namespace InputOutput
{
    // Checkmarx "SQL_Injection" / "Stored_Path_Traversal" remediation.
    //
    // Several export/upload actions (HomeController.DTToExcel, BulkUploadController.DTToExcel,
    // GetDataForValidation, UpdateBulkupload, ...) receive the *name of the stored procedure to run*
    // straight from the query string and hand it to SqlDataAdapter/SqlCommand as CommandText. Even
    // with CommandType.StoredProcedure that lets a caller run ANY procedure the DB login can reach
    // (or inject batch syntax). The fix is an allow-list: the requested name is only ever compared
    // against the fixed set below, and the value actually sent to SQL Server is always one of these
    // compile-time string literals - never the request value itself.
    //
    // To add a new export, add its procedure name to the switch below.
    public static class SqlGuard
    {
        public static string ResolveProcedure(string requested)
        {
            string name = (requested ?? string.Empty).Trim();
            if (name.StartsWith("[", StringComparison.Ordinal) && name.EndsWith("]", StringComparison.Ordinal))
            {
                name = name.Substring(1, name.Length - 2);
            }

            switch (name.ToLowerInvariant())
            {
                // Bulk upload templates / validation
                case "getdatadump": return "getDataDump";
                case "getdatadump1": return "getDataDump1";
                case "getsamriddhidata": return "getSamriddhiData";
                case "datadump_actual_291122_3_0": return "DataDump_Actual_291122_3_0";
                case "datadump_target_291122_3_0": return "DataDump_Target_291122_3_0";
                case "getdatafortest": return "getDataForTest";
                case "bulkuploadtarget_update": return "BulkUploadTarget_Update";
                case "bulkuploadactual_update": return "BulkUploadActual_Update";
                case "samiriddhibulkuploadupdate": return "SamiriddhiBulkUploadUpdate";

                // DropDownController.Tempdataremove - clears the per-user temp selection tables

                case "tbltempci_clear": return "tblTempCI_Clear";

                case "tbltempdse_clear": return "tblTempDse_Clear";

                case "tbltempdsm_clear": return "tblTempDsm_Clear";

                case "tbltempdwm_clear": return "tblTempDWM_Clear";

                case "tbltempsafety_clear": return "tblTempSafety_Clear";


                // BulkExcelUpload (CPSC) - "SpInsert" + upload category
                case "spinsertsalessatisfaction": return "SpInsertSalesSatisfaction";
                case "spinsertactivitiesdoneandopen": return "SpInsertActivitiesDoneAndOpen";
                case "spinsertretail2": return "SpInsertRetail2";
                case "spinsertcei": return "SpInsertCEI";
                case "spinserttataok": return "SpInsertTataOK";
                case "spinsertamcpolicy": return "SpInsertAMCPolicy";
                case "spinsertworkshoprevenuereport": return "SpInsertWorkshopRevenueReport";
                case "spinsertactivitydetailedreport": return "SpInsertActivityDetailedReport";
                case "spinsertcpscwrongnumber": return "SpInsertCPSCWrongNumber";
                case "spinsertdp": return "SpInsertDP";
                case "spinsertdsadmn": return "SpInsertDSAdmn";

                // Report exports (Views/Home, Views/KPI, Views/ChangeReq)
                case "getbulkentryactual": return "getBulkEntryActual";
                case "getdataokdata": return "getDataOkData";
                case "getdataokdatatarget": return "getDataOkDataTarget";
                case "getdelayeddata": return "getDelayedData";
                case "getdivwisependingdata": return "getDivWisePendingData";
                case "getdivwisependingdataactual": return "getDivWisePendingDataActual";
                case "getdivwisependingdatatarget": return "getDivWisePendingDataTarget";
                case "getpendingapprovaldata": return "getPendingApprovalData";
                case "getpendinginputdata": return "getPendingInputData";
                case "getpendinginputdatatsm_csmwies": return "getPendingInputDataTSM_CSMWies";
                case "getscorecardmonths": return "getScoreCardMonths";
                case "getsuspecteddata": return "getSuspectedData";
                case "getchangereq_logs": return "getChangeReq_Logs";
                case "getcurrentstatus_actual": return "getCurrentStatus_Actual";
                case "getcurrentstatus_target": return "getCurrentStatus_Target";
                case "newreport_actual": return "NewReport_Actual";
                case "newreport_target": return "NewReport_Target";
                case "newreport_actual_3_0": return "NewReport_Actual_3_0";
                case "newreport_target_3_0": return "NewReport_Target_3_0";
                case "data_complition_aftersales_actual": return "Data_Complition_AfterSales_Actual";
                case "data_complition_aftersales_actual_divwies": return "Data_Complition_AfterSales_Actual_DivWies";
                case "data_complition_aftersales_actual_zone": return "Data_Complition_AfterSales_Actual_Zone";
                case "data_complition_aftersales_target": return "Data_Complition_AfterSales_Target";
                case "data_complition_aftersales_target_divwies": return "Data_Complition_AfterSales_Target_DivWies";
                case "data_complition_aftersales_target_zone": return "Data_Complition_AfterSales_Target_Zone";
                case "data_complition_sales_actual": return "Data_Complition_Sales_Actual";
                case "data_complition_sales_actual_divwies": return "Data_Complition_Sales_Actual_DivWies";
                case "data_complition_sales_actual_zone": return "Data_Complition_Sales_Actual_Zone";
                case "data_complition_sales_target": return "Data_Complition_Sales_Target";
                case "data_complition_sales_target_divwies": return "Data_Complition_Sales_Target_DivWies";
                case "data_complition_sales_target_zone": return "Data_Complition_Sales_Target_Zone";
                default:
                    throw new ArgumentException("Requested report is not allowed.");
            }
        }

        // "Actual"/"Target" style flags passed through to @ForActual_Target. Restricted to a plain
        // word so nothing structural can ride along even though it's already a bound parameter.
        public static string ResolveFlag(string requested)
        {
            string flag = (requested ?? string.Empty).Trim();
            if (!Regex.IsMatch(flag, @"^[A-Za-z0-9_ ]{0,50}$"))
            {
                throw new ArgumentException("Invalid flag.");
            }
            return flag;
        }

        // Report file names come from the query string and are joined onto the Reports folder.
        // Strip any directory component and restrict to a safe character set + .xlsx so a caller
        // can't write outside ~/Reports (e.g. "..\\..\\Web.config") or drop an executable file type.
        // Separators are replaced (not stripped) because the views build names such as
        // "Actuals_<uid>_" + DateTime.ToShortDateString() + ".xlsx", which may contain '/': the
        // export (DTToExcel) and the later Download call both pass through this same function, so
        // they always agree on the final on-disk name.
        public static string SafeReportFileName(string requested)
        {
            string name = Regex.Replace(requested ?? string.Empty, @"[^A-Za-z0-9_\-\. ]", "_");
            while (name.Contains("..")) name = name.Replace("..", ".");
            name = Path.GetFileName(name);
            if (string.IsNullOrWhiteSpace(name) || name.StartsWith(".", StringComparison.Ordinal))
            {
                name = "report";
            }
            if (!name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                name = Path.GetFileNameWithoutExtension(name) + ".xlsx";
            }
            return name;
        }
    }
}
