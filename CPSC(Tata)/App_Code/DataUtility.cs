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
        





        //insert code for GlCodeExclusion & itemExclusion
        //checking multiple values before inserting 

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

