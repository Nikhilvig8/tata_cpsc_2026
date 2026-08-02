using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InputOutput.Controllers
{
    public class ReportsController : Controller
    {
        // GET: Reports
        public ActionResult Data_Complition_AfterSales_Actual_Zone(string zone)
        {
           
            return View();
        }
        public ActionResult Data_Complition_Sales_Actual_Zone(string zone)
        {

            return View();
        }

        public ActionResult Data_Complition_Sales_Target_Zone(string zone)
        {

            return View();
        }

        public ActionResult Data_Complition_AfterSales_Target_Zone(string zone)
        {

            return View();
        }

        public ActionResult Data_Complition_Sales_Target_DivWies(string state)
        {

            return View();
        }

        public ActionResult Data_Complition_AfterSales_Target_DivWies(string state)
        {

            return View();
        }
        public ActionResult Data_Complition_Sales_Actual_DivWies(string state)
        {

            return View();
        }
        public ActionResult Data_Complition_AfterSales_Actual_DivWies(string state)
        {

            return View();
        }
    }
}