using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
//using CrystalDecisions.Shared;
using System.Configuration;
namespace DataUtilityLayer
{
    public class Connection : System.Web.UI.Page
    {
        public SqlConnection ConCampus;
        public SqlConnection ConCampus1;
        public Connection()
        {
             ConnectionString =  ConfigurationManager.ConnectionStrings["constr"].ToString();
        }
        string _Domain;
        public string Domain
        {
            get
            {
                return _Domain;
            }
            set
            {
                _Domain = value;
            }
        }
        string _FROMPWD;
        public string FROMPWD
        {
            get
            {
                return _FROMPWD;
            }
            set
            {
                _FROMPWD = value;
            }
        }
        string _FROMEMAIL;
        public string FROMEMAIL
        {
            get
            {
                return _FROMEMAIL;
            }
            set
            {
                _FROMEMAIL = value;
            }
        }
        string _SMTP;
        public string SMTP
        {
            get
            {
                return _SMTP;
            }
            set
            {
                _SMTP = value;
            }
        }
        string _ConnectionString;
        public string ConnectionString
        {
            get
            {
                return _ConnectionString;
            }
            set
            {
                _ConnectionString = value;
            }
        }
                
        //public ConnectionInfo CrystalConnection
        //{
        //    get
        //    {
        //        ConnectionInfo connectionInfo = new ConnectionInfo();
        //        connectionInfo.ServerName = ConfigurationManager.AppSettings["ServerName"];
        //        connectionInfo.DatabaseName = ConfigurationManager.AppSettings["DatabaseName"];
        //        connectionInfo.UserID = ConfigurationManager.AppSettings["UserID"];
        //        connectionInfo.Password = ConfigurationManager.AppSettings["Password"];
        //        return connectionInfo;
        //    }
        //}

        /// <summary>
        /// This Property is used to access the connection string  for CampusSolution DataBase.
        /// </summary>
        protected string Scrub(string text) { return text.Replace("&nbsp;", ""); }
        
        public string ConCampus_ConnectionString
        {
            get
            {
                return ConnectionString;//System.Configuration.ConfigurationManager.AppSettings["Connection"];
            }
        }
        public string ConCampus_ConnectionString1
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["Connection"];
            }
        }

        /// <summary>
        /// This function is used to open connection for eEducation
        /// 
        /// </summary>
        public void OpeneConnection()
        {

            if (ConCampus == null)
            {
                ConCampus = new SqlConnection(ConCampus_ConnectionString);
            }
            if (ConCampus.State != System.Data.ConnectionState.Open)
            {
                @ConCampus.Open();
            }

        }
        public void OpeneConnection1()
        {

            if (ConCampus1 == null)
            {
                ConCampus1 = new SqlConnection(ConCampus_ConnectionString1);
            }
            if (ConCampus1.State != System.Data.ConnectionState.Open)
            {
                ConCampus1.Open();
            }

        }

        /// <summary>
        /// This Function is used to close the connection for eEducation
        /// </summary>
        public void CloseConnection()
        {

            if (ConCampus.State == System.Data.ConnectionState.Open)
            {
                ConCampus.Close();
            }

        }
        public void CloseConnection1()
        {

            if (ConCampus1.State == System.Data.ConnectionState.Open)
            {
                ConCampus1.Close();
            }

        }

        /// <summary>
        /// This Function is used to dispose the connection for eEducation
        /// </summary>

        public void DisposeConnection()
        {

            if (ConCampus == null)
            {
                ConCampus.Dispose();
            }

        }
        public void DisposeConnection1()
        {

            if (ConCampus1 == null)
            {
                ConCampus1.Dispose();
            }

        }
    }
}
