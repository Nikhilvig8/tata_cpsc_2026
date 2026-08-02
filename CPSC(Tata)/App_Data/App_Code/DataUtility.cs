using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using System.Configuration;
using System.Net;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;


namespace DataUtilityLayer
{
    public class DataUtility : Connection
    {
        public SqlDataAdapter daCampus;
        public DataTable dtCampus;
        public DataSet dsCampus;
        public SqlDataReader drCampus;
        string path = string.Empty;
        public Double countrecord;
        static string str = string.Empty;
        static string table = string.Empty;
        static string valuefoupdate = string.Empty;

       
        
        public bool ExecuteSqlProcedure(SqlCommand cmdCampus)
        {
            try
            {
                OpeneConnection();
                cmdCampus.Connection = ConCampus;
                cmdCampus.CommandTimeout = 1000;
                cmdCampus.CommandType = System.Data.CommandType.StoredProcedure;
                cmdCampus.ExecuteNonQuery();
                CloseConnection();
                DisposeConnection();
                return true;
            }
            catch (Exception ex )
            {
                return false;
            }

        }
        public bool ExecuteSqlProcedure2(SqlCommand cmdCampus)
        {
            try
            {
                OpeneConnection1();
                cmdCampus.Connection = ConCampus1;
                cmdCampus.CommandTimeout = 1000;
                cmdCampus.CommandType = System.Data.CommandType.StoredProcedure;
                cmdCampus.ExecuteNonQuery();
                CloseConnection1();
                DisposeConnection1();

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public string ExecuteSqlProcedure1(SqlCommand cmdCampus)
        {
            try
            {
                OpeneConnection();
                cmdCampus.Connection = ConCampus;
                cmdCampus.CommandTimeout = 1000;
                cmdCampus.CommandType = System.Data.CommandType.StoredProcedure;
                cmdCampus.ExecuteNonQuery();
                CloseConnection();
                DisposeConnection();

                return "Saved";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
              

        /// <summary>
        /// This Method is used to execute sql command for Campus Solution.
        /// </summary>
        /// <param name="cmdCampus">SqlCommand cmdCampus</param>
        /// <returns> bool </returns>
        public bool ExecuteSqlCommand(SqlCommand cmdCampus)
        {
            try
            {
                OpeneConnection();
                cmdCampus.Connection = ConCampus;
                cmdCampus.CommandTimeout = 120;
                cmdCampus.CommandType = System.Data.CommandType.Text;
                cmdCampus.ExecuteNonQuery();
                CloseConnection();
                DisposeConnection();

                return true;
            }
            catch(Exception ex )
            {
                return false;
            }

        }

        /// <summary>
        /// This Method is used to retrive data from database in a DataTable with help of StoredProcedure.
        /// </summary>
        /// <param name="cmdCampus">SqlCommand cmdCampus</param>
        /// <returns> DataTable </returns>
        public DataTable GetDataTableWithProc(SqlCommand cmdCampus)
        {
            try
            {
                OpeneConnection();
                cmdCampus.Connection = ConCampus;
                cmdCampus.CommandTimeout = 1000;
                cmdCampus.CommandType = System.Data.CommandType.StoredProcedure;
                daCampus = new SqlDataAdapter();
                dtCampus = new DataTable();
                daCampus.SelectCommand = cmdCampus;
                daCampus.Fill(dtCampus);
                CloseConnection();
                DisposeConnection();
                return dtCampus;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// This Method is used to retrive data from database in a DataTable with help of Query.
        /// </summary>
        /// <param name="cmdCampus">SqlCommand cmdCampus</param>
        /// <returns> DataTable </returns>
        public DataTable GetDataTableWithQuery(SqlCommand cmdCampus)
        {
            try
            {
                OpeneConnection();
                cmdCampus.Connection = ConCampus;
                cmdCampus.CommandTimeout = 120;
                cmdCampus.CommandType = System.Data.CommandType.Text;
                daCampus = new SqlDataAdapter();
                dtCampus = new DataTable();
                daCampus.SelectCommand = cmdCampus;
                daCampus.Fill(dtCampus);
                CloseConnection();
                DisposeConnection();
                return dtCampus;
            }
            catch (Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// This Method is used to retrive data from database in a DataSet with help of StoredProcedure.
        /// </summary>
        /// <param name="cmdCampus">SqlCommand cmdCampus</param>
        /// <returns> DataSet </returns>
        public DataSet GetDataSetWithProc(SqlCommand cmdCampus)
        {
            try
            {
                OpeneConnection();
                cmdCampus.Connection = ConCampus;
                cmdCampus.CommandTimeout = 120;
                cmdCampus.CommandType = System.Data.CommandType.StoredProcedure;
                daCampus = new SqlDataAdapter();
                dsCampus = new DataSet();
                daCampus.SelectCommand = cmdCampus;
                daCampus.Fill(dsCampus);
                CloseConnection();
                DisposeConnection();
                return dsCampus;
            }
            catch(Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// This Method is used to retrive data from database in a DataSet with help of Query.
        /// </summary>
        /// <param name="cmdCampus">SqlCommand cmdCampus</param>
        /// <returns> DataSet </returns>
        public DataSet GetDataSetWithQuery(SqlCommand cmdCampus)
        {
            try
            {
                OpeneConnection();
                cmdCampus.Connection = ConCampus;
                cmdCampus.CommandTimeout = 60;
                cmdCampus.CommandType = System.Data.CommandType.Text;
                daCampus = new SqlDataAdapter();
                dsCampus = new DataSet();
                daCampus.SelectCommand = cmdCampus;
                daCampus.Fill(dsCampus);
                CloseConnection();
                DisposeConnection();
                return dsCampus;
            }
            catch(Exception )
            {
                return null;
            }

        }


        /// <summary>
        /// This Function execute sql procedure with help of SqlDataReader
        /// </summary>
        /// <param name="cmdCampus"></param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader GetDataReaderWithProc(SqlCommand cmdCampus)
        {
            try
            {
                OpeneConnection();
                cmdCampus.Connection = ConCampus;
                cmdCampus.CommandTimeout = 60;
                cmdCampus.CommandType = System.Data.CommandType.StoredProcedure;
                drCampus = cmdCampus.ExecuteReader();
                CloseConnection();
                DisposeConnection();
                return drCampus;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// This Function execute sql Query with help of SqlDataReader
        /// </summary>
        /// <param name="cmdCampus"></param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader GetDataReaderWithQuery(SqlCommand cmdCampus)
        {
            try
            {
                OpeneConnection();
                cmdCampus.Connection = ConCampus;
                cmdCampus.CommandTimeout = 60;
                cmdCampus.CommandType = System.Data.CommandType.Text;
                drCampus = cmdCampus.ExecuteReader();
                CloseConnection();
                DisposeConnection();
                return drCampus;
            }
            catch(Exception )
            {
                return null;
            }
        }
        public void sendMail(string Subject, string Name, string Body, string To)
        {
            try
            {
                To = To.Trim();// +"" + Domain; 
               
                MailMessage mail = new MailMessage();
                mail.To.Add(To);
                //mail.Bcc.Add(To);
                mail.From = new MailAddress(FROMEMAIL, Name, System.Text.Encoding.UTF8);
                mail.Subject = Subject;
                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.Body = Body;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;
                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential(FROMEMAIL, FROMPWD);
                client.Port = 587;
                client.Host = SMTP;
                client.EnableSsl = true;
                try
                {
                    client.Send(mail);
                }
                catch (Exception ) { }
            }
            catch
            {
            }
        }
                 
        string _PoweredBy;
        public string PoweredBy
        {
            get
            {
                return _PoweredBy;
            }
            set
            {
                _PoweredBy = value;
            }
        }
        string _ApplicationName;
        public string ApplicationName
        {
            get
            {
                return _ApplicationName;
            }
            set
            {
                _ApplicationName = value;
            }
        }
        string _Header;
        public string Header
        {
            get
            {
                return _Header;
            }
            set
            {
                _Header = value;
            }
        }
        string _Footer;
        public string Footer
        {
            get
            {
                return _Footer;
            }
            set
            {
                _Footer = value;
            }
        }
        
        public void ReadToEnd(string filePath,string tblName,string DBName,string EntityName)
        {
            try
            {

                string CSVFilePathName = filePath;
                string[] Lines = File.ReadAllLines(CSVFilePathName);
                string[] Fields;
                Fields = Lines[0].Split(new char[] { '|' });
                int Cols = Fields.GetLength(0);
                DataTable dtDataSource = new DataTable();
                //1st row must be column names; force lower case to ensure matching later on.
                for (int i = 0; i < Cols; i++)
                { 
                dtDataSource.Columns.Add(Fields[i], typeof(string));
                }
                dtDataSource.Columns.Add("DBName", typeof(string));
                dtDataSource.Columns.Add("EntityName", typeof(string));
                DataRow Row;
                for (int i = 1; i < Lines.GetLength(0); i++)
                {
                    Fields = Lines[i].Split(new char[] { '|' });
                    Row = dtDataSource.NewRow();

                    for (int f = 0; f < Cols; f++)
                    {
                        Row[f] = Fields[f];
                    }
                    Row["DBName"] = DBName;
                    Row["EntityName"] = EntityName;
                    dtDataSource.Rows.Add(Row);
                }

                createsqltable(dtDataSource, tblName, DBName, EntityName);


            }

            catch (Exception ex)
            {

            }         
           

        }

        public void createsqltable(DataTable dt, String TableName1,string DBName,string EntityName)
        {
            string tablename = TableName1;
            string strconnection = ConfigurationManager.ConnectionStrings["constr"].ToString();
            string table = "";
            table += "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + tablename + "]') AND type in (N'U'))";
            table += "BEGIN ";
            table += "create table " + tablename + "";
            table += "(";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i != dt.Columns.Count - 1)
                    table += "[" + dt.Columns[i].ColumnName + "]" + " " + "varchar(max)" + ",";
                else
                    table += "[" + dt.Columns[i].ColumnName + "]" + " " + "varchar(max)";
            }
            table += ") ";
            table += "END";
            InsertQuery(table, strconnection);
            bulkcopy(strconnection, path, tablename, dt);
        }
        public void InsertQuery(string qry, string connection)
        {


            SqlConnection _connection = new SqlConnection(connection);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = qry;
            cmd.Connection = _connection;
            _connection.Open();
            cmd.ExecuteNonQuery();
            _connection.Close();

        }

        public void bulkcopy(string str, string filepath, string tablename, DataTable dt1)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(str))
            {
                //set the name of the destination table that data will be inserted into.
                //table must already exist.
                bulkCopy.DestinationTableName = tablename;
                bulkCopy.WriteToServer(dt1);               
            }
        }

        public void Droptables(string Procedurename, string connection,string TableName)
        {


            SqlConnection _connection = new SqlConnection(connection);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = Procedurename;
            cmd.Connection = _connection;
            _connection.Open();
            cmd.ExecuteNonQuery();
            _connection.Close();

        }


        //insert code for GlCodeExclusion & itemExclusion
        public void inserttable(DataTable dt, string tblname,string DBName,string EntityName)
        {
            int count1 = 0;
            string tablename = tblname;
            string strconnection = ConfigurationManager.ConnectionStrings["constr"].ToString();

            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                //build dyinamic query...
                if (tablename == "tblItemListExclusion")
                {
                    table = "";
                    table += "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + tablename + "]') AND type in (N'U'))";
                    table += "BEGIN ";
                    table += "insert into " + tablename + "";
                    table += "(ItemCode,IsExcluded,flag,EntityName,DBName) values(";

                    for (int j = 0; j <= dt.Columns.Count - 1; j++)
                    {
                        if (i != dt.Columns.Count - 1)
                            table += "'"+dt.Rows[i][j] +"'"+ "," + "1" + "," + "1" + ",'" + EntityName + "','" + DBName+"'";
                        else
                            table += "'"+dt.Rows[i][j] + "'"+"," + "1" + "," + "1" + ",'" + EntityName + "','" + DBName + "'";
                        count1 = checkreptedvalues(dt.Rows[i][j].ToString(), tablename);
                    }

                    table += ") ";
                    table += "END";
                }
                else
                {
                    table = "";
                    table += "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + tablename + "]') AND type in (N'U'))";
                    table += "BEGIN ";
                    table += "insert into " + tablename + "";
                    table += "(GLCode,IsExcluded,flag,EntityName,DBName) values(";

                    for (int j = 0; j <= dt.Columns.Count - 1; j++)
                    {
                        if (i != dt.Columns.Count - 1)
                            table += "'"+dt.Rows[i][j] + "'"+"," + "1" + "," + "1" + ",'" + EntityName.ToString() + "','" + DBName.ToString()+"'";
                        else
                            table += "'"+dt.Rows[i][j] + "'"+"," + "1" + "," + "1" + ",'" + EntityName.ToString() + "','" + DBName.ToString()+"'";
                        count1 = checkreptedvalues(dt.Rows[i][j].ToString(), tablename);
                    }

                    table += ") ";
                    table += "END";

                }
                if (count1 == 0)
                {
                    InsertQuery1(table, strconnection);
                }
                else
                {
                    string conn = ConfigurationManager.ConnectionStrings["constr"].ToString();

                    SqlConnection con2 = new SqlConnection(conn);
                    con2.Open();
                    if (tablename == "tblItemListExclusion")
                    {
                        str = "Update " + tablename + " set IsExcluded=1,flag=1,EntityName='" + EntityName.ToString() + "',DBName='" + DBName.ToString() + "' where ItemCode='" + valuefoupdate + "'";
                    }
                    else
                    {

                        str = "Update " + tablename + " set IsExcluded=1,flag=1,EntityName='" + EntityName.ToString() + "',DBName='" + DBName.ToString() + "' where GLCode='" + valuefoupdate + "'";
                    }
                    SqlCommand objCmd = new SqlCommand();
                    objCmd = new SqlCommand(str, con2);
                    int count = Convert.ToInt32(objCmd.ExecuteScalar());
                    countrecord = count;
                    con2.Close();
                }
            }
        }
        //checking multiple values before inserting 
        public int checkreptedvalues(string value, string tabname)
        {


            int trigg;
            string conn = ConfigurationManager.ConnectionStrings["constr"].ToString();

            SqlConnection con = new SqlConnection(conn);
            con.Open();
            if (tabname == "tblItemListExclusion")
            {
                str = "select count(*) from " + tabname + " where ItemCode='" + value + "'";
            }
            else
            {
                str = "select count(*) from " + tabname + " where GLCode='" + value + "'";
            }
            SqlCommand objCmd = new SqlCommand();
            objCmd = new SqlCommand(str, con);
            Double count = Convert.ToDouble(objCmd.ExecuteScalar());
            countrecord = count;
            con.Close();
            if (count > 0)
            {
                valuefoupdate = value;
                trigg = 1;
            }
            else
            {
                trigg = 0;

            }
            return trigg;
        }
        public void InsertQuery1(string qry, string connection)
        {


            SqlConnection _connection = new SqlConnection(connection);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = qry;
            cmd.Connection = _connection;
            _connection.Open();
            cmd.ExecuteNonQuery();
            _connection.Close();

        }

        //For Adjustment bulk upload


   protected void ShowNoResultFound(DataTable source, GridView gv)
    {
        source.Rows.Add(source.NewRow()); // create a new blank row to the DataTable
        // Bind the DataTable which contain a blank row to the GridView
        gv.DataSource = source;
        gv.DataBind();
        // Get the total number of columns in the GridView to know what the Column Span should be
        int columnsCount = gv.Columns.Count;
        gv.Rows[0].Cells.Clear();// clear all the cells in the row
        gv.Rows[0].Cells.Add(new TableCell()); //add a new blank cell
        gv.Rows[0].Cells[0].ColumnSpan = columnsCount; //set the column span to the new added cell

        //You can set the styles here
        gv.Rows[0].Cells[0].HorizontalAlign = HorizontalAlign.Center;
        gv.Rows[0].Cells[0].ForeColor = System.Drawing.Color.Black;
        gv.Rows[0].Cells[0].Font.Bold = true;
        //set No Results found to the new added cell
        gv.Rows[0].Cells[0].Text = "No Record Found.";
    }

  protected void EmptyGridFix(GridView grdView)
    {
        try
        {
            if(grdView.Rows.Count == 0 &&
                grdView.DataSource == null)
            {
                DataTable dt = null;
                if(grdView.DataSource is DataSet)
                {
                    dt = ((DataSet)grdView.DataSource).Tables[0].Clone();
                }
                else if (grdView.DataSource is DataTable)
                    dt = ((DataTable)grdView.DataSource).Clone();

                if(dt == null)
                    return;

                dt.Rows.Add(dt.NewRow()); // add empty row
                grdView.DataSource = dt;
                grdView.DataBind();

                // hide row
                grdView.Rows[0].Visible = false;
                grdView.Rows[0].Controls.Clear();
            }

            // normally executes at all postbacks
            if(grdView.Rows.Count == 1 &&
                grdView.DataSource == null)
            {
                bool bIsGridEmpty = true;

                // check first row that all cells empty
                for(int i = 0; i < grdView.Rows[0].Cells.Count; i++)
                {
                    if(grdView.Rows[0].Cells[i].Text != string.Empty)
                    {
                        bIsGridEmpty = false;
                    }
                }
                // hide row
                if(bIsGridEmpty)
                {
                    grdView.Rows[0].Visible = false;
                    grdView.Rows[0].Controls.Clear();
                }
            }
        }
        catch (Exception ex)
        {
          
        }
    }

  


    }
}

