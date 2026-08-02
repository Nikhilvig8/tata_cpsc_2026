using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace Execution
{
    public class Execution_Logs : ActionFilterAttribute,IExceptionFilter
    {
       
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string uid = filterContext.HttpContext.Session.Contents["Uid"].ToString();
            string message = "\r\n" + uid+" -> " + filterContext.ActionDescriptor.ControllerDescriptor.ControllerName +
                " -> " + filterContext.ActionDescriptor.ActionName + " -> OnActionExecuting \t- " +
                DateTime.Now.ToString() + "\r\n";
            LogExecutionTime(message);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            string uid = filterContext.HttpContext.Session.Contents["Uid"].ToString();
            string message = "\r\n" + uid+" -> " + filterContext.ActionDescriptor.ControllerDescriptor.ControllerName +
                " -> " + filterContext.ActionDescriptor.ActionName + " -> OnActionExecuted \t- " +
                DateTime.Now.ToString() + "\r\n";
            LogExecutionTime(message);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            string uid = filterContext.HttpContext.Session.Contents["Uid"].ToString();
            string message = "\r\n" + uid+" -> "+filterContext.RouteData.Values["controller"].ToString() +
                " -> " + filterContext.RouteData.Values["action"].ToString() +
                " -> OnResultExecuting \t- " + DateTime.Now.ToString() + "\r\n";
            LogExecutionTime(message);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            string uid = filterContext.HttpContext.Session.Contents["Uid"].ToString();
            string message = "\r\n" + uid+" -> "+filterContext.RouteData.Values["controller"].ToString() +
                " -> " + filterContext.RouteData.Values["action"].ToString() +
                " -> OnResultExecuted \t- " + DateTime.Now.ToString() + "\r\n";
            LogExecutionTime(message);
            LogExecutionTime("---------------------------------------------------------\r\n");
        }

        public void OnException(ExceptionContext filterContext)
        {
            string uid = filterContext.HttpContext.Session.Contents["Uid"].ToString();
            string message = "\r\n" + uid+" -> "+filterContext.RouteData.Values["controller"].ToString() + " -> " +
               filterContext.RouteData.Values["action"].ToString() + " -> " +
               filterContext.Exception.Message + " -> "+ filterContext.Exception.StackTrace + " \t- " + DateTime.Now.ToString() + "\n";
            LogExecutionTime(message);
            LogExecutionTime("---------------------------------------------------------\r\n");
        }

        private void LogExecutionTime(string message)
        {
            File.AppendAllText(HttpContext.Current.Server.MapPath("~/Logs/Logs.txt"), message);
                            
        }
    }
}