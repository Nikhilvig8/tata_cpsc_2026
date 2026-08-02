using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using DataUtilityLayer;
using System.Configuration;
namespace InputOutput.Models
{
    public class Transaction : DataUtilityLayer.DataUtility
    {
       public string conn = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        SqlCommand cmdObj;

        [Display(Name = "TSM_id")]
        public string TSM_id { get; set; }
        [Display(Name = "premise_location")]
        public string premise_location { get; set; }
        [Display(Name = "Dealer")]
        public string Dealer { get; set; }
        [Display(Name = "month")]
        public string Month { get; set; }

        [Display(Name = "LOB")]
        public string LOB { get; set; }

        [Display(Name = "Outlet")]
        public string Out { get; set; }

        
        [Display(Name = "LOB_Name")]
        public string LOB_Name { get; set; }

        [Display(Name = "Outlet_Name")]
        public string Out_Name { get; set; }

        [Display(Name = "monthdesc")]
        public string monthdesc { get; set; }

        [Display(Name = "Dealer_Name")]
        public string Dealer_Name { get; set; }

        [Display(Name = "KPI")]
        public string KPI { get; set; }

        [Display(Name = "Target")]
        public string Target { get; set; }
        [Display(Name = "Actual")]
        public string Actual { get; set; }
        [Display(Name = "Last_Month_Target")]
        public string Last_Month_Target { get; set; }

        [Display(Name = "Last_Month_Actual")]
        public string Last_Month_Actual { get; set; }
        [Display(Name = "Last_12_Month_Avg")]
        public string Last_12_Month_Avg { get; set; }

        [Display(Name = "Last_12_Month_Min")]
        public string Last_12_Month_Min { get; set; }
        [Display(Name = "Last_12_Month_Max")]
        
        public string Last_12_Month_Max { get; set; }
        [Display(Name = "SingelEntry_Tooltip_Target")]
        public string SingelEntry_Tooltip_Target { get; set; }

        [Display(Name = "SingelEntry_Tooltip_Actual")]
        public string SingelEntry_Tooltip_Actual { get; set; }
      
        [Display(Name = "Unique_Id")]
        public string Unique_Id { get; set; }
        [Display(Name = "Flag")]
        public string Flag { get; set; }

        [Display(Name = "TAFlag")]
        public string TAFlag { get; set; }

        [Display(Name = "Dealer_Level")]
        public string Dealer_Level { get; set; }

        [Display(Name = "Division_Level")]
        public string Division_Level { get; set; }

        [Display(Name = "LOB_Level")]
        public string LOB_Level { get; set; }
        [Display(Name = "Sorting")]
        public string Sorting { get; set; }


        [Display(Name = "Input_Type")]
        public string Input_Type { get; set; }

        [Display(Name = "Input_Pattern")]
        public string Input_Pattern { get; set; }

        [Display(Name = "Input_OnKey")]
        public string Input_OnKey { get; set; }

        [Display(Name = "KPI_Description")]
        public string KPI_Description { get; set; }

        [Display(Name = "Input_Popup")]
        public string Input_Popup { get; set; }


        [Display(Name = "IsTargetRequired")]
        public int IsTargetRequired { get; set; }


        [Display(Name = "IsActualRequired")]
        public int IsActualRequired { get; set; }

        public DataTable Transation_Data(string _Dealer, string _month, string _LOB,String _Out,string _sheetflag)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Transation_Data";

            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = _Dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = _month;
                 
            cmdObj.Parameters
                .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                .Value = _Out;
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = _LOB;
            cmdObj.Parameters
                 .Add(new SqlParameter("@User_Id", SqlDbType.NVarChar))
                 .Value = Session["Uid"].ToString();

            cmdObj.Parameters
                  .Add(new SqlParameter("@SheetFlag", SqlDbType.NVarChar))
                  .Value = _sheetflag;
           
            DataTable dt = new DataTable();
            dt = GetDataTableWithProc(cmdObj);
            return dt;
        }

        public DataTable  Approval_Transation_Data(string Uid,string _Dealer, string _month, string _LOB, String _Out, string _sheetflag)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Transation_Data";

            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = _Dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = _month;

            cmdObj.Parameters
                .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                .Value = _Out;
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = _LOB;
            cmdObj.Parameters
                 .Add(new SqlParameter("@User_Id", SqlDbType.NVarChar))
                 .Value = Uid;

            cmdObj.Parameters
                  .Add(new SqlParameter("@SheetFlag", SqlDbType.NVarChar))
                  .Value = _sheetflag;

            DataTable dt = new DataTable();
            dt = GetDataTableWithProc(cmdObj);
            return dt;
        }

        public DataTable Approved_Data(string _Unique_Id)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Select * from tblTransaction where [Unique_Id]='"+_Unique_Id+"'";

            
            DataTable dt = new DataTable();
            dt = GetDataTableWithQuery(cmdObj);
            return dt;
        }



        public bool Update_Data(string _Dealer, string _month, string _LOB,string _KPI,String _TValue,String _Out)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_Target";

            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = _Dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = _month;
            cmdObj.Parameters
                .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                .Value = _Out;
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = _LOB;

            cmdObj.Parameters
                 .Add(new SqlParameter("@t", SqlDbType.NVarChar))
                 .Value = _TValue;
            cmdObj.Parameters
                 .Add(new SqlParameter("@k", SqlDbType.NVarChar))
                 .Value = _KPI;
            cmdObj.Parameters
                 .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
                 .Value = Session["Uid"];

            if (ExecuteSqlProcedure(cmdObj))
            {
                
                return true; }
            else { return false; }

        }

        public bool Update_BULKData(string _Dealer, string _month, string _LOB, string _KPI, String _TValue, String _Out,string notification_id)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_BULKTarget";

            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = _Dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = _month;
            cmdObj.Parameters
                .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                .Value = _Out;
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = _LOB;

            cmdObj.Parameters
                 .Add(new SqlParameter("@t", SqlDbType.NVarChar))
                 .Value = _TValue;
            cmdObj.Parameters
                 .Add(new SqlParameter("@k", SqlDbType.NVarChar))
                 .Value = _KPI;
            cmdObj.Parameters
               .Add(new SqlParameter("@Notific_id", SqlDbType.NVarChar))
               .Value = notification_id;
            cmdObj.Parameters
                 .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
                 .Value = Session["Uid"];

            if (ExecuteSqlProcedure(cmdObj))
            {

                return true;
            }
            else { return false; }

        }

        public bool updateActual_UserId(string _month,String _LOB,string _Dealer,String _Out)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_ActualUser";

            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = _Dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = _month;
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = _LOB;
            cmdObj.Parameters
                  .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                  .Value = _Out;


            cmdObj.Parameters
                 .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                 .Value = Session["Uid"].ToString();

            if (ExecuteSqlProcedure(cmdObj))
            { return true; }
            else { return false; }

        }

        public bool updateActualBulk_UserId(string _month, string _Dealer)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_BulkActualUser";

            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = _Dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = _month;
         

            cmdObj.Parameters
                 .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                 .Value = Session["Uid"].ToString();

            if (ExecuteSqlProcedure(cmdObj))
            { return true; }
            else { return false; }

        }

        public bool updatehyg(string _no, String _desc, string _sel)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "UpdateHyg";

            cmdObj.Parameters
                .Add(new SqlParameter("@no", SqlDbType.NVarChar))
                .Value = _no;
            cmdObj.Parameters
                .Add(new SqlParameter("@desc", SqlDbType.NVarChar))
                .Value = _desc;
            cmdObj.Parameters
                  .Add(new SqlParameter("@Selection", SqlDbType.NVarChar))
                  .Value = _sel;
            

           

            if (ExecuteSqlProcedure(cmdObj))
            { return true; }
            else { return false; }

        }
        public bool updateTarget_UserId(string _month, String _LOB, string _Dealer, String _Out)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_TargetUser";

            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = _Dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = _month;
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = _LOB;
            cmdObj.Parameters
                  .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                  .Value = _Out;


            cmdObj.Parameters
                 .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                 .Value = Session["Uid"].ToString();

            if (ExecuteSqlProcedure(cmdObj))
            { return true; }
            else { return false; }

        }

        public bool updateTargetBulk_UserId(string _month,string _Dealer)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_BulkTargetUser";

            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = _Dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = _month;
           

            cmdObj.Parameters
                 .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                 .Value = Session["Uid"].ToString();

            if (ExecuteSqlProcedure(cmdObj))
            { return true; }
            else { return false; }

        }


        /* For update Approval status in Target_Actual_Approvels */
        public bool Update_Flag(string _Unique_id,string _Flag,string _Division_Id,string _LOB_Id,string flag,string fromwhom)
        {
            cmdObj = new SqlCommand();
           
             if (flag == "reject")
            {
                cmdObj.CommandText = "Update ttaa set [Approved_Status]='0',[App/rejected]='reject' from tblTarget_Actual_Approvels ttaa inner join tblNotifications tn on (ttaa.[Notification_id]=tn.[Notification_id]) where ttaa.[Unique_Id]='" + _Unique_id + "' and ttaa.[Type]='" + _Flag + "' and ttaa.Division_Id='" + _Division_Id + "' and ttaa.LOB_Id ='" + _LOB_Id + "' and tn.ForWhom ='" + Session["Uid"].ToString() + "' and tn.FromWhom ='" + fromwhom + "'";
            }
            else if (flag == "Rejectaccept")
            {
                cmdObj.CommandText = "Update ttaa set [Approved_Status]='0',[App/rejected]='accept' from tblTarget_Actual_Approvels ttaa inner join tblNotifications tn on (ttaa.[Notification_id]=tn.[Notification_id]) where ttaa.[Unique_Id]='" + _Unique_id + "' and ttaa.[Type]='" + _Flag + "' and ttaa.Division_Id='" + _Division_Id + "' and ttaa.LOB_Id ='" + _LOB_Id + "' and tn.ForWhom ='" + Session["Uid"].ToString() + "' and tn.FromWhom ='" + fromwhom + "'";
            }
            else
            {
                cmdObj.CommandText = "Update ttaa set ttaa.[Approved_Status]='1',ttaa.[App/rejected]='accept' from tblTarget_Actual_Approvels ttaa inner join tblNotifications tn on (ttaa.[Notification_id]=tn.[Notification_id]) where ttaa.[Unique_Id]='" + _Unique_id + "' and ttaa.[Type]='" + _Flag + "' and ttaa.Division_Id='" + _Division_Id + "' and ttaa.LOB_Id ='" + _LOB_Id + "' and tn.ForWhom ='" + Session["Uid"].ToString() + "' and tn.FromWhom ='" + fromwhom + "'";
            }




            if (ExecuteSqlCommand(cmdObj))
            {
                //Trans_Flag(_Unique_id, _Flag);
                return true; }
            else { return false; }

        }
        
        /* For update KPI Wies Approval in Transaction table */
        public void Trans_Flag(String _Unique_id,String _Flag)
        {
            cmdObj = new SqlCommand();

            if (_Flag == "Target")
            {
                cmdObj.CommandText = "Update tblTransaction set [Approved]=1 where [Unique_Id]='" + _Unique_id + "'";

            }
            else
            {
                cmdObj.CommandText = "Update tblTransaction set [Approved]=1 where [Unique_Id]='" + _Unique_id + "'";

            }



            ExecuteSqlCommand(cmdObj);
            
        }

        /* For update Notification into table as well as mapped data into Target_Actual_Approvels */
        public bool notification_Insert(string _month, string _LOB, string _Dealer,string _Out,string Flag)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Insert_Notification";

            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = _Dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = _month;
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = _LOB;
            cmdObj.Parameters
                  .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                  .Value = _Out;
            cmdObj.Parameters
                  .Add(new SqlParameter("@flag", SqlDbType.NVarChar))
                  .Value = Flag;

            cmdObj.Parameters
                 .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                 .Value = Session["Uid"].ToString();
            cmdObj.Parameters
                 .Add(new SqlParameter("@type", SqlDbType.NVarChar))
                 .Value = Session["type"].ToString();


            if (ExecuteSqlProcedure(cmdObj))
            { return true; }
            else { return false; }

        }

        public bool Bulknotification_Insert(string _month, string _LOB, string _Dealer, string _Out, string Flag,string notification_id)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Insert_BULKNotification";

            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = _Dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = _month;
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = _LOB;
            cmdObj.Parameters
                  .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                  .Value = _Out;
            cmdObj.Parameters
                  .Add(new SqlParameter("@flag", SqlDbType.NVarChar))
                  .Value = Flag;
            cmdObj.Parameters
                 .Add(new SqlParameter("@notification_id", SqlDbType.NVarChar))
                 .Value = notification_id;

            cmdObj.Parameters
                 .Add(new SqlParameter("@au", SqlDbType.NVarChar))
                 .Value = Session["Uid"].ToString();
            cmdObj.Parameters
                 .Add(new SqlParameter("@type", SqlDbType.NVarChar))
                 .Value = Session["type"].ToString();


            if (ExecuteSqlProcedure(cmdObj))
            { return true; }
            else { return false; }

        }


        public bool Update_ActualData(string _Dealer, string _month, string _LOB, string _KPI, String _AValue,String _Out)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_Actual";

            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = _Dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = _month;
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = _LOB;
                  
            cmdObj.Parameters
                  .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                  .Value = _Out;

            cmdObj.Parameters
                 .Add(new SqlParameter("@a", SqlDbType.NVarChar))
                 .Value = _AValue;
            cmdObj.Parameters
                 .Add(new SqlParameter("@k", SqlDbType.NVarChar))
                 .Value = _KPI;
            cmdObj.Parameters
                .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
                .Value = Session["Uid"];
            if (ExecuteSqlProcedure(cmdObj))
            {
               return true; }
            else { return false; }

        }
        public bool Update_BULKActualData(string _Dealer, string _month, string _LOB, string _KPI, String _AValue, String _Out,string Notification_id)
        {
            cmdObj = new SqlCommand();
            cmdObj.CommandText = "Update_BULKActual";

            cmdObj.Parameters
                .Add(new SqlParameter("@d", SqlDbType.NVarChar))
                .Value = _Dealer;
            cmdObj.Parameters
                .Add(new SqlParameter("@m", SqlDbType.NVarChar))
                .Value = _month;
            cmdObj.Parameters
                  .Add(new SqlParameter("@l", SqlDbType.NVarChar))
                  .Value = _LOB;

            cmdObj.Parameters
                  .Add(new SqlParameter("@o", SqlDbType.NVarChar))
                  .Value = _Out;

            cmdObj.Parameters
                 .Add(new SqlParameter("@a", SqlDbType.NVarChar))
                 .Value = _AValue;
            cmdObj.Parameters
                 .Add(new SqlParameter("@k", SqlDbType.NVarChar))
                 .Value = _KPI;
            cmdObj.Parameters
                .Add(new SqlParameter("@Notific_id", SqlDbType.NVarChar))
                .Value = Notification_id;
            cmdObj.Parameters
                .Add(new SqlParameter("@Uid", SqlDbType.NVarChar))
                .Value = Session["Uid"];
            if (ExecuteSqlProcedure(cmdObj))
            {
                return true;
            }
            else { return false; }

        }
    }
    public class TransactionViewModel
    {
        public TransactionViewModel()
        {
            Trans_list = new List<Transaction>();
            Trans = new Transaction();
        }
        public List<Transaction> Trans_list { get; set; }
        public Transaction Trans { get; set; }
    }
}